// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HeaderHelper.cs
// Repository:  https://github.com/sisk-http/core

using System.Net.Http.Headers;

namespace Sisk.Core.Helpers;

/// <summary>
/// Provides helper methods for working with HTTP headers.
/// </summary>
public static class HeaderHelper {

    /// <summary>
    /// Copies HTTP headers from one <see cref="HttpContentHeaders"/> instance to another.
    /// </summary>
    /// <param name="from">The source <see cref="HttpContentHeaders"/> instance.</param>
    /// <param name="to">The target <see cref="HttpContentHeaders"/> instance.</param>
    /// <param name="safe">If set to <c>true</c>, headers that are added will be validated (an exception can be throw if an header is invalid). If <c>false</c>, invalid headers could be discarded, but no exception is thrown.</param>
    public static void CopyHttpHeaders ( HttpContentHeaders from, HttpContentHeaders to, bool safe = true ) {
        foreach (var header in from) {
            if (safe) {
                to.Add ( header.Key, header.Value );
            }
            else {
                to.TryAddWithoutValidation ( header.Key, header.Value );
            }
        }
    }
}
