// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHeader.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.ManagedHttpListener.HttpSerializer;

public readonly struct HttpHeader {
    internal readonly ReadOnlyMemory<byte> NameBytes;
    internal readonly ReadOnlyMemory<byte> ValueBytes;

    public HttpHeader ( ReadOnlyMemory<byte> nameBytes, ReadOnlyMemory<byte> valueBytes ) {
        this.NameBytes = nameBytes;
        this.ValueBytes = valueBytes;
    }

    public HttpHeader ( string name, string value ) {
        this.NameBytes = Encoding.UTF8.GetBytes ( name );
        this.ValueBytes = Encoding.UTF8.GetBytes ( value );
    }

    public string Name {
        get {
            return Encoding.UTF8.GetString ( this.NameBytes.Span );
        }
    }

    public string Value {
        get {
            return Encoding.UTF8.GetString ( this.ValueBytes.Span );
        }
    }
}
