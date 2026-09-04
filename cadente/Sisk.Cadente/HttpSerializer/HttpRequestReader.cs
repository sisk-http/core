// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestReader.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Buffers.Text;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Sisk.Cadente;
using Sisk.Cadente.HttpSerializer;

static class HttpRequestReader {
    private const byte Space = (byte) ' ';
    private const byte Colon = (byte) ':';
    private const byte LineFeed = (byte) '\n';
    private const byte CarriageReturn = (byte) '\r';
    private const byte HorizontalTab = (byte) '\t';
    private const byte NumberSign = (byte) '#';

    private const int DefaultHeaderReadTimeoutMs = 30_000;
    private static ReadOnlySpan<byte> HeaderTerminator => "\r\n\r\n"u8;
    private static ReadOnlySpan<byte> Http09 => "HTTP/0.9"u8;
    private static ReadOnlySpan<byte> Http10 => "HTTP/1.0"u8;
    private static ReadOnlySpan<byte> Http11 => "HTTP/1.1"u8;
    private static ReadOnlySpan<byte> CloseValue => "close"u8;
    private static ReadOnlySpan<byte> ContinueValue => "100-continue"u8;
    private static ReadOnlySpan<byte> ChunkedValue => "chunked"u8;

    private static ReadOnlySpan<byte> TrimChars => " \t\r\n"u8;

