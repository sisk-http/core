using System.Buffers;

namespace Sisk.ManagedHttpListener.Streams;

internal class HttpBufferedReadStream : Stream
{
    private const int INITIAL_BUFFER = 4096;

    private readonly Stream _stream;
    private readonly byte[] _b;

    private long _position;
    private int _read;

    public HttpBufferedReadStream(Stream stream)
    {
        _stream = stream;
        _position = 0;
        _read = 0;
        _b = ArrayPool<byte>.Shared.Rent(INITIAL_BUFFER);
    }

    public Span<byte> BufferedBytes => _b;
    public int BufferedLength => _read;

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position { get => _position; set => Move((int)value); }

    public override void Flush()
    {
        _stream.Flush();
    }

    public override int Read(Span<byte> buffer)
    {
        byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
        try
        {
            int r = Read(sharedBuffer, (int)_position, buffer.Length);
            sharedBuffer.AsSpan().CopyTo(buffer);
            return r;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(sharedBuffer);
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int read;

        if (offset < _read)
        {
            if (offset + count > _read)
            {
                int remainder = Math.Min(count - offset, _read - (int)_position);
                Array.Copy(_b, offset, buffer, 0, remainder);
                read = remainder;
            }
            else
            {
                Array.Copy(_b, offset, buffer, 0, count);
                read = count;
            }
        }
        else
        {
            read = _stream.Read(buffer);
            Array.Copy(buffer, 0, _b, _position, read);
            _read += read;
        }

        _position += read;
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public void Move(int toPosition)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(toPosition, _read);
        _position = toPosition;
    }

    protected override void Dispose(bool disposing)
    {
        ArrayPool<byte>.Shared.Return(_b);
        base.Dispose(disposing);
    }
}
