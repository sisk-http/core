// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   UndisposableNetworkStream.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Cadente.Streams;

sealed class UndisposableNetworkStream ( Stream baseStream ) : Stream {
    private const int WriteBufferSize = 16 * 1024;
    private const int DirectWriteThreshold = 1024;
    private byte []? _writeBuffer;
    private int _writeBufferLength;

    public override bool CanRead => baseStream.CanRead;

    public override bool CanSeek => baseStream.CanSeek;

    public override bool CanWrite => baseStream.CanWrite;

    public override long Length => baseStream.Length;

    public override long Position { get => baseStream.Position; set => baseStream.Position = value; }

    public override void Flush () {
        FlushWriteBuffer ();
        baseStream.Flush ();
    }

    public override Task FlushAsync ( CancellationToken cancellationToken ) {
        cancellationToken.ThrowIfCancellationRequested ();
        Flush ();
        return Task.CompletedTask;
    }

    public override int Read ( byte [] buffer, int offset, int count ) {
        return baseStream.Read ( buffer, offset, count );
    }

    public override int Read ( Span<byte> buffer ) {
        return baseStream.Read ( buffer );
    }

    public override ValueTask<int> ReadAsync ( Memory<byte> buffer, CancellationToken cancellationToken = default ) {
        cancellationToken.ThrowIfCancellationRequested ();
        return ValueTask.FromResult ( baseStream.Read ( buffer.Span ) );
    }

    public override long Seek ( long offset, SeekOrigin origin ) {
        return baseStream.Seek ( offset, origin );
    }

    public override void SetLength ( long value ) {
        baseStream.SetLength ( value );
    }

    public override void Write ( byte [] buffer, int offset, int count ) {
        Write ( buffer.AsSpan ( offset, count ) );
    }

    public override void Write ( ReadOnlySpan<byte> buffer ) {
        if (buffer.IsEmpty)
            return;

        if (_writeBuffer is null && buffer.Length <= DirectWriteThreshold) {
            baseStream.Write ( buffer );
            return;
        }

        if (buffer.Length >= WriteBufferSize) {
            FlushWriteBuffer ();
            baseStream.Write ( buffer );
            return;
        }

        _writeBuffer ??= System.Buffers.ArrayPool<byte>.Shared.Rent ( WriteBufferSize );

        if (_writeBufferLength + buffer.Length > WriteBufferSize)
            FlushWriteBuffer ();

        buffer.CopyTo ( _writeBuffer.AsSpan ( _writeBufferLength ) );
        _writeBufferLength += buffer.Length;

        if (_writeBufferLength == WriteBufferSize)
            FlushWriteBuffer ();
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
            FlushWriteBuffer ();

            if (_writeBuffer is { } writeBuffer) {
                System.Buffers.ArrayPool<byte>.Shared.Return ( writeBuffer );
                _writeBuffer = null;
            }
        }
    }

    public override ValueTask DisposeAsync () {
        Dispose ( disposing: true );
        return ValueTask.CompletedTask;
    }

    internal void FlushBufferedContent () {
        FlushWriteBuffer ();
    }

    private void FlushWriteBuffer () {
        if (_writeBufferLength == 0 || _writeBuffer is null)
            return;

        baseStream.Write ( _writeBuffer.AsSpan ( 0, _writeBufferLength ) );
        _writeBufferLength = 0;
    }
}
