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
using Sisk.Cadente.HttpSerializer;

namespace Sisk.Cadente;

internal static class HttpHeaderExtensions {
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
}
