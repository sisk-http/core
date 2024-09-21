// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHeaderCollection.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Header = Sisk.Core.Http.HttpKnownHeaderNames;

namespace Sisk.Core.Entity;

/// <summary>
/// Represents an collection of HTTP headers with their name and values.
/// </summary>
public sealed class HttpHeaderCollection : StringKeyStore
{
    /// <summary>
    /// Create an new instance of the <see cref="HttpHeaderCollection"/> class.
    /// </summary>
    public HttpHeaderCollection() : base(StringComparer.InvariantCultureIgnoreCase)
    {
    }

    #region Helper properties

    /// <summary>
    /// Gets or sets the value of the HTTP Accept header.
    /// </summary>
    public string? Accept { get => this[Header.Accept]; set => this[Header.Accept] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Charset header.
    /// </summary>
    public string? AcceptCharset { get => this[Header.AcceptCharset]; set => this[Header.AcceptCharset] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Encoding header.
    /// </summary>
    public string? AcceptEncoding { get => this[Header.AcceptEncoding]; set => this[Header.AcceptEncoding] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Language header.
    /// </summary>
    public string? AcceptLanguage { get => this[Header.AcceptLanguage]; set => this[Header.AcceptLanguage] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Patch header.
    /// </summary>
    public string? AcceptPatch { get => this[Header.AcceptPatch]; set => this[Header.AcceptPatch] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Ranges header.
    /// </summary>
    public string? AcceptRanges { get => this[Header.AcceptRanges]; set => this[Header.AcceptRanges] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Credentials header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlAllowCredentials { get => this[Header.AccessControlAllowCredentials]; set => this[Header.AccessControlAllowCredentials] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Headers header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlAllowHeaders { get => this[Header.AccessControlAllowHeaders]; set => this[Header.AccessControlAllowHeaders] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Methods header.
    /// </summary>
    public string? AccessControlAllowMethods { get => this[Header.AccessControlAllowMethods]; set => this[Header.AccessControlAllowMethods] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Origin header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlAllowOrigin { get => this[Header.AccessControlAllowOrigin]; set => this[Header.AccessControlAllowOrigin] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Expose-Headers header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlExposeHeaders { get => this[Header.AccessControlExposeHeaders]; set => this[Header.AccessControlExposeHeaders] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Max-Age header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlMaxAge { get => this[Header.AccessControlMaxAge]; set => this[Header.AccessControlMaxAge] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Age header.
    /// </summary>
    public string? Age { get => this[Header.Age]; set => this[Header.Age] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Allow header.
    /// </summary>
    public string? Allow { get => this[Header.Allow]; set => this[Header.Allow] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Alt-Svc header.
    /// </summary>
    public string? AltSvc { get => this[Header.AltSvc]; set => this[Header.AltSvc] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Authorization header.
    /// </summary>
    public string? Authorization { get => this[Header.Authorization]; set => this[Header.Authorization] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Cache-Control header.
    /// </summary>
    public string? CacheControl { get => this[Header.CacheControl]; set => this[Header.CacheControl] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Disposition header.
    /// </summary>
    public string? ContentDisposition { get => this[Header.ContentDisposition]; set => this[Header.ContentDisposition] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Encoding header.
    /// </summary>
    public string? ContentEncoding { get => this[Header.ContentEncoding]; set => this[Header.ContentEncoding] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Language header.
    /// </summary>
    public string? ContentLanguage { get => this[Header.ContentLanguage]; set => this[Header.ContentLanguage] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Range header.
    /// </summary>
    public string? ContentRange { get => this[Header.ContentRange]; set => this[Header.ContentRange] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Type header.
    /// </summary>
    /// <remarks>
    /// Note: setting the value of this header, the value present in the response's <see cref="HttpContent"/> will be overwritten.
    /// </remarks>
    public string? ContentType { get => this[Header.ContentType]; set => this[Header.ContentType] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Cookie header.
    /// </summary>
    /// <remarks>
    /// Tip: use <see cref="HttpRequest.Cookies"/> property to getting cookies values from requests and
    /// <see cref="CookieHelper.SetCookie(string, string)"/> on <see cref="HttpResponse"/> to set cookies.
    /// </remarks>
    public string? Cookie { get => this[Header.Cookie]; set => this[Header.Cookie] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Expect header.
    /// </summary>
    public string? Expect { get => this[Header.Expect]; set => this[Header.Expect] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Expires header.
    /// </summary>
    public string? Expires { get => this[Header.Expires]; set => this[Header.Expires] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Host header.
    /// </summary>
    public string? Host { get => this[Header.Host]; set => this[Header.Host] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Origin header.
    /// </summary>
    public string? Origin { get => this[Header.Origin]; set => this[Header.Origin] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Range header.
    /// </summary>
    public string? Range { get => this[Header.Range]; set => this[Header.Range] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Referer header.
    /// </summary>
    public string? Referer { get => this[Header.Referer]; set => this[Header.Referer] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Retry-After header.
    /// </summary>
    public string? RetryAfter { get => this[Header.RetryAfter]; set => this[Header.RetryAfter] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP If-Match header.
    /// </summary>
    public string? IfMatch { get => this[Header.IfMatch]; set => this[Header.IfMatch] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP If-None-Match header.
    /// </summary>
    public string? IfNoneMatch { get => this[Header.IfNoneMatch]; set => this[Header.IfNoneMatch] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP If-Range header.
    /// </summary>
    public string? IfRange { get => this[Header.IfRange]; set => this[Header.IfRange] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP If-Modified-Since header.
    /// </summary>
    public string? IfModifiedSince { get => this[Header.IfModifiedSince]; set => this[Header.IfModifiedSince] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP If-Unmodified-Since header.
    /// </summary>
    public string? IfUnmodifiedSince { get => this[Header.IfUnmodifiedSince]; set => this[Header.IfUnmodifiedSince] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Max-Forwards header.
    /// </summary>
    public string? MaxForwards { get => this[Header.MaxForwards]; set => this[Header.MaxForwards] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Pragma header.
    /// </summary>
    public string? Pragma { get => this[Header.Pragma]; set => this[Header.Pragma] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Proxy-Authorization header.
    /// </summary>
    public string? ProxyAuthorization { get => this[Header.ProxyAuthorization]; set => this[Header.ProxyAuthorization] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP TE header.
    /// </summary>
    public string? TE { get => this[Header.TE]; set => this[Header.TE] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Trailer header.
    /// </summary>
    public string? Trailer { get => this[Header.Trailer]; set => this[Header.Trailer] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Via header.
    /// </summary>
    public string? Via { get => this[Header.Via]; set => this[Header.Via] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Set-Cookie header.
    /// </summary>
    public string? SetCookie { get => this[Header.SetCookie]; set => this[Header.SetCookie] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP User-Agent header.
    /// </summary>
    public string? UserAgent { get => this[Header.UserAgent]; set => this[Header.UserAgent] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Vary header.
    /// </summary>
    public string? Vary { get => this[Header.Vary]; set => this[Header.Vary] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP WWW-Authenticate header.
    /// </summary>
    public string? WWWAuthenticate { get => this[Header.WWWAuthenticate]; set => this[Header.WWWAuthenticate] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP X-Forwarded-For header.
    /// </summary>
    /// <remarks>
    /// Tip: enable the <see cref="HttpServerConfiguration.ResolveForwardedOriginAddress"/> property to obtain the user client proxied IP throught <see cref="HttpRequest.RemoteAddress"/>.
    /// </remarks>
    public string? XForwardedFor { get => this[Header.XForwardedFor]; set => this[Header.XForwardedFor] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP X-Forwarded-Host header.
    /// </summary>
    /// <remarks>
    /// Tip: enable the <see cref="HttpServerConfiguration.ResolveForwardedOriginHost"/> property to obtain the client requested host throught <see cref="HttpRequest.Host"/>.
    /// </remarks>
    public string? XForwardedHost { get => this[Header.XForwardedHost]; set => this[Header.XForwardedHost] = value; }

    #endregion
}
