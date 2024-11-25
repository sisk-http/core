namespace Sisk.ManagedHttpListener.Streams;

internal sealed class HttpRequestStream : Stream
{
    private readonly Stream _connectionStream;
    private long _position = 0;

    public HttpRequestStream(Stream connectionStream, long contentLength)
    {
        _connectionStream = connectionStream;
        Length = contentLength;
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length { get; }

    public override long Position { get => _position; set => throw new NotSupportedException(); }

    public override void Flush()
    {
        _connectionStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_position >= Length)
        {
            return 0;
        }
        var r = _connectionStream.Read(buffer, offset, Math.Min(count, (int)Length));
        _position += r;
        return r;
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
}
