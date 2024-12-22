// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestReader.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance;

namespace Sisk.ManagedHttpListener.HttpSerializer;

sealed class HttpRequestReader {

    Stream _stream;
    byte [] buffer;

    const int BUFFER_LOOKAHEAD_OFFSET = 64;

    const byte SPACE = 0x20;
    const byte CARRIAGE_RETURN = 0x0D; //\r
    const byte DOUBLE_DOTS = 0x3A; // :

    static Encoding HeaderEncoding = Encoding.UTF8;

    public HttpRequestReader ( Stream stream, ref byte [] buffer ) {
        this._stream = stream;
        this.buffer = buffer;
    }

    public async ValueTask<(HttpRequestReadState, HttpRequestBase?)> ReadHttpRequest () {
        try {

            int read = await this._stream.ReadAsync ( this.buffer );

            if (read == 0) {
                return (HttpRequestReadState.StreamZero, null);
            }

            var request = this.ParseHttpRequest ( ref this.buffer, read );
            return (HttpRequestReadState.RequestRead, request);
        }
        catch (Exception ex) {
            Logger.LogInformation ( $"HttpRequestReader finished with exception: {ex.Message}" );
            return (HttpRequestReadState.StreamError, null);
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    public HttpRequestBase? ParseHttpRequest ( ref byte [] inputBuffer, int length ) {

        const int STEP_READ_METHOD = 0;
        const int STEP_READ_PATH = 1;
        const int STEP_READ_VERSION = 2;
        const int STEP_READ_HEADERS = 3;

        bool requestStreamFinished = false;

        ReadOnlySpan<byte> buffer = inputBuffer;

        ReadOnlySpan<byte> method = null!;
        ReadOnlySpan<byte> path = null!;
        ReadOnlySpan<byte> version = null!;

        ReadOnlySpan<byte> headerLine;
        ReadOnlySpan<byte> headerLineName;
        ReadOnlySpan<byte> headerLineValue;

        // 0 = parse method
        // 1 = parse url
        // 2 = parse version
        // 3 = parse header line
        int step = STEP_READ_METHOD;

        int methodIndex = 0;
        int pathIndex = 0;
        int versionIndex = 0;
        int headerLineIndex = 0;
        int headerLineSepIndex = 0;

        int headerSize = -1;
        bool keepAliveEnabled = true;
        long contentLength = 0;

        List<(string, string)> headers = new List<(string, string)> ( 16 );

        ref byte firstByte = ref MemoryMarshal.GetReference ( buffer );
        for (int i = 0; i < length; i++) {
            ref byte currentByte = ref Unsafe.Add ( ref firstByte, i );

            switch (step) {
                case STEP_READ_METHOD:
                    if (currentByte == SPACE) {
                        methodIndex = i;
                        method = buffer [ 0..i ];
                        step = STEP_READ_PATH;
                    }
                    break;

                case STEP_READ_PATH:
                    if (currentByte == SPACE) {
                        pathIndex = i;
                        path = buffer [ (methodIndex + 1)..i ];
                        step = STEP_READ_VERSION;
                    }
                    break;

                case STEP_READ_VERSION:
                    if (currentByte == CARRIAGE_RETURN) {
                        versionIndex = i;
                        version = buffer [ (pathIndex + 1)..i ];
                        headerLineIndex = i + 1; //+1 includes \LF
                        step = STEP_READ_HEADERS;
                    }
                    break;

                case STEP_READ_HEADERS:

                    // checks whether the current buffer has all the request headers. if not, read more data from the buffer
                    int bufferLength = buffer.Length;
                    if (i + BUFFER_LOOKAHEAD_OFFSET > bufferLength && !requestStreamFinished) {
                        ArrayPoolExtensions.Resize ( ArrayPool<byte>.Shared, ref inputBuffer, bufferLength * 2, clearArray: false );
                        int count = inputBuffer.Length - bufferLength;
                        int read = this._stream.Read ( inputBuffer, bufferLength - 1, count );
                        if (read > 0) {
                            buffer = inputBuffer; // recreate the span over the input buffer
                            firstByte = ref MemoryMarshal.GetReference ( buffer );
                            length += read;
                        }
                        if (read < count) {
                            requestStreamFinished = true;
                        }
                    }

                    if (currentByte == CARRIAGE_RETURN) {
                        headerLine = buffer [ (headerLineIndex + 1)..i ];
                        headerLineIndex = i + 1; //+1 includes \LF

                        headerLineSepIndex = headerLine.IndexOf ( DOUBLE_DOTS );
                        if (headerLineSepIndex < 0) {
                            // finished header parsing
                            headerSize = i + 2;
                            break;
                        }

                        headerLineName = headerLine [ 0..headerLineSepIndex ];
                        headerLineValue = headerLine [ (headerLineSepIndex + 2).. ]; // +2 = : and the space

                        string headerName = HeaderEncoding.GetString ( headerLineName );
                        string headerValue = HeaderEncoding.GetString ( headerLineValue );

                        if (string.Compare ( headerName, "Content-Length", StringComparison.OrdinalIgnoreCase ) == 0) {
                            contentLength = long.Parse ( headerValue );
                        }
                        else if (string.Compare ( headerName, "Connection", StringComparison.OrdinalIgnoreCase ) == 0) {
                            keepAliveEnabled = string.Compare ( headerValue, "close", StringComparison.Ordinal ) != 0;
                        }

                        headers.Add ( (headerName, headerValue) );
                    }

                    break;
            }

            if (headerSize >= 0)
                break;
        }

        return new HttpRequestBase () {
            BufferedContent = inputBuffer,
            BufferHeaderIndex = headerSize,

            Headers = headers,
            Method = HeaderEncoding.GetString ( method ),
            Path = HeaderEncoding.GetString ( path ),
            Version = HeaderEncoding.GetString ( version ),

            ContentLength = contentLength,
            CanKeepAlive = keepAliveEnabled
        };
    }
}
