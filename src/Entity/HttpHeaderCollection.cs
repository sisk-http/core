// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHeaderCollection.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using System.Collections.Specialized;
using System.Text;
using Header = Sisk.Core.Internal.HttpKnownHeaderNames;

namespace Sisk.Core.Entity;

/// <summary>
/// Represents an collection of HTTP headers with their name and values.
/// </summary>
public sealed class HttpHeaderCollection : NameValueCollection
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

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (string key in this.Keys)
        {
            sb.AppendLine($"{key}: {this[key]}");
        }
        return sb.ToString();
    }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept header.
    /// </summary>
    public string? Accept { get => base[Header.Accept]; set => base[Header.Accept] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Charset header.
    /// </summary>
    public string? AcceptCharset { get => base[Header.AcceptCharset]; set => base[Header.AcceptCharset] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Encoding header.
    /// </summary>
    public string? AcceptEncoding { get => base[Header.AcceptEncoding]; set => base[Header.AcceptEncoding] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Language header.
    /// </summary>
    public string? AcceptLanguage { get => base[Header.AcceptLanguage]; set => base[Header.AcceptLanguage] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Patch header.
    /// </summary>
    public string? AcceptPatch { get => base[Header.AcceptPatch]; set => base[Header.AcceptPatch] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Ranges header.
    /// </summary>
    public string? AcceptRanges { get => base[Header.AcceptRanges]; set => base[Header.AcceptRanges] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Credentials header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlAllowCredentials { get => base[Header.AccessControlAllowCredentials]; set => base[Header.AccessControlAllowCredentials] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Headers header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlAllowHeaders { get => base[Header.AccessControlAllowHeaders]; set => base[Header.AccessControlAllowHeaders] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Methods header.
    /// </summary>
    public string? AccessControlAllowMethods { get => base[Header.AccessControlAllowMethods]; set => base[Header.AccessControlAllowMethods] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Origin header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlAllowOrigin { get => base[Header.AccessControlAllowOrigin]; set => base[Header.AccessControlAllowOrigin] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Expose-Headers header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlExposeHeaders { get => base[Header.AccessControlExposeHeaders]; set => base[Header.AccessControlExposeHeaders] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Max-Age header.
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlMaxAge { get => base[Header.AccessControlMaxAge]; set => base[Header.AccessControlMaxAge] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Age header.
    /// </summary>
    public string? Age { get => base[Header.Age]; set => base[Header.Age] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Allow header.
    /// </summary>
    public string? Allow { get => base[Header.Allow]; set => base[Header.Allow] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Alt-Svc header.
    /// </summary>
    public string? AltSvc { get => base[Header.AltSvc]; set => base[Header.AltSvc] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Authorization header.
    /// </summary>
    public string? Authorization { get => base[Header.Authorization]; set => base[Header.Authorization] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Cache-Control header.
    /// </summary>
    public string? CacheControl { get => base[Header.CacheControl]; set => base[Header.CacheControl] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Disposition header.
    /// </summary>
    public string? ContentDisposition { get => base[Header.ContentDisposition]; set => base[Header.ContentDisposition] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Encoding header.
    /// </summary>
    public string? ContentEncoding { get => base[Header.ContentEncoding]; set => base[Header.ContentEncoding] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Language header.
    /// </summary>
    public string? ContentLanguage { get => base[Header.ContentLanguage]; set => base[Header.ContentLanguage] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Range header.
    /// </summary>
    public string? ContentRange { get => base[Header.ContentRange]; set => base[Header.ContentRange] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Type header.
    /// </summary>
    /// <remarks>
    /// Note: setting the value of this header, the value present in the response's <see cref="HttpContent"/> will be overwritten.
    /// </remarks>
    public string? ContentType { get => base[Header.ContentType]; set => base[Header.ContentType] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Cookie header.
    /// </summary>
    /// <remarks>
    /// Tip: use <see cref="HttpRequest.Cookies"/> property to getting cookies values from requests and
    /// <see cref="CookieHelper.SetCookie(string, string)"/> on <see cref="HttpResponse"/> to set cookies.
    /// </remarks>
    public string? Cookie { get => base[Header.Cookie]; set => base[Header.Cookie] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Expect header.
    /// </summary>
    public string? Expect { get => base[Header.Expect]; set => base[Header.Expect] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Expires header.
    /// </summary>
    public string? Expires { get => base[Header.Expires]; set => base[Header.Expires] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Host header.
    /// </summary>
    public string? Host { get => base[Header.Host]; set => base[Header.Host] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Origin header.
    /// </summary>
    public string? Origin { get => base[Header.Origin]; set => base[Header.Origin] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Range header.
    /// </summary>
    public string? Range { get => base[Header.Range]; set => base[Header.Range] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Referer header.
    /// </summary>
    public string? Referer { get => base[Header.Referer]; set => base[Header.Referer] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Retry-After header.
    /// </summary>
    public string? RetryAfter { get => base[Header.RetryAfter]; set => base[Header.RetryAfter] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP If-Match header.
    /// </summary>
    public string? IfMatch { get => base[Header.IfMatch]; set => base[Header.IfMatch] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP If-None-Match header.
    /// </summary>
    public string? IfNoneMatch { get => base[Header.IfNoneMatch]; set => base[Header.IfNoneMatch] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP If-Range header.
    /// </summary>
    public string? IfRange { get => base[Header.IfRange]; set => base[Header.IfRange] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP If-Modified-Since header.
    /// </summary>
    public string? IfModifiedSince { get => base[Header.IfModifiedSince]; set => base[Header.IfModifiedSince] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP If-Unmodified-Since header.
    /// </summary>
    public string? IfUnmodifiedSince { get => base[Header.IfUnmodifiedSince]; set => base[Header.IfUnmodifiedSince] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Max-Forwards header.
    /// </summary>
    public string? MaxForwards { get => base[Header.MaxForwards]; set => base[Header.MaxForwards] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Pragma header.
    /// </summary>
    public string? Pragma { get => base[Header.Pragma]; set => base[Header.Pragma] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Proxy-Authorization header.
    /// </summary>
    public string? ProxyAuthorization { get => base[Header.ProxyAuthorization]; set => base[Header.ProxyAuthorization] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP TE header.
    /// </summary>
    public string? TE { get => base[Header.TE]; set => base[Header.TE] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Trailer header.
    /// </summary>
    public string? Trailer { get => base[Header.Trailer]; set => base[Header.Trailer] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Via header.
    /// </summary>
    public string? Via { get => base[Header.Via]; set => base[Header.Via] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Set-Cookie header.
    /// </summary>
    /// <remarks>
    /// Tip: use <see cref="HttpRequest.Cookies"/> property to getting cookies values from requests and
    /// <see cref="CookieHelper.SetCookie(string, string)"/> on <see cref="HttpResponse"/> to set cookies.
    /// </remarks>
    public string? SetCookie { get => base[Header.SetCookie]; set => base[Header.SetCookie] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP User-Agent header.
    /// </summary>
    public string? UserAgent { get => base[Header.UserAgent]; set => base[Header.UserAgent] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Vary header.
    /// </summary>
    public string? Vary { get => base[Header.Vary]; set => base[Header.Vary] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP WWW-Authenticate header.
    /// </summary>
    public string? WWWAuthenticate { get => base[Header.WWWAuthenticate]; set => base[Header.WWWAuthenticate] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP X-Forwarded-For header.
    /// </summary>
    /// <remarks>
    /// Tip: enable the <see cref="HttpServerConfiguration.ResolveForwardedOriginAddress"/> property to obtain the user client proxied IP throught <see cref="HttpRequest.RemoteAddress"/>.
    /// </remarks>
    public string? XForwardedFor { get => base[Header.XForwardedFor]; set => base[Header.XForwardedFor] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP X-Forwarded-Host header.
    /// </summary>
    /// <remarks>
    /// Tip: enable the <see cref="HttpServerConfiguration.ResolveForwardedOriginHost"/> property to obtain the client requested host throught <see cref="HttpRequest.Host"/>.
    /// </remarks>
    public string? XForwardedHost { get => base[Header.XForwardedHost]; set => base[Header.XForwardedHost] = value; }

}
