// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponse.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using Sisk.Core.Helpers;
using System.Net;
using System.Text;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents an HTTP Response.
    /// </summary>
    public class HttpResponse : CookieHelper
    {
        internal const byte HTTPRESPONSE_EMPTY = 2;  // <- theres no reason for this to exist
        internal const byte HTTPRESPONSE_SERVER_REFUSE = 4;
        internal const byte HTTPRESPONSE_SERVER_CLOSE = 6;
        internal const byte HTTPRESPONSE_CLIENT_CLOSE = 32;
        internal const byte HTTPRESPONSE_UNHANDLED_EXCEPTION = 8;

        internal long? CalculedLength = null;

        /// <summary>
        /// Creates an <see cref="HttpResponse"/> object which closes the connection with the client immediately (ECONNRESET).
        /// </summary>
        /// <returns></returns>
        public static HttpResponse Refuse()
        {
            return new HttpResponse(HTTPRESPONSE_SERVER_REFUSE);
        }

        /// <summary>
        /// Gets or sets the HTTP status code and description for this HTTP response.
        /// </summary>
        public HttpStatusInformation Status { get; set; } = new HttpStatusInformation();

        /// <summary>
        /// Gets or sets the <see cref="HttpHeaderCollection"/> instance of the HTTP response headers.
        /// </summary>
        public HttpHeaderCollection Headers { get; set; } = new HttpHeaderCollection();

        /// <summary>
        /// Gets or sets the HTTP response body contents.
        /// </summary>
        public HttpContent? Content { get; set; }

        /// <summary>
        /// Gets or sets whether the HTTP response can be sent chunked.
        /// </summary>
        /// <remarks>
        /// The response is always sent as chunked when it is not possible to determine the size of the content to send.
        /// </remarks>
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
            sb.AppendLine($"HTTP/1.1 {this.Status}");
            foreach (var header in this.Headers)
            {
                sb.Append($"{header.Key}: {header.Value}");
                sb.Append('\n');
            }
            if (this.Content?.Headers is not null)
                foreach (var header in this.Content.Headers)
                {
                    sb.Append(header.Key + ": ");
                    sb.Append(string.Join(", ", header.Value));
                    sb.Append('\n');
                }
            sb.Append('\n');

            if (includeBody && this.Content is not StreamContent)
            {
                string? s = this.Content?.ReadAsStringAsync().Result;

                if (s is not null)
                {
                    if (s.Length < 8 * SizeHelper.UnitKb)
                    {
                        sb.Append(s);
                    }
                    else
                    {
                        sb.Append($"| ({SizeHelper.HumanReadableSize(s.Length)})");
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with HTTP OK status
        /// code and no content.
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
            this.Status = status;
            this.Content = content;
        }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code.
        /// </summary>
        /// <param name="status">The <see cref="HttpStatusInformation"/> of this HTTP response.</param>
        public HttpResponse(in HttpStatusInformation status) : this()
        {
            this.Status = status;
        }

        /// <inheritdoc/>
        protected sealed override void SetCookieHeader(String name, String value)
        {
            this.Headers.Add(name, value);
        }
    }
}