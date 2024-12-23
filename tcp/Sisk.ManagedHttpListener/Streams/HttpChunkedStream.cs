// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpChunkedStream.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.ManagedHttpListener.Streams;

internal class HttpChunkedStream : Stream {
    private Stream _stream;
    int written = 0;
    bool innerStreamReturnedZero = false;

    const int CHUNKED_MAX_SIZE = 4096;
    static readonly byte [] CrLf = [ 0x0D, 0x0A ];

    public HttpChunkedStream ( Stream stream ) {
        this._stream = stream;
    }

    public override bool CanRead => this._stream.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException ();

    public override long Position { get => this.written; set => throw new NotSupportedException (); }

    public override void Flush () {
        this._stream.Flush ();
    }

    public override int Read ( byte [] buffer, int offset, int count ) {

        if (this.innerStreamReturnedZero)
            return 0;

        Span<byte> readBuffer = stackalloc byte [ Math.Min ( count - 2, CHUNKED_MAX_SIZE ) ];
        int read = this._stream.Read ( readBuffer );
        byte [] readBytesEncoded = Encoding.ASCII.GetBytes ( $"{read:x}\r\n" );

        if (read == 0) {
            this.innerStreamReturnedZero = true;
        }

        Array.Copy (
            sourceArray: readBytesEncoded,
            sourceIndex: 0,
            destinationArray: buffer,
            destinationIndex: 0,
            length: readBytesEncoded.Length );

        int copyStart = readBytesEncoded.Length;

        readBuffer [ 0..read ].CopyTo ( buffer.AsSpan () [ copyStart.. ] );

        int bufferEnd = readBytesEncoded.Length + read;
        buffer [ bufferEnd + 0 ] = 0x0D;
        buffer [ bufferEnd + 1 ] = 0x0A;

        this.written += read;

        return bufferEnd + 2;
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
}
