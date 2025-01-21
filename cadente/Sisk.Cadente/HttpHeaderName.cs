// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHeaderName.cs
// Repository:  https://github.com/sisk-http/core

// The source code below was forked from the official dotnet runtime
// 
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Sisk.Core.Http;


static class HttpHeaderName {
    /// <summary>
    /// The HTTP Accept header.
    /// <para>Specifies the media types that are acceptable for the response, allowing the client to indicate its preferences.</para>
    /// </summary>
    public const string Accept = "Accept";

    /// <summary>
    /// The HTTP Accept-Charset header.
    /// <para>Indicates the character sets that are acceptable for the response, allowing the client to specify its preferred encoding.</para>
    /// </summary>
    public const string AcceptCharset = "Accept-Charset";

    /// <summary>
    /// The HTTP Accept-Encoding header.
    /// <para>Specifies the content encodings that are acceptable for the response, allowing the client to indicate its preferences for compression.</para>
    /// </summary>
    public const string AcceptEncoding = "Accept-Encoding";

    /// <summary>
    /// The HTTP Accept-Language header.
    /// <para>Indicates the natural languages that are preferred for the response, allowing the client to specify its language preferences.</para>
    /// </summary>
    public const string AcceptLanguage = "Accept-Language";

    /// <summary>
    /// The HTTP Accept-Patch header.
    /// <para>Indicates the patch document formats that are acceptable for the response, allowing the client to specify its preferences for patching resources.</para>
    /// </summary>
    public const string AcceptPatch = "Accept-Patch";

    /// <summary>
    /// The HTTP Accept-Ranges header.
    /// <para>Indicates that the server supports range requests for the resource, allowing clients to request specific byte ranges.</para>
    /// </summary>
    public const string AcceptRanges = "Accept-Ranges";

    /// <summary>
    /// The HTTP Access-Control-Allow-Credentials header.
    /// <para>Indicates whether the response to the request can expose credentials, allowing cross-origin requests to include credentials.</para>
    /// </summary>
    public const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";

    /// <summary>
    /// The HTTP Access-Control-Allow-Headers header.
    /// <para>Specifies which headers can be used when making the actual request in a cross-origin resource sharing (CORS) context.</para>
    /// </summary>
    public const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";

    /// <summary>
    /// The HTTP Access-Control-Allow-Methods header.
    /// <para>Specifies the methods that are allowed when accessing the resource in a CORS context.</para>
    /// </summary>
    public const string AccessControlAllowMethods = "Access-Control-Allow-Methods";

    /// <summary>
    /// The HTTP Access-Control-Allow-Origin header.
    /// <para>Specifies which origins are allowed to access the resource in a CORS context, helping to control cross-origin requests.</para>
    /// </summary>
    public const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";

    /// <summary>
    /// The HTTP Access-Control-Expose-Headers header.
    /// <para>Indicates which headers can be exposed as part of the response to a cross-origin request.</para>
    /// </summary>
    public const string AccessControlExposeHeaders = "Access-Control-Expose-Headers";

    /// <summary>
    /// The HTTP Access-Control-Max-Age header.
    /// <para>Specifies how long the results of a preflight request can be cached, reducing the number of preflight requests made.</para>
    /// </summary>
    public const string AccessControlMaxAge = "Access-Control-Max-Age";

    /// <summary>
    /// The HTTP Age header.
    /// <para>Indicates the age of the object in a cache, helping clients understand how fresh the cached response is.</para>
    /// </summary>
    public const string Age = "Age";

    /// <summary>
    /// The HTTP Allow header.
    /// <para>Lists the HTTP methods that are supported by the resource, informing clients about the available actions.</para>
    /// </summary>
    public const string Allow = "Allow";

    /// <summary>
    /// The HTTP Alt-Svc header.
    /// <para>Indicates that an alternative service is available for the resource, allowing clients to connect to a different server or protocol.</para>
    /// </summary>
    public const string AltSvc = "Alt-Svc";

