// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SpanExtensions.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Internal;

static class SpanExtensions
{
    public static int IndexOf<T>(this ReadOnlySpan<T> span, T value, int start) where T : IEquatable<T>?
    {
        int pos = MemoryExtensions.IndexOf<T>(span[start..], value);
        return pos >= 0 ? start + pos : -1;
    }

    public static int IndexOfAny<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values, int start) where T : IEquatable<T>?
    {
        int pos = MemoryExtensions.IndexOfAny<T>(span[start..], values);
        return pos >= 0 ? start + pos : -1;
    }
}
