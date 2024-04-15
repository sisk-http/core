// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CookieComparer.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Internal.Net;

static class CookieComparer
{
    internal static bool Equals(Cookie left, Cookie right)
    {
        if (!string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!EqualDomains(left.Domain, right.Domain))
        {
            return false;
        }

        // NB: Only the path is case sensitive as per spec. However, many Windows applications assume
        //     case-insensitivity.
        return string.Equals(left.Path, right.Path, StringComparison.Ordinal);
    }

    internal static bool EqualDomains(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        if (left.Length != 0 && left[0] == '.') left = left.Slice(1);
        if (right.Length != 0 && right[0] == '.') right = right.Slice(1);

        return left.Equals(right, StringComparison.OrdinalIgnoreCase);
    }

    private static Func<Cookie, CookieVariant>? s_getVariantFunc;
    internal static bool IsRfc2965Variant(Cookie cookie)
    {
        s_getVariantFunc ??= (Func<Cookie, CookieVariant>)typeof(Cookie).GetProperty("Variant", BindingFlags.Instance | BindingFlags.NonPublic)?.GetGetMethod(true)?.CreateDelegate(typeof(Func<Cookie, CookieVariant>))!;
        Debug.Assert(s_getVariantFunc != null, "Reflection failed for Cookie.Variant.");
        return s_getVariantFunc(cookie) == CookieVariant.Rfc2965;
    }
}