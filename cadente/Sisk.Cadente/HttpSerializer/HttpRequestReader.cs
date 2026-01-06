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

    private const int DefaultHeaderReadTimeoutMs = 30_000;
    private static ReadOnlySpan<byte> HeaderTerminator => "\r\n\r\n"u8;
    private static ReadOnlySpan<byte> Http10 => "HTTP/1.0"u8;
    private static ReadOnlySpan<byte> CloseValue => "close"u8;
    private static ReadOnlySpan<byte> ContinueValue => "100-continue"u8;
    private static ReadOnlySpan<byte> ChunkedValue => "chunked"u8;

    [SkipLocalsInit]
    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    public static async ValueTask<HttpRequestBase?> TryReadHttpRequestAsync ( Memory<byte> sharedBuffer, Stream stream, CancellationToken cancellationToken = default, int headerReadTimeoutMs = DefaultHeaderReadTimeoutMs ) {

        int bufferLength = sharedBuffer.Length;
        int totalRead = 0;
        long deadlineTicks = Environment.TickCount64 + headerReadTimeoutMs;

        int searchStart = 0;

        try {
            while (totalRead < bufferLength) {
                long currentTicks = Environment.TickCount64;
                if (currentTicks >= deadlineTicks) {
                    Logger.LogInformation ( $"failed to parse HTTP request: header read timeout" );
                    return null;
                }

                int remainingMs = (int) (deadlineTicks - currentTicks);
                cancellationToken.ThrowIfCancellationRequested ();

                int bytesRead = await ReadWithTimeoutAsync (
                    stream,
                    sharedBuffer.Slice ( totalRead ),
                    remainingMs,
                    cancellationToken
                ).ConfigureAwait ( false );

                if (bytesRead == 0) {
                    Logger.LogInformation ( $"failed to parse HTTP request: connection closed" );
                    return null;
                }

                totalRead += bytesRead;

                int effectiveSearchStart = Math.Max ( 0, searchStart - 3 );
                ReadOnlySpan<byte> searchRegion = sharedBuffer.Span.Slice ( effectiveSearchStart, totalRead - effectiveSearchStart );

                int terminatorIndex = searchRegion.IndexOf ( HeaderTerminator );
                if (terminatorIndex >= 0) {
                    return ParseHttpRequest ( sharedBuffer.Slice ( 0, totalRead ) );
                }

                searchStart = totalRead;
            }

            Logger.LogInformation ( $"failed to parse HTTP request: headers too large" );
            return null;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) {
            Logger.LogInformation ( $"failed to parse HTTP request: header read timeout" );
            return null;
        }
        catch (OperationCanceledException) {
            Logger.LogInformation ( $"failed to parse HTTP request: operation cancelled" );
            return null;
        }
        catch (SocketException) {
            return null; // socket errors are common when client disconnects abruptly
        }
        catch (Exception ex) {
            Logger.LogInformation ( $"failed to parse HTTP request (exception): {ex.Message}" );
            return null;
        }
    }


    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static ValueTask<int> ReadWithTimeoutAsync ( Stream stream, Memory<byte> buffer, int timeoutMs, CancellationToken cancellationToken ) {

        stream.ReadTimeout = timeoutMs;
        return stream.ReadAsync ( buffer, cancellationToken );
    }

    [SkipLocalsInit]
    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    private static HttpRequestBase? ParseHttpRequest ( ReadOnlyMemory<byte> buffer ) {
        ReadOnlySpan<byte> span = buffer.Span;
        int bufferLength = span.Length;

        // Request line: METHOD SP PATH SP PROTOCOL CRLF
        int methodEnd = span.IndexOf ( Space );
        if (methodEnd <= 0)
            return null;

        ReadOnlyMemory<byte> method = buffer.Slice ( 0, methodEnd );

        int pathStart = methodEnd + 1;
        int pathEndRel = span.Slice ( pathStart ).IndexOf ( Space );
        if (pathEndRel < 0)
            return null;

        int pathEnd = pathStart + pathEndRel;
        ReadOnlyMemory<byte> path = buffer.Slice ( pathStart, pathEndRel );

        int protocolStart = pathEnd + 1;
        int protocolLineEndRel = span.Slice ( protocolStart ).IndexOf ( LineFeed );
        if (protocolLineEndRel < 0)
            return null;

        int protocolLineEnd = protocolStart + protocolLineEndRel;
        int protocolEndExclusive = protocolLineEnd;

        ref byte spanRef = ref MemoryMarshal.GetReference ( span );
        if (protocolEndExclusive > protocolStart &&
            Unsafe.Add ( ref spanRef, protocolEndExclusive - 1 ) == CarriageReturn) {
            protocolEndExclusive--;
        }

        ReadOnlyMemory<byte> protocol = buffer.Slice ( protocolStart, protocolEndExclusive - protocolStart );

        int cursor = protocolLineEnd + 1;

        long contentLength = 0;
        bool keepAliveEnabled = !protocol.Span.SequenceEqual ( Http10 );
        bool expect100 = false;
        bool isChunked = false;

        HttpHeader [] headers = ArrayPool<HttpHeader>.Shared.Rent ( 64 );
        int headerCount = 0;

        try {
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
                    goto ParseFailed;
                }

                int lineStart = cursor;
                int lineEnd = cursor + lfRel;
                int headerLineEnd = lineEnd;

                if (Unsafe.Add ( ref spanRef, headerLineEnd - 1 ) == CarriageReturn) {
                    headerLineEnd--;
                }

                int headerLineLength = headerLineEnd - lineStart;
                if (headerLineLength == 0) {
                    cursor = lineEnd + 1;
                    goto HeadersComplete;
                }

                ReadOnlySpan<byte> headerLine = span.Slice ( lineStart, headerLineLength );
                cursor = lineEnd + 1;

                int colonIndex = headerLine.IndexOf ( Colon );
                if (colonIndex <= 0)
                    continue;

                ReadOnlySpan<byte> nameSpan = headerLine.Slice ( 0, colonIndex );
                ReadOnlySpan<byte> rawValue = headerLine.Slice ( colonIndex + 1 );

                int trimLeft = 0;
                int trimRight = rawValue.Length;

                while (trimLeft < rawValue.Length && rawValue [ trimLeft ] <= 32)
                    trimLeft++;

                while (trimRight > trimLeft && rawValue [ trimRight - 1 ] <= 32)
                    trimRight--;

                int valueLength = trimRight - trimLeft;
                ReadOnlySpan<byte> valueSpan = rawValue.Slice ( trimLeft, valueLength );

                int knownHeader = GetKnownHeaderIndex ( nameSpan );
                switch (knownHeader) {
                    case 0: // Content-Length
                        if (Utf8Parser.TryParse ( valueSpan, out long parsed, out int consumed ) && consumed == valueSpan.Length) {
                            contentLength = parsed;
                        }
                        break;
                    case 1: // Connection
                        keepAliveEnabled = !Ascii.EqualsIgnoreCase ( valueSpan, CloseValue );
                        break;
                    case 2: // Expect
                        expect100 = Ascii.EqualsIgnoreCase ( valueSpan, ContinueValue );
                        break;
                    case 3: // Transfer-Encoding
                        isChunked = Ascii.EqualsIgnoreCase ( valueSpan, ChunkedValue );
                        if (isChunked)
                            contentLength = -1;
                        break;
                }

                if (Unlikely ( headerCount >= headers.Length )) {
                    HttpHeader [] newHeaders = ArrayPool<HttpHeader>.Shared.Rent ( headers.Length * 2 );
                    headers.AsSpan ( 0, headerCount ).CopyTo ( newHeaders );
                    ArrayPool<HttpHeader>.Shared.Return ( headers );
                    headers = newHeaders;
                }

                headers [ headerCount++ ] = new HttpHeader (
                    buffer.Slice ( lineStart, colonIndex ),
                    buffer.Slice ( lineStart + colonIndex + 1 + trimLeft, valueLength )
                );
            }

            goto ParseFailed;

