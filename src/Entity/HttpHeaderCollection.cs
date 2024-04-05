// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHeaderCollection.cs
// Repository:  https://github.com/sisk-http/core


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Sisk.Core.Http;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Header = Sisk.Core.Internal.HttpKnownHeaderNames;

namespace Sisk.Core.Entity;

/// <summary>
/// Represents an collection of HTTP headers with their name and values.
/// </summary>
/// <definition>
/// public class HttpHeaderCollection : NameValueCollection
/// </definition>
/// <type>
/// Class
/// </type>
public class HttpHeaderCollection : NameValueCollection
{
    /// <summary>
    /// Create an new instance of the <see cref="HttpHeaderCollection"/> class.
    /// </summary>
    public HttpHeaderCollection()
    {
    }

    /// <summary>
    /// Creates an new instance of the <see cref="HttpHeaderCollection"/> with the specified
    /// headers.
    /// </summary>
    /// <param name="headers">The header collection.</param>
    public HttpHeaderCollection(NameValueCollection headers) : base(headers)
    {
    }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept header.
    /// </summary>
    /// <definition>
    /// public string? Accept { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? Accept { get => base[Header.Accept]; set => base[Header.Accept] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Charset header.
    /// </summary>
    /// <definition>
    /// public string? AcceptCharset { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? AcceptCharset { get => base[Header.AcceptCharset]; set => base[Header.AcceptCharset] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Encoding header.
    /// </summary>
    /// <definition>
    /// public string? AcceptEncoding { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? AcceptEncoding { get => base[Header.AcceptEncoding]; set => base[Header.AcceptEncoding] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Language header.
    /// </summary>
    /// <definition>
    /// public string? AcceptLanguage { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? AcceptLanguage { get => base[Header.AcceptLanguage]; set => base[Header.AcceptLanguage] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Patch header.
    /// </summary>
    /// <definition>
    /// public string? AcceptPatch { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? AcceptPatch { get => base[Header.AcceptPatch]; set => base[Header.AcceptPatch] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Ranges header.
    /// </summary>
    /// <definition>
    /// public string? AcceptRanges { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? AcceptRanges { get => base[Header.AcceptRanges]; set => base[Header.AcceptRanges] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Credentials header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    /// <definition>
    /// public string? AccessControlAllowCredentials { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? AccessControlAllowCredentials { get => base[Header.AccessControlAllowCredentials]; set => base[Header.AccessControlAllowCredentials] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Headers header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    /// <definition>
    /// public string? AccessControlAllowHeaders { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? AccessControlAllowHeaders { get => base[Header.AccessControlAllowHeaders]; set => base[Header.AccessControlAllowHeaders] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Methods header.
    /// </summary>
    /// <definition>
    /// public string? AccessControlAllowMethods { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? AccessControlAllowMethods { get => base[Header.AccessControlAllowMethods]; set => base[Header.AccessControlAllowMethods] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Origin header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    /// <definition>
    /// public string? AccessControlAllowOrigin { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? AccessControlAllowOrigin { get => base[Header.AccessControlAllowOrigin]; set => base[Header.AccessControlAllowOrigin] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Expose-Headers header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    /// <definition>
    /// public string? AccessControlExposeHeaders { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? AccessControlExposeHeaders { get => base[Header.AccessControlExposeHeaders]; set => base[Header.AccessControlExposeHeaders] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Max-Age header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    /// <definition>
    /// public string? AccessControlMaxAge { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? AccessControlMaxAge { get => base[Header.AccessControlMaxAge]; set => base[Header.AccessControlMaxAge] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Age header.
    /// </summary>
    /// <definition>
    /// public string? Age { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? Age { get => base[Header.Age]; set => base[Header.Age] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Allow header.
    /// </summary>
    /// <definition>
    /// public string? Allow { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? Allow { get => base[Header.Allow]; set => base[Header.Allow] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Alt-Svc header.
    /// </summary>
    /// <definition>
    /// public string? AltSvc { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? AltSvc { get => base[Header.AltSvc]; set => base[Header.AltSvc] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Authorization header.
    /// </summary>
    /// <definition>
    /// public string? Authorization { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? Authorization { get => base[Header.Authorization]; set => base[Header.Authorization] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Cache-Control header.
    /// </summary>
    /// <definition>
    /// public string? CacheControl { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? CacheControl { get => base[Header.CacheControl]; set => base[Header.CacheControl] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Disposition header.
    /// </summary>
    /// <definition>
    /// public string? ContentDisposition { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? ContentDisposition { get => base[Header.ContentDisposition]; set => base[Header.ContentDisposition] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Encoding header.
    /// </summary>
    /// <definition>
    /// public string? ContentEncoding { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? ContentEncoding { get => base[Header.ContentEncoding]; set => base[Header.ContentEncoding] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Language header.
    /// </summary>
    /// <definition>
    /// public string? ContentLanguage { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? ContentLanguage { get => base[Header.ContentLanguage]; set => base[Header.ContentLanguage] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Range header.
    /// </summary>
    /// <definition>
    /// public string? ContentRange { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? ContentRange { get => base[Header.ContentRange]; set => base[Header.ContentRange] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Type header.
    /// </summary>
    /// <remarks>
    /// Note: setting the value of this header, the value present in the response's <see cref="HttpContent"/> will be overwritten.
    /// </remarks>
    /// <definition>
    /// public string? ContentType { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? ContentType { get => base[Header.ContentType]; set => base[Header.ContentType] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Cookie header.
    /// </summary>
    /// <remarks>
    /// Tip: use <see cref="HttpRequest.Cookies"/> property to getting cookies values from requests and
    /// <see cref="CookieHelper.SetCookie(string, string)"/> on <see cref="HttpResponse"/> to set cookies.
    /// </remarks>
    /// <definition>
    /// public string? Cookie { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? Cookie { get => base[Header.Cookie]; set => base[Header.Cookie] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Expect header.
    /// </summary>
    /// <definition>
    /// public string? Expect { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? Expect { get => base[Header.Expect]; set => base[Header.Expect] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Expires header.
    /// </summary>
    /// <definition>
    /// public string? Expires { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? Expires { get => base[Header.Expires]; set => base[Header.Expires] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Host header.
    /// </summary>
    /// <definition>
    /// public string? Host { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? Host { get => base[Header.Host]; set => base[Header.Host] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Origin header.
    /// </summary>
    /// <definition>
    /// public string? Origin { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? Origin { get => base[Header.Origin]; set => base[Header.Origin] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Range header.
    /// </summary>
    /// <definition>
    /// public string? Range { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? Range { get => base[Header.Range]; set => base[Header.Range] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Referer header.
    /// </summary>
    /// <definition>
    /// public string? Referer { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? Referer { get => base[Header.Referer]; set => base[Header.Referer] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Retry-After header.
    /// </summary>
    /// <definition>
    /// public string? RetryAfter { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? RetryAfter { get => base[Header.RetryAfter]; set => base[Header.RetryAfter] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Set-Cookie header.
    /// </summary>
    /// <remarks>
    /// Tip: use <see cref="HttpRequest.Cookies"/> property to getting cookies values from requests and
    /// <see cref="CookieHelper.SetCookie(string, string)"/> on <see cref="HttpResponse"/> to set cookies.
    /// </remarks>
    /// <definition>
    /// public string? SetCookie { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? SetCookie { get => base[Header.SetCookie]; set => base[Header.SetCookie] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP User-Agent header.
    /// </summary>
    /// <definition>
    /// public string? UserAgent { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? UserAgent { get => base[Header.UserAgent]; set => base[Header.UserAgent] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Vary header.
    /// </summary>
    /// <definition>
    /// public string? Vary { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? Vary { get => base[Header.Vary]; set => base[Header.Vary] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP WWW-Authenticate header.
    /// </summary>
    /// <definition>
    /// public string? WWWAuthenticate { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? WWWAuthenticate { get => base[Header.WWWAuthenticate]; set => base[Header.WWWAuthenticate] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP X-Forwarded-For header.
    /// </summary>
    /// <remarks>
    /// Tip: enable the <see cref="HttpServerConfiguration.ResolveForwardedOriginAddress"/> property to obtain the user client proxied IP throught <see cref="HttpRequest.RemoteAddress"/>.
    /// </remarks>
    /// <definition>
    /// public string? XForwardedFor { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? XForwardedFor { get => base[Header.XForwardedFor]; set => base[Header.XForwardedFor] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP X-Forwarded-Host header.
    /// </summary>
    /// <remarks>
    /// Tip: enable the <see cref="HttpServerConfiguration.ResolveForwardedOriginHost"/> property to obtain the client requested host throught <see cref="HttpRequest.Host"/>.
    /// </remarks>
    /// <definition>
    /// public string? XForwardedHost { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? XForwardedHost { get => base[Header.XForwardedHost]; set => base[Header.XForwardedHost] = value; }

}
