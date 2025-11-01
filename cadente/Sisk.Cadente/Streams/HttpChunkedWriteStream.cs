// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpChunkedWriteStream.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.Cadente.Streams;

internal class HttpChunkedWriteStream : Stream {
    private Stream _stream;

    private static readonly byte [] s_crlfBytes = "\r\n"u8.ToArray ();
    private static readonly byte [] s_finalChunkBytes = "0\r\n\r\n"u8.ToArray ();

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotImplementedException ();

    public override long Position { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public HttpChunkedWriteStream ( Stream stream ) {
        _stream = stream;
    }

    public override void Flush () {
        _stream.Flush ();
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

        if (buffer.Length == 0) {
            _stream.Write ( s_finalChunkBytes );
            return;
        }

        _stream.Write ( Encoding.ASCII.GetBytes ( (count - offset).ToString ( "X" ) ) );
        _stream.Write ( s_crlfBytes );
        _stream.Write ( buffer.AsSpan ( offset, count ) );
        _stream.Write ( s_crlfBytes );
    }

    protected override void Dispose ( bool disposing ) {

        if (disposing) {

            if (_stream != null && _stream.CanWrite) {
                _stream.Write ( s_finalChunkBytes );

                _stream.Flush ();
                _stream = null!;
            }
        }

        base.Dispose ( disposing );
    }
}
