// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHostContext.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;
using System.Text;
using Sisk.Cadente.HttpSerializer;
using Sisk.Cadente.Streams;

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP session that manages the request and response for a single connection.
/// </summary>
public sealed class HttpHostContext {

    private Stream _connectionStream;
    internal bool ResponseHeadersAlreadySent = false;

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    internal bool WriteHttpResponseHeaders () {
        if (ResponseHeadersAlreadySent) {
            return true;
        }

        ResponseHeadersAlreadySent = true;
        return HttpResponseSerializer.WriteHttpResponseHeaders ( _connectionStream, Response );
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

    internal HttpHostContext ( HttpRequestBase baseRequest, HttpHostClient client, Stream connectionStream ) {
        Client = client;
        _connectionStream = connectionStream;

        HttpRequestStream requestStream = new HttpRequestStream ( connectionStream, baseRequest );
        Request = new HttpRequest ( baseRequest, requestStream );
        Response = new HttpResponse ( this, connectionStream );
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
        public HttpHeader [] Headers { get => _baseRequest.HeadersAR; }

        /// <summary>
        /// Gets the stream containing the content of the request.
        /// </summary>
        public Stream GetRequestStream () {

            if (_baseRequest.IsExpecting100 && !wasExpectationSent) {
                wasExpectationSent = HttpResponseSerializer.WriteExpectationContinue ( _requestStream );

                if (!wasExpectationSent)
                    throw new InvalidOperationException ( "Unable to obtain the input stream for the request." );
            }

            return _requestStream;
        }

        internal HttpRequest ( HttpRequestBase request, HttpRequestStream requestStream ) {
            _baseRequest = request;
            _requestStream = requestStream;
        }
    }

    /// <summary>
    /// Represents an HTTP response.
    /// </summary>
    public sealed class HttpResponse {
        private Stream _baseOutputStream;
        private HttpHostContext _session;

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
        public List<HttpHeader> Headers { get; set; }

        /// <summary>
        /// Gets or sets an boolean indicating if this <see cref="HttpResponse"/> should be send in chunks or not.
        /// </summary>
        public bool SendChunked { get; set; }

        // MUST SPECIFY ResponseStream OR ResponseBytes, NOT BOTH
        /// <summary>
        /// Gets or sets the stream for the response content.
        /// </summary>
        public Stream? ResponseStream { get; set; }

        /// <summary>
        /// Asynchronously gets an event stream writer with UTF-8 encoding.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, with a <see cref="HttpEventStreamWriter"/> as the result.</returns>
        public HttpEventStreamWriter GetEventStream () => GetEventStream ( Encoding.UTF8 );

        /// <summary>
        /// Asynchronously gets an event stream writer with the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use for the event stream.</param>
        /// <returns>A task that represents the asynchronous operation, with a <see cref="HttpEventStreamWriter"/> as the result.</returns>
        /// <exception cref="InvalidOperationException">Thrown when unable to obtain an output stream for the response.</exception>
        public HttpEventStreamWriter GetEventStream ( Encoding encoding ) {
            Headers.Set ( new HttpHeader ( "Content-Type", "text/event-stream" ) );
            Headers.Set ( new HttpHeader ( "Cache-Control", "no-cache" ) );

            if (_session.WriteHttpResponseHeaders () == false) {
                throw new InvalidOperationException ( "Unable to obtain the output stream for the response." );
            }

            return new HttpEventStreamWriter ( _baseOutputStream, encoding );
        }

        /// <summary>
        /// Asynchronously gets the content stream for the response.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, with the response content stream as the result.</returns>
        /// <exception cref="InvalidOperationException">Thrown when unable to obtain an output stream for the response.</exception>
        public Stream GetResponseStream () {
            if (_session.WriteHttpResponseHeaders () == false) {
                throw new InvalidOperationException ( "Unable to obtain an output stream for the response." );
            }

            ResponseStream = null;
            return _baseOutputStream;
        }

        internal HttpResponse ( HttpHostContext session, Stream httpSessionStream ) {
            _session = session;
            _baseOutputStream = httpSessionStream;

            StatusCode = 200;
            StatusDescription = "Ok";

            Headers = new List<HttpHeader>
                {
                new HttpHeader ("Date", DateTime.UtcNow.ToString("R")),
                new HttpHeader ("Server", HttpHost.ServerNameHeader)
            };
        }
    }
}
