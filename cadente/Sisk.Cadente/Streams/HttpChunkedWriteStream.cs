// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpChunkedWriteStream.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Buffers.Text;

namespace Sisk.Cadente.Streams;

internal class HttpChunkedWriteStream : Stream {
    private Stream _stream;
    private byte []? _writeBuffer;
    private int _writeBufferLength;

    private static readonly byte [] s_finalChunkBytes = "0\r\n\r\n"u8.ToArray ();
    private const int ChunkBufferSize = 16 * 1024;

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotImplementedException ();

    public override long Position { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public HttpChunkedWriteStream ( Stream stream ) {
        _stream = stream;
    }

    public override void Flush () {
        FlushBufferedChunk ();
        _stream.Flush ();
    }

    public override Task FlushAsync ( CancellationToken cancellationToken ) {
        cancellationToken.ThrowIfCancellationRequested ();
        Flush ();
        return Task.CompletedTask;
    }

    public override int Read ( byte [] buffer, int offset, int count ) {
        throw new NotImplementedException ();
    }

    public override long Seek ( long offset, SeekOrigin origin ) {
        throw new NotImplementedException ();
    }

    public override void SetLength ( long value ) {
        throw new NotImplementedException ();
    }

    public override void Write ( byte [] buffer, int offset, int count ) {
        Write ( buffer.AsSpan ( offset, count ) );
    }

    public override void Write ( ReadOnlySpan<byte> buffer ) {

        if (buffer.IsEmpty) {
            return;
        }

        if (buffer.Length >= ChunkBufferSize) {
            FlushBufferedChunk ();
            WriteChunkFrame ( buffer );
            return;
        }

        _writeBuffer ??= ArrayPool<byte>.Shared.Rent ( ChunkBufferSize );

        if (_writeBufferLength + buffer.Length > ChunkBufferSize)
            FlushBufferedChunk ();

        buffer.CopyTo ( _writeBuffer.AsSpan ( _writeBufferLength ) );
        _writeBufferLength += buffer.Length;

        if (_writeBufferLength == ChunkBufferSize)
            FlushBufferedChunk ();
    }

    private void WriteChunkFrame ( ReadOnlySpan<byte> buffer ) {

        Span<byte> header = stackalloc byte [ 16 ];
        if (!Utf8Formatter.TryFormat ( buffer.Length, header, out int headerLength, new StandardFormat ( 'X' ) ))
            throw new InvalidOperationException ( "Unable to format chunk size." );

        header [ headerLength++ ] = (byte) '\r';
        header [ headerLength++ ] = (byte) '\n';

        if (buffer.Length <= ChunkBufferSize) {
            byte [] rented = ArrayPool<byte>.Shared.Rent ( headerLength + buffer.Length + 2 );
            try {
                Span<byte> output = rented.AsSpan ( 0, headerLength + buffer.Length + 2 );
                header [ ..headerLength ].CopyTo ( output );
                buffer.CopyTo ( output [ headerLength.. ] );
                output [ ^2 ] = (byte) '\r';
                output [ ^1 ] = (byte) '\n';
                _stream.Write ( output );
            }
            finally {
                ArrayPool<byte>.Shared.Return ( rented );
            }

            return;
        }

        _stream.Write ( header [ ..headerLength ] );
        _stream.Write ( buffer );
        _stream.Write ( "\r\n"u8 );
    }

    public override Task WriteAsync ( byte [] buffer, int offset, int count, CancellationToken cancellationToken ) {
        cancellationToken.ThrowIfCancellationRequested ();
        Write ( buffer.AsSpan ( offset, count ) );
        return Task.CompletedTask;
    }

    public override ValueTask WriteAsync ( ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default ) {
        cancellationToken.ThrowIfCancellationRequested ();
        Write ( buffer.Span );
        return ValueTask.CompletedTask;
    }

    protected override void Dispose ( bool disposing ) {

        if (disposing) {
            FlushBufferedChunk ();

            try {
                if (_stream != null && _stream.CanWrite) {
                    _stream.Write ( s_finalChunkBytes );
                    _stream.Flush ();
                }
            }
            catch {
            }

            if (_writeBuffer is { } writeBuffer) {
                ArrayPool<byte>.Shared.Return ( writeBuffer );
                _writeBuffer = null;
            }

            _stream = null!;
        }

        base.Dispose ( disposing );
    }

    public override ValueTask DisposeAsync () {
        Dispose ( disposing: true );
        return ValueTask.CompletedTask;
    }

    private void FlushBufferedChunk () {
        if (_writeBufferLength == 0 || _writeBuffer is null)
            return;

        WriteChunkFrame ( _writeBuffer.AsSpan ( 0, _writeBufferLength ) );
        _writeBufferLength = 0;
    }
}
