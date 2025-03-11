// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   InitializationParameterCollection.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;
using System.Collections.Specialized;

namespace Sisk.Core.Http.Hosting;

/// <summary>
/// Provides a collection of HTTP server initialization variables.
/// </summary>
public sealed class InitializationParameterCollection : IDictionary<string, string?> {
    private readonly NameValueCollection _decorator = new NameValueCollection ();
    private bool _isReadonly;

    /// <summary>
    /// Gets an instance of <see cref="NameValueCollection"/> with the values of this class.
    /// </summary>
    public NameValueCollection AsNameValueCollection () => _decorator;

    /// <summary>
    /// Ensures that the parameter defined by name <paramref name="parameterName"/> is present and not empty in this collection.
    /// </summary>
    /// <remarks>
    /// If the parameter doens't meet the above requirements, an <see cref="ArgumentNullException"/> exception is thrown.
    /// </remarks>
    /// <param name="parameterName">The parameter name which will be evaluated.</param>
    public void EnsureNotNullOrEmpty ( string parameterName ) {
        if (string.IsNullOrEmpty ( _decorator [ parameterName ] ))
            throw new ArgumentException ( SR.Format ( SR.InitializationParameterCollection_NullOrEmptyParameter, parameterName ) );
    }

    /// <summary>
    /// Ensures that the parameter defined by name <paramref name="parameterName"/> is present in this collection.
    /// </summary>
    /// <remarks>
    /// If the parameter doens't meet the above requirements, an <see cref="ArgumentNullException"/> exception is thrown.
    /// </remarks>
    /// <param name="parameterName">The parameter name which will be evaluated.</param>
    public void EnsureNotNull ( string parameterName ) {
        if (string.IsNullOrEmpty ( _decorator [ parameterName ] ))
            throw new ArgumentException ( SR.Format ( SR.InitializationParameterCollection_NullParameter, parameterName ) );
    }

    /// <inheritdoc/>
    /// <exclude/>
    public string? this [ string key ] {
        get => _decorator [ key ];
        set {
            ThrowIfReadonly ();
            _decorator [ key ] = value;
        }
    }

    /// <inheritdoc/>
    /// <exclude/>
    public ICollection<string> Keys {
        get {
            List<string> _keys = new List<string> ();
            foreach (string s in _decorator.Keys)
                _keys.Add ( s );
            return _keys.ToArray ();
        }
    }

    /// <inheritdoc/>
    /// <exclude/>
    public ICollection<string?> Values {
        get {
            List<string?> _keys = new List<string?> ();
            foreach (string s in _decorator.Keys)
                _keys.Add ( _decorator [ s ] );
            return _keys.ToArray ();
        }
    }

    /// <summary>
    /// Gets the specified value if present in this parameter collection, or throw an exception
    /// if the value is not present.
    /// </summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <param name="option">Specifies the <see cref="GetValueOption"/> used for getting the value.</param>
    public string GetValueOrThrow ( string parameterName, GetValueOption option = GetValueOption.NotNullOrEmpty ) {
        string? value = this [ parameterName ];

        if (option == GetValueOption.NotNull && value is null)
            throw new ArgumentException ( SR.Format ( SR.InitializationParameterCollection_NullOrEmptyParameter, parameterName ) );

        if (option == GetValueOption.NotNullOrEmpty && string.IsNullOrEmpty ( value ))
            throw new ArgumentException ( SR.Format ( SR.InitializationParameterCollection_NullOrEmptyParameter, parameterName ) );

        if (option == GetValueOption.NotNullOrWhiteSpace && string.IsNullOrWhiteSpace ( value ))
            throw new ArgumentException ( SR.Format ( SR.InitializationParameterCollection_NullOrEmptyParameter, parameterName ) );

        return value!;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public int Count => _decorator.Count;

    /// <inheritdoc/>
    /// <exclude/>
    public bool IsReadOnly => _isReadonly;

    /// <inheritdoc/>
    /// <exclude/>
    public void Add ( string key, string? value ) {
        ThrowIfReadonly ();
        _decorator [ key ] = value;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public void Add ( KeyValuePair<string, string?> item ) {
        ThrowIfReadonly ();
        _decorator [ item.Key ] = item.Value;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public void Clear () {
        ThrowIfReadonly ();
        _decorator.Clear ();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool Contains ( KeyValuePair<string, string?> item ) {
        return ContainsKey ( item.Key ) && _decorator [ item.Key ] == item.Value;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool ContainsKey ( string key ) {
        return ContainsKey ( key );
    }

    /// <inheritdoc/>
    /// <exclude/>
    public void CopyTo ( KeyValuePair<string, string?> [] array, int arrayIndex ) {
        throw new NotImplementedException ();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator () {
        return (IEnumerator<KeyValuePair<string, string?>>) _decorator.GetEnumerator ();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool Remove ( string key ) {
        ThrowIfReadonly ();
        _decorator.Remove ( key );
        return true;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool Remove ( KeyValuePair<string, string?> item ) {
        ThrowIfReadonly ();
        _decorator.Remove ( item.Key );
        return true;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool TryGetValue ( string key, out string? value ) {
        if (ContainsKey ( key )) {
            value = _decorator [ key ];
            return true;
        }
        else {
            value = null;
            return false;
        }
    }

    /// <inheritdoc/>
    /// <exclude/>
    IEnumerator IEnumerable.GetEnumerator () {
        return _decorator.GetEnumerator ();
    }

    internal void MakeReadonly () {
        _isReadonly = true;
    }

    void ThrowIfReadonly () {
        if (_isReadonly) {
            throw new InvalidOperationException ( "Cannot modify this collection: it is read-only." );
        }
    }

    /// <summary>
    /// Represents the option used in the method <see cref="GetValueOrThrow(string, GetValueOption)"/>.
    /// </summary>
    public enum GetValueOption {
        /// <summary>
        /// The method should throw if the value is not present in the collection, but allow empty
        /// values.
        /// </summary>
        NotNull,

        /// <summary>
        /// The method should throw if the value is not present in the collection or has an empty value.
        /// </summary>
        NotNullOrEmpty,

        /// <summary>
        /// The method should throw if the value is not present in the collection, has an empty value or
        /// consists of whitespaces (spaces, tabs, etc.).
        /// </summary>
        NotNullOrWhiteSpace
    }
}
