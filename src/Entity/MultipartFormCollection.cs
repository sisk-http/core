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
/// <definition>
/// public class MultipartFormCollection : IEnumerable{{MultipartObject}}, IReadOnlyList{{MultipartObject}},
///     IReadOnlyCollection{{MultipartObject}}
/// </definition>
/// <type>
/// Class
/// </type>
public class MultipartFormCollection : IEnumerable<MultipartObject>, IReadOnlyList<MultipartObject>, IReadOnlyCollection<MultipartObject>
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
    /// <definition>
    /// public string? GetItem(string name, bool ignoreCase = false)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
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
    /// <definition>
    /// public string? GetString(string name, bool ignoreCase = false, Encoding? encoding = null)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public string? GetString(string name, bool ignoreCase = false, Encoding? encoding = null)
    {
        Encoding _enc = encoding ?? Encoding.UTF8;
        return _items.FirstOrDefault(i => string.Compare(name, i.Name, ignoreCase) == 0)?.ReadContentAsString(_enc);
    }

    /// <summary>
    /// Gets an <see cref="StringValue"/> object from the form item content string.
    /// </summary>
    /// <param name="name">The form item name.</param>
    /// <definition>
    /// public StringValue GetStringValue(string name)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public StringValue GetStringValue(string name)
    {
        return new StringValue(name, "multipart form", GetItem(name, true)?.ReadContentAsString());
    }

    /// <nodoc/>
    /// <inheritdoc/>
    public MultipartObject this[int index] => ((IReadOnlyList<MultipartObject>)_items)[index];

    /// <nodoc/>
    /// <inheritdoc/>
    public int Count => ((IReadOnlyCollection<MultipartObject>)_items).Count;

    /// <nodoc/>
    /// <inheritdoc/>
    public IEnumerator<MultipartObject> GetEnumerator()
    {
        return ((IEnumerable<MultipartObject>)_items).GetEnumerator();
    }

    /// <nodoc/>
    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_items).GetEnumerator();
    }

    /// <nodoc/>
    /// <inheritdoc/>
    public static implicit operator MultipartObject[](MultipartFormCollection t)
    {
        return t._items.ToArray();
    }
}
