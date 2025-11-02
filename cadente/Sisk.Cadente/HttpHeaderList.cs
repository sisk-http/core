// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHeaderList.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sisk.Cadente;

/// <summary>
/// Represents a typed list of <see cref="HttpHeader"/>.
/// </summary>
public sealed class HttpHeaderList : IList<HttpHeader> {

    private static HttpHeaderListNameComparer _nameComparer = new ();
    internal List<HttpHeader> _headers;
    internal bool isReadOnly = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHeaderList"/> class.
    /// </summary>
    public HttpHeaderList () {
        _headers = new List<HttpHeader> ();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHeaderList"/> class that contains elements copied from the specified collection.
    /// </summary>
    /// <param name="headers">The collection whose elements are copied to the new list.</param>
    public HttpHeaderList ( IEnumerable<HttpHeader> headers ) {
        _headers = new List<HttpHeader> ( headers );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHeaderList"/> class that contains elements copied from the specified collection.
    /// </summary>
    /// <param name="headers">The collection whose elements are copied to the new list.</param>
    /// <param name="readOnly">
    /// <see langword="true"/> to make the list read-only; otherwise, <see langword="false"/>.
    /// </param>
    public HttpHeaderList ( IEnumerable<HttpHeader> headers, bool readOnly = false ) {
        _headers = new List<HttpHeader> ( headers );
        isReadOnly = readOnly;
    }

    /// <inheritdoc/>
    public HttpHeader this [ int index ] {
        get => ((IList<HttpHeader>) _headers) [ index ];
        set {
            ThrowIfReadOnly ();
            ((IList<HttpHeader>) _headers) [ index ] = value;
        }
    }

    /// <inheritdoc/>
    public int Count => ((ICollection<HttpHeader>) _headers).Count;

    /// <inheritdoc/>
    public bool IsReadOnly => isReadOnly;

    /// <inheritdoc/>
    public void Add ( HttpHeader item ) {
        ThrowIfReadOnly ();

        ((ICollection<HttpHeader>) _headers).Add ( item );
    }

    /// <summary>
    /// Sets the specified <see cref="HttpHeader"/> in the collection, replacing any existing header with the same name.
    /// </summary>
    public void Set ( HttpHeader header ) {
        Remove ( header );
        Add ( header );
    }

    /// <summary>
    /// Gets the values of all headers that match the specified name.
    /// </summary>
    /// <param name="name">The name of the header to retrieve.</param>
    /// <returns>
    /// An array of header values that match the specified name. If no headers match, an empty array is returned.
    /// </returns>
    public string [] Get ( string name ) {
        return _headers.Where ( h => h.Name.Equals ( name, StringComparison.OrdinalIgnoreCase ) ).Select ( h => h.Value ).ToArray ();
    }

    /// <inheritdoc/>
    public void Clear () {
        ThrowIfReadOnly ();

        ((ICollection<HttpHeader>) _headers).Clear ();
    }

    /// <inheritdoc/>
    public bool Contains ( HttpHeader item ) {
        return ((ICollection<HttpHeader>) _headers).Contains ( item, _nameComparer );
    }

    /// <summary>
    /// Determines whether the collection contains a header with the specified name.
    /// </summary>
    /// <param name="name">The name of the header to locate.</param>
    /// <returns>
    /// <see langword="true"/> if a header with the specified name is found; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains ( string name ) {
        return Contains ( new HttpHeader ( name, string.Empty ) );
    }

    /// <inheritdoc/>
    public void CopyTo ( HttpHeader [] array, int arrayIndex ) {
        ((ICollection<HttpHeader>) _headers).CopyTo ( array, arrayIndex );
    }

    /// <inheritdoc/>
    public IEnumerator<HttpHeader> GetEnumerator () {
        return ((IEnumerable<HttpHeader>) _headers).GetEnumerator ();
    }

    /// <inheritdoc/>
    public int IndexOf ( HttpHeader item ) {
        return ((IList<HttpHeader>) _headers).IndexOf ( item );
    }

    /// <inheritdoc/>
    public void Insert ( int index, HttpHeader item ) {
        ThrowIfReadOnly ();

        ((IList<HttpHeader>) _headers).Insert ( index, item );
    }

    /// <summary>
    /// Removes all <see cref="HttpHeader"/> instances that match the specified header.
    /// </summary>
    /// <param name="item">The header to remove.</param>
    /// <returns>
    /// <see langword="true"/> if one or more headers were removed; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Remove ( HttpHeader item ) {
        ThrowIfReadOnly ();

        int removed = 0;
        for (int i = _headers.Count - 1; i >= 0; i--) {
            HttpHeader h = _headers [ i ];

            if (_nameComparer.Equals ( h, item )) {
                _headers.RemoveAt ( i );
            }
        }

        return removed > 0;
    }

    /// <summary>
    /// Removes all <see cref="HttpHeader"/> instances that match the specified header name.
    /// </summary>
    /// <param name="name">The name of the header to remove.</param>
    /// <returns>
    /// <see langword="true"/> if one or more headers were removed; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Remove ( string name ) {
        return Remove ( new HttpHeader ( name, string.Empty ) );
    }

    /// <inheritdoc/>
    public void RemoveAt ( int index ) {
        ThrowIfReadOnly ();

        ((IList<HttpHeader>) _headers).RemoveAt ( index );
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator () {
        return ((IEnumerable) _headers).GetEnumerator ();
    }

    void ThrowIfReadOnly () {
        if (isReadOnly) {
            throw new InvalidOperationException ( "The HttpHeaderList is read-only and cannot be modified." );
        }
    }

    private class HttpHeaderListNameComparer : IEqualityComparer<HttpHeader> {
        public bool Equals ( HttpHeader x, HttpHeader y ) {
            return Ascii.EqualsIgnoreCase ( x.NameBytes.Span, y.NameBytes.Span );
        }

        public int GetHashCode ( [DisallowNull] HttpHeader obj ) {
            var sp = obj.NameBytes.Span;
            int ln = sp.Length;
            int hc = ln;
            for (int i = 0; i < ln; i++) {
                byte h = sp [ i ];
                hc = unchecked(hc * 314159 + h);
            }
            return hc;
        }
    }
}
