// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   UndisposableNetworkStream.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Cadente.Streams;

sealed class UndisposableNetworkStream : Stream {
    private readonly Stream baseStream;
    private Memory<byte> responseBuffer;
    private int pendingHeaderLength;
    private readonly Action? pendingHeadersWritten;

    public UndisposableNetworkStream ( Stream baseStream ) {
        this.baseStream = baseStream;
    }

    public UndisposableNetworkStream ( Stream baseStream, Memory<byte> responseBuffer, int pendingHeaderLength, Action pendingHeadersWritten ) {
        this.baseStream = baseStream;
        this.responseBuffer = responseBuffer;
        this.pendingHeaderLength = pendingHeaderLength;
        this.pendingHeadersWritten = pendingHeadersWritten;
    }

    internal bool HasPendingHeaders => pendingHeaderLength > 0;

    public override bool CanRead => baseStream.CanRead;

    public override bool CanSeek => baseStream.CanSeek;

    public override bool CanWrite => baseStream.CanWrite;

    public override long Length => baseStream.Length;

    public override long Position { get => baseStream.Position; set => baseStream.Position = value; }

    public override void Flush () {
        WritePendingHeaders ();
        baseStream.Flush ();
    }

    public override int Read ( byte [] buffer, int offset, int count ) {
        return baseStream.Read ( buffer, offset, count );
    }

    public override int Read ( Span<byte> buffer ) {
        return baseStream.Read ( buffer );
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
        if (pendingHeaderLength > 0) {
            if (pendingHeaderLength + buffer.Length <= responseBuffer.Length) {
                buffer.CopyTo ( responseBuffer.Span [ pendingHeaderLength.. ] );
                baseStream.Write ( responseBuffer.Span [ ..(pendingHeaderLength + buffer.Length) ] );
                pendingHeaderLength = 0;
                return;
            }

            WritePendingHeaders ();
        }

        baseStream.Write ( buffer );
    }

    public override Task WriteAsync ( byte [] buffer, int offset, int count, CancellationToken cancellationToken ) {
        return WriteAsync ( buffer.AsMemory ( offset, count ), cancellationToken ).AsTask ();
    }

    public override ValueTask WriteAsync ( ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default ) {
        if (pendingHeaderLength > 0) {
            if (pendingHeaderLength + buffer.Length <= responseBuffer.Length) {
                buffer.CopyTo ( responseBuffer [ pendingHeaderLength.. ] );
                ValueTask writeTask = baseStream.WriteAsync ( responseBuffer [ ..(pendingHeaderLength + buffer.Length) ], cancellationToken );
                pendingHeaderLength = 0;
                return writeTask;
            }

            return WritePendingHeadersAndBodyAsync ( buffer, cancellationToken );
        }

        return baseStream.WriteAsync ( buffer, cancellationToken );
    }

    public override Task FlushAsync ( CancellationToken cancellationToken ) {
        if (pendingHeaderLength > 0) {
            return FlushPendingHeadersAsync ( cancellationToken ).AsTask ();
        }

        return baseStream.FlushAsync ( cancellationToken );
    }

    protected override void Dispose ( bool disposing ) {
        if (disposing) {
            WritePendingHeaders ();
        }
    }

    public override ValueTask DisposeAsync () {
        if (pendingHeaderLength > 0) {
            return FlushPendingHeadersAsync ( default );
        }

        return ValueTask.CompletedTask;
    }

    internal void WritePendingHeaders () {
        if (pendingHeaderLength <= 0)
            return;

        baseStream.Write ( responseBuffer.Span [ ..pendingHeaderLength ] );
        pendingHeaderLength = 0;
        pendingHeadersWritten?.Invoke ();
    }

    internal async ValueTask WritePendingHeadersAsync ( CancellationToken cancellationToken ) {
        if (pendingHeaderLength <= 0)
            return;

        await baseStream.WriteAsync ( responseBuffer [ ..pendingHeaderLength ], cancellationToken ).ConfigureAwait ( false );
        pendingHeaderLength = 0;
        pendingHeadersWritten?.Invoke ();
    }

    private async ValueTask FlushPendingHeadersAsync ( CancellationToken cancellationToken ) {
        await WritePendingHeadersAsync ( cancellationToken ).ConfigureAwait ( false );
        await baseStream.FlushAsync ( cancellationToken ).ConfigureAwait ( false );
    }

    private async ValueTask WritePendingHeadersAndBodyAsync ( ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken ) {
        await WritePendingHeadersAsync ( cancellationToken ).ConfigureAwait ( false );
        await baseStream.WriteAsync ( buffer, cancellationToken ).ConfigureAwait ( false );
    }
}
