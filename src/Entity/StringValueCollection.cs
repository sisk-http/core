// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
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
public sealed class StringValueCollection : StringKeyStore
{
    internal string paramName;

    /// <summary>
    /// Creates an new <see cref="StringValueCollection"/> instance with values from another
    /// <see cref="IDictionary"/> instance.
    /// </summary>
    public StringValueCollection(IDictionary<string, string?> values) : base(StringComparer.InvariantCultureIgnoreCase)
    {
        this.paramName = "StringValue";
    }

    /// <summary>
    /// Creates an new empty <see cref="StringValueCollection"/>.
    /// </summary>
    public StringValueCollection() : base(StringComparer.InvariantCultureIgnoreCase)
    {
        this.paramName = "StringValue";
    }

    internal StringValueCollection(string paramName) : base(StringComparer.InvariantCultureIgnoreCase)
    {
        this.paramName = paramName;
    }

    /// <summary>
    /// Gets or sets an <see cref="StringValue"/> item by their key name.
    /// </summary>
    public new StringValue this[string name] { get => this.GetItem(name); set => this.Set(name, value.Value ?? string.Empty); }

    /// <summary>
    /// Gets an <see cref="StringValue"/> from their key name. If the object was
    /// not found by their name, an empty non-null <see cref="StringValue"/> with no value is
    /// returned.
    /// </summary>
    public StringValue GetItem(string name)
    {
        this.TryGetValue(name, out string[] value);
        return new StringValue(name, this.paramName, value.LastOrDefault());
    }

    /// <summary>
    /// Gets an array of <see cref="StringValue"/> from their key name. If the object was
    /// not found by their name, an empty array of <see cref="StringValue"/> is returned.
    /// </summary>
    public StringValue[] GetItems(string name)
    {
        this.TryGetValue(name, out string[] value);
        return value
            .Select(v => new StringValue(name, this.paramName, v))
            .ToArray();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public new IEnumerator<StringValue> GetEnumerator()
    {
        foreach (string key in this.Keys)
        {
            yield return new StringValue(key, this.paramName, this[key]);
        }
    }

    /// <summary>
    /// Tries to get the last <see cref="StringValue"/> associated with the specified key.
    /// </summary>
    /// <param name="key">The key for which to retrieve the values.</param>
    /// <param name="value">When this method returns, the <see cref="StringValue"/> containing the value, or empty <see cref="StringValue"/>.</param>
    /// <returns>
    /// <c>true</c> if the key was found; otherwise, <c>false</c>.
    /// </returns>
    public bool TryGetValue(string key, out StringValue value)
    {
        var sv = this.GetItem(key);
        value = sv;
        return !sv.IsNull;
    }
}
