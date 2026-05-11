// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpChunkedReadStream2.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Runtime.CompilerServices;

namespace Sisk.Cadente.Streams;

sealed class HttpChunkedReadStream2 : EndableStream {
    Stream _s;
    byte [] _buffer;

    int currentBlockSize = -1;
    int currentBlockRead = 0;

    private static readonly sbyte [] _hexLookup = new sbyte [ 256 ];

    static HttpChunkedReadStream2 () {
        Array.Fill ( _hexLookup, (sbyte) -1 );
        for (byte c = (byte) '0'; c <= (byte) '9'; c++)
            _hexLookup [ c ] = (sbyte) (c - '0');
        for (byte c = (byte) 'a'; c <= (byte) 'f'; c++)
            _hexLookup [ c ] = (sbyte) (c - 'a' + 10);
        for (byte c = (byte) 'A'; c <= (byte) 'F'; c++)
            _hexLookup [ c ] = (sbyte) (c - 'A' + 10);
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static int ParseHexSize ( ReadOnlySpan<byte> buffer ) {
        long result = 0;
        bool hasDigit = false;

        for (int i = 0; i < buffer.Length; i++) {
            byte b = buffer [ i ];
            if (b == (byte) ';' || b == (byte) '\r' || b == (byte) '\n')
                break;

            sbyte digit = _hexLookup [ b ];
            if (digit < 0)
                throw new ChunkParseException ( $"Invalid chunked transfer encoding: malformed chunk size." );

            hasDigit = true;
            result = (result << 4) | (byte) digit;
            if (result > int.MaxValue)
                throw new ChunkParseException ( $"Invalid chunked transfer encoding: chunk size is too large." );
        }

        if (!hasDigit)
            throw new ChunkParseException ( $"Invalid chunked transfer encoding: empty chunk size." );

        return (int) result;
    }

    public HttpChunkedReadStream2 ( Stream s ) {
        _s = s;
        _buffer = ArrayPool<byte>.Shared.Rent ( 8192 );
    }

    public override bool CanRead => true;

    public override bool CanSeek => throw new NotImplementedException ();

    public override bool CanWrite => throw new NotImplementedException ();

    public override long Length => throw new NotImplementedException ();

    public override long Position { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public override void Flush () {
        throw new NotImplementedException ();
    }

    public override int Read ( byte [] buffer, int offset, int count ) {

        if (IsEnded || currentBlockSize == 0) {
            return 0;
        }

        if (currentBlockSize == -1) {
            int headerLength = ReadMetadataLine ();

            if (headerLength < 0)
                return 0;

            ReadOnlySpan<byte> headerBytes = _buffer.AsSpan ( 0, headerLength );

            if (headerBytes.IsEmpty)
                throw new ChunkParseException ( "Invalid chunked transfer encoding: empty chunk size." );

            int parsedSize;
            try {
                parsedSize = ParseHexSize ( headerBytes );
            }
            catch (ChunkParseException) {
                throw;
            }
            catch (Exception ex) {
                throw new ChunkParseException ( $"Invalid chunked transfer encoding: malformed chunk size.", ex );
            }

            if (parsedSize == 0) {
                currentBlockSize = 0;
                ConsumeTrailerSection ();
                FinishReading ();
                return 0;
            }

            if (parsedSize < 0)
                throw new ChunkParseException ( $"Invalid chunked transfer encoding: negative chunk size {parsedSize}." );

            currentBlockSize = parsedSize;
            currentBlockRead = 0;
        }

        var expected = Math.Min ( count, currentBlockSize - currentBlockRead );
        var read = _s.Read ( buffer, offset, expected );

        currentBlockRead += read;

        if (currentBlockRead == currentBlockSize) {
            ReadRequiredCrlf ();
            currentBlockSize = -1;
            currentBlockRead = 0;
        }

        return read;
    }

    private int ReadMetadataLine () {
        int ptr = 0;

        while (true) {
            int b = _s.ReadByte ();
            if (b == -1) {
                if (ptr == 0)
                    return -1;

                throw new ChunkParseException ( "Invalid chunked transfer encoding: incomplete chunk metadata." );
            }

            if (ptr >= _buffer.Length)
                throw new ChunkParseException ( "Chunk metadata line too long." );

            _buffer [ ptr++ ] = (byte) b;

            if (ptr >= 2 && _buffer [ ptr - 2 ] == '\r' && _buffer [ ptr - 1 ] == '\n')
                return ptr - 2;

            if (b == '\n')
                throw new ChunkParseException ( "Invalid chunked transfer encoding: expected CRLF." );
        }
    }

    private void ReadRequiredCrlf () {
        int read = 0;

        while (read < 2) {
            int count = _s.Read ( _buffer, read, 2 - read );
            if (count == 0)
                throw new ChunkParseException ( "Invalid chunked transfer encoding: incomplete chunk terminator." );

            read += count;
        }

        if (_buffer [ 0 ] != '\r' || _buffer [ 1 ] != '\n')
            throw new ChunkParseException ( "Invalid chunked transfer encoding: expected CRLF after chunk data." );
    }

    private void ConsumeTrailerSection () {
        while (true) {
            int trailerLineLength = ReadMetadataLine ();
            if (trailerLineLength < 0)
                throw new ChunkParseException ( "Invalid chunked transfer encoding: incomplete trailer section." );

            if (trailerLineLength == 0)
                return;
        }
    }

    public override long Seek ( long offset, SeekOrigin origin ) {
        throw new NotImplementedException ();
    }

    public override void SetLength ( long value ) {
        throw new NotImplementedException ();
    }

    public override void Write ( byte [] buffer, int offset, int count ) {
        throw new NotImplementedException ();
    }
}
