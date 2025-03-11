// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   TypedValueDictionary.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Sisk.Core.Entity;

/// <summary>
/// Represents the base class for storing and retriving data by their type.
/// </summary>
public class TypedValueDictionary : IDictionary<string, object?> {
    readonly Dictionary<string, object?> _values;

    /// <summary>
    /// Creates an new <see cref="TypedValueDictionary"/> instance with default parameters.
    /// </summary>
    public TypedValueDictionary () {
        _values = new ();
    }

    /// <summary>
    /// Creates an new <see cref="TypedValueDictionary"/> instance with default parameters with the specified
    /// <see cref="StringComparer"/>.
    /// </summary>
    public TypedValueDictionary ( StringComparer keyComparer ) {
        _values = new Dictionary<string, object?> ( keyComparer );
    }

    /// <summary>
    /// Gets the Type full qualified key name.
    /// </summary>
    /// <param name="t">The type to get their qualified key name.</param>
    protected string GetTypeKeyName ( Type t ) =>
        t.FullName + "+" + t.GetHashCode ();

    /// <summary>
    /// Determines whether the specified <typeparamref name="T"/> singleton is defined in this context.
    /// </summary>
    /// <typeparam name="T">The singleton type.</typeparam>
    public bool IsSet<T> () where T : notnull {
        return _values.ContainsKey ( GetTypeKeyName ( typeof ( T ) ) );
    }

    /// <summary>
    /// Determines whether the specified <typeparamref name="T"/> singleton is defined in this context and tries to
    /// output it.
    /// </summary>
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>True if the object is find with the specified key; otherwise, false.</returns>
    /// <typeparam name="T">The singleton type.</typeparam>
    public bool IsSet<T> ( [NotNullWhen ( true )] out T value ) where T : notnull {
        var b = TryGetValue ( GetTypeKeyName ( typeof ( T ) ), out var v );
        value = b ? (T) v! : default!;
        return b;
    }

    /// <summary>
    /// Removes an singleton object from it's type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The singleton type.</typeparam>
    public bool Unset<T> () where T : notnull {
        return _values.Remove ( GetTypeKeyName ( typeof ( T ) ) );
    }

    /// <summary>
    /// Creates and adds an singleton of <typeparamref name="T"/> in this context bag.
    /// </summary>
    /// <typeparam name="T">The object that will be defined in this context bag.</typeparam>
    public T Set<T> () where T : notnull, new() {
        return Set ( new T () );
    }

    /// <summary>
    /// Adds an singleton of <typeparamref name="T"/> in this context bag.
    /// </summary>
    /// <typeparam name="T">The object that will be defined in this context bag.</typeparam>
    /// <param name="value">The instance of <typeparamref name="T"/> which will be defined in this context bag.</param>
    public T Set<T> ( T value ) where T : notnull {
        Type contextType = typeof ( T );
        _values [ GetTypeKeyName ( contextType ) ] = value;
        return value;
    }

    /// <summary>
    /// Gets a singleton previously defined in this context bag via it's type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object defined in this context bag.</typeparam>
    public T Get<T> () where T : notnull {
        if (IsSet ( out T value )) {
            return value;
        }
        throw new ArgumentException ( SR.Format ( SR.HttpContextBagRepository_UndefinedDynamicProperty, typeof ( T ).FullName ) );
    }

    /// <summary>
    /// Gets a singleton previously defined in this context bag via its type <typeparamref name="T"/>.
    /// Returns the default value if the object is not defined.
    /// </summary>
    /// <typeparam name="T">The type of the object defined in this context bag.</typeparam>
    /// <returns>The object of type <typeparamref name="T"/> if it exists; otherwise, <see langword="null"></see>.</returns>
    public T? GetOrDefault<T> () where T : notnull {
        if (IsSet ( out T value )) {
            return value;
        }
        return default;
    }

    /// <summary>
    /// Gets a singleton previously defined in this context bag via its type <typeparamref name="T"/>.
    /// If it does not exist, it adds the object to the context bag using the provided <paramref name="getter"/> function.
    /// </summary>
    /// <typeparam name="T">The type of the object defined in this context bag.</typeparam>
    /// <param name="getter">A function that provides the object to be added if it does not exist.</param>
    /// <returns>The object of type <typeparamref name="T"/> from the context bag.</returns>
    public T GetOrAdd<T> ( Func<T> getter ) where T : notnull {
        if (IsSet ( out T value )) {
            return value;
        }
        else {
            value = getter ();
            return Set ( value );
        }
    }

