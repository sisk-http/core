// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MultipartFormCollection.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sisk.Core.Entity;

/// <summary>
/// Represents an class which hosts an multipart form data contents.
/// </summary>
public sealed class MultipartFormCollection : IReadOnlyList<MultipartObject>, IReadOnlyDictionary<string, MultipartObject>
{
    private readonly IList<MultipartObject> _items;

    internal MultipartFormCollection(IEnumerable<MultipartObject> items)
    {
        this._items = items.ToImmutableList() ?? throw new ArgumentNullException(nameof(items));
    }

    /// <summary>
    /// Reads an form item contents by it's name.
    /// </summary>
    /// <param name="name">The form item name.</param>
    /// <param name="ignoreCase">Optional. Determines if this method should use an case-insensitive search to find the specified item.</param>
    public MultipartObject? GetItem(string name, bool ignoreCase = false)
    {
        return this._items.FirstOrDefault(i => string.Compare(name, i.Name, ignoreCase) == 0);
    }

    /// <summary>
    /// Reads an form item contents by it's name and returns their content as string.
    /// </summary>
    /// <param name="name">The form item name.</param>
    /// <param name="ignoreCase">Optional. Determines if this method should use an case-insensitive search to find the specified item.</param>
    /// <param name="encoding">Optional. Specifies the <see cref="Encoding"/> used to read the content.</param>
    public string? GetString(string name, bool ignoreCase = false, Encoding? encoding = null)
    {
        Encoding _enc = encoding ?? Encoding.UTF8;
        return this._items.FirstOrDefault(i => string.Compare(name, i.Name, ignoreCase) == 0)?.ReadContentAsString(_enc);
    }

    /// <summary>
    /// Gets an <see cref="StringValue"/> object from the form item content string.
    /// </summary>
    /// <param name="name">The form item name.</param>
    public StringValue GetStringValue(string name)
    {
        return new StringValue(name, "multipart form", this.GetItem(name, true)?.ReadContentAsString());
    }

    /// <exclude/>
    /// <inheritdoc/>
    public MultipartObject this[int index] => ((IReadOnlyList<MultipartObject>)this._items)[index];

    /// <exclude/>
    /// <inheritdoc/>
    public MultipartObject this[string name] => this.GetItem(name, false) ?? throw new KeyNotFoundException();

    /// <inheritdoc/>
    public int Count => ((IReadOnlyCollection<MultipartObject>)this._items).Count;


    /// <inheritdoc/>
    public IEnumerable<string> Keys
    {
        get
        {
            for (int i = 0; i < this._items.Count; i++)
            {
                MultipartObject? item = this._items[i];
                yield return item.Name;
            }
        }
    }


    /// <inheritdoc/>
    public IEnumerable<MultipartObject> Values
    {
        get
        {
            for (int i = 0; i < this._items.Count; i++)
            {
                MultipartObject? item = this._items[i];
                yield return item;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerator<MultipartObject> GetEnumerator()
    {
        return ((IEnumerable<MultipartObject>)this._items).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)this._items).GetEnumerator();
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key)
    {
        return this._items.Any(i => i.Name.CompareTo(key) == 0);
    }

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out MultipartObject value)
    {
        var i = this._items.FirstOrDefault(item => item.Name.CompareTo(key) == 0);
        if (i is null)
        {
            value = default;
            return false;
        }
        else
        {
            value = i;
            return true;
        }
    }

    IEnumerator<KeyValuePair<string, MultipartObject>> IEnumerable<KeyValuePair<string, MultipartObject>>.GetEnumerator()
    {
        for (int i = 0; i < this._items.Count; i++)
        {
            MultipartObject? item = this._items[i];
            yield return new KeyValuePair<string, MultipartObject>(item.Name, item);
        }
    }

    /// <exclude/>
    /// <inheritdoc/>
    public static implicit operator MultipartObject[](MultipartFormCollection t)
    {
        return t._items.ToArray();
    }
}
