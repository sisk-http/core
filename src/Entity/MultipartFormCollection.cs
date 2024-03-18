// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MultipartFormCollection.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System.Collections;

namespace Sisk.Core.Entity;

/// <summary>
/// Represents an class which hosts an multipart form data contents.
/// </summary>
/// <definition>
/// public class MultipartFormCollection : IEnumerable{{MultipartObject}}
/// </definition>
/// <type>
/// Class
/// </type>
public class MultipartFormCollection : IEnumerable<MultipartObject>
{
    private List<MultipartObject> _items;

    internal MultipartFormCollection(List<MultipartObject> items)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
    }

    /// <summary>
    /// Reads an form item by it's name and return an string representation
    /// of it's value.
    /// </summary>
    /// <param name="name">The form item name.</param>
    /// <definition>
    /// public string? GetString(string name)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public string? GetString(string name)
    {
        return GetItem(name)?.ReadContentAsString();
    }

    /// <summary>
    /// Reads an form item by it's name.
    /// </summary>
    /// <param name="name">The form item name.</param>
    /// <definition>
    /// public MultipartObject? GetItem(string name)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public MultipartObject? GetItem(string name)
    {
        return _items
             .FirstOrDefault(i => string.Compare(name, i.Name, true) == 0);
    }

    /// <summary>
    /// Reads an form item by it's name and casts it into an <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The supported structure type which will be converted to.</typeparam>
    /// <param name="name">The form item name.</param>
    /// <param name="defaultValue">The default value if the item is not found.</param>
    /// <definition>
    /// .NET 6:
    /// public T GetItem{{T}}(string name, T defaultValue = default) where T : struct
    /// 
    /// .NET 7 and above:
    /// public T GetItem{{T}}(string name, T defaultValue = default) where T : struct, IParsable{{T}}
    /// </definition>
    /// <type>
    /// Method
    /// </type>
#if NET6_0
    public T GetItem<T>(string name, T defaultValue = default) where T : struct
#elif NET7_0_OR_GREATER
    public T GetItem<T>(string name, T defaultValue = default) where T : struct, IParsable<T>
#endif
    {
        string? value = _items
            .FirstOrDefault(i => string.Compare(name, i.Name, true) == 0)?
            .ReadContentAsString();

        if (value == null) return defaultValue;

        try
        {
#if NET6_0
            return (T)Parseable.ParseInternal<T>(value);
#elif NET7_0_OR_GREATER
            return T.Parse(value, null);
#endif
        }
        catch (InvalidCastException)
        {
            throw new InvalidCastException(string.Format(SR.HttpRequest_GetQueryValue_CastException, name, typeof(T).FullName));
        }
    }

    /// <nodocs/>
    public static implicit operator MultipartObject[](MultipartFormCollection t)
    {
        return t._items.ToArray();
    }

    /// <inheritdoc/>
    /// <nodoc/>
    public IEnumerator<MultipartObject> GetEnumerator()
    {
        return ((IEnumerable<MultipartObject>)_items).GetEnumerator();
    }

    /// <inheritdoc/>
    /// <nodoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_items).GetEnumerator();
    }
}
