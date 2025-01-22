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

namespace Sisk.Cadente.HttpSerializer;

sealed class HttpRequestReader {

    Stream _stream;
    IMemoryOwner<byte> bufferOwnership;

    const byte SPACE = 0x20;
    const byte CARRIAGE_RETURN = 0x0D; //\r
    const byte DOUBLE_DOTS = 0x3A; // :

    public HttpRequestReader ( Stream stream, ref IMemoryOwner<byte> bufferOwnership ) {
        this._stream = stream;
        this.bufferOwnership = bufferOwnership;
    }

    public async Task<(HttpRequestReadState, HttpRequestBase?)> ReadHttpRequest () {
        try {

            int read = await this._stream.ReadAsync ( this.bufferOwnership.Memory );

            if (read == 0) {
                return (HttpRequestReadState.StreamZero, null);
            }

            var request = this.ParseHttpRequest ( read );
            return (HttpRequestReadState.RequestRead, request);
        }
        catch (Exception ex) {
            Logger.LogInformation ( $"HttpRequestReader finished with exception: {ex.Message}" );
            return (HttpRequestReadState.StreamError, null);
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    public HttpRequestBase? ParseHttpRequest ( int length ) {

        const int STEP_READ_METHOD = 0;
        const int STEP_READ_PATH = 1;
        const int STEP_READ_VERSION = 2;
        const int STEP_READ_HEADERS = 3;

        bool requestStreamFinished = false;

        Memory<byte> memory = this.bufferOwnership.Memory;
        Span<byte> memSpan = memory.Span;

        ReadOnlyMemory<byte> method = null!;
        ReadOnlyMemory<byte> path = null!;

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

        List<HttpHeader> headers = new List<HttpHeader> ( 64 );

        ref byte firstByte = ref MemoryMarshal.GetReference ( memSpan );
        for (int i = 0; i < length; i++) {
            ref byte currentByte = ref Unsafe.Add ( ref firstByte, i );

            switch (step) {
                case STEP_READ_METHOD:
                    if (currentByte == SPACE) {
                        methodIndex = i;
                        method = memory [ 0..i ];
                        step = STEP_READ_PATH;
                    }
                    break;

                case STEP_READ_PATH:
                    if (currentByte == SPACE) {
                        pathIndex = i;
                        path = memory [ (methodIndex + 1)..i ];
                        step = STEP_READ_VERSION;
                    }
                    break;

                case STEP_READ_VERSION:
                    if (currentByte == CARRIAGE_RETURN) {
                        versionIndex = i;
                        // version = memSpan [ (pathIndex + 1)..i ];
                        headerLineIndex = i + 1; //+1 includes \LF
                        step = STEP_READ_HEADERS;
                    }
                    break;

                case STEP_READ_HEADERS:

                    // checks whether the current buffer has all the request headers. 
                    if (!requestStreamFinished && i + 32/*BUFFER_LOOKAHEAD_OFFSET*/ > memSpan.Length) {

                        // headers are too big
                        return null;
                    }

                    if (currentByte == CARRIAGE_RETURN) {
                        headerLine = memSpan [ (headerLineIndex + 1)..i ];
                        headerLineIndex = i + 1; //+1 includes \LF

                        headerLineSepIndex = headerLine.IndexOf ( DOUBLE_DOTS );
                        if (headerLineSepIndex < 0) {
                            // finished header parsing
                            headerSize = i + 2;
                            break;
                        }

                        headerLineName = headerLine [ 0..headerLineSepIndex ];
                        headerLineValue = headerLine [ (headerLineSepIndex + 2).. ]; // +2 = : and the space

                        if (Ascii.EqualsIgnoreCase ( headerLineName, "Content-Length"u8 )) {
                            contentLength = long.Parse ( Encoding.ASCII.GetString ( headerLineValue ) );
                        }
                        else if (Ascii.EqualsIgnoreCase ( headerLineName, "Connection"u8 )) {
                            keepAliveEnabled = !headerLineValue.SequenceEqual ( "close"u8 );
                        }

                        headers.Add ( new HttpHeader ( headerLineName.ToArray (), headerLineValue.ToArray () ) );
                    }

                    break;
            }

            if (headerSize >= 0)
                break;
        }

        return new HttpRequestBase () {
            BufferedContent = this.bufferOwnership.Memory [ headerSize.. ],

            Headers = headers.ToArray (),
            MethodRef = method,
            PathRef = path,

            ContentLength = contentLength,
            CanKeepAlive = keepAliveEnabled
        };
    }
}
