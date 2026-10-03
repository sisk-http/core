// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   LimitedReadStream.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Internal;

sealed class LimitedReadStream : Stream {
    readonly Stream inner;
    readonly long maxLength;
    long totalRead;

    public LimitedReadStream ( Stream inner, long maxLength ) {
        this.inner = inner;
        this.maxLength = maxLength;
    }

    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException ();
    public override long Position { get => totalRead; set => throw new NotSupportedException (); }

    public override int Read ( byte [] buffer, int offset, int count ) => Account ( inner.Read ( buffer, offset, count ) );

    public override int Read ( Span<byte> buffer ) => Account ( inner.Read ( buffer ) );

    public override async Task<int> ReadAsync ( byte [] buffer, int offset, int count, CancellationToken cancellationToken ) =>
        Account ( await inner.ReadAsync ( buffer.AsMemory ( offset, count ), cancellationToken ).ConfigureAwait ( false ) );

    public override async ValueTask<int> ReadAsync ( Memory<byte> buffer, CancellationToken cancellationToken = default ) =>
        Account ( await inner.ReadAsync ( buffer, cancellationToken ).ConfigureAwait ( false ) );

    public override void Flush () { }
    public override long Seek ( long offset, SeekOrigin origin ) => throw new NotSupportedException ();
    public override void SetLength ( long value ) => throw new NotSupportedException ();
    public override void Write ( byte [] buffer, int offset, int count ) => throw new NotSupportedException ();

    int Account ( int read ) {
        totalRead += read;
        if (totalRead > maxLength) {
            throw new InsufficientMemoryException ( SR.StreamUtil_CopyOverflow );
        }
        return read;
    }
}
