// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestReader.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;
using Sisk.Cadente;
using Sisk.Cadente.HttpSerializer;

static class HttpRequestReader {
    private const byte Space = (byte) ' ';
    private const byte Colon = (byte) ':';
    private const byte LineFeed = (byte) '\n';
    private const byte CarriageReturn = (byte) '\r';

    public static async Task<HttpRequestBase?> TryReadHttpRequestAsync (
        Memory<byte> sharedBuffer,
        Stream stream,
        CancellationToken cancellationToken = default ) {
        try {
            int read = await stream.ReadAsync ( sharedBuffer, cancellationToken ).ConfigureAwait ( false );
            if (read <= 0) {
                return null;
            }

            return ParseHttpRequest ( sharedBuffer [ ..read ] );
        }
        catch {
            return null;
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    private static HttpRequestBase? ParseHttpRequest ( ReadOnlyMemory<byte> buffer ) {
        // Request line: METHOD SP PATH SP PROTOCOL CRLF

        int methodEnd = buffer.Span.IndexOf ( Space );
        if (methodEnd <= 0) {
            return null;
        }

        ReadOnlyMemory<byte> method = buffer [ ..methodEnd ];

        int pathStart = methodEnd + 1;
        int pathEndRel = buffer.Span [ pathStart.. ].IndexOf ( Space );
        if (pathEndRel < 0) {
            return null;
        }

        int pathEnd = pathStart + pathEndRel;
        ReadOnlyMemory<byte> path = buffer [ pathStart..pathEnd ];

        int protocolStart = pathEnd + 1;
        int protocolLineEndRel = buffer.Span [ protocolStart.. ].IndexOf ( LineFeed );
        if (protocolLineEndRel < 0) {
            return null;
        }

        int protocolLineEnd = protocolStart + protocolLineEndRel;
        int protocolEndExclusive = protocolLineEnd;

        if (protocolEndExclusive > protocolStart && buffer.Span [ protocolEndExclusive - 1 ] == CarriageReturn) {
            protocolEndExclusive--;
        }

        ReadOnlyMemory<byte> protocol = buffer [ protocolStart..protocolEndExclusive ];

        int cursor = protocolLineEnd + 1; // avança além do '\n'

        long contentLength = -1;
        bool keepAliveEnabled = !Ascii.EqualsIgnoreCase ( protocol.Span, "HTTP/1.0"u8 );
        bool expect100 = false;
        bool isChunked = false;

        var headers = new List<HttpHeader> ( 32 );
        bool headersTerminated = false;

        while (cursor < buffer.Length) {
            int lfRel = buffer.Span [ cursor.. ].IndexOf ( LineFeed );
            if (lfRel < 0) {
                return null; // header incompleto, precisa de mais dados
            }

            if (lfRel == 0) {
                cursor += 1; // linha vazia com apenas '\n'
                headersTerminated = true;
                break;
            }

            int lineEnd = cursor + lfRel;
            int headerEndExclusive = lineEnd;
            if (buffer.Span [ headerEndExclusive - 1 ] == CarriageReturn) {
                headerEndExclusive--;
                if (headerEndExclusive == cursor) {
                    cursor = lineEnd + 1; // "\r\n" puro
                    headersTerminated = true;
                    break;
                }
            }

            ReadOnlyMemory<byte> headerLine = buffer [ cursor..headerEndExclusive ];
            cursor = lineEnd + 1; // pula '\n'

            int colonIndex = headerLine.Span.IndexOf ( Colon );
            if (colonIndex <= 0) {
                continue; // cabeçalho malformado, ignora
            }

            ReadOnlyMemory<byte> name = headerLine [ ..colonIndex ];
            ReadOnlyMemory<byte> value = headerLine [ (colonIndex + 1).. ];

            var trimmedRange = Ascii.Trim ( value.Span );
            value = value [ trimmedRange ];

            if (Ascii.EqualsIgnoreCase ( name.Span, "Content-Length"u8 )) {
                if (Utf8Parser.TryParse ( value.Span, out long parsed, out int consumed ) && consumed == value.Length) {
                    contentLength = parsed;
                }
            }
            else if (Ascii.EqualsIgnoreCase ( name.Span, "Connection"u8 )) {
                keepAliveEnabled = !Ascii.EqualsIgnoreCase ( value.Span, "close"u8 );
            }
            else if (Ascii.EqualsIgnoreCase ( name.Span, "Expect"u8 )) {
                expect100 = Ascii.EqualsIgnoreCase ( value.Span, "100-continue"u8 );
            }
            else if (Ascii.EqualsIgnoreCase ( name.Span, "Transfer-Encoding"u8 )) {
                isChunked = Ascii.EqualsIgnoreCase ( value.Span, "chunked"u8 );
                if (isChunked) {
                    contentLength = -1;
                }
            }

            headers.Add ( new HttpHeader ( name, value ) );
        }

        if (!headersTerminated) {
            return null; // cabeçalhos não terminados corretamente
        }

        ReadOnlyMemory<byte> bufferedContent = expect100
            ? ReadOnlyMemory<byte>.Empty
            : buffer [ cursor.. ];

        return new HttpRequestBase {
            MethodRef = method,
            PathRef = path,
            Headers = headers.ToArray (),
            ContentLength = contentLength,
            CanKeepAlive = keepAliveEnabled,
            IsChunked = isChunked,
            IsExpecting100 = expect100,
            BufferedContent = bufferedContent
        };
    }
}