    /// <summary>
    /// The HTTP Authorization header.
    /// <para>Contains credentials for authenticating the client with the server, often used for basic or bearer token authentication.</para>
    /// </summary>
    public const string Authorization = "Authorization";

    /// <summary>
    /// The HTTP Cache-Control header.
    /// <para>Directs caching mechanisms on how to cache the response, including directives for expiration and revalidation.</para>
    /// </summary>
    public const string CacheControl = "Cache-Control";

    /// <summary>
    /// The HTTP Connection header.
    /// <para>Controls whether the network connection stays open after the current transaction finishes, allowing for persistent connections.</para>
    /// </summary>
    public const string Connection = "Connection";

    /// <summary>
    /// The HTTP Content-Disposition header.
    /// <para>Indicates if the content should be displayed inline in the browser or treated as an attachment to be downloaded.</para>
    /// </summary>
    public const string ContentDisposition = "Content-Disposition";

    /// <summary>
    /// The HTTP Content-Encoding header.
    /// <para>Specifies the encoding transformations that have been applied to the response body, such as gzip or deflate.</para>
    /// </summary>
    public const string ContentEncoding = "Content-Encoding";

    /// <summary>
    /// The HTTP Content-Language header.
    /// <para>Indicates the natural language(s) of the intended audience for the response, helping clients understand the content's language.</para>
    /// </summary>
    public const string ContentLanguage = "Content-Language";

    /// <summary>
    /// The HTTP Content-Length header.
    /// <para>Indicates the size of the response body in bytes, allowing the client to know how much data to expect.</para>
    /// </summary>
    public const string ContentLength = "Content-Length";

    /// <summary>
    /// The HTTP Content-Location header.
    /// <para>Indicates an alternate location for the returned data, often used for redirecting clients to a different resource.</para>
    /// </summary>
    public const string ContentLocation = "Content-Location";

    /// <summary>
    /// The HTTP Content-MD5 header.
    /// <para>Contains the MD5 hash of the response body, allowing clients to verify the integrity of the received data.</para>
    /// </summary>
    public const string ContentMD5 = "Content-MD5";

    /// <summary>
    /// The HTTP Content-Range header.
    /// <para>Indicates the part of a document that the server is returning, used in range requests to specify byte ranges.</para>
    /// </summary>
    public const string ContentRange = "Content-Range";

    /// <summary>
    /// The HTTP Content-Security-Policy header.
    /// <para>Defines security policies for the content, helping to prevent cross-site scripting (XSS) and other code injection attacks.</para>
    /// </summary>
    public const string ContentSecurityPolicy = "Content-Security-Policy";

    /// <summary>
    /// The HTTP Content-Type header.
    /// <para>Indicates the media type of the resource, allowing the client to understand how to process the response body.</para>
    /// </summary>
    public const string ContentType = "Content-Type";

    /// <summary>
    /// The HTTP Cookie header.
    /// <para>Contains stored HTTP cookies previously sent by the server, allowing the server to identify the client on subsequent requests.</para>
    /// </summary>
    public const string Cookie = "Cookie";

    /// <summary>
    /// The HTTP Cookie2 header.
    /// <para>Used to send cookies in a more advanced format, primarily for compatibility with older versions of HTTP.</para>
    /// </summary>
    public const string Cookie2 = "Cookie2";

    /// <summary>
    /// The HTTP Date header.
    /// <para>Indicates the date and time at which the message was sent, helping clients understand the freshness of the response.</para>
    /// </summary>
    public const string Date = "Date";

    /// <summary>
    /// The HTTP ETag header.
    /// <para>Provides a unique identifier for a specific version of a resource, allowing clients to cache and validate resources efficiently.</para>
    /// </summary>
    public const string ETag = "ETag";

    /// <summary>
    /// The HTTP Expect header.
    /// <para>Indicates that the client expects certain behaviors from the server, such as support for specific features or conditions.</para>
    /// </summary>
    public const string Expect = "Expect";

    /// <summary>
    /// The HTTP Expires header.
    /// <para>Indicates the date and time after which the response is considered stale, helping clients manage caching.</para>
    /// </summary>
    public const string Expires = "Expires";

