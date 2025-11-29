// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpChunkedReadStream2.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Text;

namespace Sisk.Cadente.Streams;

sealed class HttpChunkedReadStream2 : EndableStream {
    Stream _s;
    byte [] _buffer;

    int currentBlockSize = -1;
    int currentBlockRead = 0;

    public HttpChunkedReadStream2 ( Stream s ) {
        _s = s;
        _buffer = ArrayPool<byte>.Shared.Rent ( 8192 );
    }

    public override bool CanRead => true;

    public override bool CanSeek => throw new NotImplementedException ();

    public override bool CanWrite => throw new NotImplementedException ();

    public override long Length => throw new NotImplementedException ();

    public override long Position { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }

    public override void Flush () {
        throw new NotImplementedException ();
    }

    public override int Read ( byte [] buffer, int offset, int count ) {

        if (IsEnded || currentBlockSize == 0) {
            return 0;
        }

        if (currentBlockSize == -1) {
            // read next chunked header
            int ptr = 0;
            while (true) {
                int b = _s.ReadByte ();
                if (b == -1)
                    break;
                _buffer [ ptr++ ] = (byte) b;

                if (ptr >= 2 && _buffer [ ptr - 1 ] == '\n' && _buffer [ ptr - 2 ] == '\r') {
                    break;
                }
                if (ptr >= _buffer.Length)
                    throw new InvalidOperationException ( "Chunk header too long" );
            }

            if (ptr == 0)
                return 0;

            var headerLine = Encoding.ASCII.GetString ( _buffer, 0, ptr - 2 );
            var extIndex = headerLine.IndexOf ( ';' );
            var numberString = extIndex > -1 ? headerLine.Substring ( 0, extIndex ) : headerLine;

            if (numberString == "0") {
                currentBlockSize = 0;
                FinishReading ();
                return 0;
            }

            currentBlockSize = Convert.ToInt32 ( numberString, 16 );
            currentBlockRead = 0;
        }

        var expected = Math.Min ( count, currentBlockSize - currentBlockRead );
        var read = _s.Read ( buffer, offset, expected );

        currentBlockRead += read;

        if (currentBlockRead == currentBlockSize) {
            // read trailing \r\n
            int r = 0;
            while (r < 2) {
                int x = _s.Read ( _buffer, 0, 2 - r );
                if (x == 0)
                    break;
                r += x;
            }
            currentBlockSize = -1;
            currentBlockRead = 0;
        }

        return read;
    }

    public override long Seek ( long offset, SeekOrigin origin ) {
        throw new NotImplementedException ();
    }

    public override void SetLength ( long value ) {
        throw new NotImplementedException ();
    }

    public override void Write ( byte [] buffer, int offset, int count ) {
        throw new NotImplementedException ();
    }
}