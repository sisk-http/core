// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpChunkedStream.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Text;

namespace Sisk.Cadente.Streams;

internal class HttpChunkedReadStream : Stream {
    private Stream _stream;
    private byte [] _buffer;
    int written = 0;
    bool innerStreamReturnedZero = false;

    private static readonly byte [] s_crlfBytes = "\r\n"u8.ToArray ();
    private static readonly byte [] s_finalChunkBytes = "0\r\n\r\n"u8.ToArray ();

    public HttpChunkedReadStream ( Stream stream ) {
        _stream = stream;
        _buffer = ArrayPool<byte>.Shared.Rent ( 128 * 1024 );
    }

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException ();

    public override long Position { get => written; set => throw new NotSupportedException (); }

    public override void Flush () {
        _stream.Flush ();
    }

    public override int Read ( byte [] buffer, int offset, int count ) {

        if (innerStreamReturnedZero)
            return 0;

        var position = 0;
        var bytesRead = _stream.Read ( _buffer, 0, Math.Min ( _buffer.Length, count ) );
        if (bytesRead == 0) {
            innerStreamReturnedZero = true;

            // write last chunk
            WriteToBuffer ( s_finalChunkBytes );
            return s_finalChunkBytes.Length;
        }

        var bytesReadHex = Encoding.ASCII.GetBytes ( bytesRead.ToString ( "X" ) );

        WriteToBuffer ( bytesReadHex );
        WriteToBuffer ( s_crlfBytes );
        WriteToBuffer ( _buffer.AsSpan ( 0, bytesRead ) );
        WriteToBuffer ( s_crlfBytes );

        return position;

        void WriteToBuffer ( Span<byte> data ) {

            data.CopyTo ( buffer.AsSpan ( offset + position ) );
            position += data.Length;
        }
    }

    public override long Seek ( long offset, SeekOrigin origin ) {
        throw new NotSupportedException ();
    }

    public override void SetLength ( long value ) {
        throw new NotSupportedException ();
    }

    public override void Write ( byte [] buffer, int offset, int count ) {
        throw new NotSupportedException ();
    }

    protected override void Dispose ( bool disposing ) {

        if (_stream == null)
            return;

        if (disposing) {

            _stream.Dispose ();
            ArrayPool<byte>.Shared.Return ( _buffer );

            _stream = null!;
            _buffer = null!;
        }
    }
}
