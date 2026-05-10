// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHostContext.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;
using System.Globalization;
using System.Text;
using System.Threading;
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

    internal bool ResponseHeadersAlreadySent = false;

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    internal Task WriteHttpResponseHeadersAsync () {
        if (ResponseHeadersAlreadySent) {
            return Task.CompletedTask;
        }

        ResponseHeadersAlreadySent = true;
        return HttpResponseSerializer.WriteHttpResponseHeadersAsync ( _connection.responsePool.Memory, _connection.networkStream, Response );
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    internal void WriteHttpResponseHeaders () {
        if (ResponseHeadersAlreadySent) {
            return;
        }

        ResponseHeadersAlreadySent = true;
        HttpResponseSerializer.WriteHttpResponseHeaders ( _connection.responsePool.Memory, _connection.networkStream, Response );
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    internal void WriteHttpResponse ( ReadOnlySpan<byte> body ) {
        if (ResponseHeadersAlreadySent) {
            _connection.networkStream.Write ( body );
            return;
        }

        ResponseHeadersAlreadySent = true;
        int headerSize = HttpResponseSerializer.GetResponseHeadersBytes ( _connection.responsePool.Memory.Span, Response );
        Memory<byte> responseBuffer = _connection.responsePool.Memory;

        if (headerSize + body.Length <= responseBuffer.Length) {
            body.CopyTo ( responseBuffer.Span [ headerSize.. ] );
            _connection.networkStream.Write ( responseBuffer.Span [ ..(headerSize + body.Length) ] );
        }
        else {
            _connection.networkStream.Write ( responseBuffer.Span [ ..headerSize ] );
            _connection.networkStream.Write ( body );
        }
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

    /// <summary>
    /// Aborts the underlying network stream, forcibly closing the connection.
    /// </summary>
    public void Abort () {
        _connection.networkStream.Close ();
    }

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
        internal EndableStream? _readingStream;
        private HttpHeaderList? _headers;

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
        /// Gets a value indicating whether the request <b>may</b> have a message body.
        /// </summary>
        /// <remarks>
        /// This property returns <c>true</c> if the content length is greater than zero or if the request uses chunked transfer encoding,
        /// indicating that a body <b>may</b> be present. However, for chunked encoding, the actual content could still be empty even when this property returns true.
        /// Use this property to determine whether it is necessary to attempt reading or processing the request body, but do not assume that a body is always present.
        /// </remarks>
        public bool HasBody { get => ContentLength > 0 || _baseRequest.IsChunked; }

        /// <summary>
        /// Gets the headers associated with the request.
        /// </summary>
        public HttpHeaderList Headers => _headers ??= new HttpHeaderList ( _baseRequest.Headers.ToArray (), readOnly: true );

        /// <summary>
        /// Gets the stream containing the content of the request.
        /// </summary>
        public Stream GetRequestStream () {
            return GetRequestStreamCore ( true );
        }

        internal Stream GetRequestStreamCore ( bool sendExpectation ) {
            if (ContentLength == 0) {
                return Stream.Null;
            }

            if (_readingStream is { }) {

                if (_readingStream.IsEnded) {
                    return Stream.Null;
                }
                else {
                    return _readingStream;
                }
            }

            if (_baseRequest.IsExpecting100 && !wasExpectationSent && sendExpectation) {
                wasExpectationSent = HttpResponseSerializer.WriteExpectationContinue ( _requestStream );

                if (!wasExpectationSent)
                    throw new InvalidOperationException ( "Unable to obtain the input stream for the request." );
            }

            _readingStream = _baseRequest.IsChunked switch {
                true => new HttpChunkedReadStream2 ( _requestStream ),
                false => _requestStream
            };

            return _readingStream;
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
        private static readonly ReadOnlyMemory<byte> DateHeaderName = "Date"u8.ToArray ();
        private static readonly ReadOnlyMemory<byte> ServerHeaderName = "Server"u8.ToArray ();
        private static long _cachedDateSecond = -1;
        private static byte [] _cachedDateValue = Array.Empty<byte> ();
        private static string? _cachedServerName;
        private static byte [] _cachedServerValue = Array.Empty<byte> ();

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
            PrepareResponseStream ( chunked );

            await _session.WriteHttpResponseHeadersAsync ().ConfigureAwait ( false );

            headersSent = true;
            return CreateOutputStream ( chunked );
        }

        internal Stream GetResponseStream ( bool chunked = false ) {
            PrepareResponseStream ( chunked );

            _session.WriteHttpResponseHeaders ();

            headersSent = true;
            return CreateOutputStream ( chunked );
        }

        internal void WriteInlineContent ( ReadOnlySpan<byte> content ) {
            PrepareResponseStream ( chunked: false );
            _session.WriteHttpResponse ( content );
            headersSent = true;
        }

        private void PrepareResponseStream ( bool chunked ) {
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
        }

        private Stream CreateOutputStream ( bool chunked ) {
            return chunked switch {
                true => new HttpChunkedWriteStream ( _baseOutputStream ),
                false => new UndisposableNetworkStream ( _baseOutputStream )
            };
        }

        private static HttpHeader CreateDateHeader () {
            long currentSecond = DateTimeOffset.UtcNow.ToUnixTimeSeconds ();
            byte [] dateValue = Volatile.Read ( ref _cachedDateValue );

            if (Volatile.Read ( ref _cachedDateSecond ) != currentSecond || dateValue.Length == 0) {
                dateValue = Encoding.ASCII.GetBytes ( DateTime.UtcNow.ToString ( "R", CultureInfo.InvariantCulture ) );
                Volatile.Write ( ref _cachedDateValue, dateValue );
                Volatile.Write ( ref _cachedDateSecond, currentSecond );
            }

            return HttpHeader.CreateUnchecked ( DateHeaderName, dateValue );
        }

        private static HttpHeader CreateServerHeader () {
            string serverName = HttpHost.ServerNameHeader;
            string? cachedName = Volatile.Read ( ref _cachedServerName );
            byte [] serverValue = Volatile.Read ( ref _cachedServerValue );

            if (!string.Equals ( cachedName, serverName, StringComparison.Ordinal ) || serverValue.Length == 0) {
                serverValue = Encoding.UTF8.GetBytes ( serverName );
                Volatile.Write ( ref _cachedServerValue, serverValue );
                Volatile.Write ( ref _cachedServerName, serverName );
            }

            return HttpHeader.CreateUnchecked ( ServerHeaderName, serverValue );
        }

        internal HttpResponse ( HttpHostContext session, Stream httpSessionStream ) {
            _session = session;
            _baseOutputStream = httpSessionStream;

            StatusCode = 200;
            StatusDescription = "Ok";

            Headers = new HttpHeaderList ( 2 )
            {
                CreateDateHeader (),
                CreateServerHeader ()
            };
        }
    }
}
