// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpStatusInformation.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents a structure that holds an HTTP response status information, with it's status code and description.
    /// </summary>
    public readonly struct HttpStatusInformation : IEquatable<HttpStatusInformation>, IEquatable<HttpStatusCode>, IEquatable<int>
    {
        private readonly int __statusCode;
        private readonly string __description;

        /// <summary>
        /// Gets the short description of the HTTP message.
        /// </summary>
        /// <remarks>
        /// Custom status descriptions is only supported for plain HTTP/1.1 and 1.0 transfers.
        /// </remarks>
        public string Description
        {
            get => this.__description;
        }

        /// <summary>
        /// Gets the numeric HTTP status code of the HTTP message.
        /// </summary>
        public int StatusCode
        {
            get => this.__statusCode;
        }

        /// <summary>
        /// Creates an new <see cref="HttpStatusInformation"/> with default parameters (200 OK) status.
        /// </summary>
        public HttpStatusInformation()
        {
            this.__statusCode = 200;
            this.__description = "OK";
        }

        /// <summary>
        /// Creates an new <see cref="HttpStatusInformation"/> instance with given parameters.
        /// </summary>
        /// <param name="statusCode">Sets the numeric HTTP status code of the HTTP message.</param>
        public HttpStatusInformation(int statusCode)
        {
            ValidateStatusCode(statusCode);
            this.__statusCode = statusCode;
            this.__description = GetStatusCodeDescription(statusCode);
        }

        /// <summary>
        /// Creates an new <see cref="HttpStatusInformation"/> instance with given parameters.
        /// </summary>
        /// <param name="statusCode">Sets the numeric HTTP status code of the HTTP message.</param>
        public HttpStatusInformation(HttpStatusCode statusCode)
        {
            int s = (int)statusCode;
            ValidateStatusCode(s);
            this.__statusCode = s;
            this.__description = GetStatusCodeDescription(statusCode);
        }

        /// <summary>
        /// Creates an new <see cref="HttpStatusInformation"/> instance with given parameters.
        /// </summary>
        /// <remarks>
        /// Custom status descriptions is only supported for plain HTTP/1.1 and 1.0 transfers.
        /// </remarks>
        /// <param name="description">Sets the short description of the HTTP message.</param>
        /// <param name="statusCode">Sets the numeric HTTP status code of the HTTP message.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpStatusInformation(int statusCode, string description)
        {
            ValidateStatusCode(statusCode);
            ValidateDescription(description);
            this.__statusCode = statusCode;
            this.__description = description;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateStatusCode(int st)
        {
            if (st < 100 || st > 999)
                throw new ProtocolViolationException(SR.HttpStatusCode_IllegalStatusCode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateDescription(string s)
        {
            if (s.Length > 8192) throw new ProtocolViolationException(SR.HttpStatusCode_IllegalStatusReason);
        }

        /// <summary>
        /// Gets the description of the HTTP status based on its description.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        public static string GetStatusCodeDescription(int statusCode)
        {
            ValidateStatusCode(statusCode);
            return HttpStatusDescription.Get(statusCode);
        }

        /// <summary>
        /// Gets the description of the HTTP status based on its description.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        public static string GetStatusCodeDescription(HttpStatusCode statusCode)
        {
            return GetStatusCodeDescription((int)statusCode);
        }

        /// <summary>
        /// Gets an <see cref="HttpStatusCode"/> corresponding to this instance, or null if the HTTP status does not match any value.
        /// </summary>
        /// <returns>
        /// An <see cref="HttpStatusCode"/> or null if the HTTP status matches no entry on it.
        /// </returns>
        public HttpStatusCode? GetHttpStatusCode()
        {
            HttpStatusCode s = (HttpStatusCode)this.__statusCode;
            if (Enum.IsDefined(s))
            {
                return s;
            }
            return null;
        }

        /// <inheritdoc/>
        /// <exclude/>
        public readonly bool Equals(HttpStatusInformation other)
        {
            return other.__statusCode.Equals(this.__statusCode) && other.__description.Equals(this.__description, StringComparison.Ordinal);
        }

        /// <inheritdoc/>
        /// <exclude/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is HttpStatusInformation other)
            {
                return this.Equals(other);
            }
            return false;
        }

        /// <inheritdoc/>
        /// <exclude/>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.__statusCode, this.__description);
        }

        /// <summary>
        /// Gets an string representation of this HTTP Status Code.
        /// </summary>
        public override string ToString()
        {
            return $"{this.__statusCode} {this.__description}";
        }

        /// <inheritdoc/>
        /// <exclude/>
        public bool Equals(HttpStatusCode other)
        {
            return this.StatusCode == (int)other;
        }

        /// <inheritdoc/>
        /// <exclude/>
        public bool Equals(int other)
        {
            return this.__statusCode.Equals(other);
        }

        /// <inheritdoc/>
        /// <exclude/>
        public static implicit operator HttpStatusInformation(HttpStatusCode statusCode)
        {
            return new HttpStatusInformation(statusCode);
        }

        /// <inheritdoc/>
        /// <exclude/>
        public static implicit operator HttpStatusInformation(int statusCode)
        {
            return new HttpStatusInformation(statusCode);
        }

        /// <inheritdoc/>
        /// <exclude/>
        public static bool operator ==(HttpStatusInformation a, HttpStatusInformation b)
        {
            return a.Equals(b);
        }

        /// <inheritdoc/>
        /// <exclude/>
        public static bool operator !=(HttpStatusInformation a, HttpStatusInformation b)
        {
            return !a.Equals(b);
        }

        /// <inheritdoc/>
        /// <exclude/>
        public static bool operator ==(HttpStatusInformation a, int b)
        {
            return a.Equals(b);
        }

        /// <inheritdoc/>
        /// <exclude/>
        public static bool operator !=(HttpStatusInformation a, int b)
        {
            return !a.Equals(b);
        }

        /// <inheritdoc/>
        /// <exclude/>
        public static bool operator ==(HttpStatusInformation a, HttpStatusCode b)
        {
            return a.Equals(b);
        }

        /// <inheritdoc/>
        /// <exclude/>
        public static bool operator !=(HttpStatusInformation a, HttpStatusCode b)
        {
            return a.Equals(b);
        }

        #region "Helper properties"

        // 1xx ----------------------------------------------------

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 100 Continue status.
        /// </summary>
        public static HttpStatusInformation Continue { get; } = new HttpStatusInformation(100);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 101 Switching Protocols status.
        /// </summary>
        public static HttpStatusInformation SwitchingProtocols { get; } = new HttpStatusInformation(101);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 102 Processing status.
        /// </summary>
        public static HttpStatusInformation Processing { get; } = new HttpStatusInformation(102);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 103 Early Hints status.
        /// </summary>
        public static HttpStatusInformation EarlyHints { get; } = new HttpStatusInformation(103);

        // 2xx ----------------------------------------------------

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 200 OK status.
        /// </summary>
        public static HttpStatusInformation Ok { get; } = new HttpStatusInformation(200);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 201 Created status.
        /// </summary>
        public static HttpStatusInformation Created { get; } = new HttpStatusInformation(201);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 202 Accepted status.
        /// </summary>
        public static HttpStatusInformation Accepted { get; } = new HttpStatusInformation(202);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 203 Non-Authoritative Information status.
        /// </summary>
        public static HttpStatusInformation NonAuthoritativeInformation { get; } = new HttpStatusInformation(203);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 204 No Content status.
        /// </summary>
        public static HttpStatusInformation NoContent { get; } = new HttpStatusInformation(204);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 205 Reset Content status.
        /// </summary>
        public static HttpStatusInformation ResetContent { get; } = new HttpStatusInformation(205);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 206 Partial Content status.
        /// </summary>
        public static HttpStatusInformation PartialContent { get; } = new HttpStatusInformation(206);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 207 Multi-Status status.
        /// </summary>
        public static HttpStatusInformation MultiStatus { get; } = new HttpStatusInformation(207);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 208 Already Reported status.
        /// </summary>
        public static HttpStatusInformation AlreadyReported { get; } = new HttpStatusInformation(208);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 226 IM Used status.
        /// </summary>
        public static HttpStatusInformation ImUsed { get; } = new HttpStatusInformation(226);

        // 3xx ----------------------------------------------------

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 300 Multiple Choices status.
        /// </summary>
        public static HttpStatusInformation MultipleChoices { get; } = new HttpStatusInformation(300);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 301 Moved Permanently status.
        /// </summary>
        public static HttpStatusInformation MovedPermanently { get; } = new HttpStatusInformation(301);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 302 Found status.
        /// </summary>
        public static HttpStatusInformation Found { get; } = new HttpStatusInformation(302);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 303 See Other status.
        /// </summary>
        public static HttpStatusInformation SeeOther { get; } = new HttpStatusInformation(303);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 304 Not Modified status.
        /// </summary>
        public static HttpStatusInformation NotModified { get; } = new HttpStatusInformation(304);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 305 Use Proxy status.
        /// </summary>
        public static HttpStatusInformation UseProxy { get; } = new HttpStatusInformation(305);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 306 Switch Proxy status.
        /// </summary>
        public static HttpStatusInformation SwitchProxy { get; } = new HttpStatusInformation(306);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 307 Temporary Redirect status.
        /// </summary>
        public static HttpStatusInformation TemporaryRedirect { get; } = new HttpStatusInformation(307);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 308 Permanent Redirect status.
        /// </summary>
        public static HttpStatusInformation PermanentRedirect { get; } = new HttpStatusInformation(308);

        // 4xx ----------------------------------------------------

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 400 Bad Request status.
        /// </summary>
        public static HttpStatusInformation BadRequest { get; } = new HttpStatusInformation(400);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 401 Unauthorized status.
        /// </summary>
        public static HttpStatusInformation Unauthorized { get; } = new HttpStatusInformation(401);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 402 Payment Required status.
        /// </summary>
        public static HttpStatusInformation PaymentRequired { get; } = new HttpStatusInformation(402);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 403 Forbidden status.
        /// </summary>
        public static HttpStatusInformation Forbidden { get; } = new HttpStatusInformation(403);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 404 Not Found status.
        /// </summary>
        public static HttpStatusInformation NotFound { get; } = new HttpStatusInformation(404);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 405 Method Not Allowed status.
        /// </summary>
        public static HttpStatusInformation MethodNotAllowed { get; } = new HttpStatusInformation(405);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 406 Not Acceptable status.
        /// </summary>
        public static HttpStatusInformation NotAcceptable { get; } = new HttpStatusInformation(406);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 407 Proxy Authentication Required status.
        /// </summary>
        public static HttpStatusInformation ProxyAuthenticationRequired { get; } = new HttpStatusInformation(407);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 408 Request Timeout status.
        /// </summary>
        public static HttpStatusInformation RequestTimeout { get; } = new HttpStatusInformation(408);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 409 Conflict status.
        /// </summary>
        public static HttpStatusInformation Conflict { get; } = new HttpStatusInformation(409);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 410 Gone status.
        /// </summary>
        public static HttpStatusInformation Gone { get; } = new HttpStatusInformation(410);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 411 Length Required status.
        /// </summary>
        public static HttpStatusInformation LengthRequired { get; } = new HttpStatusInformation(411);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 412 Precondition Failed status.
        /// </summary>
        public static HttpStatusInformation PreconditionFailed { get; } = new HttpStatusInformation(412);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 413 Payload Too Large status.
        /// </summary>
        public static HttpStatusInformation PayloadTooLarge { get; } = new HttpStatusInformation(413);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 414 URI Too Long status.
        /// </summary>
        public static HttpStatusInformation UriTooLong { get; } = new HttpStatusInformation(414);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 415 Unsupported Media Type status.
        /// </summary>
        public static HttpStatusInformation UnsupportedMediaType { get; } = new HttpStatusInformation(415);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 416 Range Not Satisfiable status.
        /// </summary>
        public static HttpStatusInformation RangeNotSatisfiable { get; } = new HttpStatusInformation(416);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 417 Expectation Failed status.
        /// </summary>
        public static HttpStatusInformation ExpectationFailed { get; } = new HttpStatusInformation(417);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 418 I'm a teapot status.
        /// </summary>
        public static HttpStatusInformation ImATeapot { get; } = new HttpStatusInformation(418);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 421 Misdirected Request status.
        /// </summary>
        public static HttpStatusInformation MisdirectedRequest { get; } = new HttpStatusInformation(421);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 422 Unprocessable Entity status.
        /// </summary>
        public static HttpStatusInformation UnprocessableEntity { get; } = new HttpStatusInformation(422);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 423 Locked status.
        /// </summary>
        public static HttpStatusInformation Locked { get; } = new HttpStatusInformation(423);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 424 Failed Dependency status.
        /// </summary>
        public static HttpStatusInformation FailedDependency { get; } = new HttpStatusInformation(424);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 426 Upgrade Required status.
        /// </summary>
        public static HttpStatusInformation UpgradeRequired { get; } = new HttpStatusInformation(426);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 428 Precondition Required status.
        /// </summary>
        public static HttpStatusInformation PreconditionRequired { get; } = new HttpStatusInformation(428);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 429 Too Many Requests status.
        /// </summary>
        public static HttpStatusInformation TooManyRequests { get; } = new HttpStatusInformation(429);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 431 Request Header Fields Too Large status.
        /// </summary>
        public static HttpStatusInformation RequestHeaderFieldsTooLarge { get; } = new HttpStatusInformation(431);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 451 Unavailable For Legal Reasons status.
        /// </summary>
        public static HttpStatusInformation UnavailableForLegalReasons { get; } = new HttpStatusInformation(451);

        // 5xx ----------------------------------------------------

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 500 Internal Server Error status.
        /// </summary>
        public static HttpStatusInformation InternalServerError { get; } = new HttpStatusInformation(500);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 501 Not Implemented status.
        /// </summary>
        public static HttpStatusInformation NotImplemented { get; } = new HttpStatusInformation(501);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 502 Bad Gateway status.
        /// </summary>
        public static HttpStatusInformation BadGateway { get; } = new HttpStatusInformation(502);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 503 Service Unavailable status.
        /// </summary>
        public static HttpStatusInformation ServiceUnavailable { get; } = new HttpStatusInformation(503);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 504 Gateway Timeout status.
        /// </summary>
        public static HttpStatusInformation GatewayTimeout { get; } = new HttpStatusInformation(504);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 505 HTTP Version Not Supported status.
        /// </summary>
        public static HttpStatusInformation HttpVersionNotSupported { get; } = new HttpStatusInformation(505);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 506 Variant Also Negotiates status.
        /// </summary>
        public static HttpStatusInformation VariantAlsoNegotiates { get; } = new HttpStatusInformation(506);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 507 Insufficient Storage status.
        /// </summary>
        public static HttpStatusInformation InsufficientStorage { get; } = new HttpStatusInformation(507);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 508 Loop Detected status.
        /// </summary>
        public static HttpStatusInformation LoopDetected { get; } = new HttpStatusInformation(508);

        /// <summary>
        /// Gets an <see cref="HttpStatusInformation"/> with an HTTP 510 Not Extended status.
        /// </summary>
        public static HttpStatusInformation NotExtended { get; } = new HttpStatusInformation(510);
        #endregion
    }
}
