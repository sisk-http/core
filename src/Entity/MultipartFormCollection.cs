// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MultipartFormCollection.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

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
    /// Gets the last form item by their name. This search is case-insensitive.
    /// </summary>
    /// <param name="name">The form item name.</param>
    public MultipartObject? GetItem(string name)
    {
        return this._items.LastOrDefault(i => string.Compare(name, i.Name, true) == 0);
    }

    /// <summary>
    /// Gets all form items that shares the specified name. This search is case-insensitive.
    /// </summary>
    /// <param name="name">The form item name.</param>
    /// <returns>An array of <see cref="MultipartObject"/> with the specified name.</returns>
    public MultipartObject[] GetItems(string name)
    {
        return this._items
            .Where(i => string.Compare(name, i.Name, true) == 0)
            .ToArray();
    }

    /// <summary>
    /// Gets an <see cref="StringValue"/> object from the form item
    /// content string. This method reads the contents of the last matched last item with the
    /// request encoding.
    /// </summary>
    /// <param name="name">The form item name.</param>
    public StringValue GetStringValue(string name)
    {
        return new StringValue(name, "multipart form", this.GetItem(name)?.ReadContentAsString());
    }

    /// <exclude/>
    /// <inheritdoc/>
    public MultipartObject this[int index] => this._items[index];

    /// <exclude/>
    /// <inheritdoc/>
    public MultipartObject this[string name] => this.GetItem(name) ?? throw new KeyNotFoundException();

    /// <inheritdoc/>
    public int Count => this._items.Count;

    /// <summary>
    /// Creates an array with the <see cref="MultipartObject"/> in this collection.
    /// </summary>
    public MultipartObject[] ToArray()
    {
        return this._items.ToArray();
    }

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
