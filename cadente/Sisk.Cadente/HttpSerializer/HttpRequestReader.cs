using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sisk.Cadente;
using Sisk.Cadente.HttpSerializer;

static class HttpRequestReader {
    private const byte Space = (byte) ' ';
    private const byte Colon = (byte) ':';
    private const byte LineFeed = (byte) '\n';
    private const byte CarriageReturn = (byte) '\r';

    public static async ValueTask<HttpRequestBase?> TryReadHttpRequestAsync (
        Memory<byte> sharedBuffer,
        Stream stream,
        CancellationToken cancellationToken = default ) {
        try {
            int read = await stream.ReadAsync ( sharedBuffer, cancellationToken ).ConfigureAwait ( false );
            if (read <= 0) {
                return null;
            }

            return ParseHttpRequest ( sharedBuffer.Span [ ..read ] );
        }
        catch {
            return null;
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    private static HttpRequestBase? ParseHttpRequest ( ReadOnlySpan<byte> buffer ) {
        // Request line: METHOD SP PATH SP PROTOCOL CRLF

        int methodEnd = buffer.IndexOf ( Space );
        if (methodEnd <= 0) {
            return null;
        }

        ReadOnlySpan<byte> method = buffer [ ..methodEnd ];

        int pathStart = methodEnd + 1;
        int pathEndRel = buffer [ pathStart.. ].IndexOf ( Space );
        if (pathEndRel < 0) {
            return null;
        }

        int pathEnd = pathStart + pathEndRel;
        ReadOnlySpan<byte> path = buffer [ pathStart..pathEnd ];

        int protocolStart = pathEnd + 1;
        int protocolLineEndRel = buffer [ protocolStart.. ].IndexOf ( LineFeed );
        if (protocolLineEndRel < 0) {
            return null;
        }

        int protocolLineEnd = protocolStart + protocolLineEndRel;
        int protocolEndExclusive = protocolLineEnd;

        if (protocolEndExclusive > protocolStart && buffer [ protocolEndExclusive - 1 ] == CarriageReturn) {
            protocolEndExclusive--;
        }

        ReadOnlySpan<byte> protocol = buffer [ protocolStart..protocolEndExclusive ];

        int cursor = protocolLineEnd + 1; // avança além do '\n'

        long contentLength = -1;
        bool keepAliveEnabled = !Ascii.EqualsIgnoreCase ( protocol, "HTTP/1.0"u8 );
        bool expect100 = false;
        bool isChunked = false;

        var headers = new List<HttpHeader> ( 32 );
        bool headersTerminated = false;

        while (cursor < buffer.Length) {
            int lfRel = buffer [ cursor.. ].IndexOf ( LineFeed );
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
            if (buffer [ headerEndExclusive - 1 ] == CarriageReturn) {
                headerEndExclusive--;
                if (headerEndExclusive == cursor) {
                    cursor = lineEnd + 1; // "\r\n" puro
                    headersTerminated = true;
                    break;
                }
            }

            ReadOnlySpan<byte> headerLine = buffer [ cursor..headerEndExclusive ];
            cursor = lineEnd + 1; // pula '\n'

            int colonIndex = headerLine.IndexOf ( Colon );
            if (colonIndex <= 0) {
                continue; // cabeçalho malformado, ignora
            }

            ReadOnlySpan<byte> name = headerLine [ ..colonIndex ];
            ReadOnlySpan<byte> value = TrimAsciiWhitespace ( headerLine [ (colonIndex + 1).. ] );

            if (Ascii.EqualsIgnoreCase ( name, "Content-Length"u8 )) {
                if (Utf8Parser.TryParse ( value, out long parsed, out int consumed ) && consumed == value.Length) {
                    contentLength = parsed;
                }
            }
            else if (Ascii.EqualsIgnoreCase ( name, "Connection"u8 )) {
                keepAliveEnabled = !TokenListContains ( value, "close"u8 );
            }
            else if (Ascii.EqualsIgnoreCase ( name, "Expect"u8 )) {
                expect100 = TokenListContains ( value, "100-continue"u8 );
            }
            else if (Ascii.EqualsIgnoreCase ( name, "Transfer-Encoding"u8 )) {
                isChunked = TokenListContains ( value, "chunked"u8 );
                if (isChunked) {
                    contentLength = -1;
                }
            }

            headers.Add ( new HttpHeader ( name.ToArray (), value.ToArray () ) );
        }

        if (!headersTerminated) {
            return null; // cabeçalhos não terminados corretamente
        }

        ReadOnlyMemory<byte> bufferedContent = expect100
            ? ReadOnlyMemory<byte>.Empty
            : buffer [ cursor.. ].ToArray ();

        return new HttpRequestBase {
            MethodRef = method.ToArray (),
            PathRef = path.ToArray (),
            Headers = headers.ToArray (),
            ContentLength = contentLength,
            CanKeepAlive = keepAliveEnabled,
            IsChunked = isChunked,
            IsExpecting100 = expect100,
            BufferedContent = bufferedContent
        };
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static ReadOnlySpan<byte> TrimAsciiWhitespace ( ReadOnlySpan<byte> span ) {
        int start = 0;
        int end = span.Length;

        while (start < end && IsAsciiWhitespace ( span [ start ] )) {
            start++;
        }

        while (end > start && IsAsciiWhitespace ( span [ end - 1 ] )) {
            end--;
        }

        return span [ start..end ];
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static bool IsAsciiWhitespace ( byte value )
        => value is (byte) ' ' or (byte) '\t' or CarriageReturn or LineFeed;

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static bool TokenListContains ( ReadOnlySpan<byte> source, ReadOnlySpan<byte> token ) {
        while (!source.IsEmpty) {
            int commaIndex = source.IndexOf ( (byte) ',' );
            ReadOnlySpan<byte> part;

            if (commaIndex >= 0) {
                part = source [ ..commaIndex ];
                source = source [ (commaIndex + 1).. ];
            }
            else {
                part = source;
                source = ReadOnlySpan<byte>.Empty;
            }

            part = TrimAsciiWhitespace ( part );

            if (!part.IsEmpty && Ascii.EqualsIgnoreCase ( part, token )) {
                return true;
            }
        }

        return false;
    }
}