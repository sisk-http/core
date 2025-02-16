using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Sisk.IniConfiguration.Core.Serialization;

namespace Sisk.IniConfiguration.Core;

/// <summary>
/// Represents an INI section, which contains it's own properties.
/// </summary>
public sealed class IniSection : IDictionary<string, string []> {
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
        this.items = new List<KeyValuePair<string, string>> ();
        this.Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IniSection"/> class with the specified name and items.
    /// </summary>
    /// <param name="name">The name of the INI section.</param>
    /// <param name="items">A collection of key-value pairs to be added to the section.</param>
    public IniSection ( string name, IEnumerable<KeyValuePair<string, string>> items ) {
        this.items = items.ToList ();
        this.Name = name;
    }

    /// <summary>
    /// Gets all values associated with the specified property name, performing an case-insensitive search.
    /// </summary>
    /// <param name="key">The property name.</param>
    public string [] this [ string key ] {
        get {
            return this.items
                .Where ( k => IniReader.IniNamingComparer.Compare ( key, k.Key ) == 0 )
                .Select ( k => k.Value )
                .ToArray ();
        }
    }

    /// <summary>
    /// Gets all keys defined in this INI section, without duplicates.
    /// </summary>
    public ICollection<string> Keys {
        get {
            return this.items.Select ( i => i.Key ).Distinct ().ToArray ();
        }
    }

    /// <summary>
    /// Gets all values defined in this INI section.
    /// </summary>
    public ICollection<string []> Values {
        get {
            List<string []> values = new List<string []> ( this.Count );
            using (var e = this.GetEnumerator ()) {
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
    public int Count => this.items.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    string [] IDictionary<string, string []>.this [ string key ] {
        get => this.GetMany ( key );
        set {
            this.Remove ( key );
            this.Add ( key, value );
        }
    }

    /// <summary>
    /// Gets the last value defined in this INI section by their property name.
    /// </summary>
    /// <param name="key">The property name.</param>
    /// <returns>The last value associated with the specified property name, or null if nothing is found.</returns>
    public string? GetOne ( string key ) {
        return this.items
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
        for (int i = 0; i < this.items.Count; i++) {
            var item = this.items [ i ];

            if (IniReader.IniNamingComparer.Compare ( item.Key, key ) == 0)
                return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string []>> GetEnumerator () {
        string [] keysDistinct = this.items.Select ( i => i.Key ).Distinct ().ToArray ();

        foreach (string key in keysDistinct) {
            string [] valuesByKey = this.items
                .Where ( i => i.Key == key )
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
        return this.GetEnumerator ();
    }

    /// <inheritdoc/>
    public void Add ( string key, string [] value ) {
        foreach (var val in value)
            this.items.Add ( new KeyValuePair<string, string> ( key, val ) );
    }

    /// <summary>
    /// Adds a new key-value pair to the INI section.
    /// </summary>
    /// <param name="key">The key to be added.</param>
    /// <param name="value">The value associated with the key, or <c>null</c> to set an empty value.</param>
    public void Add ( string key, string? value ) {
        this.items.Add ( new KeyValuePair<string, string> ( key, value ?? string.Empty ) );
    }

    /// <inheritdoc/>
    public bool Remove ( string key ) {
        for (int i = 0; i < this.items.Count; i++) {
            var item = this.items [ i ];

            if (IniReader.IniNamingComparer.Compare ( item.Key, key ) == 0) {
                this.items.RemoveAt ( i );
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc/>
    public void Add ( KeyValuePair<string, string []> item ) {
        this.Add ( item.Key, item.Value );
    }

    /// <inheritdoc/>
    public void Clear () {
        this.items.Clear ();
    }

    /// <inheritdoc/>
    public bool Contains ( KeyValuePair<string, string []> item ) {
        for (int i = 0; i < this.items.Count; i++) {
            var current = this.items [ i ];

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
        for (int i = 0; i < this.items.Count; i++) {
            var current = this.items [ i ];

            if (IniReader.IniNamingComparer.Compare ( current.Key, item.Key ) == 0 &&
                item.Value.Any ( i => i == current.Value )) {
                this.items.RemoveAt ( i );
                return true;
            }
        }
        return false;
    }
}
