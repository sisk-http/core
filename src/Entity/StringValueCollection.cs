﻿// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   StringValueCollection.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;

namespace Sisk.Core.Entity;

/// <summary>
/// Represents an collection of <see cref="StringValue"/>.
/// </summary>
public sealed class StringValueCollection : StringKeyStoreCollection {
    internal string paramName;

    /// <summary>
    /// Creates an new <see cref="StringValueCollection"/> instance with values from another
    /// <see cref="IDictionary"/> instance.
    /// </summary>
    public StringValueCollection ( IDictionary<string, string?> values ) : base ( StringComparer.InvariantCultureIgnoreCase ) {
        base.AddRange ( values );
        paramName = "StringValue";
    }

    /// <summary>
    /// Creates an new <see cref="StringValueCollection"/> instance with values from another
    /// <see cref="IDictionary"/> instance.
    /// </summary>
    public StringValueCollection ( IDictionary<string, string []> values ) : base ( StringComparer.InvariantCultureIgnoreCase ) {
        base.AddRange ( values );
        paramName = "StringValue";
    }

    /// <summary>
    /// Creates an new empty <see cref="StringValueCollection"/>.
    /// </summary>
    public StringValueCollection () : base ( StringComparer.InvariantCultureIgnoreCase ) {
        paramName = "StringValue";
    }

    internal StringValueCollection ( string paramName ) : base ( StringComparer.InvariantCultureIgnoreCase ) {
        this.paramName = paramName;
    }

    /// <summary>
    /// Gets or sets an <see cref="StringValue"/> item by their key name.
    /// </summary>
    public new StringValue this [ string name ] { get => GetItem ( name ); set => Set ( name, value.Value ?? string.Empty ); }

    /// <summary>
    /// Gets an <see cref="StringValue"/> from their key name. If the object was
    /// not found by their name, an empty non-null <see cref="StringValue"/> with no value is
    /// returned.
    /// </summary>
    public StringValue GetItem ( string name ) {
        TryGetValue ( name, out string [] value );
        return new StringValue ( name, paramName, value.LastOrDefault () );
    }

    /// <summary>
    /// Gets an array of <see cref="StringValue"/> from their key name. If the object was
    /// not found by their name, an empty array of <see cref="StringValue"/> is returned.
    /// </summary>
    public StringValue [] GetItems ( string name ) {
        TryGetValue ( name, out string [] value );
        return value
            .Select ( v => new StringValue ( name, paramName, v ) )
            .ToArray ();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public new IEnumerator<StringValue> GetEnumerator () {
        foreach (string key in Keys) {
            yield return new StringValue ( key, paramName, this [ key ] );
        }
    }

    /// <summary>
    /// Tries to get the last <see cref="StringValue"/> associated with the specified key.
    /// </summary>
    /// <param name="key">The key for which to retrieve the values.</param>
    /// <param name="value">When this method returns, the <see cref="StringValue"/> containing the value, or empty <see cref="StringValue"/>.</param>
    /// <returns>
    /// <see langword="true"></see> if the key was found; otherwise, <see langword="false"></see>.
    /// </returns>
    public bool TryGetValue ( string key, out StringValue value ) {
        var sv = GetItem ( key );
        value = sv;
        return !sv.IsNull;
    }
}
