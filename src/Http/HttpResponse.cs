// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponse.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Text;
using Sisk.Core.Entity;
using Sisk.Core.Helpers;

namespace Sisk.Core.Http {
    /// <summary>
    /// Represents an HTTP Response.
    /// </summary>
    public class HttpResponse {
        internal const byte HTTPRESPONSE_EMPTY = 2;  // <- theres no reason for this to exist
        internal const byte HTTPRESPONSE_SERVER_REFUSE = 4;
        internal const byte HTTPRESPONSE_SERVER_CLOSE = 6;
        internal const byte HTTPRESPONSE_CLIENT_CLOSE = 32;
        internal const byte HTTPRESPONSE_UNHANDLED_EXCEPTION = 8;

        internal long? CalculedLength;

        /// <summary>
        /// Creates an <see cref="HttpResponse"/> object which closes the connection with the client immediately (ECONNRESET).
        /// </summary>
        public static HttpResponse Refuse () {
            return new HttpResponse ( HTTPRESPONSE_SERVER_REFUSE );
        }

        /// <summary>
        /// Gets or sets the HTTP status code and description for this HTTP response.
        /// </summary>
        public HttpStatusInformation Status { get; set; } = new HttpStatusInformation ();

        /// <summary>
        /// Gets or sets the <see cref="HttpHeaderCollection"/> instance of the HTTP response headers.
        /// </summary>
        public HttpHeaderCollection Headers { get; set; } = new HttpHeaderCollection ();

        /// <summary>
        /// Gets or sets the HTTP response body contents.
        /// </summary>
        public HttpContent? Content { get; set; }

        /// <summary>
        /// Gets or sets whether the HTTP response will be sent chunked. When setting this property to <see langword="true"></see>,
        /// the Content-Length header is automatically omitted.
        /// </summary>
        /// <remarks>
        /// The response is always sent as chunked when it is not possible to determine the size of the content to send.
        /// </remarks>
        public bool SendChunked { get; set; }

        internal byte internalStatus;

        internal HttpResponse ( byte internalStatus ) {
            this.internalStatus = internalStatus;
        }

        /// <summary>
        /// Gets a visual representation of this HTTP response.
        /// </summary>
        /// <param name="includeBody">Determines whether the message content will also be included in the return from this function.</param>
        public string GetRawHttpResponse ( bool includeBody = true ) {
            StringBuilder sb = new StringBuilder ();
            sb.AppendLine ( null, $"HTTP/1.1 {Status}" );
            foreach (var header in Headers) {
                sb.Append ( null, $"{header.Key}: {header.Value}" );
                sb.Append ( '\n' );
            }
            if (Content?.Headers is not null)
                foreach (var header in Content.Headers) {
                    sb.Append ( header.Key + ": " );
                    sb.Append ( string.Join ( ", ", header.Value ) );
                    sb.Append ( '\n' );
                }
            sb.Append ( '\n' );

            if (includeBody && Content is not StreamContent) {
                string? s = Content?.ReadAsStringAsync ().Result;

                if (s is not null) {
                    if (s.Length < 8 * SizeHelper.UnitKb) {
                        sb.Append ( s );
                    }
                    else {
                        sb.Append ( null, $"| ({SizeHelper.HumanReadableSize ( s.Length )})" );
                    }
                }
            }

            return sb.ToString ();
        }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with HTTP OK status
        /// code and no content.
        /// </summary>
        public HttpResponse () {
        }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code.
        /// </summary>
        /// <param name="status">The <see cref="HttpStatusCode"/> of this HTTP response.</param>
        public HttpResponse ( HttpStatusCode status ) : this ( status, null ) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code.
        /// </summary>
        /// <param name="status">The status code of this HTTP response.</param>
        public HttpResponse ( int status ) : this ( (HttpStatusCode) status, null ) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code and HTTP content.
        /// </summary>
        /// <param name="status">The status code of this HTTP response.</param>
        /// <param name="content">The response content, if any.</param>
        public HttpResponse ( int status, HttpContent? content ) : this ( (HttpStatusCode) status, content ) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given HTTP content, with default status code as 200 OK.
        /// </summary>
        /// <param name="content">The response content, if any.</param>
        public HttpResponse ( HttpContent? content ) : this ( HttpStatusCode.OK, content ) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instanec with given string content and status code as 200 OK.
        /// </summary>
        /// <param name="stringContent">The UTF-8 string content.</param>
        public HttpResponse ( string stringContent ) : this ( HttpStatusCode.OK, new StringContent ( stringContent ) ) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code and HTTP contents.
        /// </summary>
        /// <param name="status">The <see cref="HttpStatusCode"/> of this HTTP response.</param>
        /// <param name="content">The response content, if any.</param>
        public HttpResponse ( HttpStatusCode status, HttpContent? content ) {
            Status = status;
            Content = content;
        }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code.
        /// </summary>
        /// <param name="status">The <see cref="HttpStatusInformation"/> of this HTTP response.</param>
        public HttpResponse ( in HttpStatusInformation status ) : this () {
            Status = status;
        }

        #region Cookie setter helpers
        /// <summary>
        /// Sets a cookie and sends it in the response to be set by the client.
        /// </summary>
        /// <param name="cookie">The cookie object.</param>
        public void SetCookie ( Cookie cookie ) {
            Headers.Add ( HttpKnownHeaderNames.SetCookie, CookieHelper.BuildCookieHeaderValue ( cookie ) );
        }

        /// <summary>
        /// Sets a cookie and sends it in the response to be set by the client.
        /// </summary>
        /// <param name="name">The cookie name.</param>
        /// <param name="value">The cookie value.</param>
        public void SetCookie ( string name, string value ) {
            Headers.Add ( HttpKnownHeaderNames.SetCookie, CookieHelper.BuildCookieHeaderValue ( name, value ) );
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
        public void SetCookie ( string name,
            string value,
            DateTime? expires = null,
            TimeSpan? maxAge = null,
            string? domain = null,
            string? path = null,
            bool? secure = null,
            bool? httpOnly = null,
            string? sameSite = null ) {
            Headers.Add ( HttpKnownHeaderNames.SetCookie, CookieHelper.BuildCookieHeaderValue ( name, value, expires, maxAge, domain, path, secure, httpOnly, sameSite ) );
        }
        #endregion

        /// <inheritdoc/>
        public override string ToString () {
            return Status.ToString ();
        }

        /// <inheritdoc/>
        public override int GetHashCode () {
            return HashCode.Combine ( Status, Headers, Content );
        }

        /// <inheritdoc/>
        public override bool Equals ( object? obj ) {
            if (obj is HttpResponse res) {
                return res.GetHashCode () == GetHashCode ();
            }
            return false;
        }
    }
}