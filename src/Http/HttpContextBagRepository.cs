// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpContextBagRepository.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Sisk.Core.Http;

/// <summary>
/// Represents a repository of information stored over the lifetime of a request.
/// </summary>
/// <definition>
/// public class HttpContextBag : IDictionary{{string, object?}}
/// </definition>
/// <type>
/// Class
/// </type>
public class HttpContextBagRepository : IDictionary<string, object?>
{
    private readonly Dictionary<string, object?> _values = new();

    private static string GetTypeKeyName(Type t) =>
        t.Name + "+" + t.GetHashCode();

    /// <summary>
    /// Creates an new instance of the <see cref="HttpContextBagRepository"/> class.
    /// </summary>
    /// <definition>
    /// public HttpContextBagRepository()
    /// </definition>
    /// <type>
    /// Constructor
    /// </type>
    public HttpContextBagRepository()
    {

    }

    /// <summary>
    /// Gets or sets an bag item by it's key.
    /// </summary>
    /// <param name="key">The bag item key name.</param>
    public Object? this[String key] { get => _values[key]; set => _values[key] = value; }

    /// <summary>
    /// Gets an collection containing the keys in this repository.
    /// </summary>
    /// <definition>
    /// public ICollection{{String}} Keys { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public ICollection<String> Keys => _values.Keys;

    /// <summary>
    /// Gets an collection containing the values in this repository.
    /// </summary>
    /// <definition>
    /// public ICollection{{Object?}} Values { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public ICollection<Object?> Values => _values.Values;

    /// <summary>
    /// Gets the total count of items in this repository.
    /// </summary>
    /// <definition>
    /// public int Count { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public Int32 Count => _values.Count;

    /// <summary>
    /// Gets an boolean indicating if this repository is read-only. This property is always <c>false</c>.
    /// </summary>
    /// <definition>
    /// public bool IsReadOnly { get; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public Boolean IsReadOnly => false;

    /// <summary>
    /// Determines whether the specified <typeparamref name="T"/> singleton is defined in this context.
    /// </summary>
    /// <typeparam name="T">The singleton type.</typeparam>
    /// <definition>
    /// public bool IsSet{{T}}() where T : notnull
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public bool IsSet<T>() where T : notnull
    {
        return _values.ContainsKey(GetTypeKeyName(typeof(T)));
    }

    /// <summary>
    /// Removes an singleton object from it's type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The singleton type.</typeparam>
    /// <definition>
    /// public void Unset{{T}}() where T : notnull
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void Unset<T>() where T : notnull
    {
        _values.Remove(GetTypeKeyName(typeof(T)));
    }

    /// <summary>
    /// Creates and adds an singleton of <typeparamref name="T"/> in this context bag.
    /// </summary>
    /// <typeparam name="T">The object that will be defined in this context bag.</typeparam>
    /// <definition>
    /// public T Set{{T}}() where T : notnull, new()
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public T Set<T>() where T : notnull, new()
    {
        return Set<T>(new T());
    }

    /// <summary>
    /// Adds an singleton of <typeparamref name="T"/> in this context bag.
    /// </summary>
    /// <typeparam name="T">The object that will be defined in this context bag.</typeparam>
    /// <param name="value">The instance of <typeparamref name="T"/> which will be defined in this context bag.</param>
    /// <definition>
    /// public T Set{{T}}(T value) where T : notnull
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public T Set<T>(T value) where T : notnull
    {
        Type contextType = typeof(T);
        _values[GetTypeKeyName(contextType)] = value;
        return value;
    }

    /// <summary>
    /// Gets a singleton previously defined in this context bag via its type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object defined in this context bag.</typeparam>
    /// <definition>
    /// public T Get{{T}}() where T : notnull
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public T Get<T>() where T : notnull
    {
        Type contextType = typeof(T);
        string key = GetTypeKeyName(contextType);
        if (ContainsKey(key))
        {
            return (T)_values[key]!;
        }
        else
        {
            throw new ArgumentException(string.Format(SR.HttpContextBagRepository_UndefinedDynamicProperty, contextType.FullName));
        }
    }

    /// <inheritdoc />
    /// <nodoc />
    public void Add(String key, Object? value)
    {
        _values[key] = value;
    }

    /// <inheritdoc />
    /// <nodoc />
    public void Add(KeyValuePair<String, Object?> item)
    {
        _values[item.Key] = item.Value;
    }

    /// <inheritdoc />
    /// <nodoc />
    public void Clear()
    {
        _values.Clear();
    }

    /// <inheritdoc />
    /// <nodoc />
    public Boolean Contains(KeyValuePair<String, Object?> item)
    {
        return _values.ContainsKey(item.Key) && _values.ContainsValue(item.Value);
    }

    /// <inheritdoc />
    /// <nodoc />
    public Boolean ContainsKey(String key)
    {
        return _values.ContainsKey(key);
    }

    /// <summary>
    /// This method is not implemented and will throw an <see cref="NotImplementedException"/>.
    /// </summary>
    /// <nodoc />
    public void CopyTo(KeyValuePair<String, Object?>[] array, Int32 arrayIndex)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    /// <nodoc />
    public IEnumerator<KeyValuePair<String, Object?>> GetEnumerator()
    {
        return _values.GetEnumerator();
    }

    /// <inheritdoc />
    /// <nodoc />
    public Boolean Remove(String key)
    {
        return _values.Remove(key);
    }

    /// <inheritdoc />
    /// <nodoc />
    public Boolean Remove(KeyValuePair<String, Object?> item)
    {
        return _values.Remove(item.Key);
    }

    /// <inheritdoc />
    /// <nodoc />
    public Boolean TryGetValue(String key, [MaybeNullWhen(false)] out Object? value)
    {
        bool b = _values.TryGetValue(key, out Object? v);
        value = v;
        return b;
    }

    /// <inheritdoc />
    /// <nodoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
