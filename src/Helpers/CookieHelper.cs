// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CookieHelper.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System.Net;
using System.Web;

namespace Sisk.Core.Helpers;

/// <summary>
/// Provides a class that contains useful methods for working with cookies in HTTP responses.
/// </summary>
public static class CookieHelper
{
    /// <summary>
    /// Builds the cookie header value and returns an string from it.
    /// </summary>
    /// <param name="cookie">The <see cref="Cookie"/> instance to build the cookie string.</param>
    public static string BuildCookieHeaderValue(Cookie cookie)
    {
        return BuildCookieHeaderValue(
            name: cookie.Name,
            value: cookie.Value,
            expires: cookie.Expires,
            domain: cookie.Domain,
            path: cookie.Path,
            secure: cookie.Secure,
            httpOnly: cookie.HttpOnly
        );
    }

    /// <summary>
    /// Builds the cookie header value and returns an string from it.
    /// </summary>
    /// <param name="name">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    /// <param name="expires">The cookie expirity date.</param>
    /// <param name="maxAge">The cookie max duration after being set.</param>
    /// <param name="domain">The domain where the cookie will be valid.</param>
    /// <param name="path">The path where the cookie will be valid.</param>
    /// <param name="secure">Determines if the cookie will only be stored in an secure context.</param>
    /// <param name="httpOnly">Determines if the cookie will be only available in the HTTP context.</param>
    /// <param name="sameSite">The cookie SameSite parameter.</param>
    public static string BuildCookieHeaderValue(
        string name,
        string value,
        DateTime? expires = null,
        TimeSpan? maxAge = null,
        string? domain = null,
        string? path = null,
        bool? secure = null,
        bool? httpOnly = null,
        string? sameSite = null)
    {
        List<string> syntax = new List<string>();
        syntax.Add($"{HttpUtility.UrlEncode(name)}={HttpUtility.UrlEncode(value)}");
        if (expires is not null)
        {
            syntax.Add($"Expires={expires.Value.ToUniversalTime():r}");
        }
        if (maxAge is not null)
        {
            syntax.Add($"Max-Age={maxAge.Value.TotalSeconds}");
        }
        if (domain is not null)
        {
            string d = domain;

            d = d.RemoveStart("https://", StringComparison.OrdinalIgnoreCase);
            d = d.RemoveStart("http://", StringComparison.OrdinalIgnoreCase);
            d = d.TrimEnd('/');

            syntax.Add($"Domain={d}");
        }
        if (path is not null)
        {
            syntax.Add($"Path={path}");
        }
        if (secure == true)
        {
            syntax.Add($"Secure");
        }
        if (httpOnly == true)
        {
            syntax.Add($"HttpOnly");
        }
        if (sameSite is not null)
        {
            syntax.Add($"SameSite={sameSite}");
        }

        return string.Join("; ", syntax);
    }
}
