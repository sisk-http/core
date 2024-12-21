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

public sealed class HttpRequestReader {

    const int BUFFER_SIZE = 512;

    const byte SPACE = 0x20;
    const byte CARRIAGE_RETURN = 0x0D; //\r
    const byte DOUBLE_DOTS = 0x3A; // :

    const int BUFFER_LOOKAHEAD_OFFSET = 64;

    // do not dispose
    private readonly Stream _stream;

    static Encoding HeaderEncoding = Encoding.UTF8;

    public HttpRequestReader ( Stream stream ) {
        this._stream = stream;
    }

    public HttpRequestBase? ReadHttpRequest () {
        byte [] buffer = ArrayPool<byte>.Shared.Rent ( BUFFER_SIZE );
        try {
            int read = this._stream.Read ( buffer );
            if (read == 0)
                return null;

            return ParseHttpRequest ( ref buffer, read, this._stream );
        }
        finally {
            ArrayPool<byte>.Shared.Return ( buffer );
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    public static unsafe HttpRequestBase? ParseHttpRequest ( ref byte [] inputBuffer, int length, Stream requestStream ) {
        // 
        bool requestStreamReturnedZero = false;

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
        int step = 0;

        int methodIndex = 0;
        int pathIndex = 0;
        int versionIndex = 0;
        int headerLineIndex = 0;
        int headerLineSepIndex = 0;

        int headerSize = -1;
        long contentLength = 0;
        List<(string, string)> headers = new List<(string, string)> ( 16 );

        ref byte firstByte = ref MemoryMarshal.GetReference ( buffer );
        for (int i = 0; i < length; i++) {
            ref byte b = ref Unsafe.Add ( ref firstByte, i );

            switch (step) {
                case 0:
                    if (b == SPACE) {
                        methodIndex = i;
                        method = buffer [ 0..i ];
                        step = 1;
                    }
                    break;

                case 1:
                    if (b == SPACE) {
                        pathIndex = i;
                        path = buffer [ (methodIndex + 1)..i ];
                        step = 2;
                    }
                    break;

                case 2:
                    if (b == CARRIAGE_RETURN) {
                        versionIndex = i;
                        version = buffer [ (pathIndex + 1)..i ];
                        headerLineIndex = i + 1; //+1 includes \LF
                        step = 3;
                    }
                    break;

                case 3:
                    if (b == CARRIAGE_RETURN) {
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

                        headers.Add ( (headerName, headerValue) );
                    }

                    // checks whether the current buffer has all the request headers. if not, read more data from the buffer
                    int bufferLength = buffer.Length;
                    if (i + BUFFER_LOOKAHEAD_OFFSET > bufferLength && !requestStreamReturnedZero) {
                        ArrayPoolExtensions.Resize ( ArrayPool<byte>.Shared, ref inputBuffer, bufferLength * 2, clearArray: false );
                        int nextRead = requestStream.Read ( inputBuffer, bufferLength - 1, inputBuffer.Length - bufferLength );
                        if (nextRead > 0) {
                            buffer = inputBuffer; // recreate the span over the input buffer
                            firstByte = ref MemoryMarshal.GetReference ( buffer );
                            length += nextRead;
                        }
                        else {
                            requestStreamReturnedZero = true;
                        }
                    }

                    break;
            }

            if (headerSize >= 0)
                break;
        }

        return new HttpRequestBase (
            method: HeaderEncoding.GetString ( method ),
            path: HeaderEncoding.GetString ( path ),
            version: HeaderEncoding.GetString ( version ),
            headerEnd: headerSize,
            headers: headers,
            bufferedContent: inputBuffer,
            contentLength: contentLength
        );
    }
}
