// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CookieParser.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections.Specialized;
using System.Net;

namespace Sisk.Core.Internal;

internal static class CookieParser
{
    public static NameValueCollection ParseCookieString(string? cookieHeader)
    {
        NameValueCollection cookies = new NameValueCollection();
        if (!string.IsNullOrWhiteSpace(cookieHeader))
        {
            string[] cookiePairs = cookieHeader.Split(SharedChars.Semicolon);

            for (int i = 0; i < cookiePairs.Length; i++)
            {
                string cookieExpression = cookiePairs[i];

                if (string.IsNullOrWhiteSpace(cookieExpression))
                    continue;

                int eqPos = cookieExpression.IndexOf(SharedChars.Equal);
                if (eqPos < 0)
                {
                    cookies[cookieExpression] = string.Empty;
                    continue;
                }
                else
                {
                    string cookieName = cookieExpression.Substring(0, eqPos).Trim();
                    string cookieValue = cookieExpression.Substring(eqPos + 1).Trim();

                    if (string.IsNullOrWhiteSpace(cookieName))
                    {
                        // provided an name/value pair, but no name
                        continue;
                    }

                    cookies[cookieName] = WebUtility.UrlDecode(cookieValue);
                }
            }
        }

        return cookies;
    }
}
