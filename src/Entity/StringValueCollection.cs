// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   StringValueCollection.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Sisk.Core.Entity;

/// <summary>
/// Represents an collection of <see cref="StringValue"/>.
/// </summary>
public sealed class StringValueCollection : IEnumerable<StringValue>, IEnumerable<KeyValuePair<string, string>>, IReadOnlyDictionary<string, StringValue>
{
    internal Dictionary<string, string?> items;
    internal string paramName;

    /// <summary>
    /// Represents an empty <see cref="StringValueCollection"/> field.
    /// </summary>
    public static readonly StringValueCollection Empty = new StringValueCollection("empty");

    internal static StringValueCollection FromNameValueCollection(string paramName, NameValueCollection col)
    {
        StringValueCollection vcol = new StringValueCollection(paramName);

        for (int i = 0; i < col.Keys.Count; i++)
        {
            string? keyValue = col.Keys[i];
            string? value;

            if (keyValue is null)
            {
                value = col[i];
                if (value is null) continue;
                vcol.items.Add(value, string.Empty);
            }
            else
            {
                value = col[keyValue];
                vcol.items.Add(keyValue, value);
            }
        }
        return vcol;
    }

    internal StringValueCollection(string paramName)
    {
        items = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        this.paramName = paramName;
    }

    internal void SetItem(string key, string? value)
    {
        items[key] = value;
    }

    /// <summary>
    /// Gets an <see cref="IDictionary"/> object with the data of this <see cref="StringValueCollection"/>
    /// with their keys and values.
    /// </summary>
    public IDictionary<string, string?> AsDictionary() => items;

    /// <summary>
    /// Gets an <see cref="NameValueCollection"/> with the data of this <see cref="StringValueCollection"/>.
    /// </summary>
    public NameValueCollection AsNameValueCollection()
    {
        NameValueCollection n = new NameValueCollection();

        foreach (var item in items)
        {
            n.Add(item.Key, item.Value);
        }

        return n;
    }

    /// <summary>
    /// Gets the number of items defined in this <see cref="StringValueCollection"/>.
    /// </summary>
    public int Count { get => items.Count; }

    /// <inheritdoc/>
    /// <exclude/>
    public IEnumerable<string> Keys => items.Keys;

    /// <inheritdoc/>
    /// <exclude/>
    public IEnumerable<StringValue> Values
    {
        get
        {
            foreach (var item in items)
            {
                yield return new StringValue(item.Key, paramName, item.Value);
            }
        }
    }

    /// <summary>
    /// Gets an <see cref="StringValue"/> item by their key name.
    /// </summary>
    public StringValue this[string name] { get => GetItem(name); }

    /// <summary>
    /// Gets an <see cref="StringValue"/> from their key name. If the object was
    /// not found by their name, an empty non-null <see cref="StringValue"/> with no value is
    /// returned.
    /// </summary>
    public StringValue GetItem(string name)
    {
        items.TryGetValue(name, out string? value);
        return new StringValue(name, paramName, value);
    }

    /// <inheritdoc/>
    /// <exclude/>
    public IEnumerator<StringValue> GetEnumerator()
    {
        foreach (string key in items.Keys)
        {
            yield return new StringValue(key, paramName, items[key]);
        }
    }

    /// <inheritdoc/>
    /// <exclude/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    /// <exclude/>
    IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
    {
        return items.GetEnumerator();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool ContainsKey(string key)
    {
        return items.ContainsKey(key);
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool TryGetValue(string key, [NotNull()] out StringValue value)
    {
        var sv = GetItem(key);
        value = sv;
        return sv.IsNull;
    }

    /// <inheritdoc/>
    /// <exclude/>
    IEnumerator<KeyValuePair<string, StringValue>> IEnumerable<KeyValuePair<string, StringValue>>.GetEnumerator()
    {
        foreach (string key in items.Keys)
        {
            yield return new KeyValuePair<string, StringValue>(key, new StringValue(key, paramName, items[key]));
        }
    }

    /// <inheritdoc/>
    /// <exclude/>
    public static implicit operator Dictionary<string, string?>(StringValueCollection vcol)
    {
        return (Dictionary<string, string?>)vcol.AsDictionary();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public static implicit operator NameValueCollection(StringValueCollection vcol)
    {
        return vcol.AsNameValueCollection();
    }
}
