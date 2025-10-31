// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestReader.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sisk.Cadente.HttpSerializer;

sealed class HttpRequestReader {

    Stream _stream;

    const byte SPACE = (byte) ' ';
    const byte LINE_FEED = (byte) '\n';
    const byte COLON = (byte) ':';

    private static ReadOnlySpan<byte> RequestLineDelimiters => [ LINE_FEED, 0 ];
    private static ReadOnlySpan<byte> RequestHeaderLineSpaceDelimiters => [ SPACE, 0 ];

    public HttpRequestReader ( Stream stream ) {
        _stream = stream;
    }

    public bool TryReadHttpRequest ( [NotNullWhen ( true )] out HttpRequestBase? request ) {
        try {

            Span<byte> buffer = stackalloc byte [ HttpConnection.REQUEST_BUFFER_SIZE ];
            int read = _stream.Read ( buffer );

            request = ParseHttpRequest ( buffer [ ..read ] );
            return request != null;
        }
        catch (Exception ex) {
            Logger.LogInformation ( $"HttpRequestReader finished with exception: {ex.Message}" );
            request = null;
            return false;
        }
    }

    HttpRequestBase? ParseHttpRequest ( scoped ReadOnlySpan<byte> buffer ) {

        SpanReader<byte> reader = new SpanReader<byte> ( buffer );

        if (!reader.TryReadToAny ( out ReadOnlySpan<byte> method, RequestHeaderLineSpaceDelimiters, advancePastDelimiter: true )) {
            return null;
        }
        if (!reader.TryReadToAny ( out ReadOnlySpan<byte> path, RequestHeaderLineSpaceDelimiters, advancePastDelimiter: true )) {
            return null;
        }
        if (!reader.TryReadToAny ( out ReadOnlySpan<byte> protocol, RequestLineDelimiters, advancePastDelimiter: true )) {
            return null;
        }

        long contentLength = 0;
        bool keepAliveEnabled = true;
        bool expect100 = false;
        bool isChunked = false;

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
            else if (Ascii.EqualsIgnoreCase ( headerLineName, "Transfer-Encoding"u8 )) {
                isChunked = Ascii.Equals ( headerLineValue, "chunked"u8 );
            }

            headers.Add ( new HttpHeader ( headerLineName.ToArray (), headerLineValue.ToArray () ) );
        }

        return new HttpRequestBase () {
            BufferedContent = expect100 ? Memory<byte>.Empty : buffer [ reader.Consumed.. ].ToArray (),

            Headers = headers.ToArray (),
            MethodRef = method.ToArray (),
            PathRef = path.ToArray (),

            ContentLength = isChunked switch {
                true => -1,
                false => contentLength
            },
            CanKeepAlive = keepAliveEnabled,

            IsExpecting100 = expect100,
            IsChunked = isChunked
        };
    }
}
