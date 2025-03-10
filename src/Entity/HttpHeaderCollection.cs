// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHeaderCollection.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using Sisk.Core.Http;
using Header = Sisk.Core.Http.HttpKnownHeaderNames;

namespace Sisk.Core.Entity;

/// <summary>
/// Represents an collection of HTTP headers with their name and values.
/// </summary>
public sealed class HttpHeaderCollection : StringKeyStoreCollection {
    static readonly StringComparer _comparer = StringComparer.OrdinalIgnoreCase;

    /// <summary>
    /// Create an new instance of the <see cref="HttpHeaderCollection"/> class.
    /// </summary>
    public HttpHeaderCollection () : base ( _comparer ) {
    }

    /// <summary>
    /// Create an new instance of the <see cref="HttpHeaderCollection"/> class with values from another
    /// collection.
    /// </summary>
    /// <param name="items">The inner collection to add to this collection.</param>
    public HttpHeaderCollection ( IDictionary<string, string []> items ) : base ( _comparer ) {
        this.AddRange ( items );
    }

    /// <summary>
    /// Create an new instance of the <see cref="HttpHeaderCollection"/> class with values from another
    /// collection.
    /// </summary>
    /// <param name="items">The inner collection to add to this collection.</param>
    public HttpHeaderCollection ( IDictionary<string, string?> items ) : base ( _comparer ) {
        this.AddRange ( items );
    }

    /// <summary>
    /// Create an new instance of the <see cref="HttpHeaderCollection"/> class with values from another
    /// collection.
    /// </summary>
    /// <param name="items">The inner collection to add to this collection.</param>
    public HttpHeaderCollection ( WebHeaderCollection items ) : base ( _comparer ) {
        this.AddRange ( FromNameValueCollection ( items ) );
    }
    #region Helper properties

    /// <summary>
    /// Gets the value of the HTTP Accept header.
    /// <para>Specifies the media types that are acceptable for the response, allowing the client to indicate its preferences.</para>
    /// </summary>
    public string? Accept { get => this [ Header.Accept ]; }

    /// <summary>
    /// Gets the value of the HTTP Accept-Charset header.
    /// <para>Indicates the character sets that are acceptable for the response, allowing the client to specify its preferred encoding.</para>
    /// </summary>
    public string? AcceptCharset { get => this [ Header.AcceptCharset ]; }

    /// <summary>
    /// Gets the value of the HTTP Accept-Encoding header.
    /// <para>Specifies the content encodings that are acceptable for the response, allowing the client to indicate its preferences for compression.</para>
    /// </summary>
    public string? AcceptEncoding { get => this [ Header.AcceptEncoding ]; }

    /// <summary>
    /// Gets the value of the HTTP Accept-Language header.
    /// <para>Indicates the natural languages that are preferred for the response, allowing the client to specify its language preferences.</para>
    /// </summary>
    public string? AcceptLanguage { get => this [ Header.AcceptLanguage ]; }

    /// <summary>
    /// Gets the value of the HTTP Accept-Patch header.
    /// <para>Indicates the patch document formats that are acceptable for the response, allowing the client to specify its preferences for patching resources.</para>
    /// </summary>
    public string? AcceptPatch { get => this [ Header.AcceptPatch ]; }

