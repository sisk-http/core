// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHeader.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP header, consisting of a name and a value.
/// </summary>
public readonly struct HttpHeader {

    internal readonly ReadOnlyMemory<byte> NameBytes;
    internal readonly ReadOnlyMemory<byte> ValueBytes;

    static Encoding HeaderEncoding = Encoding.UTF8;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHeader"/> struct with the specified name and value as byte arrays.
    /// </summary>
    /// <param name="nameBytes">The byte array representing the name of the header.</param>
    /// <param name="valueBytes">The byte array representing the value of the header.</param>
    public HttpHeader ( ReadOnlyMemory<byte> nameBytes, ReadOnlyMemory<byte> valueBytes ) {
        this.NameBytes = nameBytes;
        this.ValueBytes = valueBytes;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHeader"/> struct with the specified name and value as strings.
    /// </summary>
    /// <param name="name">The name of the header.</param>
    /// <param name="value">The value of the header.</param>
    public HttpHeader ( string name, string value ) {
        this.NameBytes = HeaderEncoding.GetBytes ( name );
        this.ValueBytes = HeaderEncoding.GetBytes ( value );
    }

    /// <summary>
    /// Gets the name of the header as a string.
    /// </summary>
    public string Name {
        get {
            return HeaderEncoding.GetString ( this.NameBytes.Span );
        }
    }

    /// <summary>
    /// Gets the value of the header as a string.
    /// </summary>
    public string Value {
        get {
            return HeaderEncoding.GetString ( this.ValueBytes.Span );
        }
    }

    /// <summary>
    /// Gets the string representation of this <see cref="HttpHeader"/>.
    /// </summary>
    public override string ToString () {
        return $"{this.Name}: {this.Value}";
    }
}
