// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   StringExtensions.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;

namespace Sisk.Core.Internal;

internal static class StringExtensions {
    [return: NotNullIfNotNull ( nameof ( s ) )]
    public static string? RemoveStart ( this string? s, string term, StringComparison comparisonType = StringComparison.Ordinal ) {
        if (s is null)
            return null;
        if (s.StartsWith ( term, comparisonType ))
            return s [ term.Length.. ];

        return s;
    }
}
