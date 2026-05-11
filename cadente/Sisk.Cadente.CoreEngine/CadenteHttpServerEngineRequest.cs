
// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CadenteHttpServerEngineRequest.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Sisk.Core.Http;
using Sisk.Core.Http.Engine;

namespace Sisk.Cadente.CoreEngine {

    /// <summary>
    /// Represents an HTTP request within the Cadente engine context.
    /// </summary>
    public sealed class CadenteHttpServerEngineRequest : HttpServerEngineContextRequest {
        private static readonly Version Http11Version = new Version ( 1, 1 );

        internal readonly HttpHostContext _context;
        private readonly HttpHostContext.HttpRequest _request;
        private Uri? _url;
        private NameValueCollection? _queryString;
        private NameValueCollection? _headers;
        private Guid _requestTraceIdentifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="CadenteHttpServerEngineRequest"/> class.
        /// </summary>
        /// <param name="request">The underlying HTTP request object.</param>
        /// <param name="context">The HTTP host context.</param>
        public CadenteHttpServerEngineRequest ( HttpHostContext.HttpRequest request, HttpHostContext context ) {
            _request = request;
            _context = context;
        }

        /// <inheritdoc/>
        public override bool IsLocal => IPAddress.IsLoopback ( _context.Client.ClientEndpoint.Address );

        /// <inheritdoc/>
        public override string? RawUrl => _request.Path;

        /// <inheritdoc/>
        public override NameValueCollection QueryString {
            get {
                if (_queryString is { }) {
                    return _queryString;
                }

                var query = new NameValueCollection ();
                string rawUrl = _request.Path;
                int queryStart = rawUrl.IndexOf ( '?', StringComparison.Ordinal );

                if (queryStart >= 0) {
                    var queryString = System.Web.HttpUtility.ParseQueryString ( rawUrl [ queryStart.. ] );
                    foreach (var key in queryString.AllKeys) {
                        query.Add ( key, queryString [ key ] );
                    }
                }

                return _queryString = query;
            }
        }

        /// <inheritdoc/>
        public override Version ProtocolVersion => Http11Version;

        /// <inheritdoc/>
        public override string UserHostName => GetRequestAuthority ();

        /// <inheritdoc/>
        public override Uri? Url => _url ??= CreateUrl ();

        /// <inheritdoc/>
        public override string HttpMethod => _request.Method;

        /// <inheritdoc/>
        public override IPEndPoint LocalEndPoint => _context.Host.Endpoint;

        /// <inheritdoc/>
        public override IPEndPoint RemoteEndPoint => _context.Client.ClientEndpoint;

        /// <inheritdoc/>
        public override Guid RequestTraceIdentifier {
            get {
                if (_requestTraceIdentifier == Guid.Empty) {
                    _requestTraceIdentifier = Guid.NewGuid ();
                }

                return _requestTraceIdentifier;
            }
        }

        /// <inheritdoc/>
        public override NameValueCollection Headers {
            get {
                if (_headers is { }) {
                    return _headers;
                }

                var headers = new NameValueCollection ();
                foreach (var header in _request.Headers) {
                    headers.Add ( header.Name, header.Value );
                }
                return _headers = headers;
            }
        }

        /// <inheritdoc/>
        public override Stream InputStream => new WrappedCompatibleNetworkStream ( _request.GetRequestStream () );

        /// <inheritdoc/>
        public override long ContentLength64 => _request.ContentLength;

        /// <inheritdoc/>
        public override bool IsSecureConnection => _context.Client.IsSecureConnection;

        /// <inheritdoc/>
        public override Encoding ContentEncoding {
            get {
                var contentTypeCharset = _request.Headers.FirstOrDefault ( h => h.Name.Equals ( HttpKnownHeaderNames.ContentType, StringComparison.OrdinalIgnoreCase ) );
                if (contentTypeCharset.IsEmpty) {
                    return Encoding.Default;
                }

                if (MediaTypeHeaderValue.TryParse ( contentTypeCharset.Value, out var contentTypeValue )) {
                    if (contentTypeValue.CharSet is null) {
                        return Encoding.Default;
                    }

                    try {
                        return Encoding.GetEncoding ( contentTypeValue.CharSet );
                    }
                    catch {
                        return Encoding.Default;
                    }
                }
                else {
                    return Encoding.Default;
                }
            }
        }

        private Uri CreateUrl () {
            string scheme = IsSecureConnection ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
            string authority = GetRequestAuthority ();

            if (!IsValidAuthority ( scheme, authority )) {
                throw new Sisk.Core.Http.HttpRequestException ( "Invalid Host header." );
            }

            if (!_request.Path.StartsWith ( "/", StringComparison.Ordinal )) {
                throw new Sisk.Core.Http.HttpRequestException ( "Invalid request target." );
            }

            return new Uri ( $"{scheme}://{authority}{_request.Path}" );
        }

        private string GetRequestAuthority () {
            string? host = _request.Headers.Get ( HttpHeaderName.Host ).FirstOrDefault ();
            if (!string.IsNullOrWhiteSpace ( host )) {
                return host.Trim ();
            }

            var localAddress = LocalEndPoint.Address;
            string localHost = localAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
                ? $"[{localAddress}]"
                : localAddress.ToString ();

            return $"{localHost}:{LocalEndPoint.Port}";
        }

        private static bool IsValidAuthority ( string scheme, string authority ) {
            if (string.IsNullOrWhiteSpace ( authority ) || authority.Contains ( '/' ) || authority.Contains ( '\\' ) || authority.Contains ( '@' )) {
                return false;
            }

            return Uri.TryCreate ( $"{scheme}://{authority}/", UriKind.Absolute, out Uri? parsed )
                && string.IsNullOrEmpty ( parsed.UserInfo )
                && parsed.AbsolutePath == "/"
                && string.IsNullOrEmpty ( parsed.Query )
                && string.IsNullOrEmpty ( parsed.Fragment );
        }
    }
}