    /// <summary>
    /// Gets a singleton previously defined in this context bag via its type <typeparamref name="T"/>.
    /// If it does not exist, it adds the object to the context bag by creating a new instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object defined in this context bag. It must have a public parameterless constructor.</typeparam>
    /// <returns>The object of type <typeparamref name="T"/> from the context bag.</returns>
    public T GetOrAdd<T> () where T : notnull, new() {
        if (IsSet ( out T value )) {
            return value;
        }
        else {
            value = new T ();
            return Set ( value );
        }
    }

    /// <summary>
    /// Asynchronously gets a singleton previously defined in this context bag via its type <typeparamref name="T"/>.
    /// If it does not exist, it adds the object to the context bag using the provided asynchronous <paramref name="getter"/> function.
    /// </summary>
    /// <typeparam name="T">The type of the object defined in this context bag.</typeparam>
    /// <param name="getter">An asynchronous function that provides the object to be added if it does not exist.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the object of type <typeparamref name="T"/> from the context bag.</returns>
    public async Task<T> GetOrAddAsync<T> ( Func<Task<T>> getter ) where T : notnull {
        if (IsSet ( out T value )) {
            return value;
        }
        else {
            value = await getter ();
            return Set ( value );
        }
    }

    /// <inheritdoc />
    /// <exclude />
    public object? this [ string key ] { get => _values [ key ]; set => _values [ key ] = value; }

    /// <inheritdoc />
    /// <exclude />
    public ICollection<string> Keys => _values.Keys;

    /// <inheritdoc />
    /// <exclude />
    public ICollection<object?> Values => _values.Values;

    /// <inheritdoc />
    /// <exclude />
    public int Count => _values.Count;

    /// <inheritdoc />
    /// <exclude />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    /// <exclude />
    public void Add ( string key, object? value ) {
        _values.Add ( key, value );
    }

    /// <inheritdoc />
    /// <exclude />
    public void Add ( KeyValuePair<string, object?> item ) {
        _values.Add ( item.Key, item.Value );
    }

    /// <inheritdoc />
    /// <exclude />
    public void Clear () {
        _values.Clear ();
    }

    /// <inheritdoc />
    /// <exclude />
    public bool Contains ( KeyValuePair<string, object?> item ) {
        return _values.Contains ( item );
    }

    /// <inheritdoc />
    /// <exclude />
    public bool ContainsKey ( string key ) {
        return _values.ContainsKey ( key );
    }

    /// <inheritdoc />
    /// <exclude />
    public void CopyTo ( KeyValuePair<string, object?> [] array, int arrayIndex ) {
        ((ICollection<KeyValuePair<string, object?>>) _values).CopyTo ( array, arrayIndex );
    }

    /// <inheritdoc />
    /// <exclude />
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator () {
        return _values.GetEnumerator ();
    }

    /// <inheritdoc />
    /// <exclude />
    public bool Remove ( string key ) {
        return _values.Remove ( key );
    }

    /// <inheritdoc />
    /// <exclude />
    public bool Remove ( KeyValuePair<string, object?> item ) {
        return ((ICollection<KeyValuePair<string, object?>>) _values).Remove ( item );
    }

    /// <inheritdoc />
    /// <exclude />
    public bool TryGetValue ( string key, [MaybeNullWhen ( false )] out object? value ) {
        return _values.TryGetValue ( key, out value );
    }

    /// <summary>
    /// Gets the value associated with the specified key and casts it into <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type which will be casted into.</typeparam>
    /// <param name="key">The key whose to get.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>true if the object is find with the specified key; otherwise, false.</returns>
    public bool TryGetValue<TResult> ( string key, [MaybeNullWhen ( false )] out TResult? value ) {
        bool b = _values.TryGetValue ( key, out var v );
        if (b) {
            value = (TResult?) v;
            return true;
        }
        else {
            value = default;
            return false;
        }
    }

    /// <inheritdoc />
    /// <exclude />
    IEnumerator IEnumerable.GetEnumerator () {
        return ((IEnumerable) _values).GetEnumerator ();
    }
}
