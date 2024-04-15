// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CookieParser.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Internal.Net;
struct CookieParser
{
    private CookieTokenizer _tokenizer;
    private Cookie? _savedCookie;

    internal CookieParser(string cookieString)
    {
        _tokenizer = new CookieTokenizer(cookieString);
        _savedCookie = null;
    }

#if SYSTEM_NET_PRIMITIVES_DLL
        private static bool InternalSetNameMethod(Cookie cookie, string? value)
        {
            return cookie.InternalSetName(value);
        }
#else
    private static Func<Cookie, string?, bool>? s_internalSetNameMethod;
    private static Func<Cookie, string?, bool> InternalSetNameMethod
    {
        get
        {
            if (s_internalSetNameMethod == null)
            {
                // TODO https://github.com/dotnet/runtime/issues/19348:
                // We need to use Cookie.InternalSetName instead of the Cookie.set_Name wrapped in a try catch block, as
                // Cookie.set_Name keeps the original name if the string is empty or null.
                // Unfortunately this API is internal so we use reflection to access it. The method is cached for performance reasons.
                MethodInfo? method = typeof(Cookie).GetMethod("InternalSetName", BindingFlags.Instance | BindingFlags.NonPublic);
                Debug.Assert(method != null, "We need to use an internal method named InternalSetName that is declared on Cookie.");
                s_internalSetNameMethod = (Func<Cookie, string?, bool>)Delegate.CreateDelegate(typeof(Func<Cookie, string?, bool>), method);
            }

            return s_internalSetNameMethod;
        }
    }
#endif

    private static FieldInfo? s_isQuotedDomainField;
    private static FieldInfo IsQuotedDomainField
    {
        get
        {
            if (s_isQuotedDomainField == null)
            {
                // TODO https://github.com/dotnet/runtime/issues/19348:
                FieldInfo? field = typeof(Cookie).GetField("IsQuotedDomain", BindingFlags.Instance | BindingFlags.NonPublic);
                Debug.Assert(field != null, "We need to use an internal field named IsQuotedDomain that is declared on Cookie.");
                s_isQuotedDomainField = field;
            }

            return s_isQuotedDomainField;
        }
    }

    private static FieldInfo? s_isQuotedVersionField;
    private static FieldInfo IsQuotedVersionField
    {
        get
        {
            if (s_isQuotedVersionField == null)
            {
                // TODO https://github.com/dotnet/runtime/issues/19348:
                FieldInfo? field = typeof(Cookie).GetField("IsQuotedVersion", BindingFlags.Instance | BindingFlags.NonPublic);
                Debug.Assert(field != null, "We need to use an internal field named IsQuotedVersion that is declared on Cookie.");
                s_isQuotedVersionField = field;
            }

            return s_isQuotedVersionField;
        }
    }

    // Get
    //
    // Gets the next cookie or null if there are no more cookies.
    internal Cookie? Get()
    {
        Cookie? cookie = null;

        // Only the first occurrence of an attribute value must be counted.
        bool commentSet = false;
        bool commentUriSet = false;
        bool domainSet = false;
        bool expiresSet = false;
        bool pathSet = false;
        bool portSet = false; // Special case: may have no value in header.
        bool versionSet = false;
        bool secureSet = false;
        bool discardSet = false;

        do
        {
            CookieToken token = _tokenizer.Next(cookie == null, true);
            if (cookie == null && (token == CookieToken.NameValuePair || token == CookieToken.Attribute))
            {
                cookie = new Cookie();
                InternalSetNameMethod(cookie, _tokenizer.Name);
                cookie.Value = _tokenizer.Value;
            }
            else
            {
                switch (token)
                {
                    case CookieToken.NameValuePair:
                        switch (_tokenizer.Token)
                        {
                            case CookieToken.Comment:
                                if (!commentSet)
                                {
                                    commentSet = true;
                                    cookie!.Comment = _tokenizer.Value;
                                }
                                break;

                            case CookieToken.CommentUrl:
                                if (!commentUriSet)
                                {
                                    commentUriSet = true;
                                    if (Uri.TryCreate(CheckQuoted(_tokenizer.Value), UriKind.Absolute, out Uri? parsed))
                                    {
                                        cookie!.CommentUri = parsed;
                                    }
                                }
                                break;

                            case CookieToken.Domain:
                                if (!domainSet)
                                {
                                    domainSet = true;
                                    cookie!.Domain = CheckQuoted(_tokenizer.Value);
                                    IsQuotedDomainField.SetValue(cookie, _tokenizer.Quoted);
                                }
                                break;

                            case CookieToken.Expires:
                                if (!expiresSet)
                                {
                                    expiresSet = true;

                                    if (DateTime.TryParse(CheckQuoted(_tokenizer.Value),
                                        CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AdjustToUniversal, out DateTime expires))
                                    {
                                        cookie!.Expires = expires;
                                    }
                                    else
                                    {
                                        // This cookie will be rejected
                                        InternalSetNameMethod(cookie!, string.Empty);
                                    }
                                }
                                break;

                            case CookieToken.MaxAge:
                                if (!expiresSet)
                                {
                                    expiresSet = true;
                                    if (int.TryParse(CheckQuoted(_tokenizer.Value), out int parsed))
                                    {
                                        cookie!.Expires = DateTime.Now.AddSeconds(parsed);
                                    }
                                    else
                                    {
                                        // This cookie will be rejected
                                        InternalSetNameMethod(cookie!, string.Empty);
                                    }
                                }
                                break;

                            case CookieToken.Path:
                                if (!pathSet)
                                {
                                    pathSet = true;
                                    cookie!.Path = _tokenizer.Value;
                                }
                                break;

                            case CookieToken.Port:
                                if (!portSet)
                                {
                                    portSet = true;
                                    try
                                    {
                                        cookie!.Port = _tokenizer.Value;
                                    }
                                    catch
                                    {
                                        // This cookie will be rejected
                                        InternalSetNameMethod(cookie!, string.Empty);
                                    }
                                }
                                break;

                            case CookieToken.Version:
                                if (!versionSet)
                                {
                                    versionSet = true;
                                    int parsed;
                                    if (int.TryParse(CheckQuoted(_tokenizer.Value), out parsed))
                                    {
                                        cookie!.Version = parsed;
                                        IsQuotedVersionField.SetValue(cookie, _tokenizer.Quoted);
                                    }
                                    else
                                    {
                                        // This cookie will be rejected
                                        InternalSetNameMethod(cookie!, string.Empty);
                                    }
                                }
                                break;
                        }
                        break;

                    case CookieToken.Attribute:
                        switch (_tokenizer.Token)
                        {
                            case CookieToken.Discard:
                                if (!discardSet)
                                {
                                    discardSet = true;
                                    cookie!.Discard = true;
                                }
                                break;

                            case CookieToken.Secure:
                                if (!secureSet)
                                {
                                    secureSet = true;
                                    cookie!.Secure = true;
                                }
                                break;

                            case CookieToken.HttpOnly:
                                cookie!.HttpOnly = true;
                                break;

                            case CookieToken.Port:
                                if (!portSet)
                                {
                                    portSet = true;
                                    cookie!.Port = string.Empty;
                                }
                                break;
                        }
                        break;
                }
            }
        } while (!_tokenizer.Eof && !_tokenizer.EndOfCookie);

        return cookie;
    }

