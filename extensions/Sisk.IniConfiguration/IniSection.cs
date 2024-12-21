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
using Sisk.IniConfiguration.Serializer;

namespace Sisk.IniConfiguration;

/// <summary>
/// Represents an INI section, which contains it's own properties.
/// </summary>
public sealed class IniSection : IReadOnlyDictionary<string, string []> {
    internal (string, string) [] items;

    /// <summary>
    /// Gets the INI section name.
    /// </summary>
    public string Name { get; }

    internal static IniSection [] MergeIniSections ( IniSection [] sections ) {
        var sectionNames = sections
            .DistinctBy ( s => s.Name, IniReader.IniNamingComparer )
            .Select ( s => s.Name )
            .ToArray ();

        List<IniSection> result = new List<IniSection> ( sectionNames.Length );
        for (int i = 0; i < sectionNames.Length; i++) {
            string currentName = sectionNames [ i ];
            List<(string, string)> allProperties = new ();

            for (int j = 0; j < sections.Length; j++) {
                IniSection s = sections [ j ];
                if (IniReader.IniNamingComparer.Compare ( s.Name, currentName ) == 0) {
                    allProperties.AddRange ( s.items );
                }
            }

            result.Add ( new IniSection ( currentName, allProperties.ToArray () ) );
        }

        return result.ToArray ();
    }

    internal IniSection ( string name, (string, string) [] items ) {
        this.items = items;
        this.Name = name;
    }

    /// <summary>
    /// Gets all values associated with the specified property name, performing an case-insensitive search.
    /// </summary>
    /// <param name="key">The property name.</param>
    public string [] this [ string key ] {
        get {
            return this.items
                .Where ( k => IniReader.IniNamingComparer.Compare ( key, k.Item1 ) == 0 )
                .Select ( k => k.Item2 )
                .ToArray ();
        }
    }

    /// <summary>
    /// Gets all keys defined in this INI section, without duplicates.
    /// </summary>
    public IEnumerable<string> Keys {
        get {
            return this.items.Select ( i => i.Item1 ).Distinct ().ToArray ();
        }
    }

    /// <summary>
    /// Gets all values defined in this INI section.
    /// </summary>
    public IEnumerable<string []> Values {
        get {
            using (var e = this.GetEnumerator ()) {
                while (e.MoveNext ()) {
                    yield return e.Current.Value;
                }
            }
        }
    }

    /// <summary>
    /// Gets the number of properties in this INI section.
    /// </summary>
    public int Count => this.items.Length;

    /// <summary>
    /// Gets the last value defined in this INI section by their property name.
    /// </summary>
    /// <param name="key">The property name.</param>
    /// <returns>The last value associated with the specified property name, or null if nothing is found.</returns>
    public string? GetOne ( string key ) {
        return this.items
            .Where ( k => IniReader.IniNamingComparer.Compare ( key, k.Item1 ) == 0 )
            .Select ( k => k.Item2 )
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
        for (int i = 0; i < this.items.Length; i++) {
            (string, string?) item = this.items [ i ];

            if (IniReader.IniNamingComparer.Compare ( item.Item1, key ) == 0)
                return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string []>> GetEnumerator () {
        string [] keysDistinct = this.items.Select ( i => i.Item1 ).Distinct ().ToArray ();

        foreach (string key in keysDistinct) {
            string [] valuesByKey = this.items
                .Where ( i => i.Item1 == key )
                .Select ( i => i.Item2 )
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
        return this.GetEnumerator ();
    }
}
