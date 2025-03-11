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
        s = clientStream;
        this.baseRequest = baseRequest;
        bufferedByteLength = this.baseRequest.BufferedContent.Length;
    }

    public override bool CanRead => s.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => baseRequest.ContentLength;

    public override long Position { get => read; set => s.Position = value; }

    public override void Flush () {
        s.Flush ();
    }

    public override int Read ( byte [] buffer, int offset, int count ) {
        if (read >= baseRequest.ContentLength) {
            return 0;
        }

        int bufferRead = ReadFromBuffer ( buffer, offset, count );
        if (bufferRead > 0) {
            read += bufferRead;
            return bufferRead;
        }
        else {
            int streamRead = s.Read ( buffer, offset, count );
            read += streamRead;
            return streamRead;
        }
    }

    int ReadFromBuffer ( byte [] buffer, int offset, int count ) {
        long availableRead = Math.Min ( bufferedByteLength, baseRequest.ContentLength ) - read;

        if (availableRead <= 0)
            return 0;

        long toRead = Math.Min ( count, availableRead );

        int bufferOffset = read;

        var bufferMemory = new Memory<byte> ( buffer );
        baseRequest.BufferedContent [ bufferOffset.. ].CopyTo ( bufferMemory [ offset..(offset + (int) toRead) ] );

        return (int) toRead;
    }

    public override long Seek ( long offset, SeekOrigin origin ) {
        return s.Seek ( offset, origin );
    }

    public override void SetLength ( long value ) {
        s.SetLength ( value );
    }

    public override void Write ( byte [] buffer, int offset, int count ) {
        s.Write ( buffer, offset, count );
    }
}
