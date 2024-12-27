// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestStream.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Cadente.HttpSerializer;

namespace Sisk.Cadente.Streams;

internal class HttpRequestStream : Stream {
    private Stream s;
    private HttpRequestBase baseRequest;
    int read = 0;
    int bufferedByteLength = 0;

    public HttpRequestStream ( Stream clientStream, HttpRequestBase baseRequest ) {
        this.s = clientStream;
        this.baseRequest = baseRequest;
        this.bufferedByteLength = this.baseRequest.BufferedContent.Length;
    }

    public override bool CanRead => this.s.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => this.baseRequest.ContentLength;

    public override long Position { get => this.read; set => this.s.Position = value; }

    public override void Flush () {
        this.s.Flush ();
    }

    public override int Read ( byte [] buffer, int offset, int count ) {
        if (this.read >= this.baseRequest.ContentLength) {
            return 0;
        }

        int bufferRead = this.ReadFromBuffer ( buffer, offset, count );
        if (bufferRead > 0) {
            this.read += bufferRead;
            return bufferRead;
        }
        else {
            int streamRead = this.s.Read ( buffer, offset, count );
            this.read += streamRead;
            return streamRead;
        }
    }

    int ReadFromBuffer ( byte [] buffer, int offset, int count ) {
        int requestedRead = count - offset;
        long availableRead = Math.Min ( this.bufferedByteLength, this.baseRequest.ContentLength ) - this.read;

        if (availableRead <= 0)
            return 0;

        long toRead = Math.Min ( requestedRead, availableRead );

        int bufferOffset = this.read + this.baseRequest.BufferHeaderIndex;
        Array.Copy ( this.baseRequest.BufferedContent, bufferOffset, buffer, offset, toRead );

        return (int) toRead;
    }

    public override long Seek ( long offset, SeekOrigin origin ) {
        return this.s.Seek ( offset, origin );
    }

    public override void SetLength ( long value ) {
        this.s.SetLength ( value );
    }

    public override void Write ( byte [] buffer, int offset, int count ) {
        this.s.Write ( buffer, offset, count );
    }
}
