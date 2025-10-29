
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
        internal readonly HttpHostContext _context;
        private readonly HttpHostContext.HttpRequest _request;

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
                var query = new NameValueCollection ();
                var url = new Uri ( "http://localhost" + _request.Path );
                var queryString = System.Web.HttpUtility.ParseQueryString ( url.Query );
                foreach (var key in queryString.AllKeys) {
                    query.Add ( key, queryString [ key ] );
                }
                return query;
            }
        }

        /// <inheritdoc/>
        public override Version ProtocolVersion => new Version ( 1, 1 );

        /// <inheritdoc/>
        public override string UserHostName => throw new NotImplementedException ();

        /// <inheritdoc/>
        public override Uri? Url => new Uri ( "http://localhost" + _request.Path );

        /// <inheritdoc/>
        public override string HttpMethod => _request.Method;

        /// <inheritdoc/>
        public override IPEndPoint LocalEndPoint => _context.Host.Endpoint;

        /// <inheritdoc/>
        public override IPEndPoint RemoteEndPoint => _context.Client.ClientEndpoint;

        /// <inheritdoc/>
        public override Guid RequestTraceIdentifier { get; } = Guid.NewGuid ();

        /// <inheritdoc/>
        public override NameValueCollection Headers {
            get {
                var headers = new NameValueCollection ();
                foreach (var header in _request.Headers) {
                    headers.Add ( header.Name, header.Value );
                }
                return headers;
            }
        }

        /// <inheritdoc/>
        public override Stream InputStream => _request.GetRequestStream ();

        /// <inheritdoc/>
        public override long ContentLength64 => _request.ContentLength;

        /// <inheritdoc/>
        public override bool IsSecureConnection => _context.Client.ClientCertificate != null;

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
    }
}
