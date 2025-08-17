
// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CadenteHttpServerEngineResponse.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Sisk.Core.Http;
using Sisk.Core.Http.Engine;

namespace Sisk.Cadente.CoreEngine {

    /// <summary>
    /// Represents an HTTP response within the Cadente engine context.
    /// </summary>
    public sealed class CadenteHttpServerEngineResponse : HttpServerEngineContextResponse {
        private readonly HttpHostContext.HttpResponse _response;
        private readonly HttpHostContext _httpHostContext;
        private CadenteHttpServerEngineContext? _context;

        internal void SetContext ( CadenteHttpServerEngineContext context ) {
            _context = context;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CadenteHttpServerEngineResponse"/> class.
        /// </summary>
        /// <param name="response">The underlying HTTP response object.</param>
        /// <param name="httpHostContext">The HTTP host context.</param>
        public CadenteHttpServerEngineResponse ( HttpHostContext.HttpResponse response, HttpHostContext httpHostContext ) {
            _response = response;
            _httpHostContext = httpHostContext;
        }

        /// <inheritdoc/>
        public override int StatusCode { get => _response.StatusCode; set => _response.StatusCode = value; }
        /// <inheritdoc/>
        public override string StatusDescription { get => _response.StatusDescription; set => _response.StatusDescription = value; }
        /// <inheritdoc/>
        public override bool KeepAlive { get => _httpHostContext.KeepAlive; set => _httpHostContext.KeepAlive = value; }
        /// <inheritdoc/>
        public override bool SendChunked { get => _response.SendChunked; set => _response.SendChunked = value; }
        /// <inheritdoc/>
        public override long ContentLength64 {
            get {
                if (_response.ResponseStream is { CanSeek: true }) {
                    return _response.ResponseStream.Length;
                }
                else if (_response.Headers.FirstOrDefault ( h => h.Name.Equals ( HttpKnownHeaderNames.ContentLength, StringComparison.OrdinalIgnoreCase ) ) is { IsEmpty: false } contentLenHeader) {
                    return long.Parse ( contentLenHeader.Value );
                }
                else {
                    return 0;
                }
            }
            set {
                var hIndex = _response.Headers.FindIndex ( h => h.Name.Equals ( HttpKnownHeaderNames.ContentLength, StringComparison.OrdinalIgnoreCase ) );
                if (hIndex >= 0) {
                    _response.Headers.RemoveAt ( hIndex );
                }

                _response.Headers.Add ( new HttpHeader ( HttpKnownHeaderNames.ContentLength, value.ToString ( CultureInfo.InvariantCulture ) ) );
            }
        }

        /// <inheritdoc/>
        public override string? ContentType {
            get {
                var header = _response.Headers.FirstOrDefault ( h => h.Name.Equals ( HttpKnownHeaderNames.ContentType, StringComparison.OrdinalIgnoreCase ) );
                return header.Value;
            }
            set {
                if (string.IsNullOrEmpty ( value )) {
                    var hIndex = _response.Headers.FindIndex ( h => h.Name.Equals ( HttpKnownHeaderNames.ContentType, StringComparison.OrdinalIgnoreCase ) );
                    if (hIndex >= 0) {
                        _response.Headers.RemoveAt ( hIndex );
                    }
                }
                else {
                    _response.Headers.Add ( new HttpHeader ( HttpKnownHeaderNames.ContentType, value ) );
                }
            }
        }

        /// <inheritdoc/>
        public override WebHeaderCollection Headers {
            get {
                var headers = new WebHeaderCollection ();
                foreach (var header in _response.Headers) {
                    headers.Add ( header.Name, header.Value );
                }
                return headers;
            }
            set {
                _response.Headers.Clear ();
                foreach (var key in value.AllKeys) {
                    if (key is null)
                        continue;
                    _response.Headers.Add ( new HttpHeader ( key, value [ key ] ?? "" ) );
                }
            }
        }

        /// <inheritdoc/>
        public override Stream OutputStream => _response.GetResponseStream ();

        /// <inheritdoc/>
        public override void Abort () {
            throw new NotSupportedException ();
        }

        /// <inheritdoc/>
        public override void AppendHeader ( string name, string value ) {
            _response.Headers.Add ( new HttpHeader ( name, value ) );
        }

        /// <inheritdoc/>
        public override void Close () {
            _context?.CompleteProcessing ();
            _response.ResponseStream?.Close ();
        }

        /// <inheritdoc/>
        public override void Dispose () {
            _context?.CompleteProcessing ();
            _response.ResponseStream?.Dispose ();
        }
    }
}
