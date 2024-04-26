// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponse.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using Sisk.Core.Routing;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents an HTTP Response.
    /// </summary>
    public class HttpResponse : CookieHelper
    {
        internal const byte HTTPRESPONSE_EMPTY = 2;
        internal const byte HTTPRESPONSE_SERVER_REFUSE = 4;
        internal const byte HTTPRESPONSE_SERVER_CLOSE = 6;
        internal const byte HTTPRESPONSE_CLIENT_CLOSE = 32;
        internal const byte HTTPRESPONSE_ERROR = 8;
        internal long CalculedLength = -1;

        /// <summary>
        /// Creates an new empty <see cref="HttpResponse"/> with no status code or contents. This will cause to the HTTP server to close the
        /// connection between the server and the client and don't deliver any response.
        /// </summary>
        public static HttpResponse CreateEmptyResponse()
        {
            return new HttpResponse(HTTPRESPONSE_EMPTY);
        }

        /// <summary>
        /// Creates an new redirect <see cref="HttpResponse"/> with given location header.
        /// </summary>
        /// <param name="location">The absolute or relative URL path which the client must be redirected to.</param>
        public static HttpResponse CreateRedirectResponse(string location)
        {
            HttpResponse res = new HttpResponse();
            res.Status = System.Net.HttpStatusCode.MovedPermanently;
            res.Headers.Add("Location", location);

            return res;
        }

        /// <summary>
        /// Creates an new redirect <see cref="HttpResponse"/> which redirects to the route path defined in a action. The provided method must have a valid RouteAttribute attribute.
        /// </summary>
        /// <param name="action">The receiving action contains a RouteAttribute attribute and its method is GET or ANY.</param>
        public static HttpResponse CreateRedirectResponse(RouteAction action)
        {
            var definition = RouteDefinition.GetFromCallback(action);
            if (!definition.Method.HasFlag(RouteMethod.Get)) throw new InvalidOperationException(SR.HttpResponse_Redirect_NotMatchGet);
            return CreateRedirectResponse(definition.Path);
        }

        /// <summary>
        /// Gets or sets the HTTP status code and description for this HTTP response.
        /// </summary>
        public HttpStatusInformation StatusInformation { get; set; } = new HttpStatusInformation();

        /// <summary>
        /// Gets or sets the HTTP response status code.
        /// </summary>
        public HttpStatusCode Status { get => (HttpStatusCode)StatusInformation.StatusCode; set => StatusInformation = new HttpStatusInformation(value); }

        /// <summary>
        /// Gets a <see cref="HttpHeaderCollection"/> instance of the HTTP response headers.
        /// </summary>
        public HttpHeaderCollection Headers { get; private set; } = new HttpHeaderCollection();

        /// <summary>
        /// Gets or sets the HTTP response body contents.
        /// </summary>
        public HttpContent? Content { get; set; }

        /// <summary>
        /// Gets or sets whether the HTTP response can be sent chunked.
        /// </summary>
        public bool SendChunked { get; set; } = false;

        internal byte internalStatus = 0;

        internal HttpResponse(byte internalStatus)
        {
            this.internalStatus = internalStatus;
        }

        /// <summary>
        /// Gets the raw HTTP response message.
        /// </summary>
        /// <param name="includeBody">Determines whether the message content will also be included in the return from this function.</param>
        public string GetRawHttpResponse(bool includeBody = true)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"HTTP/1.1 {StatusInformation}");
            foreach (string header in Headers)
            {
                sb.Append(header + ": ");
                sb.Append(Headers[header]);
                sb.Append('\n');
            }
            if (Content?.Headers is not null)
                foreach (var header in Content.Headers)
                {
                    sb.Append(header.Key + ": ");
                    sb.Append(string.Join(", ", header.Value));
                    sb.Append('\n');
                }
            sb.Append('\n');

            if (includeBody && Content is not StreamContent)
            {
                sb.Append(Content?.ReadAsStringAsync().Result);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Sets an UTF-8 string in this <see cref="HttpResponse"/> content.
        /// </summary>
        /// <param name="content">The UTF-8 string containing the response body.</param>
        /// <returns>The self <see cref="HttpResponse"/> object.</returns>
        public HttpResponse WithContent(string content)
        {
            Content = new StringContent(content);
            return this;
        }

        /// <summary>
        /// Sets an content in this <see cref="HttpResponse"/> object.
        /// </summary>
        /// <param name="content">The HTTP content which implements <see cref="HttpContent"/>.</param>
        /// <returns>The self <see cref="HttpResponse"/> object.</returns>
        public HttpResponse WithContent(HttpContent content)
        {
            Content = content;
            return this;
        }

        /// <summary>
        /// Sets an HTTP header in this <see cref="HttpResponse"/> object.
        /// </summary>
        /// <param name="headerKey">The name of the header.</param>
        /// <param name="headerValue">The header value.</param>
        /// <returns>The self <see cref="HttpResponse"/> object.</returns>
        public HttpResponse WithHeader(string headerKey, string headerValue)
        {
            Headers.Set(headerKey, headerValue);
            return this;
        }

        /// <summary>
        /// Sets an array of HTTP header in this <see cref="HttpResponse"/> object.
        /// </summary>
        /// <param name="headers">An collection of headers, described by their value (header names) and keys (header values).</param>
        /// <returns>The self <see cref="HttpResponse"/> object.</returns>
        public HttpResponse WithHeader(NameValueCollection headers)
        {
            foreach (string key in headers.Keys)
                Headers.Set(key, headers[key]);
            return this;
        }

        /// <summary>
        /// Sets the HTTP response status code.
        /// </summary>
        /// <param name="status">The HTTP status code.</param>
        /// <returns>The self <see cref="HttpResponse"/> object.</returns>
        public HttpResponse WithStatus(int status)
        {
            Status = (HttpStatusCode)status;
            return this;
        }

        /// <summary>
        /// Sets the HTTP response status information from an <see cref="HttpStatusInformation"/> object.
        /// </summary>
        /// <param name="statusInformation">The status information.</param>
        /// <returns>The self <see cref="HttpResponse"/> object.</returns>
        public HttpResponse WithStatus(HttpStatusInformation statusInformation)
        {
            StatusInformation = statusInformation;
            return this;
        }

        /// <summary>
        /// Sets the HTTP response status code.
        /// </summary>
        /// <param name="status">The HTTP status code.</param>
        /// <returns>The self <see cref="HttpResponse"/> object.</returns>
        public HttpResponse WithStatus(HttpStatusCode status)
        {
            Status = status;
            return this;
        }

        /// <summary>
        /// Sets a cookie and sends it in the response to be set by the client.
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
        public HttpResponse WithCookie(string name, string value, DateTime? expires = null, TimeSpan? maxAge = null, string? domain = null, string? path = null, bool? secure = null, bool? httpOnly = null, string? sameSite = null)
        {
            SetCookie(name, value, expires, maxAge, domain, path, secure, httpOnly, sameSite);
            return this;
        }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with HTTP OK status code and no content.
        /// </summary>
        public HttpResponse()
        {
        }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code.
        /// </summary>
        /// <param name="status">The <see cref="HttpStatusCode"/> of this HTTP response.</param>
        public HttpResponse(HttpStatusCode status) : this(status, null) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code.
        /// </summary>
        /// <param name="status">The status code of this HTTP response.</param>
        public HttpResponse(int status) : this((HttpStatusCode)status, null) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code and HTTP content.
        /// </summary>
        /// <param name="status">The status code of this HTTP response.</param>
        /// <param name="content">The response content, if any.</param>
        public HttpResponse(int status, HttpContent? content) : this((HttpStatusCode)status, content) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given HTTP content, with default status code as 200 OK.
        /// </summary>
        /// <param name="content">The response content, if any.</param>
        public HttpResponse(HttpContent? content) : this(HttpStatusCode.OK, content) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instanec with given string content and status code as 200 OK.
        /// </summary>
        /// <param name="stringContent">The UTF-8 string content.</param>
        public HttpResponse(string stringContent) : this(HttpStatusCode.OK, new StringContent(stringContent)) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code and HTTP contents.
        /// </summary>
        /// <param name="status">The <see cref="HttpStatusCode"/> of this HTTP response.</param>
        /// <param name="content">The response content, if any.</param>
        public HttpResponse(HttpStatusCode status, HttpContent? content)
        {
            Status = status;
            Content = content;
        }

        /// <inheritdoc/>
        protected override void SetCookieHeader(String name, String value)
        {
            Headers.Set(name, value);
        }
    }
}
