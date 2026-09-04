// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   BufferedConnectionStream.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Cadente.Streams;

sealed class BufferedConnectionStream : Stream {
    private readonly Stream baseStream;
    private readonly Memory<byte> writeBuffer;
    private int bufferedWriteLength;

    public BufferedConnectionStream ( Stream baseStream, Memory<byte> writeBuffer ) {
        this.baseStream = baseStream;
        this.writeBuffer = writeBuffer;
    }

    public override bool CanRead => baseStream.CanRead;

    public override bool CanSeek => baseStream.CanSeek;

    public override bool CanWrite => baseStream.CanWrite;

    public override long Length => baseStream.Length;

    public override long Position { get => baseStream.Position; set => baseStream.Position = value; }

    public override void Flush () {
        WritePendingBytes ();
        baseStream.Flush ();
    }

    public override async Task FlushAsync ( CancellationToken cancellationToken ) {
        await WritePendingBytesAsync ( cancellationToken ).ConfigureAwait ( false );
        await baseStream.FlushAsync ( cancellationToken ).ConfigureAwait ( false );
    }

    public override int Read ( byte [] buffer, int offset, int count ) {
        WritePendingBytes ();
        return baseStream.Read ( buffer, offset, count );
    }

    public override int Read ( Span<byte> buffer ) {
        WritePendingBytes ();
        return baseStream.Read ( buffer );
    }

    public override int ReadByte () {
        WritePendingBytes ();
        return baseStream.ReadByte ();
    }

    public override async ValueTask<int> ReadAsync ( Memory<byte> buffer, CancellationToken cancellationToken = default ) {
        await WritePendingBytesAsync ( cancellationToken ).ConfigureAwait ( false );
        return await baseStream.ReadAsync ( buffer, cancellationToken ).ConfigureAwait ( false );
    }

    public override Task<int> ReadAsync ( byte [] buffer, int offset, int count, CancellationToken cancellationToken ) {
        return ReadAsync ( buffer.AsMemory ( offset, count ), cancellationToken ).AsTask ();
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
        if (buffer.Length >= writeBuffer.Length) {
            WritePendingBytes ();
            baseStream.Write ( buffer );
            return;
        }

        if (bufferedWriteLength + buffer.Length > writeBuffer.Length) {
            WritePendingBytes ();
        }

        buffer.CopyTo ( writeBuffer.Span [ bufferedWriteLength.. ] );
        bufferedWriteLength += buffer.Length;
    }

    public override Task WriteAsync ( byte [] buffer, int offset, int count, CancellationToken cancellationToken ) {
        return WriteAsync ( buffer.AsMemory ( offset, count ), cancellationToken ).AsTask ();
    }

    public override ValueTask WriteAsync ( ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default ) {
        if (bufferedWriteLength + buffer.Length <= writeBuffer.Length) {
            buffer.CopyTo ( writeBuffer [ bufferedWriteLength.. ] );
            bufferedWriteLength += buffer.Length;
            return ValueTask.CompletedTask;
        }

        return WriteSlowAsync ( buffer, cancellationToken );
    }

    protected override void Dispose ( bool disposing ) {
        if (disposing) {
            try {
                WritePendingBytes ();
            }
            finally {
                baseStream.Dispose ();
            }
        }

        base.Dispose ( disposing );
    }

    public override async ValueTask DisposeAsync () {
        try {
            await WritePendingBytesAsync ( default ).ConfigureAwait ( false );
        }
        finally {
            await baseStream.DisposeAsync ().ConfigureAwait ( false );
        }

        GC.SuppressFinalize ( this );
    }

    private void WritePendingBytes () {
        if (bufferedWriteLength == 0)
            return;

        baseStream.Write ( writeBuffer.Span [ ..bufferedWriteLength ] );
        bufferedWriteLength = 0;
    }

    private async ValueTask WritePendingBytesAsync ( CancellationToken cancellationToken ) {
        if (bufferedWriteLength == 0)
            return;

        int length = bufferedWriteLength;
        bufferedWriteLength = 0;
        await baseStream.WriteAsync ( writeBuffer [ ..length ], cancellationToken ).ConfigureAwait ( false );
    }

    private async ValueTask WriteSlowAsync ( ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken ) {
        await WritePendingBytesAsync ( cancellationToken ).ConfigureAwait ( false );

        if (buffer.Length >= writeBuffer.Length) {
            await baseStream.WriteAsync ( buffer, cancellationToken ).ConfigureAwait ( false );
        }
        else {
            buffer.CopyTo ( writeBuffer );
            bufferedWriteLength = buffer.Length;
        }
    }
}
