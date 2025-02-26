﻿// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHeader.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP header, consisting of a name and a value.
/// </summary>
public readonly struct HttpHeader : IEquatable<HttpHeader> {

    internal readonly ReadOnlyMemory<byte> NameBytes;
    internal readonly ReadOnlyMemory<byte> ValueBytes;

    static Encoding HeaderEncoding = Encoding.UTF8;

    /// <summary>
    /// Gets a value indicating whether this <see cref="HttpHeader"/> has any empty value or name.
    /// </summary>
    public bool IsEmpty { get => this.NameBytes.IsEmpty || this.ValueBytes.IsEmpty; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHeader"/> struct with the specified name and value as byte arrays.
    /// </summary>
    /// <param name="nameBytes">The byte array representing the name of the header.</param>
    /// <param name="valueBytes">The byte array representing the value of the header.</param>
    public HttpHeader ( in ReadOnlyMemory<byte> nameBytes, in ReadOnlyMemory<byte> valueBytes ) {
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

    /// <inheritdoc/>
    public override bool Equals ( [NotNullWhen ( true )] object? obj ) {
        if (obj is HttpHeader other) {
            return this.Equals ( other );
        }
        else {
            return object.Equals ( this, obj );
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode () {
        return HashCode.Combine ( this.NameBytes, this.ValueBytes );
    }

    /// <inheritdoc/>
    public bool Equals ( HttpHeader other ) {
        return this.NameBytes.Span.SequenceEqual ( other.NameBytes.Span ) &&
               this.ValueBytes.Span.SequenceEqual ( other.ValueBytes.Span );
    }
}
