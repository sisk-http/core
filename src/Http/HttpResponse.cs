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
        [Obsolete("This method is deprecated and should not be used.")]
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
        [Obsolete("This method is deprecated and should not be used.")]
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
            foreach (var header in Headers)
            {
                sb.Append($"{header.Key}: {header.Value}");
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
                string? s = Content?.ReadAsStringAsync().Result;

                if (s is not null)
                {
                    if (s.Length < 8 * HttpServer.UnitKb)
                    {
                        sb.Append(s);
                    }
                    else
                    {
                        sb.Append($"| ({HttpServer.HumanReadableSize(s.Length)} bytes)");
                    }
                }
            }

            return sb.ToString();
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
        protected sealed override void SetCookieHeader(String name, String value)
        {
            Headers.Add(name, value);
        }
    }
}
