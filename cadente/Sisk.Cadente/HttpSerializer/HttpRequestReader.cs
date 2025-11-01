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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sisk.Cadente.HttpSerializer;
static class HttpRequestReader {

    const byte SPACE = (byte) ' ';
    const byte LINE_FEED = (byte) '\n';
    const byte CARRIAGE_RETURN = (byte) '\r';
    const byte COLON = (byte) ':';

    private static readonly byte [] RequestLineDelimiters = [ LINE_FEED, 0 ];
    private static readonly byte [] RequestHeaderLineDelimiters = [ SPACE, 0 ];

    public static async ValueTask<HttpRequestBase?> TryReadHttpRequestAsync ( Stream stream ) {

        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent ( 2048 );

        try {
            Memory<byte> memory = owner.Memory;
            int read = await stream.ReadAsync ( memory ).ConfigureAwait ( false );
            if (read == 0) {
                owner.Dispose ();
                return null;
            }

            ReadOnlyMemory<byte> slice = memory.Slice ( 0, read );
            HttpRequestBase? request = ParseHttpRequest ( slice, owner );

            if (request is null) {
                owner.Dispose ();
            }

            return request;
        }
        catch {
            owner.Dispose ();
            throw;
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    static HttpRequestBase? ParseHttpRequest ( ReadOnlyMemory<byte> buffer, IMemoryOwner<byte> owner ) {
        ReadOnlySequence<byte> sequence = new ( buffer );
        SequenceReader<byte> reader = new ( sequence );

        if (!reader.TryReadToAny ( out ReadOnlySequence<byte> methodSeq, RequestHeaderLineDelimiters, advancePastDelimiter: true )) {
            return null;
        }
        ReadOnlyMemory<byte> method = SequenceSlice ( sequence, buffer, methodSeq );

        if (!reader.TryReadToAny ( out ReadOnlySequence<byte> pathSeq, RequestHeaderLineDelimiters, advancePastDelimiter: true )) {
            return null;
        }
        ReadOnlyMemory<byte> path = SequenceSlice ( sequence, buffer, pathSeq );

        if (!reader.TryReadTo ( out ReadOnlySequence<byte> protocolSeq, LINE_FEED ))
            return null;

        ReadOnlyMemory<byte> protocol = TrimTrailingCR ( SequenceSlice ( sequence, buffer, protocolSeq ) );
        reader.Advance ( 1 ); // '\n'

        bool keepAliveEnabled = true;
        bool expect100 = false;
        bool isChunked = false;
        long contentLength = 0;

        var headerWriter = new ArrayBufferWriter<HttpHeader> ( 32 );

        while (reader.TryReadTo ( out ReadOnlySequence<byte> headerLineSeq, LINE_FEED )) {
            ReadOnlyMemory<byte> headerLine = SequenceSlice ( sequence, buffer, headerLineSeq );

            if (headerLine.Length == 1 && headerLine.Span [ 0 ] == CARRIAGE_RETURN)
                break;

            ReadOnlySpan<byte> headerSpan = headerLine.Span;
            int colonIndex = headerSpan.IndexOf ( COLON );
            if (colonIndex <= 0)
                continue;

            ReadOnlyMemory<byte> headerName = headerLine.Slice ( 0, colonIndex );
            ReadOnlyMemory<byte> headerValue = TrimHeaderValue ( headerLine.Slice ( colonIndex + 1 ) );

            ReadOnlySpan<byte> nameSpan = headerName.Span;
            ReadOnlySpan<byte> valueSpan = headerValue.Span;

            if (Ascii.EqualsIgnoreCase ( nameSpan, "Content-Length"u8 )) {
                if (!Utf8Parser.TryParse ( valueSpan, out contentLength, out _ ))
                    contentLength = 0;
            }
            else if (Ascii.EqualsIgnoreCase ( nameSpan, "Connection"u8 )) {
                keepAliveEnabled = !Ascii.Equals ( valueSpan, "close"u8 );
            }
            else if (Ascii.EqualsIgnoreCase ( nameSpan, "Expect"u8 )) {
                expect100 = Ascii.Equals ( valueSpan, "100-continue"u8 );
            }
            else if (Ascii.EqualsIgnoreCase ( nameSpan, "Transfer-Encoding"u8 )) {
                isChunked = Ascii.Equals ( valueSpan, "chunked"u8 );
            }

            Span<HttpHeader> slot = headerWriter.GetSpan ( 1 );
            slot [ 0 ] = new HttpHeader ( headerName, headerValue );
            headerWriter.Advance ( 1 );
        }

        ReadOnlyMemory<byte> bufferedContent = expect100
            ? ReadOnlyMemory<byte>.Empty
            : buffer.Slice ( (int) reader.Consumed );

        return new HttpRequestBase {
            BufferOwner = owner,
            RawBuffer = buffer,
            MethodRef = method,
            PathRef = path,
            Headers = headerWriter.WrittenMemory,
            BufferedContent = bufferedContent,
            CanKeepAlive = keepAliveEnabled,
            ContentLength = isChunked ? -1 : contentLength,
            IsChunked = isChunked,
            IsExpecting100 = expect100
        };
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    static ReadOnlyMemory<byte> SequenceSlice (
        ReadOnlySequence<byte> whole,
        ReadOnlyMemory<byte> backing,
        ReadOnlySequence<byte> slice ) {
        int offset = (int) whole.Slice ( 0, slice.Start ).Length;
        return backing.Slice ( offset, (int) slice.Length );
    }

    static ReadOnlyMemory<byte> TrimTrailingCR ( ReadOnlyMemory<byte> value ) {
        if (!value.IsEmpty && value.Span [ ^1 ] == CARRIAGE_RETURN)
            return value.Slice ( 0, value.Length - 1 );

        return value;
    }

    static ReadOnlyMemory<byte> TrimHeaderValue ( ReadOnlyMemory<byte> value ) {
        ReadOnlySpan<byte> span = value.Span;

        int start = 0;
        while (start < span.Length && (span [ start ] == SPACE || span [ start ] == (byte) '\t'))
            start++;

        int end = span.Length - 1;
        while (end >= start &&
               (span [ end ] == SPACE || span [ end ] == (byte) '\t' || span [ end ] == CARRIAGE_RETURN))
            end--;

        if (end < start)
            return ReadOnlyMemory<byte>.Empty;

        int length = end - start + 1;
        return value.Slice ( start, length );
    }
}