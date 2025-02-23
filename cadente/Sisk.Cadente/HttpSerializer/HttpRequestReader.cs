// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestReader.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Text;

namespace Sisk.Cadente.HttpSerializer;

sealed class HttpRequestReader {

    Stream _stream;
    Memory<byte> buffer;

    const byte SPACE = (byte) ' ';
    const byte LINE_FEED = (byte) '\n';
    const byte COLON = (byte) ':';

    private static ReadOnlySpan<byte> RequestLineDelimiters => [ LINE_FEED, 0 ];
    private static ReadOnlySpan<byte> RequestHeaderLineSpaceDelimiters => [ SPACE, 0 ];

    public HttpRequestReader ( Stream stream, ref byte [] bufferOwnership ) {
        this._stream = stream;
        this.buffer = bufferOwnership;
    }

    public async Task<(HttpRequestReadState, HttpRequestBase?)> ReadHttpRequest () {
        try {
            int read = await this._stream.ReadAsync ( this.buffer );

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

    HttpRequestBase? ParseHttpRequest ( int length ) {

        ReadOnlyMemory<byte> bufferPart = this.buffer [ 0..length ];
        SequenceReader<byte> reader = new SequenceReader<byte> ( new ReadOnlySequence<byte> ( bufferPart ) );

        if (!reader.TryReadToAny ( out ReadOnlySpan<byte> method, RequestHeaderLineSpaceDelimiters, advancePastDelimiter: true )) {
            return null;
        }
        if (!reader.TryReadToAny ( out ReadOnlySpan<byte> path, RequestHeaderLineSpaceDelimiters, advancePastDelimiter: true )) {
            return null;
        }
        if (!reader.TryReadToAny ( out ReadOnlySpan<byte> protocol, RequestHeaderLineSpaceDelimiters, advancePastDelimiter: true )) {
            return null;
        }

        long contentLength = 0;
        bool keepAliveEnabled = true;
        bool expect100 = false;

        List<HttpHeader> headers = new List<HttpHeader> ( 32 );
        while (reader.TryReadToAny ( out ReadOnlySpan<byte> headerLine, RequestLineDelimiters, advancePastDelimiter: true )) {

            int headerLineSepIndex = headerLine.IndexOf ( COLON );
            if (headerLineSepIndex < 0) {
                break;
            }

            ReadOnlySpan<byte> headerLineName = headerLine [ 0..headerLineSepIndex ];

            // + 2 below includes the ": " from the header line, and
            //  ^1 removes the trailing \r
            ReadOnlySpan<byte> headerLineValue = headerLine [ (headerLineSepIndex + 2)..^1 ];

            if (Ascii.EqualsIgnoreCase ( headerLineName, "Content-Length"u8 )) {
                contentLength = long.Parse ( Encoding.ASCII.GetString ( headerLineValue ) );
            }
            else if (Ascii.EqualsIgnoreCase ( headerLineName, "Connection"u8 )) {
                keepAliveEnabled = !Ascii.Equals ( headerLineValue, "close"u8 );
            }
            else if (Ascii.EqualsIgnoreCase ( headerLineName, "Expect"u8 )) {
                expect100 = Ascii.Equals ( headerLineValue, "100-continue"u8 );
            }

            headers.Add ( new HttpHeader ( headerLineName.ToArray (), headerLineValue.ToArray () ) );
        }

        return new HttpRequestBase () {
            BufferedContent = expect100 ? Memory<byte>.Empty : bufferPart [ (int) reader.Consumed.. ],

            Headers = headers.ToArray (),
            MethodRef = method.ToArray (),
            PathRef = path.ToArray (),

            ContentLength = contentLength,
            CanKeepAlive = keepAliveEnabled,

            IsExpecting100 = expect100
        };
    }
}
