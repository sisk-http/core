// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHeaderExtensions.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Sisk.Cadente;

/// <summary>
/// Provides extension methods for collections of <see cref="HttpHeader"/>.
/// </summary>
public static class HttpHeaderExtensions {

    /// <summary>
    /// Sets an <see cref="HttpHeader"/> in the list. If a header with the same name already exists, it is removed before the new header is added.
    /// This operation is thread-safe.
    /// </summary>
    /// <param name="headers">The list of <see cref="HttpHeader"/> to modify.</param>
    /// <param name="header">The <see cref="HttpHeader"/> to set.</param>
    public static void Set ( this List<HttpHeader> headers, in HttpHeader header ) {
        lock (((ICollection) headers).SyncRoot) {
            var span = CollectionsMarshal.AsSpan ( headers );
            for (int i = span.Length - 1; i >= 0; i--) {
                if (Ascii.EqualsIgnoreCase ( span [ i ].NameBytes.Span, header.NameBytes.Span )) {
                    headers.RemoveAt ( i );
                }
            }

            headers.Add ( header );
        }
    }

    /// <summary>
    /// Removes all <see cref="HttpHeader"/> with the given name from the list. Thread-safe.
    /// </summary>
    /// <param name="headers">The list of <see cref="HttpHeader"/> to modify.</param>
    /// <param name="headerName">The name of the header to remove.</param>
    public static void Remove ( this List<HttpHeader> headers, string headerName ) {
        lock (((ICollection) headers).SyncRoot) {
            var span = CollectionsMarshal.AsSpan ( headers );
            for (int i = span.Length - 1; i >= 0; i--) {
                if (Ascii.EqualsIgnoreCase ( span [ i ].NameBytes.Span, headerName )) {
                    headers.RemoveAt ( i );
                }
            }
        }
    }
}