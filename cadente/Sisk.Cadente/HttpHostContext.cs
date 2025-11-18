// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHostContext.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;
using Sisk.Cadente.HttpSerializer;
using Sisk.Cadente.Streams;
using Sisk.Core.Http;

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP session that manages the request and response for a single connection.
/// </summary>
public sealed class HttpHostContext {

    private HttpHost _host;
    private HttpConnection _connection;

    internal CancellationTokenSource abortedSource = new CancellationTokenSource ();
    internal bool ResponseHeadersAlreadySent = false;

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    internal Task WriteHttpResponseHeadersAsync () {
        if (ResponseHeadersAlreadySent) {
            return Task.FromResult ( true );
        }

        ResponseHeadersAlreadySent = true;
        return HttpResponseSerializer.WriteHttpResponseHeaders ( _connection.responsePool.Memory, _connection.networkStream, Response );
    }

    /// <summary>
    /// Gets the HTTP request associated with this session.
    /// </summary>
    public HttpRequest Request { get; }

    /// <summary>
    /// Gets the HTTP response associated with this session.
    /// </summary>
    public HttpResponse Response { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the connection should be kept alive.
    /// </summary>
    public bool KeepAlive { get; set; } = true;

    /// <summary>
    /// Gets the associated <see cref="HttpHostClient"/> with this HTTP context.
    /// </summary>
    public HttpHostClient Client { get; }

    /// <summary>
    /// Gets the associated <see cref="HttpHost"/> which created this HTTP context.
    /// </summary>
    public HttpHost Host => _host;

    internal HttpHostContext ( HttpHost host, HttpConnection connection, HttpRequestBase baseRequest, HttpHostClient client ) {
        Client = client;
        _connection = connection;
        _host = host;

        HttpRequestStream requestStream = new HttpRequestStream ( _connection.networkStream, baseRequest );
        Request = new HttpRequest ( baseRequest, requestStream );
        Response = new HttpResponse ( this, _connection.networkStream );
    }

    /// <summary>
    /// Represents an HTTP request.
    /// </summary>
    public sealed class HttpRequest {

        bool wasExpectationSent = false;
        private HttpRequestStream _requestStream;
        private HttpRequestBase _baseRequest;

        /// <summary>
        /// Gets the HTTP method (e.g., GET, POST) of the request.
        /// </summary>
        public string Method { get => _baseRequest.Method; }

        /// <summary>
        /// Gets the path of the requested resource.
        /// </summary>
        public string Path { get => _baseRequest.Path; }

        /// <summary>
        /// Gets the content length of the request.
        /// </summary>
        public long ContentLength { get => _baseRequest.ContentLength; }

        /// <summary>
        /// Gets the headers associated with the request.
        /// </summary>
        public HttpHeaderList Headers { get; }

        /// <summary>
        /// Gets the stream containing the content of the request.
        /// </summary>
        public Stream GetRequestStream () {

            if (ContentLength == 0) {
                return Stream.Null;
            }

            if (_baseRequest.IsExpecting100 && !wasExpectationSent) {
                wasExpectationSent = HttpResponseSerializer.WriteExpectationContinue ( _requestStream );

                if (!wasExpectationSent)
                    throw new InvalidOperationException ( "Unable to obtain the input stream for the request." );
            }

            return _baseRequest.IsChunked switch {
                true => new HttpChunkedReadStream2 ( _requestStream ),
                false => _requestStream
            };
        }

        internal HttpRequest ( HttpRequestBase request, HttpRequestStream requestStream ) {
            _baseRequest = request;
            _requestStream = requestStream;
            Headers = new HttpHeaderList ( _baseRequest.Headers.ToArray (), readOnly: true );
        }
    }

    /// <summary>
    /// Represents an HTTP response.
    /// </summary>
    public sealed class HttpResponse {
        private Stream _baseOutputStream;
        private HttpHostContext _session;
        internal bool headersSent = false;

        /// <summary>
        /// Gets or sets the HTTP status code of the response.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the status description of the response.
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Gets or sets the list of headers associated with the response.
        /// </summary>
        public HttpHeaderList Headers { get; set; }

        /// <summary>
        /// Asynchronously gets the content stream for the response.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, with the response content stream as the result.</returns>
        /// <exception cref="InvalidOperationException">Thrown when unable to obtain an output stream for the response.</exception>
        public async Task<Stream> GetResponseStreamAsync ( bool chunked = false ) {
            if (headersSent) {
                throw new InvalidOperationException ( "Headers already sent for this HTTP response." );
            }

            if (chunked) {
                Headers.Set ( new HttpHeader ( HttpHeaderName.TransferEncoding, "chunked" ) );
                Headers.Remove ( HttpHeaderName.ContentLength );
            }
            else {
                if (!Headers.Contains ( HttpHeaderName.ContentLength ) &&
                    !Headers.Get ( HttpHeaderName.Upgrade ).Contains ( "websocket" )) {

                    throw new InvalidOperationException ( "Content-Length header must be set for non-chunked responses." );
                }
            }

            await _session.WriteHttpResponseHeadersAsync ();

            headersSent = true;
            return chunked switch {
                true => new HttpChunkedWriteStream ( _baseOutputStream ),
                false => new UndisposableNetworkStream ( _baseOutputStream )
            };
        }

        internal HttpResponse ( HttpHostContext session, Stream httpSessionStream ) {
            _session = session;
            _baseOutputStream = httpSessionStream;

            StatusCode = 200;
            StatusDescription = "Ok";

            Headers = new HttpHeaderList ()
            {
                new HttpHeader ("Date", DateTime.UtcNow.ToString("R")),
                new HttpHeader ("Server", HttpHost.ServerNameHeader)
            };
        }
    }
}
