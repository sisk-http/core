// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpChunkedReadStream2.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Cadente.Streams;

sealed class HttpChunkedReadStream2 : Stream {
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

        if (currentBlockSize == 0) {
            return 0;
        }
        else if (currentBlockSize > 0) {
            var expected = Math.Min ( count, currentBlockSize - currentBlockRead );
            var read = _s.Read ( buffer, offset, expected );

            currentBlockRead += read;

            if (currentBlockRead == currentBlockSize) {
                // read trailing \r\n
                _ = _s.Read ( _buffer, 0, 2 );
                currentBlockSize = -1;
                currentBlockRead = 0;
            }

            return read;
        }
        else if (currentBlockSize == -1) {
            // read next chunked header
            int read = _s.Read ( _buffer, 0, 16 );
            var headerLine = Encoding.ASCII.GetString ( _buffer, 0, read );

            var numEnd = headerLine.IndexOf ( '\r' );
            var numberString = headerLine.Substring ( 0, numEnd );

            if (numberString == "0") {
                currentBlockSize = 0;
                return 0;
            }

            var number = Convert.ToInt32 ( numberString, 16 );

            var current = _buffer [ 0..read ].AsSpan () [ (numEnd + 2).. ];
            current.CopyTo ( buffer.AsSpan () [ offset.. ] );

            currentBlockSize = number;
            currentBlockRead = current.Length;

            return current.Length;
        }

        return 1;
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