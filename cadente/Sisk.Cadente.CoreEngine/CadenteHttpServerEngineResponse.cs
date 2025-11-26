
// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CadenteHttpServerEngineResponse.cs
// Repository:  https://github.com/sisk-http/core

using System.Globalization;
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
        private Lazy<Stream> _outputStream;
        private CadenteEngineHeaderList _headers;

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
            _outputStream = new Lazy<Stream> ( () => _response.GetResponseStreamAsync ( SendChunked ).ConfigureAwait ( false ).GetAwaiter ().GetResult () );
            _headers = new CadenteEngineHeaderList ( _response );
        }

        /// <inheritdoc/>
        public override int StatusCode { get => _response.StatusCode; set => _response.StatusCode = value; }
        /// <inheritdoc/>
        public override string StatusDescription { get => _response.StatusDescription; set => _response.StatusDescription = value; }
        /// <inheritdoc/>
        public override bool KeepAlive { get => _httpHostContext.KeepAlive; set => _httpHostContext.KeepAlive = value; }
        /// <inheritdoc/>
        public override bool SendChunked { get; set; }
        /// <inheritdoc/>
        public override long ContentLength64 {
            get {
                if (_response.Headers.FirstOrDefault ( h => h.Name.Equals ( HttpKnownHeaderNames.ContentLength, StringComparison.OrdinalIgnoreCase ) ) is { IsEmpty: false } contentLenHeader) {
                    return long.Parse ( contentLenHeader.Value );
                }
                else {
                    return 0;
                }
            }
            set {
                ArgumentOutOfRangeException.ThrowIfNegative ( value );
                _response.Headers.Set ( new HttpHeader ( HttpKnownHeaderNames.ContentLength, value.ToString ( CultureInfo.InvariantCulture ) ) );
            }
        }

        /// <inheritdoc/>
        public override string? ContentType {
            get {
                var header = _response.Headers.FirstOrDefault ( h => h.Name.Equals ( HttpKnownHeaderNames.ContentType, StringComparison.OrdinalIgnoreCase ) );
                return header.Value;
            }
            set {
                if (value is { }) {
                    _response.Headers.Set ( new HttpHeader ( HttpKnownHeaderNames.ContentType, value ) );
                }
                else {
                    _response.Headers.Remove ( HttpKnownHeaderNames.ContentType );
                }
            }
        }

        /// <inheritdoc/>
        public override Stream OutputStream => _outputStream.Value;

        /// <inheritdoc/>
        public override IHttpEngineHeaderList Headers => _headers;

        /// <inheritdoc/>
        public override void Abort () {
            _httpHostContext.Abort ();
        }

        /// <inheritdoc/>
        public override void AppendHeader ( string name, string value ) {
            _response.Headers.Add ( new HttpHeader ( name, value ) );
        }

        /// <inheritdoc/>
        public override void Close () {
            Dispose ();
        }

        /// <inheritdoc/>
        public override void Dispose () {
            if (_outputStream.IsValueCreated) {
                _outputStream.Value.Dispose ();
            }

            _context?.CompleteProcessing ();
        }
    }
}
