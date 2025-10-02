// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IniSection.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Sisk.IniConfiguration.Core.Serialization;

namespace Sisk.IniConfiguration.Core;

/// <summary>
/// Represents an INI section, which contains it's own properties.
/// </summary>
public sealed class IniSection : IDictionary<string, string []>, IEquatable<IniSection> {
    internal List<KeyValuePair<string, string>> items;

    /// <summary>
    /// Gets the INI section name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IniSection"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the INI section.</param>
    public IniSection ( string name ) {
        ArgumentException.ThrowIfNullOrEmpty ( name, nameof ( name ) );
        items = new List<KeyValuePair<string, string>> ();
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IniSection"/> class with the specified name and items.
    /// </summary>
    /// <param name="name">The name of the INI section.</param>
    /// <param name="items">A collection of key-value pairs to be added to the section.</param>
    public IniSection ( string name, IEnumerable<KeyValuePair<string, string>> items ) {
        ArgumentException.ThrowIfNullOrEmpty ( name, nameof ( name ) );
        ArgumentNullException.ThrowIfNull ( items, nameof ( items ) );

        this.items = items.ToList ();
        Name = name;
    }

    /// <summary>
    /// Gets all values associated with the specified property name, performing an case-insensitive search.
    /// </summary>
    /// <param name="key">The property name.</param>
    public string [] this [ string key ] {
        get {
            return items
                .Where ( k => IniReader.IniNamingComparer.Compare ( key, k.Key ) == 0 )
                .Select ( k => k.Value )
                .ToArray ();
        }
        set {
            Remove ( key );
            Add ( key, value );
        }
    }

    /// <summary>
    /// Gets all keys defined in this INI section, without duplicates.
    /// </summary>
    public ICollection<string> Keys {
        get {
            return items
                .Select ( i => i.Key )
                .Distinct ( IniReader.IniNamingComparer )
                .ToArray ();
        }
    }

    /// <summary>
    /// Gets all values defined in this INI section.
    /// </summary>
    public ICollection<string []> Values {
        get {
            List<string []> values = new List<string []> ( Count );
            using (var e = GetEnumerator ()) {
                while (e.MoveNext ()) {
                    values.Add ( e.Current.Value );
                }
            }
            return values.ToArray ();
        }
    }

    /// <summary>
    /// Gets the number of properties in this INI section.
    /// </summary>
    public int Count => items.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets the last value defined in this INI section by their property name.
    /// </summary>
    /// <param name="key">The property name.</param>
    /// <returns>The last value associated with the specified property name, or null if nothing is found.</returns>
    public string? GetOne ( string key ) {
        return items
            .Where ( k => IniReader.IniNamingComparer.Compare ( key, k.Key ) == 0 )
            .Select ( k => k.Value )
            .LastOrDefault ();
    }

    /// <summary>
    /// Gets all values defined in this INI section by their property name.
    /// </summary>
    /// <param name="key">The property name.</param>
    /// <returns>All values associated with the specified property name.</returns>
    public string [] GetMany ( string key ) {
        return this [ key ];
    }

    /// <summary>
    /// Gets an boolean indicating if the specified key/property name is
    /// defined in this <see cref="IniSection"/>.
    /// </summary>
    /// <param name="key">The property name.</param>
    /// <returns>An <see cref="bool"/> indicating if the specified property name is defined or not.</returns>
    public bool ContainsKey ( string key ) {
        for (int i = 0; i < items.Count; i++) {
            var item = items [ i ];

            if (IniReader.IniNamingComparer.Compare ( item.Key, key ) == 0)
                return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string []>> GetEnumerator () {
        string [] keysDistinct = items
            .Select ( i => i.Key )
            .Distinct ( IniReader.IniNamingComparer )
            .ToArray ();

        foreach (string key in keysDistinct) {
            string [] valuesByKey = items
                .Where ( i => IniReader.IniNamingComparer.Equals ( i.Key, key ) )
                .Select ( i => i.Value )
                .ToArray ();

            yield return new KeyValuePair<string, string []> ( key, valuesByKey );
        }
    }

    /// <inheritdoc/>
    public bool TryGetValue ( string key, [MaybeNullWhen ( false )] out string [] value ) {
        value = this [ key ];
        return value.Length > 0;
    }

    IEnumerator IEnumerable.GetEnumerator () {
        return GetEnumerator ();
    }

    /// <inheritdoc/>
    public void Add ( string key, string [] value ) {
        for (int i = 0; i < value.Length; i++) {
            string? val = value [ i ];
            items.Add ( new KeyValuePair<string, string> ( key, val ) );
        }
    }

    /// <summary>
    /// Adds a new key-value pair to the INI section.
    /// </summary>
    /// <param name="key">The key to be added.</param>
    /// <param name="value">The value associated with the key, or <c>null</c> to set an empty value.</param>
    public void Add ( string key, string? value ) {
        items.Add ( new KeyValuePair<string, string> ( key, value ?? string.Empty ) );
    }

    /// <inheritdoc/>
    public bool Remove ( string key ) {
        for (int i = 0; i < items.Count; i++) {
            var item = items [ i ];

            if (IniReader.IniNamingComparer.Compare ( item.Key, key ) == 0) {
                items.RemoveAt ( i );
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc/>
    public void Add ( KeyValuePair<string, string []> item ) {
        Add ( item.Key, item.Value );
    }

    /// <inheritdoc/>
    public void Clear () {
        items.Clear ();
    }

    /// <inheritdoc/>
    public bool Contains ( KeyValuePair<string, string []> item ) {
        for (int i = 0; i < items.Count; i++) {
            var current = items [ i ];

            if (IniReader.IniNamingComparer.Compare ( current.Key, item.Key ) == 0 &&
                item.Value.Any ( i => i == current.Value ))
                return true;
        }
        return false;
    }

    /// <summary>
    /// This method is not supported and will throw an <see cref="NotSupportedException"/>.
    /// </summary>
    public void CopyTo ( KeyValuePair<string, string []> [] array, int arrayIndex ) {
        throw new NotSupportedException ();
    }

    /// <inheritdoc/>
    public bool Remove ( KeyValuePair<string, string []> item ) {
        for (int i = 0; i < items.Count; i++) {
            var current = items [ i ];

            if (IniReader.IniNamingComparer.Compare ( current.Key, item.Key ) == 0 &&
                item.Value.Any ( i => i == current.Value )) {
                items.RemoveAt ( i );
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode () {
        int hash = IniReader.IniNamingComparer.GetHashCode ( Name );

        for (int i = 0; i < items.Count; i++) {
            KeyValuePair<string, string> item = items [ i ];

            hash ^= HashCode.Combine (
                IniReader.IniNamingComparer.GetHashCode ( item.Key ),
                IniReader.IniNamingComparer.GetHashCode ( item.Value ) );
        }

        return hash;
    }

    /// <inheritdoc/>
    public override bool Equals ( object? obj ) {
        if (obj is IniSection iniSection) {
            return Equals ( iniSection );
        }
        return false;
    }

    /// <inheritdoc/>
    public bool Equals ( IniSection? other ) {
        return GetHashCode () == other?.GetHashCode ();
    }
}