    [SkipLocalsInit]
    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    public static async ValueTask<(HttpRequestBase? Request, int BufferedLength)> TryReadHttpRequestAsync ( Memory<byte> sharedBuffer, int bufferedLength, Stream stream, CancellationToken cancellationToken = default, int headerReadTimeoutMs = DefaultHeaderReadTimeoutMs, CancellationTokenSource? timeoutSource = null ) {

        int bufferLength = sharedBuffer.Length;
        int totalRead = bufferedLength;
        long deadlineTicks = Environment.TickCount64 + headerReadTimeoutMs;
        int searchStart = 0;

        try {
            while (true) {
                int effectiveSearchStart = Math.Max ( 0, searchStart - 3 );
                ReadOnlySpan<byte> searchRegion = sharedBuffer.Span.Slice ( effectiveSearchStart, totalRead - effectiveSearchStart );

                if (searchRegion.IndexOf ( HeaderTerminator ) >= 0) {
                    return (ParseHttpRequest ( sharedBuffer.Slice ( 0, totalRead ) ), totalRead);
                }

                if (totalRead >= bufferLength) {
                    Logger.LogInformation ( $"failed to parse HTTP request: headers too large" );
                    return (null, totalRead);
                }

                long currentTicks = Environment.TickCount64;
                if (currentTicks >= deadlineTicks) {
                    Logger.LogInformation ( $"failed to parse HTTP request: header read timeout" );
                    return (null, totalRead);
                }

                int remainingMs = (int) (deadlineTicks - currentTicks);
                cancellationToken.ThrowIfCancellationRequested ();
                searchStart = totalRead;

                int bytesRead = await ReadWithTimeoutAsync (
                    stream,
                    sharedBuffer.Slice ( totalRead ),
                    remainingMs,
                    cancellationToken,
                    timeoutSource
                ).ConfigureAwait ( false );

                if (bytesRead == 0) {
                    Logger.LogInformation ( $"failed to parse HTTP request: connection closed" );
                    return (null, totalRead);
                }

                totalRead += bytesRead;
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
            Logger.LogInformation ( $"failed to parse HTTP request: header read timeout" );
            return (null, totalRead);
        }
        catch (OperationCanceledException) {
            Logger.LogInformation ( $"failed to parse HTTP request: operation cancelled" );
            return (null, totalRead);
        }
        catch (SocketException sex) {
            Logger.LogInformation ( $"failed to parse HTTP request: {sex.Message}" );
            return (null, totalRead);
        }
        catch (Exception ex) {
            Logger.LogInformation ( $"failed to parse HTTP request (exception): {ex.Message}" );
            return (null, totalRead);
        }
    }


    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask<int> ReadWithTimeoutAsync ( Stream stream, Memory<byte> buffer, int timeoutMs, CancellationToken cancellationToken, CancellationTokenSource? timeoutSource ) {
        if (timeoutMs <= 0) {
            throw new OperationCanceledException ();
        }

        if (timeoutSource is not null && !cancellationToken.CanBeCanceled) {
            timeoutSource.CancelAfter ( timeoutMs );
            try {
                return await stream.ReadAsync ( buffer, timeoutSource.Token ).ConfigureAwait ( false );
            }
            finally {
                if (!timeoutSource.IsCancellationRequested) {
                    timeoutSource.CancelAfter ( Timeout.Infinite );
                }
            }
        }

        using var timeoutCts = cancellationToken.CanBeCanceled
            ? CancellationTokenSource.CreateLinkedTokenSource ( cancellationToken )
            : new CancellationTokenSource ( timeoutMs );

        if (cancellationToken.CanBeCanceled)
            timeoutCts.CancelAfter ( timeoutMs );

        return await stream.ReadAsync ( buffer, timeoutCts.Token ).ConfigureAwait ( false );
    }

    [SkipLocalsInit]
    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    private static HttpRequestBase? ParseHttpRequest ( ReadOnlyMemory<byte> buffer ) {
        ReadOnlySpan<byte> span = buffer.Span;
        int bufferLength = span.Length;
        string? failReason = null;

        // Request line: METHOD SP PATH SP PROTOCOL CRLF
        int methodEnd = span.IndexOf ( Space );
        if (methodEnd <= 0) {
            Logger.LogInformation ( $"failed to parse HTTP request: missing or empty request method in request line" );
            return null;
        }

        ReadOnlyMemory<byte> method = buffer.Slice ( 0, methodEnd );

        int pathStart = methodEnd + 1;
        int pathEndRel = span.Slice ( pathStart ).IndexOf ( Space );
        if (pathEndRel < 0) {
            Logger.LogInformation ( $"failed to parse HTTP request: missing request path in request line" );
            return null;
        }

        int pathEnd = pathStart + pathEndRel;
        ReadOnlyMemory<byte> path = buffer.Slice ( pathStart, pathEndRel );
        if (pathEndRel == 0) {
            Logger.LogInformation ( $"failed to parse HTTP request: missing request target in request line" );
            return null;
        }
        if (path.Span.Contains ( NumberSign )) {
            Logger.LogInformation ( $"failed to parse HTTP request: fragment is not allowed in request target" );
            return null;
        }

        int protocolStart = pathEnd + 1;
        int protocolLineEndRel = span.Slice ( protocolStart ).IndexOf ( LineFeed );
        if (protocolLineEndRel < 0) {
            Logger.LogInformation ( $"failed to parse HTTP request: missing HTTP protocol version in request line" );
            return null;
        }

        int protocolLineEnd = protocolStart + protocolLineEndRel;
        int protocolEndExclusive = protocolLineEnd;

        ref byte spanRef = ref MemoryMarshal.GetReference ( span );
        if (protocolEndExclusive > protocolStart &&
            Unsafe.Add ( ref spanRef, protocolEndExclusive - 1 ) == CarriageReturn) {
            protocolEndExclusive--;
        }

        ReadOnlyMemory<byte> protocol = buffer.Slice ( protocolStart, protocolEndExclusive - protocolStart );
        if (!IsSupportedHttpProtocol ( protocol.Span )) {
            Logger.LogInformation ( $"failed to parse HTTP request: unsupported HTTP protocol version '{Encoding.ASCII.GetString ( protocol.Span )}'" );
            return null;
        }

        int cursor = protocolLineEnd + 1;

        long contentLength = 0;
        bool keepAliveEnabled = !protocol.Span.SequenceEqual ( Http10 );
        bool expect100 = false;
        bool isChunked = false;
        bool seenTransferEncoding = false;
        bool seenHost = false;
        bool contentLengthExplicit = false;
        int headersStart = cursor;

        while (cursor < bufferLength) {
                byte currentByte = Unsafe.Add ( ref spanRef, cursor );

                if (currentByte == LineFeed) {
                    cursor++;
                    goto HeadersComplete;
                }

                if (currentByte == CarriageReturn) {
                    if (cursor + 1 < bufferLength && Unsafe.Add ( ref spanRef, cursor + 1 ) == LineFeed) {
                        cursor += 2;
                        goto HeadersComplete;
                    }
                }

                ReadOnlySpan<byte> remaining = span.Slice ( cursor );
                int lfRel = remaining.IndexOf ( LineFeed );
                if (lfRel < 0) {
                    failReason = "incomplete header line: no LF found";
                    goto ParseFailed;
                }

                int lineStart = cursor;
                int lineEnd = cursor + lfRel;
                int headerLineEnd = lineEnd;

                if (Unsafe.Add ( ref spanRef, headerLineEnd - 1 ) == CarriageReturn) {
                    headerLineEnd--;
                }
                else {
                    // Bare LF without preceding CR violates RFC 9112 §2.2 and enables header injection
                    failReason = "bare LF without preceding CR in header line violates RFC 9112 §2.2";
                    goto ParseFailed;
                }

                int headerLineLength = headerLineEnd - lineStart;
                if (headerLineLength == 0) {
                    cursor = lineEnd + 1;
                    goto HeadersComplete;
                }

                ReadOnlySpan<byte> headerLine = span.Slice ( lineStart, headerLineLength );
                cursor = lineEnd + 1;

                if (headerLine [ 0 ] == Space || headerLine [ 0 ] == HorizontalTab) {
                    failReason = "obs-fold header line violates RFC 9112 §5.2";
                    goto ParseFailed;
                }

                int colonIndex = headerLine.IndexOf ( Colon );
                if (colonIndex < 0) {
                    failReason = "header line without colon violates RFC 9112 §5";
                    goto ParseFailed;
                }
                if (colonIndex == 0) {
                    failReason = "empty header name violates RFC 9110 §5.1";
                    goto ParseFailed; // empty header name (RFC 9110 §5.1)
                }

                ReadOnlySpan<byte> nameSpan = headerLine.Slice ( 0, colonIndex );
                ReadOnlySpan<byte> rawValue = headerLine.Slice ( colonIndex + 1 );

                ReadOnlySpan<byte> valueSpan = rawValue.Trim ( TrimChars );

                if (HttpHeader.ContainsInvalidNameBytes ( nameSpan )) {
                    failReason = "header name contains not allowed characters";
                    goto ParseFailed;
                }
                if (HttpHeader.ContainsInvalidValueBytes ( valueSpan )) {
                    failReason = "header value contains not allowed characters";
                    goto ParseFailed;
                }

                int knownHeader = GetKnownHeaderIndex ( nameSpan );
                switch (knownHeader) {
                    case 0: // Content-Length
                        if (IsAsciiDigits ( valueSpan ) && Utf8Parser.TryParse ( valueSpan, out long parsed, out int consumed ) && consumed == valueSpan.Length) {
                            if (contentLengthExplicit && contentLength != parsed) {
                                failReason = "conflicting duplicate Content-Length headers violate RFC 9112 §6.3.3";
                                goto ParseFailed;
                            }

                            contentLength = parsed;
                            contentLengthExplicit = true;
                        }
                        else {
                            // Invalid Content-Length value (non-numeric, negative, overflow) — RFC 9110 §8.6
                            failReason = $"invalid Content-Length value '{Encoding.ASCII.GetString ( valueSpan )}' (RFC 9110 §8.6)";
                            goto ParseFailed;
                        }
                        break;
                    case 1: // Connection
                        keepAliveEnabled = !TokenListContains ( valueSpan, CloseValue );
                        break;
                    case 2: // Expect
                        expect100 = TokenListContains ( valueSpan, ContinueValue );
                        break;
                    case 3: // Transfer-Encoding
                        if (seenTransferEncoding) {
                            failReason = "duplicate Transfer-Encoding header is a request-smuggling vector (RFC 9112 §6.3.3)";
                            goto ParseFailed; // duplicate TE = request-smuggling vector (RFC 9112 §6.3.3)
                        }
                        seenTransferEncoding = true;
                        if (!TokenListEqualsSingle ( valueSpan, ChunkedValue )) {
                            failReason = $"unsupported Transfer-Encoding value '{Encoding.ASCII.GetString ( valueSpan )}' (only chunked is supported)";
                            goto ParseFailed;
                        }
                        isChunked = true;
                        contentLength = -1;
                        break;
                    case 4: // Host
                        if (seenHost) {
                            failReason = "duplicate Host header violates RFC 9112 §3.2";
                            goto ParseFailed;
                        }
                        seenHost = true;
                        if (valueSpan.IsEmpty) {
                            failReason = "empty Host header violates RFC 9112 §3.2";
                            goto ParseFailed;
                        }
                        break;
                }
        }

        goto ParseFailed;

HeadersComplete:
            // RFC 9112 §6.3.3: presence of both TE and CL is a request-smuggling vector
            if (isChunked && contentLengthExplicit) {
                Logger.LogInformation ( $"failed to parse HTTP request: both Transfer-Encoding and Content-Length present, request-smuggling vector (RFC 9112 §6.3.3)" );
                return null;
            }

            if (protocol.Span.SequenceEqual ( Http11 ) && !seenHost) {
                Logger.LogInformation ( $"failed to parse HTTP request: missing Host header violates RFC 9112 §3.2" );
                return null;
            }

            return new HttpRequestBase {
                HeaderLength = cursor,
                MethodRef = method,
                PathRef = path,
                HeaderBlockRef = buffer.Slice ( headersStart, cursor - headersStart ),
                ContentLength = contentLength,
                CanKeepAlive = keepAliveEnabled,
                IsChunked = isChunked,
                IsExpecting100 = expect100,
                BufferedContent = buffer.Slice ( cursor )
            };

ParseFailed:
        Logger.LogInformation ( $"failed to parse HTTP request: {failReason ?? "malformed request"}" );
        return null;
    }

    private static ReadOnlySpan<byte> ContentLengthName => "Content-Length"u8;
    private static ReadOnlySpan<byte> ConnectionName => "Connection"u8;
    private static ReadOnlySpan<byte> ExpectName => "Expect"u8;
    private static ReadOnlySpan<byte> TransferEncodingName => "Transfer-Encoding"u8;
    private static ReadOnlySpan<byte> HostName => "Host"u8;

    internal static HttpHeader [] ParseHeaders ( ReadOnlyMemory<byte> headerBlock ) {
        ReadOnlySpan<byte> span = headerBlock.Span;
        HttpHeader [] headers = ArrayPool<HttpHeader>.Shared.Rent ( 16 );
        int headerCount = 0;
        int cursor = 0;

        try {
            while (cursor < span.Length) {
                ReadOnlySpan<byte> remaining = span.Slice ( cursor );
                int lfRel = remaining.IndexOf ( LineFeed );
                if (lfRel < 0) {
                    break;
                }

                int lineEnd = cursor + lfRel;
                int headerLineEnd = lineEnd;

                if (headerLineEnd > cursor && span [ headerLineEnd - 1 ] == CarriageReturn) {
                    headerLineEnd--;
                }

                if (headerLineEnd == cursor) {
                    break;
                }

                ReadOnlySpan<byte> headerLine = span.Slice ( cursor, headerLineEnd - cursor );
                int colonIndex = headerLine.IndexOf ( Colon );
                if (colonIndex > 0) {
                    ReadOnlySpan<byte> rawValue = headerLine.Slice ( colonIndex + 1 );
                    ReadOnlySpan<byte> valueSpan = rawValue.Trim ( TrimChars );

                    if (headerCount >= headers.Length) {
                        HttpHeader [] newHeaders = ArrayPool<HttpHeader>.Shared.Rent ( headers.Length * 2 );
                        headers.AsSpan ( 0, headerCount ).CopyTo ( newHeaders );
                        ArrayPool<HttpHeader>.Shared.Return ( headers );
                        headers = newHeaders;
                    }

                    headers [ headerCount++ ] = new HttpHeader (
                        headerBlock.Slice ( cursor, colonIndex ),
                        headerBlock.Slice ( cursor + colonIndex + 1 + (rawValue.Length - valueSpan.Length), valueSpan.Length )
                    );
                }

                cursor = lineEnd + 1;
            }

            HttpHeader [] finalHeaders = new HttpHeader [ headerCount ];
            headers.AsSpan ( 0, headerCount ).CopyTo ( finalHeaders );
            ArrayPool<HttpHeader>.Shared.Return ( headers );
            return finalHeaders;
        }
        catch {
            ArrayPool<HttpHeader>.Shared.Return ( headers );
            throw;
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static int GetKnownHeaderIndex ( ReadOnlySpan<byte> name ) {
        if (name.IsEmpty)
            return -1;

        switch ((name [ 0 ] | 0x20, name.Length)) {
            case ((byte) 'c', 14 ):
                return Ascii.EqualsIgnoreCase ( name, ContentLengthName ) ? 0 : -1;
            case ((byte) 'c', 10 ):
                return Ascii.EqualsIgnoreCase ( name, ConnectionName ) ? 1 : -1;
            case ((byte) 'e', 6 ):
                return Ascii.EqualsIgnoreCase ( name, ExpectName ) ? 2 : -1;
            case ((byte) 't', 17 ):
                return Ascii.EqualsIgnoreCase ( name, TransferEncodingName ) ? 3 : -1;
            case ((byte) 'h', 4 ):
                return Ascii.EqualsIgnoreCase ( name, HostName ) ? 4 : -1;
            default:
                return -1;
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static bool IsSupportedHttpProtocol ( ReadOnlySpan<byte> protocol )
        => protocol.SequenceEqual ( Http11 )
        || protocol.SequenceEqual ( Http10 )
        || protocol.SequenceEqual ( Http09 );

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static bool TokenListContains ( ReadOnlySpan<byte> value, ReadOnlySpan<byte> expectedToken ) {
        while (!value.IsEmpty) {
            int commaIndex = value.IndexOf ( (byte) ',' );
            ReadOnlySpan<byte> token = commaIndex >= 0
                ? value.Slice ( 0, commaIndex )
                : value;

            if (Ascii.EqualsIgnoreCase ( token.Trim ( TrimChars ), expectedToken )) {
                return true;
            }

            if (commaIndex < 0) {
                break;
            }

            value = value.Slice ( commaIndex + 1 );
        }

        return false;
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static bool TokenListEqualsSingle ( ReadOnlySpan<byte> value, ReadOnlySpan<byte> expectedToken ) {
        return value.IndexOf ( (byte) ',' ) < 0
            && Ascii.EqualsIgnoreCase ( value.Trim ( TrimChars ), expectedToken );
    }

    private static bool IsAsciiDigits ( ReadOnlySpan<byte> value ) {
        if (value.IsEmpty)
            return false;

        foreach (byte b in value) {
            if ((uint) (b - '0') > 9)
                return false;
        }

        return true;
    }
}
