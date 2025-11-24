// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerHandlerSortingComparer.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.Core.Http.Handlers;

sealed class HttpServerHandlerSortingComparer : IComparer<HttpServerHandler> {

    public static readonly HttpServerHandlerSortingComparer Instance = new HttpServerHandlerSortingComparer ();

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public int Compare ( HttpServerHandler? x, HttpServerHandler? y ) {
        if (x is null && y is null) {
            return 0;
        }
        if (x is null) {
            return -1;
        }
        if (y is null) {
            return 1;
        }
        return x.Priority.CompareTo ( y.Priority );
    }
}
