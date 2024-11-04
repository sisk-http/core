// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   StringKeyStore.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace Sisk.Core.Entity;

/// <summary>
/// Represents a collection of string keys associated with multiple string values.
/// </summary>
public class StringKeyStore : IDictionary<string, string[]>
{
    readonly internal List<(string, List<string>)> items;
    bool isReadOnly = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringKeyStore"/> class,
    /// </summary>
    public StringKeyStore()
    {
        this.Comparer = StringComparer.CurrentCulture;
        this.items = new(64);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringKeyStore"/> class with a specified comparer.
    /// </summary>
    /// <param name="comparer">The comparer used for key equality.</param>
    public StringKeyStore(IEqualityComparer<string> comparer)
    {
        this.Comparer = comparer;
        this.items = new(64);
    }

    #region Internal methods

    internal void AddInternal(string key, IEnumerable<string> values)
    {
        for (int i = 0; i < this.items.Count; i++)
        {
            var item = this.items[i];
            if (this.Comparer.Equals(item.Item1, key))
            {
                item.Item2.AddRange(values);
                return;
            }
        }
        this.items.Add((key, new List<string>(values)));
    }

    internal void SetItemInternal(string key, string value)
    {
        this.RemoveItemInternal(key);
        this.items.Add((key, new List<string>() { value }));
    }

    internal bool RemoveItemInternal(string key)
    {
        int itemKey = this.IndexOf(key);
        if (itemKey >= 0)
        {
            this.items.RemoveAt(itemKey);
            return true;
        }
        return false;
    }

    #endregion

    #region Static methods

    /// <summary>
    /// Creates a new instance of the <see cref="StringKeyStore"/> from a query string.
    /// The query string should be in the format of "key1=value1&amp;key2=value2".
    /// </summary>
    /// <param name="queryString">The query string containing the key-value pairs to import.</param>
    /// <returns>A new <see cref="StringKeyStore"/> populated with the key-value pairs from the query string.</returns>
    public static StringKeyStore FromQueryString(string queryString)
    {
        var keyStore = new StringKeyStore(StringComparer.InvariantCultureIgnoreCase);
        keyStore.ImportQueryString(queryString);
        return keyStore;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="StringKeyStore"/> from a cookie string.
    /// The query string should be in the format of "key1=value1; key2=value2".
    /// </summary>
    /// <param name="queryString">The query string containing the key-value pairs to import.</param>
    /// <returns>A new <see cref="StringKeyStore"/> populated with the key-value pairs from the query string.</returns>
    public static StringKeyStore FromCookieString(string queryString)
    {
        var keyStore = new StringKeyStore(StringComparer.InvariantCultureIgnoreCase);
        keyStore.ImportCookieString(queryString);
        return keyStore;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="StringKeyStore"/> from a <see cref="NameValueCollection"/>.
    /// </summary>
    /// <param name="collection">The <see cref="NameValueCollection"/> containing the key-value pairs to import.</param>
    /// <returns>A new <see cref="StringKeyStore"/> populated with the key-value pairs from the query string.</returns>
    public static StringKeyStore FromNameValueCollection(NameValueCollection collection)
    {
        var keyStore = new StringKeyStore(StringComparer.InvariantCultureIgnoreCase);
        keyStore.ImportNameValueCollection(collection);
        return keyStore;
    }
    #endregion

    #region Import methods

    /// <summary>
    /// Imports key-value pairs from a <see cref="NameValueCollection"/> into the <see cref="StringKeyStore"/>.
    /// Each key can have multiple associated values.
    /// </summary>
    /// <param name="items">The <see cref="NameValueCollection"/> containing the key-value pairs to import.</param>
    public void ImportNameValueCollection(NameValueCollection items)
    {
        this.ThrowIfReadOnly();
        foreach (string headerName in items)
        {
            string[]? values = items.GetValues(headerName);
            if (values is not null)
            {
                this.AddInternal(headerName, values);
            }
        }
    }

    /// <summary>
    /// Imports key-value pairs from a query string into the <see cref="StringKeyStore"/>.
    /// The query string should be in the format of "key1=value1&amp;key2=value2".
    /// </summary>
    /// <param name="queryString">The query string containing the key-value pairs to import.</param>
    public void ImportQueryString(string queryString)
    {
        this.ThrowIfReadOnly();
        this.ParseQueryString(queryString, SharedChars.Amp, SharedChars.Equal);
    }

    /// <summary>
    /// Imports key-value pairs from a cookie string into the <see cref="StringKeyStore"/>.
    /// The query string should be in the format of "key1=value1; key2=value2".
    /// </summary>
    /// <param name="queryString">The query string containing the key-value pairs to import.</param>
    public void ImportCookieString(string queryString)
    {
        this.ThrowIfReadOnly();
        this.ParseQueryString(queryString, SharedChars.Semicolon, SharedChars.Equal);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the <see cref="IEqualityComparer{T}"/> used to compare
    /// keys in this <see cref="StringKeyStore"/>.
    /// </summary>
    public IEqualityComparer<string> Comparer { get; }

    /// <summary>
    /// Gets or sets the array of values associated with the specified key.
    /// Returns <c>null</c> if the key does not exist in the store.
    /// </summary>
    public string? this[string key]
    {
        get
        {
            var values = this.GetValues(key).ToArray();
            return values.Length switch
            {
                0 => null,
                1 => values[0],
                _ => string.Join(", ", values)
            };
        }
        set
        {
            if (value != null)
                this.Set(key, value);
        }
    }

    /// <summary>
    /// Gets the collection of keys in the <see cref="StringKeyStore"/>.
    /// </summary>
    public ICollection<string> Keys => this.items.Select(i => i.Item1).ToArray();

    /// <summary>
    /// Gets the collection of values in the <see cref="StringKeyStore"/> as arrays.
    /// Each key may have multiple associated values.
    /// </summary>
    public ICollection<string[]> Values => this.items.Select(v => v.Item2.ToArray()).ToArray();

    /// <summary>
    /// Gets the number of key-value pairs in the <see cref="StringKeyStore"/>.
    /// </summary>
    public int Count => this.items.Count;

    /// <summary>
    /// Gets a value indicating whether the <see cref="StringKeyStore"/> is read-only.
    /// </summary>
    public bool IsReadOnly => this.isReadOnly;

    #endregion

    #region Add/Setters

    /// <summary>
    /// Adds an array of values associated with the specified key.
    /// </summary>
    /// <param name="key">The key to which the values will be added.</param>
    /// <param name="value">The array of values to associate with the key.</param>
    public void Add(string key, string[] value)
    {
        this.ThrowIfReadOnly();
        this.AddInternal(key, value);
    }

    /// <summary>
    /// Adds a collection of values associated with the specified key.
    /// </summary>
    /// <param name="key">The key to which the values will be added.</param>
    /// <param name="value">The collection of values to associate with the key.</param>
    public void Add(string key, IEnumerable<string> value)
    {
        this.ThrowIfReadOnly();
        this.AddInternal(key, value);
    }

    /// <summary>
    /// Adds a single value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to which the value will be added.</param>
    /// <param name="value">The value to associate with the key.</param>
    public void Add(string key, string value)
    {
        this.ThrowIfReadOnly();
        this.AddInternal(key, new string[1] { value });
    }

    /// <summary>
    /// Adds a key-value pair to the <see cref="StringKeyStore"/>.
    /// </summary>
    /// <param name="item">The key-value pair to add, where the key is associated with an array of values.</param>
    public void Add(KeyValuePair<string, string[]> item)
    {
        this.ThrowIfReadOnly();
        this.AddInternal(item.Key, item.Value);
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of this collection.
    /// </summary>
    /// <param name="items">The collection whose items should be added to the end of this collection.</param>
    public void AddRange(IEnumerable<KeyValuePair<string, string[]>> items)
    {
        foreach (KeyValuePair<string, string[]> item in items)
            this.Add(item);
    }

    /// <summary>
    /// Sets the elements of the specified collection, replacing existing values.
    /// </summary>
    /// <param name="items">The collection whose items should be replaced or added to this collection.</param>
    public void SetRange(IEnumerable<KeyValuePair<string, string[]>> items)
    {
        foreach (KeyValuePair<string, string[]> item in items)
            this.Set(item);
    }

    /// <summary>
    /// Sets the value associated with the specified key, replacing any existing values.
    /// </summary>
    /// <param name="item">The key-value pair to add, where the key is associated with an array of values.</param>
    public void Set(KeyValuePair<string, string[]> item) => this.Set(item.Key, item.Value);

    /// <summary>
    /// Sets the value associated with the specified key, replacing any existing values.
    /// </summary>
    /// <param name="key">The key for which to set the value.</param>
    /// <param name="value">The value to associate with the key.</param>
    public void Set(string key, string value) => this.Set(key, [value]);

    /// <summary>
    /// Sets the collection of values associated with the specified key, replacing any existing values.
    /// </summary>
    /// <param name="key">The key for which to set the values.</param>
    /// <param name="value">The collection of values to associate with the key.</param>
    public void Set(string key, IEnumerable<string> value)
    {
        this.Remove(key);
        this.Add(key, value);
    }

    #endregion

    #region Getters
    /// <summary>
    /// Retrieves the last value associated with the specified key.
    /// Returns <c>null</c> if the key does not exist.
    /// </summary>
    /// <param name="name">The key for which to retrieve the value.</param>
    /// <returns>
    /// The last value associated with the specified key, or <c>null</c> if the key is not found.
    /// </returns>
    public string? GetValue(string name)
    {
        if (this.TryGetValue(name, out var values))
        {
            return values.LastOrDefault();
        }
        return null;
    }

    /// <summary>
    /// Retrieves all values associated with the specified key.
    /// Returns an empty array if the key does not exist.
    /// </summary>
    /// <param name="name">The key for which to retrieve the values.</param>
    /// <returns>
    /// An array of values associated with the specified key, or an empty array if the key is not found.
    /// </returns>
    public string[] GetValues(string name)
    {
        if (this.TryGetValue(name, out var values))
        {
            return values.ToArray();
        }
        return Array.Empty<string>();
    }

    #endregion

    #region Removers

    /// <summary>
    /// Removes all key-value pairs from the <see cref="StringKeyStore"/>.
    /// Throws an exception if the store is read-only.
    /// </summary>
    public void Clear()
    {
        this.ThrowIfReadOnly();
        this.items.Clear();
    }

    /// <summary>
    /// Removes the value associated with the specified key from the <see cref="StringKeyStore"/>.
    /// Throws an exception if the store is read-only.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    /// <returns>
    /// <c>true</c> if the key was successfully removed; otherwise, <c>false</c>.
    /// </returns>
    public bool Remove(string key)
    {
        this.ThrowIfReadOnly();
        return this.RemoveItemInternal(key);
    }

    #endregion

    #region Other methods
    /// <summary>
    /// Marks the <see cref="StringKeyStore"/> as read-only, preventing further modifications.
    /// </summary>
    public void MakeReadOnly()
    {
        this.isReadOnly = true;
    }

    /// <summary>
    /// Determines whether the <see cref="StringKeyStore"/> contains a specific key.
    /// </summary>
    /// <param name="key">The key to locate in the <see cref="StringKeyStore"/>.</param>
    /// <returns>
    /// <c>true</c> if the <see cref="StringKeyStore"/> contains an element with the specified key; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsKey(string key)
    {
        return this.IndexOf(key) >= 0;
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
    {
        foreach (var item in this.items)
            yield return new KeyValuePair<string, string[]>(item.Item1, item.Item2.ToArray());
    }

    /// <summary>
    /// Tries to get the array of values associated with the specified key.
    /// </summary>
    /// <param name="key">The key for which to retrieve the values.</param>
    /// <param name="value">When this method returns, contains the array of values associated with the specified key, or an empty array if the key is not found.</param>
    /// <returns>
    /// <c>true</c> if the key was found and the values were retrieved; otherwise, <c>false</c>.
    /// </returns>
    public bool TryGetValue(string key, out string[] value)
    {
        for (int i = 0; i < this.items.Count; i++)
        {
            var item = this.items[i];
            if (this.Comparer.Equals(item.Item1, key))
            {
                value = item.Item2.ToArray();
                return true;
            }
        }
        value = Array.Empty<string>();
        return false;
    }

    /// <summary>
    /// Copies the contents of this <see cref="StringKeyStore"/> into an <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    public IDictionary<string, string[]> AsDictionary()
    {
        Dictionary<string, string[]> dict = new Dictionary<string, string[]>(this.Comparer);
        for (int i = 0; i < this.items.Count; i++)
        {
            var item = this.items[i];
            dict.Add(item.Item1, item.Item2.ToArray());
        }
        return dict;
    }

    /// <summary>
    /// Copies the contents of this <see cref="StringKeyStore"/> into an
    /// <see cref="NameValueCollection"/>, with values separated with an comma (,).
    /// </summary>
    public NameValueCollection AsNameValueCollection()
    {
        NameValueCollection nm = new NameValueCollection();
        for (int i = 0; i < this.items.Count; i++)
        {
            var item = this.items[i];
            nm.Add(item.Item1, string.Join(", ", item.Item2));
        }
        return nm;
    }
    #endregion

    #region Overrides/operators
    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in this)
        {
            sb.AppendLine($"{item.Key}: {string.Join(", ", item.Value)}");
        }
        return sb.ToString();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public static implicit operator Dictionary<string, string?>(StringKeyStore vcol)
    {
        return (Dictionary<string, string?>)vcol.AsDictionary();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public static implicit operator NameValueCollection(StringKeyStore vcol)
    {
        return vcol.AsNameValueCollection();
    }
    #endregion

    #region Private/interface methods

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    void ICollection<KeyValuePair<string, string[]>>.CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, string[]>>)this.items).CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<string, string[]>>.Contains(KeyValuePair<string, string[]> item)
    {
        if (this.TryGetValue(item.Key, out var value))
        {
            return item.Value.All(value.Contains);
        }
        return false;
    }

    bool ICollection<KeyValuePair<string, string[]>>.Remove(KeyValuePair<string, string[]> item)
    {
        return this.Remove(item.Key);
    }

    void ThrowIfReadOnly()
    {
        if (this.isReadOnly)
            throw new InvalidOperationException(SR.Collection_ReadOnly);
    }

    string[] IDictionary<string, string[]>.this[string key] { get => this.GetValues(key); set => this.Set(key, value); }

    int IndexOf(string key)
    {
        for (int i = 0; i < this.items.Count; i++)
        {
            var item = this.items[i];
            if (this.Comparer.Equals(item.Item1, key))
                return i;
        }
        return -1;
    }

    void ParseQueryString(string queryString, char pairSeparator, char valueSeparator)
    {
        if (!string.IsNullOrWhiteSpace(queryString))
        {
            string[] kvPairs = queryString.Split(pairSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < kvPairs.Length; i++)
            {
                string part = kvPairs[i];

                int eqPos = part.IndexOf(valueSeparator, StringComparison.Ordinal);
                if (eqPos < 0)
                {
                    this.AddInternal(part, new string[1] { string.Empty });
                    continue;
                }
                else
                {
                    string key = part.Substring(0, eqPos).Trim();
                    string value = part.Substring(eqPos + 1).Trim();

                    if (string.IsNullOrWhiteSpace(key))
                    {
                        // provided an name/value pair, but no name
                        continue;
                    }

                    this.AddInternal(key, new string[1] { WebUtility.UrlDecode(value) });
                }
            }
        }
    }

    #endregion
}
