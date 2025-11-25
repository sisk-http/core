// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHeaderList.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Sisk.Cadente;

/// <summary>
/// Represents a typed list of <see cref="HttpHeader"/>.
/// </summary>
public sealed class HttpHeaderList : IList<HttpHeader> {

    private static readonly HttpHeaderListNameComparer _nameComparer = new ();

    internal IList<HttpHeader> _headers;
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

    internal HttpHeaderList(IList<HttpHeader> headers, bool readOnly)
    {
        _headers = headers;
        isReadOnly = readOnly;
    }

    // New constructor to wrap ReadOnlyMemory without copying to a List
    internal HttpHeaderList(ReadOnlyMemory<HttpHeader> headers)
    {
        _headers = new MemoryWrapper(headers);
        isReadOnly = true;
    }

    internal void SetBuffer(ReadOnlyMemory<HttpHeader> headers)
    {
        if (_headers is MemoryWrapper wrapper)
        {
            wrapper.SetBuffer(headers);
        }
        else
        {
            _headers = new MemoryWrapper(headers);
            isReadOnly = true;
        }
    }

    internal ReadOnlySpan<HttpHeader> AsSpan()
    {
        if (_headers is List<HttpHeader> list)
            return CollectionsMarshal.AsSpan(list);
        if (_headers is HttpHeader[] array)
            return array.AsSpan();
        if (_headers is MemoryWrapper wrapper)
            return wrapper.Span;
        return ReadOnlySpan<HttpHeader>.Empty;
    }

    /// <inheritdoc/>
    public HttpHeader this [ int index ] {
        get => _headers [ index ];
        set {
            ThrowIfReadOnly ();
            _headers [ index ] = value;
        }
    }

    /// <inheritdoc/>
    public int Count => _headers.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => isReadOnly;

    /// <inheritdoc/>
    public void Add ( HttpHeader item ) {
        ThrowIfReadOnly ();

        _headers.Add ( item );
    }

    /// <summary>
    /// Sets the specified <see cref="HttpHeader"/> in the collection, replacing any existing header with the same name.
    /// </summary>
    public void Set ( HttpHeader header ) {
        Remove ( header );
        _headers.Add ( header );
    }

    /// <summary>
    /// Gets the values of all headers that match the specified name.
    /// </summary>
    /// <param name="name">The name of the header to retrieve.</param>
    /// <returns>
    /// An array of header values that match the specified name. If no headers match, an empty array is returned.
    /// </returns>
    public string [] Get ( string name ) {
        var result = new List<string> ();
        ReadOnlySpan<HttpHeader> span;

        if (_headers is List<HttpHeader> list)
        {
             span = CollectionsMarshal.AsSpan(list);
        }
        else if (_headers is HttpHeader[] array)
        {
             span = array.AsSpan();
        }
        else if (_headers is MemoryWrapper wrapper)
        {
             span = wrapper.Span;
        }
        else
        {
             foreach(var header in _headers)
             {
                 if (Ascii.EqualsIgnoreCase ( name, header.NameBytes.Span ))
                    result.Add ( header.Value );
             }
             return result.ToArray();
        }

        for (int i = 0; i < span.Length; i++) {
            ref readonly var header = ref span [ i ];
            if (Ascii.EqualsIgnoreCase ( name, header.NameBytes.Span ))
                result.Add ( header.Value );
        }
        return result.ToArray ();
    }

    /// <inheritdoc/>
    public void Clear () {
        ThrowIfReadOnly ();

        _headers.Clear ();
    }

    /// <inheritdoc/>
    public bool Contains ( HttpHeader item ) {
         ReadOnlySpan<HttpHeader> span;
        if (_headers is List<HttpHeader> list)
        {
             span = CollectionsMarshal.AsSpan(list);
        }
        else if (_headers is HttpHeader[] array)
        {
             span = array.AsSpan();
        }
        else if (_headers is MemoryWrapper wrapper)
        {
             span = wrapper.Span;
        }
        else
        {
            return _headers.Contains(item);
        }

        for (int i = 0; i < span.Length; i++) {
            ref readonly var header = ref span [ i ];
            if (_nameComparer.Equals ( header, item ))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Determines whether the collection contains a header with the specified name.
    /// </summary>
    /// <param name="name">The name of the header to locate.</param>
    /// <returns>
    /// <see langword="true"/> if a header with the specified name is found; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains ( string name ) {
        ReadOnlySpan<HttpHeader> span;
        if (_headers is List<HttpHeader> list)
        {
             span = CollectionsMarshal.AsSpan(list);
        }
        else if (_headers is HttpHeader[] array)
        {
             span = array.AsSpan();
        }
        else if (_headers is MemoryWrapper wrapper)
        {
             span = wrapper.Span;
        }
        else
        {
             foreach(var header in _headers)
             {
                 if (Ascii.EqualsIgnoreCase ( name, header.NameBytes.Span ))
                    return true;
             }
             return false;
        }

        for (int i = 0; i < span.Length; i++) {
            ref readonly var header = ref span [ i ];
            if (Ascii.EqualsIgnoreCase ( name, header.NameBytes.Span ))
                return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public void CopyTo ( HttpHeader [] array, int arrayIndex ) {
        _headers.CopyTo ( array, arrayIndex );
    }

    /// <inheritdoc/>
    public IEnumerator<HttpHeader> GetEnumerator () {
        return _headers.GetEnumerator ();
    }

    /// <inheritdoc/>
    public int IndexOf ( HttpHeader item ) {
        return _headers.IndexOf ( item );
    }

    /// <inheritdoc/>
    public void Insert ( int index, HttpHeader item ) {
        ThrowIfReadOnly ();

        _headers.Insert ( index, item );
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
                removed++;
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

        _headers.RemoveAt ( index );
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

    private class MemoryWrapper : IList<HttpHeader>
    {
        private ReadOnlyMemory<HttpHeader> _memory;

        public ReadOnlySpan<HttpHeader> Span => _memory.Span;

        public MemoryWrapper(ReadOnlyMemory<HttpHeader> memory)
        {
            _memory = memory;
        }

        public void SetBuffer(ReadOnlyMemory<HttpHeader> memory)
        {
            _memory = memory;
        }

        public HttpHeader this[int index] { get => _memory.Span[index]; set => throw new NotSupportedException(); }

        public int Count => _memory.Length;

        public bool IsReadOnly => true;

        public void Add(HttpHeader item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Contains(HttpHeader item)
        {
             var span = _memory.Span;
             for(int i=0; i<span.Length; i++)
             {
                 if (_nameComparer.Equals(span[i], item)) return true;
             }
             return false;
        }
        public void CopyTo(HttpHeader[] array, int arrayIndex)
        {
             _memory.Span.CopyTo(array.AsSpan(arrayIndex));
        }

        public IEnumerator<HttpHeader> GetEnumerator()
        {
             // This allocates an enumerator, but it's unavoidable if we return IEnumerator<T>
             // But we can optimize by implementing a struct enumerator if needed, but the interface requires boxing.
             // We can just iterate the memory.
             for(int i=0; i<_memory.Length; i++)
             {
                 yield return _memory.Span[i];
             }
        }

        public int IndexOf(HttpHeader item)
        {
             var span = _memory.Span;
             for(int i=0; i<span.Length; i++)
             {
                 if (_nameComparer.Equals(span[i], item)) return i;
             }
             return -1;
        }

        public void Insert(int index, HttpHeader item) => throw new NotSupportedException();
        public bool Remove(HttpHeader item) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
