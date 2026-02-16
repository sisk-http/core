using System;
using System.IO;

namespace tests.Tests;

internal sealed class RandomStream : Stream
{
    private readonly long _length;
    private long _position;
    private readonly Random _random;

    public RandomStream(long length, int seed)
    {
        _length = length;
        _random = new Random(seed);
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => _length;
    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException();
    }

    public override void Flush() { }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return Read(buffer.AsSpan(offset, count));
    }

    public override int Read(Span<byte> buffer)
    {
        long remaining = _length - _position;
        if (remaining <= 0) return 0;

        int toRead = (int)Math.Min(buffer.Length, remaining);
        _random.NextBytes(buffer.Slice(0, toRead));
        _position += toRead;
        return toRead;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