    /// <summary>
    /// Gets or sets the value of the HTTP Accept-Ranges header.
    /// <para>Indicates that the server supports range requests for the resource, allowing clients to request specific byte ranges.</para>
    /// </summary>
    public string? AcceptRanges { get => this [ Header.AcceptRanges ]; set => this [ Header.AcceptRanges ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Credentials header.
    /// <para>Indicates whether the response to the request can expose credentials, allowing cross-origin requests to include credentials.</para>
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlAllowCredentials { get => this [ Header.AccessControlAllowCredentials ]; set => this [ Header.AccessControlAllowCredentials ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Headers header.
    /// <para>Specifies which headers can be used when making the actual request in a cross-origin resource sharing (CORS) context.</para>
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlAllowHeaders { get => this [ Header.AccessControlAllowHeaders ]; set => this [ Header.AccessControlAllowHeaders ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Methods header.
    /// <para>Specifies the methods that are allowed when accessing the resource in a CORS context.</para>
    /// </summary>
    public string? AccessControlAllowMethods { get => this [ Header.AccessControlAllowMethods ]; set => this [ Header.AccessControlAllowMethods ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Allow-Origin header.
    /// <para>Specifies which origins are allowed to access the resource in a CORS context, helping to control cross-origin requests.</para>
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlAllowOrigin { get => this [ Header.AccessControlAllowOrigin ]; set => this [ Header.AccessControlAllowOrigin ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Expose-Headers header.
    /// <para>Indicates which headers can be exposed as part of the response to a cross-origin request.</para>
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlExposeHeaders { get => this [ Header.AccessControlExposeHeaders ]; set => this [ Header.AccessControlExposeHeaders ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Access-Control-Max-Age header.
    /// <para>Specifies how long the results of a preflight request can be cached, reducing the number of preflight requests made.</para>
    /// </summary>
    /// <remarks>
    /// Note: this header can be overwritten by the current <see cref="CrossOriginResourceSharingHeaders"/> configuration.
    /// </remarks>
    public string? AccessControlMaxAge { get => this [ Header.AccessControlMaxAge ]; set => this [ Header.AccessControlMaxAge ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Age header.
    /// <para>Indicates the age of the object in a cache, helping clients understand how fresh the cached response is.</para>
    /// </summary>
    public string? Age { get => this [ Header.Age ]; set => this [ Header.Age ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Allow header.
    /// <para>Lists the HTTP methods that are supported by the resource, informing clients about the available actions.</para>
    /// </summary>
    public string? Allow { get => this [ Header.Allow ]; set => this [ Header.Allow ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Authorization header.
    /// <para>Contains credentials for authenticating the client with the server, often used for basic or bearer token authentication.</para>
    /// </summary>
    public string? Authorization { get => this [ Header.Authorization ]; set => this [ Header.Authorization ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Cache-Control header.
    /// <para>Directs caching mechanisms on how to cache the response, including directives for expiration and revalidation.</para>
    /// </summary>
    public string? CacheControl { get => this [ Header.CacheControl ]; set => this [ Header.CacheControl ] = value; }

    /// <summary>
    /// Gets the value of the HTTP Connection header. To set this header in a HTTP response, use the
    /// <see cref="HttpServerConfiguration.KeepAlive"/> property.
    /// </summary>
    public string? Connection { get => this [ Header.Connection ]; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Disposition header.
    /// <para>Indicates if the content should be displayed inline in the browser or treated as an attachment to be downloaded.</para>
    /// </summary>
    public string? ContentDisposition { get => this [ Header.ContentDisposition ]; set => this [ Header.ContentDisposition ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Encoding header.
    /// <para>Specifies the encoding transformations that have been applied to the response body, such as gzip or deflate. This
    /// header should not be interpreted as the content text charset.</para>
    /// </summary>
    public string? ContentEncoding { get => this [ Header.ContentEncoding ]; set => this [ Header.ContentEncoding ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Language header.
    /// <para>Indicates the natural language(s) of the intended audience for the response, helping clients understand the content's language.</para>
    /// </summary>
    public string? ContentLanguage { get => this [ Header.ContentLanguage ]; set => this [ Header.ContentLanguage ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Range header.
    /// <para>Indicates the size of the response body in bytes, allowing the client to know how much data to expect.</para>
    /// </summary>
    public string? ContentRange { get => this [ Header.ContentRange ]; set => this [ Header.ContentRange ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Location header.
    /// <para>Indicates an alternate location for the returned data, often used for redirecting clients to a different resource.</para>
    /// </summary>
    public string? ContentLocation { get => this [ Header.ContentLocation ]; set => this [ Header.ContentLocation ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-MD5 header.
    /// <para>Contains the MD5 hash of the response body in an base-64 format, allowing clients to verify the integrity of the received data.</para>
    /// </summary>
    public string? ContentMD5 { get => this [ Header.ContentMD5 ]; set => this [ Header.ContentMD5 ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Security-Policy header.
    /// <para>Defines security policies for the content, helping to prevent cross-site scripting (XSS) and other code injection attacks.</para>
    /// </summary>
    public string? ContentSecurityPolicy { get => this [ Header.ContentSecurityPolicy ]; set => this [ Header.ContentSecurityPolicy ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Content-Type header.
    /// <para>Indicates the media type of the resource, allowing the client to understand how to process the response body.</para>
    /// </summary>
    /// <remarks>
    /// Note: setting the value of this header, the value present in the response's <see cref="HttpContent"/> will be overwritten.
    /// </remarks>
    public string? ContentType { get => this [ Header.ContentType ]; set => this [ Header.ContentType ] = value; }

    /// <summary>
    /// Gets the value of the HTTP Cookie header.
    /// <para>Contains stored HTTP cookies previously sent by the server, allowing the server to identify the client on subsequent requests.</para>
    /// </summary>
    /// <remarks>
    /// Tip: use <see cref="HttpRequest.Cookies"/> property to getting cookies values from requests and
    /// <see cref="HttpResponse.SetCookie(string, string)"/> on <see cref="HttpResponse"/> to set cookies.
    /// </remarks>
    public string? Cookie { get => this [ Header.Cookie ]; }

    /// <summary>
    /// Gets or sets the value of the HTTP ETag header.
    /// <para>Provides a unique identifier for a specific version of a resource, allowing clients to cache and validate resources efficiently.</para>
    /// </summary>
    public string? ETag { get => this [ Header.ETag ]; set => this [ Header.ETag ] = value; }

    /// <summary>
    /// Gets the value of the HTTP Expect header.
    /// <para>Indicates that the client expects certain behaviors from the server, such as support for specific features or conditions.</para>
    /// </summary>
    public string? Expect { get => this [ Header.Expect ]; }

    /// <summary>
    /// Gets or sets the value of the HTTP Expires header.
    /// <para>Indicates the date and time after which the response is considered stale, helping clients manage caching.</para>
    /// </summary>
    public string? Expires { get => this [ Header.Expires ]; set => this [ Header.Expires ] = value; }

    /// <summary>
    /// Gets the value of the HTTP Host header.
    /// <para>Specifies the domain name of the server and the TCP port number on which the server is listening, allowing for virtual hosting.</para>
    /// </summary>
    public string? Host { get => this [ Header.Host ]; }

    /// <summary>
    /// Gets the value of the HTTP Origin header.
    /// <para>Indicates the origin of the request, helping servers implement CORS and manage cross-origin requests.</para>
    /// </summary>
    public string? Origin { get => this [ Header.Origin ]; }

    /// <summary>
    /// Gets the value of the HTTP Range header.
    /// <para>Used to request a specific range of bytes from a resource, allowing clients to download large files in parts.</para>
    /// </summary>
    public string? Range { get => this [ Header.Range ]; }

    /// <summary>
    /// Gets the value of the HTTP Referer header.
    /// <para>Indicates the URL of the resource from which the request originated, helping servers understand the source of traffic.</para>
    /// </summary>
    public string? Referer { get => this [ Header.Referer ]; }

    /// <summary>
    /// Gets or sets the value of the HTTP Retry-After header.
    /// <para>Indicates how long the client should wait before making a follow-up request, often used in rate limiting scenarios.</para>
    /// </summary>
    public string? RetryAfter { get => this [ Header.RetryAfter ]; set => this [ Header.RetryAfter ] = value; }

    /// <summary>
    /// Gets the value of the HTTP If-Match header.
    /// <para>Used to make a conditional request, allowing the client to specify that the request should only be processed if the resource matches the given ETag.</para>
    /// </summary>
    public string? IfMatch { get => this [ Header.IfMatch ]; }

    /// <summary>
    /// Gets the value of the HTTP If-None-Match header.
    /// <para>Used to make a conditional request, allowing the client to specify that the resource should only be returned if it has been modified since the given date.</para>
    /// </summary>
    public string? IfNoneMatch { get => this [ Header.IfNoneMatch ]; }

    /// <summary>
    /// Gets the value of the HTTP If-Range header.
    /// <para>Used to make a conditional range request, allowing the client to specify that the range should only be returned if the resource has not changed.</para>
    /// </summary>
    public string? IfRange { get => this [ Header.IfRange ]; }

    /// <summary>
    /// Gets the value of the HTTP If-Modified-Since header.
    /// <para>Used to make a conditional request, allowing the client to specify that the resource should only be returned if it has been modified since the given date.</para>
    /// </summary>
    public string? IfModifiedSince { get => this [ Header.IfModifiedSince ]; }

    /// <summary>
    /// Gets the value of the HTTP If-Unmodified-Since header.
    /// <para>Used to make a conditional request, allowing the client to specify that the resource should only be returned if it has not been modified since the given date.</para>
    /// </summary>
    public string? IfUnmodifiedSince { get => this [ Header.IfUnmodifiedSince ]; }

    /// <summary>
    /// Gets or sets the value of the HTTP Max-Forwards header.
    /// <para>Used in OPTIONS requests to limit the number of times the request can be forwarded by proxies.</para>
    /// </summary>
    public string? MaxForwards { get => this [ Header.MaxForwards ]; set => this [ Header.MaxForwards ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Pragma header.
    /// <para>Used to include implementation-specific directives that might apply to any recipient along the request/response chain.</para>
    /// </summary>
    public string? Pragma { get => this [ Header.Pragma ]; set => this [ Header.Pragma ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Proxy-Authorization header.
    /// <para>Contains credentials for authenticating the client with a proxy server, allowing access to the requested resource.</para>
    /// </summary>
    public string? ProxyAuthorization { get => this [ Header.ProxyAuthorization ]; set => this [ Header.ProxyAuthorization ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Proxy-Authenticate header.
    /// <para>Used by a proxy server to request authentication from the client, indicating the authentication method required.</para>
    /// </summary>
    public string? ProxyAuthenticate { get => this [ Header.ProxyAuthenticate ]; set => this [ Header.ProxyAuthenticate ] = value; }

    /// <summary>
    /// Gets the value of the HTTP TE header.
    /// <para>Indicates the transfer encodings that are acceptable for the response, allowing for content negotiation.</para>
    /// </summary>
    public string? TE { get => this [ Header.TE ]; }

    /// <summary>
    /// Gets or sets the value of the HTTP Via header.
    /// <para>Used to track message forwards and proxies, indicating the intermediate protocols and recipients involved in the request/response chain.</para>
    /// </summary>
    public string? Via { get => this [ Header.Via ]; set => this [ Header.Via ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Location header.
    /// <para>Indicates an alternate location for the returned data, often used for redirecting clients to a different resource.</para>
    /// </summary>
    public string? Location { get => this [ Header.Location ]; set => this [ Header.Location ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP Set-Cookie header.
    /// <para>Used to send cookies from the server to the client, allowing the server to store state information on the client.</para>
    /// </summary>
    /// <remarks>
    /// Note: setting this property, it will override all previous Set-Cookie headers. Use the <see cref="StringKeyStoreCollection.Add(string, string)"/> method
    /// to add more than one Set-Cookie header or use the <see cref="HttpResponse.SetCookie(string, string)"/> method.
    /// </remarks>
    public string? SetCookie { get => this [ Header.SetCookie ]; set => this [ Header.SetCookie ] = value; }

    /// <summary>
    /// Gets the value of the HTTP User-Agent header.
    /// <para>Contains information about the user agent (browser or application) making the request, including its version and platform.</para>
    /// </summary>
    public string? UserAgent { get => this [ Header.UserAgent ]; }

    /// <summary>
    /// Gets or sets the value of the HTTP Vary header.
    /// <para>Indicates that the response varies based on the value of the specified request headers, allowing for content negotiation.</para>
    /// </summary>
    public string? Vary { get => this [ Header.Vary ]; set => this [ Header.Vary ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP WWW-Authenticate header.
    /// <para>Used in response to a request for authentication, indicating the authentication method that should be used to access the resource.</para>
    /// </summary>
    public string? WWWAuthenticate { get => this [ Header.WWWAuthenticate ]; set => this [ Header.WWWAuthenticate ] = value; }

    /// <summary>
    /// Gets the value of the HTTP X-Forwarded-For header.
    /// <para>Used to identify the originating IP address of a client connecting to a web server through an HTTP proxy or load balancer.</para>
    /// </summary>
    /// <remarks>
    /// Tip: use the <see cref="HttpServerConfiguration.ForwardingResolver"/> property to obtain the user client proxied IP throught <see cref="HttpRequest.RemoteAddress"/>.
    /// </remarks>
    public string? XForwardedFor { get => this [ Header.XForwardedFor ]; }

    /// <summary>
    /// Gets the value of the HTTP X-Forwarded-Host header
    /// <para>Used to identify the original host requested by the client in the Host HTTP request header, often used in proxy setups.</para>
    /// </summary>
    /// <remarks>
    /// Tip: use the <see cref="HttpServerConfiguration.ForwardingResolver"/> property to obtain the client requested host throught <see cref="HttpRequest.Host"/>.
    /// </remarks>
    public string? XForwardedHost { get => this [ Header.XForwardedHost ]; }

    /// <summary>
    /// Gets or sets the value of the HTTP X-Frame-Options header.
    /// <para>Used to control whether a browser should be allowed to render a page in a iframe, frame, embed or object tag, helping to prevent clickjacking attacks.</para>
    /// </summary>
    public string? XFrameOptions { get => this [ Header.XFrameOptions ]; set => this [ Header.XFrameOptions ] = value; }

    /// <summary>
    /// Gets or sets the value of the HTTP X-UA-Compatible header.
    /// <para>Used to specify the document mode that Internet Explorer should use to render the page, helping to ensure compatibility with older versions.</para>
    /// </summary>
    public string? XUACompatible { get => this [ Header.XUACompatible ]; set => this [ Header.XUACompatible ] = value; }
    #endregion
}
