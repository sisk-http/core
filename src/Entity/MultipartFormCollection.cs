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
using System.Text;

namespace Sisk.Core.Entity;

/// <summary>
/// Represents an class which hosts an multipart form data contents.
/// </summary>
public sealed class MultipartFormCollection : IEnumerable<MultipartObject>, IReadOnlyList<MultipartObject>, IReadOnlyCollection<MultipartObject>
{
    private readonly IList<MultipartObject> _items;

    internal MultipartFormCollection(IEnumerable<MultipartObject> items)
    {
        _items = items.ToImmutableList() ?? throw new ArgumentNullException(nameof(items));
    }

    /// <summary>
    /// Reads an form item contents by it's name.
    /// </summary>
    /// <param name="name">The form item name.</param>
    /// <param name="ignoreCase">Optional. Determines if this method should use an case-insensitive search to find the specified item.</param>
    public MultipartObject? GetItem(string name, bool ignoreCase = false)
    {
        return _items.FirstOrDefault(i => string.Compare(name, i.Name, ignoreCase) == 0);
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
        return _items.FirstOrDefault(i => string.Compare(name, i.Name, ignoreCase) == 0)?.ReadContentAsString(_enc);
    }

    /// <summary>
    /// Gets an <see cref="StringValue"/> object from the form item content string.
    /// </summary>
    /// <param name="name">The form item name.</param>
    public StringValue GetStringValue(string name)
    {
        return new StringValue(name, "multipart form", GetItem(name, true)?.ReadContentAsString());
    }

    /// <exclude/>
    /// <inheritdoc/>
    public MultipartObject this[int index] => ((IReadOnlyList<MultipartObject>)_items)[index];

    /// <exclude/>
    /// <inheritdoc/>
    public int Count => ((IReadOnlyCollection<MultipartObject>)_items).Count;

    /// <exclude/>
    /// <inheritdoc/>
    public IEnumerator<MultipartObject> GetEnumerator()
    {
        return ((IEnumerable<MultipartObject>)_items).GetEnumerator();
    }

    /// <exclude/>
    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_items).GetEnumerator();
    }

    /// <exclude/>
    /// <inheritdoc/>
    public static implicit operator MultipartObject[](MultipartFormCollection t)
    {
        return t._items.ToArray();
    }
}
