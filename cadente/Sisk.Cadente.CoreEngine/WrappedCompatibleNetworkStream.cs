// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   WrappedCompatibleNetworkStream.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Runtime.CompilerServices;

namespace Sisk.Cadente.CoreEngine;

sealed class WrappedCompatibleNetworkStream ( Stream inner ) : Stream {

    private Stream _s = inner;

    public override bool CanRead => _s.CanRead;

    public override bool CanSeek => _s.CanSeek;

    public override bool CanWrite => _s.CanWrite;

    public override long Length => _s.Length;

    public override long Position { get => _s.Position; set => _s.Position = value; }

    public override void Flush () {
        try {
            _s.Flush ();
        }
        catch (IOException ioex) {
            throw GetWrappedException ( ioex, 10 );
        }
    }

    public override int Read ( byte [] buffer, int offset, int count ) {
        try {
            return _s.Read ( buffer, offset, count );
        }
        catch (IOException ioex) {
            throw GetWrappedException ( ioex, 11 );
        }
    }

    public override long Seek ( long offset, SeekOrigin origin ) {
        try {
            return _s.Seek ( offset, origin );
        }
        catch (IOException ioex) {
            throw GetWrappedException ( ioex, 12 );
        }
    }

    public override void SetLength ( long value ) {
        try {
            _s.SetLength ( value );
        }
        catch (IOException ioex) {
            throw GetWrappedException ( ioex, 12 );
        }
    }

    public override void Write ( byte [] buffer, int offset, int count ) {
        try {
            _s.Write ( buffer, offset, count );
        }
        catch (IOException ioex) {
            throw GetWrappedException ( ioex, 12 );
        }
    }

    protected override void Dispose ( bool disposing ) {
        _s.Dispose ();
    }

    static Exception GetWrappedException ( Exception ex, int state ) {
        var httpsysException = new HttpListenerException ( 100 + state, ex.Message );
        ref Exception? inner = ref GetInnerExceptionPrivateField ( httpsysException );
        inner = ex;

        return httpsysException;
    }

    [UnsafeAccessor ( UnsafeAccessorKind.Field, Name = "_innerException" )]
    extern static ref Exception? GetInnerExceptionPrivateField ( Exception ex );
}
