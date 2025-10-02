// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
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
public sealed class MultipartFormCollection : IReadOnlyList<MultipartObject>, IReadOnlyDictionary<string, MultipartObject>, IEnumerable<MultipartObject> {
    private readonly IList<MultipartObject> _items;

    internal MultipartFormCollection ( IEnumerable<MultipartObject> items ) {
        _items = items.ToImmutableList () ?? throw new ArgumentNullException ( nameof ( items ) );
    }

    /// <summary>
    /// Retrieves a <see cref="MultipartObject"/> instance by its file name.
    /// </summary>
    /// <param name="name">The filename of the <see cref="MultipartObject"/> to retrieve.</param>
    /// <returns>The <see cref="MultipartObject"/> instance with the specified filename, or <see langword="null"/> if no matching file is found.</returns>
    public MultipartObject? GetFile ( string name ) {
        return _items.LastOrDefault ( i => string.Equals ( name, i.Filename, StringComparison.OrdinalIgnoreCase ) );
    }

    /// <summary>
    /// Gets the last form item by their name. This search is case-insensitive.
    /// </summary>
    /// <param name="name">The form item name.</param>
    public MultipartObject? GetItem ( string name ) {
        return _items.LastOrDefault ( i => string.Equals ( name, i.Name, StringComparison.OrdinalIgnoreCase ) );
    }

    /// <summary>
    /// Gets all form items that shares the specified name. This search is case-insensitive.
    /// </summary>
    /// <param name="name">The form item name.</param>
    /// <returns>An array of <see cref="MultipartObject"/> with the specified name.</returns>
    public MultipartObject [] GetItems ( string name ) {
        return _items
            .Where ( i => string.Equals ( name, i.Name, StringComparison.OrdinalIgnoreCase ) )
            .ToArray ();
    }

    /// <summary>
    /// Gets an <see cref="StringValue"/> object from the form item
    /// content string. This method reads the contents of the last matched last item with the
    /// request encoding.
    /// </summary>
    /// <param name="name">The form item name.</param>
    public StringValue GetStringValue ( string name ) {
        return new StringValue ( name, "multipart form", GetItem ( name )?.ReadContentAsString () );
    }

    /// <exclude/>
    /// <inheritdoc/>
    public MultipartObject this [ int index ] => _items [ index ];

    /// <exclude/>
    /// <inheritdoc/>
    public MultipartObject this [ string name ] => GetItem ( name ) ?? throw new KeyNotFoundException ();

    /// <inheritdoc/>
    public int Count => _items.Count;

    /// <summary>
    /// Creates an array with the <see cref="MultipartObject"/> in this collection.
    /// </summary>
    public MultipartObject [] ToArray () {
        return _items.ToArray ();
    }

    /// <summary>
    /// Gets a collection of <see cref="MultipartObject"/> instances that represent files.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="MultipartObject"/> instances.</returns>
    public IEnumerable<MultipartObject> Files {
        get {
            for (int i = 0; i < _items.Count; i++) {
                MultipartObject? item = _items [ i ];
                if (item.IsFile) {
                    yield return item;
                }
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<string> Keys {
        get {
            for (int i = 0; i < _items.Count; i++) {
                MultipartObject? item = _items [ i ];
                yield return item.Name;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<MultipartObject> Values {
        get {
            for (int i = 0; i < _items.Count; i++) {
                MultipartObject? item = _items [ i ];
                yield return item;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerator<MultipartObject> GetEnumerator () {
        return _items.GetEnumerator ();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator () {
        return ((IEnumerable) _items).GetEnumerator ();
    }

    /// <inheritdoc/>
    public bool ContainsKey ( string key ) {
        return _items.Any ( i => i.Name.Equals ( key, StringComparison.OrdinalIgnoreCase ) );
    }

    /// <inheritdoc/>
    public bool TryGetValue ( string key, [MaybeNullWhen ( false )] out MultipartObject value ) {
        var i = _items.FirstOrDefault ( item => item.Name.Equals ( key, StringComparison.OrdinalIgnoreCase ) );
        if (i is null) {
            value = default;
            return false;
        }
        else {
            value = i;
            return true;
        }
    }

    IEnumerator<KeyValuePair<string, MultipartObject>> IEnumerable<KeyValuePair<string, MultipartObject>>.GetEnumerator () {
        for (int i = 0; i < _items.Count; i++) {
            MultipartObject? item = _items [ i ];
            yield return new KeyValuePair<string, MultipartObject> ( item.Name, item );
        }
    }

    /// <exclude/>
    /// <inheritdoc/>
    public static implicit operator MultipartObject [] ( MultipartFormCollection t ) {
        return t._items.ToArray ();
    }
}