    internal Cookie? GetServer()
    {
        Cookie? cookie = _savedCookie;
        _savedCookie = null;

        // Only the first occurrence of an attribute value must be counted.
        bool domainSet = false;
        bool pathSet = false;
        bool portSet = false; // Special case: may have no value in header.

        do
        {
            bool first = cookie == null || string.IsNullOrEmpty(cookie.Name);
            CookieToken token = _tokenizer.Next(first, false);

            if (first && (token == CookieToken.NameValuePair || token == CookieToken.Attribute))
            {
                cookie ??= new Cookie();
                InternalSetNameMethod(cookie, _tokenizer.Name);
                cookie.Value = _tokenizer.Value;
            }
            else
            {
                switch (token)
                {
                    case CookieToken.NameValuePair:
                        switch (_tokenizer.Token)
                        {
                            case CookieToken.Domain:
                                if (!domainSet)
                                {
                                    domainSet = true;
                                    cookie!.Domain = CheckQuoted(_tokenizer.Value);
                                    IsQuotedDomainField.SetValue(cookie, _tokenizer.Quoted);
                                }
                                break;

                            case CookieToken.Path:
                                if (!pathSet)
                                {
                                    pathSet = true;
                                    cookie!.Path = _tokenizer.Value;
                                }
                                break;

                            case CookieToken.Port:
                                if (!portSet)
                                {
                                    portSet = true;
                                    try
                                    {
                                        cookie!.Port = _tokenizer.Value;
                                    }
                                    catch (CookieException)
                                    {
                                        // This cookie will be rejected
                                        InternalSetNameMethod(cookie!, string.Empty);
                                    }
                                }
                                break;

                            case CookieToken.Version:
                                // this is a new cookie, this token is for the next cookie.
                                _savedCookie = new Cookie();
                                if (int.TryParse(_tokenizer.Value, out int parsed))
                                {
                                    _savedCookie.Version = parsed;
                                }
                                return cookie;

                            case CookieToken.Unknown:
                                // this is a new cookie, the token is for the next cookie.
                                _savedCookie = new Cookie();
                                InternalSetNameMethod(_savedCookie, _tokenizer.Name);
                                _savedCookie.Value = _tokenizer.Value;
                                return cookie;
                        }
                        break;

                    case CookieToken.Attribute:
                        if (_tokenizer.Token == CookieToken.Port && !portSet)
                        {
                            portSet = true;
                            cookie!.Port = string.Empty;
                        }
                        break;
                }
            }
        } while (!_tokenizer.Eof && !_tokenizer.EndOfCookie);
        return cookie;
    }

    internal static string CheckQuoted(string value)
    {
        return (value.Length >= 2 && value.StartsWith('\"') && value.EndsWith('\"'))
            ? value.Substring(1, value.Length - 2)
            : value;
    }

    internal bool EndofHeader()
    {
        return _tokenizer.Eof;
    }
}