    /// <summary>
    /// The HTTP Host header.
    /// <para>Specifies the domain name of the server and the TCP port number on which the server is listening, allowing for virtual hosting.</para>
    /// </summary>
    public const string Host = "Host";

    /// <summary>
    /// The HTTP If-Match header.
    /// <para>Used to make a conditional request, allowing the client to specify that the request should only be processed if the resource matches the given ETag.</para>
    /// </summary>
    public const string IfMatch = "If-Match";

    /// <summary>
    /// The HTTP If-Modified-Since header.
    /// <para>Used to make a conditional request, allowing the client to specify that the resource should only be returned if it has been modified since the given date.</para>
    /// </summary>
    public const string IfModifiedSince = "If-Modified-Since";

    /// <summary>
    /// The HTTP If-None-Match header.
    /// <para>Used to make a conditional request, allowing the client to specify that the resource should only be returned if it does not match the given ETag.</para>
    /// </summary>
    public const string IfNoneMatch = "If-None-Match";

    /// <summary>
    /// The HTTP If-Range header.
    /// <para>Used to make a conditional range request, allowing the client to specify that the range should only be returned if the resource has not changed.</para>
    /// </summary>
    public const string IfRange = "If-Range";

    /// <summary>
    /// The HTTP If-Unmodified-Since header.
    /// <para>Used to make a conditional request, allowing the client to specify that the resource should only be returned if it has not been modified since the given date.</para>
    /// </summary>
    public const string IfUnmodifiedSince = "If-Unmodified-Since";

    /// <summary>
    /// The HTTP Keep-Alive header.
    /// <para>Used to specify parameters for persistent connections, allowing the client and server to maintain an open connection for multiple requests.</para>
    /// </summary>
    public const string KeepAlive = "Keep-Alive";

    /// <summary>
    /// The HTTP Last-Modified header.
    /// <para>Indicates the date and time at which the resource was last modified, helping clients determine if they need to refresh their cached version.</para>
    /// </summary>
    public const string LastModified = "Last-Modified";

    /// <summary>
    /// The HTTP Link header.
    /// <para>Used to provide relationships between the current resource and other resources, often used for navigation and linking.</para>
    /// </summary>
    public const string Link = "Link";

    /// <summary>
    /// The HTTP Location header.
    /// <para>Used in redirection responses to indicate the URL to which the client should redirect.</para>
    /// </summary>
    public const string Location = "Location";

    /// <summary>
    /// The HTTP Max-Forwards header.
    /// <para>Used in OPTIONS requests to limit the number of times the request can be forwarded by proxies.</para>
    /// </summary>
    public const string MaxForwards = "Max-Forwards";

    /// <summary>
    /// The HTTP Origin header.
    /// <para>Indicates the origin of the request, helping servers implement CORS and manage cross-origin requests.</para>
    /// </summary>
    public const string Origin = "Origin";

    /// <summary>
    /// The HTTP P3P header.
    /// <para>Used to indicate the privacy policy of the server, allowing clients to understand how their data will be handled.</para>
    /// </summary>
    public const string P3P = "P3P";

    /// <summary>
    /// The HTTP Pragma header.
    /// <para>Used to include implementation-specific directives that might apply to any recipient along the request/response chain.</para>
    /// </summary>
    public const string Pragma = "Pragma";

    /// <summary>
    /// The HTTP Proxy-Authenticate header.
    /// <para>Used by a proxy server to request authentication from the client, indicating the authentication method required.</para>
    /// </summary>
    public const string ProxyAuthenticate = "Proxy-Authenticate";

    /// <summary>
    /// The HTTP Proxy-Authorization header.
    /// <para>Contains credentials for authenticating the client with a proxy server, allowing access to the requested resource.</para>
    /// </summary>
    public const string ProxyAuthorization = "Proxy-Authorization";

