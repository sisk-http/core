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
        if (this.ResponseHeadersAlreadySent) {
            return true;
        }

        this.ResponseHeadersAlreadySent = true;
        return HttpResponseSerializer.WriteHttpResponseHeaders ( this._connectionStream, this.Response );
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
        this.Client = client;
        this._connectionStream = connectionStream;

        HttpRequestStream requestStream = new HttpRequestStream ( connectionStream, baseRequest );
        this.Request = new HttpRequest ( baseRequest, requestStream );
        this.Response = new HttpResponse ( this, connectionStream );
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
        public string Method { get => this._baseRequest.Method; }

        /// <summary>
        /// Gets the path of the requested resource.
        /// </summary>
        public string Path { get => this._baseRequest.Path; }

        /// <summary>
        /// Gets the content length of the request.
        /// </summary>
        public long ContentLength { get => this._baseRequest.ContentLength; }

        /// <summary>
        /// Gets the headers associated with the request.
        /// </summary>
        public HttpHeader [] Headers { get => this._baseRequest.HeadersAR; }

        /// <summary>
        /// Gets the stream containing the content of the request.
        /// </summary>
        public Stream GetRequestStream () {

            if (this._baseRequest.IsExpecting100 && !this.wasExpectationSent) {
                this.wasExpectationSent = HttpResponseSerializer.WriteExpectationContinue ( this._requestStream );

                if (!this.wasExpectationSent)
                    throw new InvalidOperationException ( "Unable to obtain the input stream for the request." );
            }

            return this._requestStream;
        }

        internal HttpRequest ( HttpRequestBase request, HttpRequestStream requestStream ) {
            this._baseRequest = request;
            this._requestStream = requestStream;
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
        public HttpEventStreamWriter GetEventStream () => this.GetEventStream ( Encoding.UTF8 );

        /// <summary>
        /// Asynchronously gets an event stream writer with the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use for the event stream.</param>
        /// <returns>A task that represents the asynchronous operation, with a <see cref="HttpEventStreamWriter"/> as the result.</returns>
        /// <exception cref="InvalidOperationException">Thrown when unable to obtain an output stream for the response.</exception>
        public HttpEventStreamWriter GetEventStream ( Encoding encoding ) {
            this.Headers.Set ( new HttpHeader ( "Content-Type", "text/event-stream" ) );
            this.Headers.Set ( new HttpHeader ( "Cache-Control", "no-cache" ) );

            if (this._session.WriteHttpResponseHeaders () == false) {
                throw new InvalidOperationException ( "Unable to obtain the output stream for the response." );
            }

            return new HttpEventStreamWriter ( this._baseOutputStream, encoding );
        }

        /// <summary>
        /// Asynchronously gets the content stream for the response.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, with the response content stream as the result.</returns>
        /// <exception cref="InvalidOperationException">Thrown when unable to obtain an output stream for the response.</exception>
        public Stream GetResponseStream () {
            if (this._session.WriteHttpResponseHeaders () == false) {
                throw new InvalidOperationException ( "Unable to obtain an output stream for the response." );
            }

            this.ResponseStream = null;
            return this._baseOutputStream;
        }

        internal HttpResponse ( HttpHostContext session, Stream httpSessionStream ) {
            this._session = session;
            this._baseOutputStream = httpSessionStream;

            this.StatusCode = 200;
            this.StatusDescription = "Ok";

            this.Headers = new List<HttpHeader>
                {
                new HttpHeader ("Date", DateTime.UtcNow.ToString("R")),
                new HttpHeader ("Server", "Sisk")
            };
        }
    }
}
