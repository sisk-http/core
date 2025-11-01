// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   UndisposableStream.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Cadente.Streams;

sealed class UndisposableNetworkStream ( Stream baseStream ) : Stream {

    public override bool CanRead => baseStream.CanRead;

    public override bool CanSeek => baseStream.CanSeek;

    public override bool CanWrite => baseStream.CanWrite;

    public override long Length => baseStream.Length;

    public override long Position { get => baseStream.Position; set => baseStream.Position = value; }

    public override void Flush () {
        baseStream.Flush ();
    }

    public override int Read ( byte [] buffer, int offset, int count ) {
        return baseStream.Read ( buffer, offset, count );
    }

    public override long Seek ( long offset, SeekOrigin origin ) {
        return baseStream.Seek ( offset, origin );
    }

    public override void SetLength ( long value ) {
        baseStream.SetLength ( value );
    }

    public override void Write ( byte [] buffer, int offset, int count ) {
        baseStream.Write ( buffer, offset, count );
    }
}