    /// <summary>
    /// The HTTP Proxy-Connection header.
    /// <para>Used to control whether the network connection to the proxy server should be kept open after the current transaction.</para>
    /// </summary>
    public const string ProxyConnection = "Proxy-Connection";

    /// <summary>
    /// The HTTP Public-Key-Pins header.
    /// <para>Used to prevent man-in-the-middle attacks by specifying which public keys are valid for the server's certificate.</para>
    /// </summary>
    public const string PublicKeyPins = "Public-Key-Pins";

    /// <summary>
    /// The HTTP Range header.
    /// <para>Used to request a specific range of bytes from a resource, allowing clients to download large files in parts.</para>
    /// </summary>
    public const string Range = "Range";

    /// <summary>
    /// The HTTP Referer header.
    /// <para>Indicates the URL of the resource from which the request originated, helping servers understand the source of traffic.</para>
    /// </summary>
    public const string Referer = "Referer";

    /// <summary>
    /// The HTTP Retry-After header.
    /// <para>Indicates how long the client should wait before making a follow-up request, often used in rate limiting scenarios.</para>
    /// </summary>
    public const string RetryAfter = "Retry-After";

    /// <summary>
    /// The HTTP Sec-WebSocket-Accept header.
    /// <para>Used in the WebSocket handshake to confirm the server's acceptance of the connection request.</para>
    /// </summary>
    public const string SecWebSocketAccept = "Sec-WebSocket-Accept";

    /// <summary>
    /// The HTTP Sec-WebSocket-Extensions header.
    /// <para>Used to negotiate WebSocket extensions during the handshake, allowing for additional features and capabilities.</para>
    /// </summary>
    public const string SecWebSocketExtensions = "Sec-WebSocket-Extensions";

    /// <summary>
    /// The HTTP Sec-WebSocket-Key header.
    /// <para>Contains a base64-encoded value used to establish a WebSocket connection, ensuring the request is valid.</para>
    /// </summary>
    public const string SecWebSocketKey = "Sec-WebSocket-Key";

    /// <summary>
    /// The HTTP Sec-WebSocket-Protocol header.
    /// <para>Used to specify subprotocols that the client wishes to use during the WebSocket connection.</para>
    /// </summary>
    public const string SecWebSocketProtocol = "Sec-WebSocket-Protocol";

    /// <summary>
    /// The HTTP Sec-WebSocket-Version header.
    /// <para>Indicates the version of the WebSocket protocol that the client wishes to use.</para>
    /// </summary>
    public const string SecWebSocketVersion = "Sec-WebSocket-Version";

    /// <summary>
    /// The HTTP Server header.
    /// <para>Contains information about the server software handling the request, often used for informational purposes.</para>
    /// </summary>
    public const string Server = "Server";

    /// <summary>
    /// The HTTP Set-Cookie header.
    /// <para>Used to send cookies from the server to the client, allowing the server to store state information on the client.</para>
    /// </summary>
    public const string SetCookie = "Set-Cookie";

    /// <summary>
    /// The HTTP Set-Cookie2 header.
    /// <para>Used to send cookies in a more advanced format, primarily for compatibility with older versions of HTTP.</para>
    /// </summary>
    public const string SetCookie2 = "Set-Cookie2";

    /// <summary>
    /// The HTTP Strict-Transport-Security header.
    /// <para>Enforces secure (HTTPS) connections to the server, helping to prevent man-in-the-middle attacks.</para>
    /// </summary>
    public const string StrictTransportSecurity = "Strict-Transport-Security";

    /// <summary>
    /// The HTTP TE header.
    /// <para>Indicates the transfer encodings that are acceptable for the response, allowing for content negotiation.</para>
    /// </summary>
    public const string TE = "TE";

    /// <summary>
    /// The HTTP TSV header.
    /// <para>Used to indicate the type of data being sent in a transaction, often used in specific applications or protocols.</para>
    /// </summary>
    public const string TSV = "TSV";

    /// <summary>
    /// The HTTP Trailer header.
    /// <para>Indicates that the sender will include additional fields in the message trailer, which can be used for metadata.</para>
    /// </summary>
    public const string Trailer = "Trailer";