HeadersComplete:
            HttpHeader [] finalHeaders = new HttpHeader [ headerCount ];
            headers.AsSpan ( 0, headerCount ).CopyTo ( finalHeaders );
            ArrayPool<HttpHeader>.Shared.Return ( headers );

            ReadOnlyMemory<byte> bufferedContent = expect100
                ? ReadOnlyMemory<byte>.Empty
                : buffer.Slice ( cursor );

            return new HttpRequestBase {
                MethodRef = method,
                PathRef = path,
                Headers = finalHeaders,
                ContentLength = contentLength,
                CanKeepAlive = keepAliveEnabled,
                IsChunked = isChunked,
                IsExpecting100 = expect100,
                BufferedContent = bufferedContent
            };

ParseFailed:
            ArrayPool<HttpHeader>.Shared.Return ( headers );
            return null;
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

        return (name [ 0 ], name.Length) switch {
            ((byte) 'C', 14 ) => 0,  // Content-Length
            ((byte) 'c', 14 ) => 0,
            ((byte) 'C', 10 ) => 1,  // Connection
            ((byte) 'c', 10 ) => 1,
            ((byte) 'E', 6 ) => 2,  // Expect
            ((byte) 'e', 6 ) => 2,
            ((byte) 'T', 17 ) => 3,  // Transfer-Encoding
            ((byte) 't', 17 ) => 3,
            _ => -1
        };
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static bool Unlikely ( bool condition ) => condition;
}