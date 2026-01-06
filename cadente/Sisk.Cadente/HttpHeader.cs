// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHeader.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP header, consisting of a name and a value.
/// </summary>
public readonly struct HttpHeader : IEquatable<HttpHeader> {

    static readonly SearchValues<byte> _headerNameInvalidBytes = SearchValues.Create ( "()<>@,;:\\\"/[]?={} \t\r\n\0"u8.ToArray () );
    static readonly SearchValues<byte> _headerValueInvalidBytes = SearchValues.Create ( "\r\n\0"u8.ToArray () );

    internal readonly ReadOnlyMemory<byte> NameBytes;
    internal readonly ReadOnlyMemory<byte> ValueBytes;

    static Encoding HeaderEncoding = Encoding.UTF8;

    /// <summary>
    /// Gets a value indicating whether this <see cref="HttpHeader"/> has a empty name.
    /// </summary>
    public bool IsEmpty { get => NameBytes.IsEmpty; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHeader"/> struct with the specified name and value as byte arrays.
    /// </summary>
    /// <param name="nameBytes">The byte array representing the name of the header.</param>
    /// <param name="valueBytes">The byte array representing the value of the header.</param>
    public HttpHeader ( in ReadOnlyMemory<byte> nameBytes, in ReadOnlyMemory<byte> valueBytes ) {
        NameBytes = nameBytes;

        var trimmedRange = Ascii.Trim ( valueBytes.Span );
        ValueBytes = valueBytes [ trimmedRange ];

        ValidateHeaderBytes ();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHeader"/> struct with the specified name and value as strings.
    /// </summary>
    /// <param name="name">The name of the header.</param>
    /// <param name="value">The value of the header.</param>
    public HttpHeader ( string name, string value ) {
        NameBytes = HeaderEncoding.GetBytes ( name );
        ValueBytes = HeaderEncoding.GetBytes ( value.Trim () );

        ValidateHeaderBytes ();
    }

    void ValidateHeaderBytes () {
        if (NameBytes.IsEmpty) {
            throw new ArgumentException ( "Header name cannot be empty.", nameof ( NameBytes ) );
        }
        if (NameBytes.Span.IndexOfAny ( _headerNameInvalidBytes ) >= 0) {
            throw new ArgumentException ( "Header name contains not allowed characters.", nameof ( NameBytes ) );
        }
        if (ValueBytes.Span.IndexOfAny ( _headerValueInvalidBytes ) >= 0) {
            throw new ArgumentException ( "Header value contains not allowed characters.", nameof ( ValueBytes ) );
        }
    }

    /// <summary>
    /// Gets the name of the header as a string.
    /// </summary>
    public string Name {
        get {
            return HeaderEncoding.GetString ( NameBytes.Span );
        }
    }

    /// <summary>
    /// Gets the value of the header as a string.
    /// </summary>
    public string Value {
        get {
            return HeaderEncoding.GetString ( ValueBytes.Span );
        }
    }

    /// <summary>
    /// Gets the string representation of this <see cref="HttpHeader"/>.
    /// </summary>
    public override string ToString () {
        return $"{Name}: {Value}";
    }

    /// <inheritdoc/>
    public override bool Equals ( [NotNullWhen ( true )] object? obj ) {
        if (obj is HttpHeader other) {
            return Equals ( other );
        }
        else {
            return object.Equals ( this, obj );
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode () {
        return HashCode.Combine ( NameBytes, ValueBytes );
    }

    /// <inheritdoc/>
    public bool Equals ( HttpHeader other ) {
        return NameBytes.Span.SequenceEqual ( other.NameBytes.Span ) &&
               ValueBytes.Span.SequenceEqual ( other.ValueBytes.Span );
    }
}