    /// <summary>
    /// The HTTP Transfer-Encoding header.
    /// <para>Specifies the form of encoding used to safely transfer the payload body to the user.</para>
    /// </summary>
    public const string TransferEncoding = "Transfer-Encoding";

    /// <summary>
    /// The HTTP Upgrade header.
    /// <para>Indicates that the client prefers to upgrade to a different protocol, such as switching from HTTP/1.1 to HTTP/2.</para>
    /// </summary>
    public const string Upgrade = "Upgrade";

    /// <summary>
    /// The HTTP Upgrade-Insecure-Requests header.
    /// <para>Indicates that the client prefers to receive an upgraded version of the resource over HTTPS instead of HTTP.</para>
    /// </summary>
    public const string UpgradeInsecureRequests = "Upgrade-Insecure-Requests";

    /// <summary>
    /// The HTTP User-Agent header.
    /// <para>Contains information about the user agent (browser or application) making the request, including its version and platform.</para>
    /// </summary>
    public const string UserAgent = "User-Agent";

    /// <summary>
    /// The HTTP Vary header.
    /// <para>Indicates that the response varies based on the value of the specified request headers, allowing for content negotiation.</para>
    /// </summary>
    public const string Vary = "Vary";

    /// <summary>
    /// The HTTP Via header.
    /// <para>Used to track message forwards and proxies, indicating the intermediate protocols and recipients involved in the request/response chain.</para>
    /// </summary>
    public const string Via = "Via";

    /// <summary>
    /// The HTTP WWW-Authenticate header.
    /// <para>Used in response to a request for authentication, indicating the authentication method that should be used to access the resource.</para>
    /// </summary>
    public const string WWWAuthenticate = "WWW-Authenticate";

    /// <summary>
    /// The HTTP Warning header.
    /// <para>Provides additional information about the status or transformation of a message, often used for caching and validation.</para>
    /// </summary>
    public const string Warning = "Warning";

    /// <summary>
    /// The HTTP X-Content-Duration header.
    /// <para>Specifies the duration of the content in seconds, often used for media files.</para>
    /// </summary>
    public const string XContentDuration = "X-Content-Duration";

    /// <summary>
    /// The HTTP X-Content-Type-Options header.
    /// <para>Used to prevent MIME type sniffing, ensuring that the browser respects the declared content type.</para>
    /// </summary>
    public const string XContentTypeOptions = "X-Content-Type-Options";

    /// <summary>
    /// The HTTP X-Frame-Options header.
    /// <para>Used to control whether a browser should be allowed to render a page in a iframe, frame, embed or object tag, helping to prevent clickjacking attacks.</para>
    /// </summary>
    public const string XFrameOptions = "X-Frame-Options";

    /// <summary>
    /// The HTTP X-Powered-By header.
    /// <para>Indicates the technology or framework that powers the web application, often used for informational purposes.</para>
    /// </summary>
    public const string XPoweredBy = "X-Powered-By";

    /// <summary>
    /// The HTTP X-Forwarded-Host header.
    /// <para>Used to identify the original host requested by the client in the Host HTTP request header, often used in proxy setups.</para>
    /// </summary>
    public const string XForwardedHost = "X-Forwarded-Host";

    /// <summary>
    /// The HTTP X-Forwarded-For header.
    /// <para>Used to identify the originating IP address of a client connecting to a web server through an HTTP proxy or load balancer.</para>
    /// </summary>
    public const string XForwardedFor = "X-Forwarded-For";

    /// <summary>
    /// The HTTP X-Request-ID header.
    /// <para>Used to uniquely identify a request for tracking and debugging purposes, often generated by the client or server.</para>
    /// </summary>
    public const string XRequestID = "X-Request-ID";

    /// <summary>
    /// The HTTP X-UA-Compatible header.
    /// <para>Used to specify the document mode that Internet Explorer should use to render the page, helping to ensure compatibility with older versions.</para>
    /// </summary>
    public const string XUACompatible = "X-UA-Compatible";
}