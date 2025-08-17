// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequest.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Sisk.Core.Entity;
using Sisk.Core.Helpers;
using Sisk.Core.Http.Engine;
using Sisk.Core.Http.Streams;
using Sisk.Core.Internal;
using Sisk.Core.Routing;

namespace Sisk.Core.Http {
    /// <summary>
    /// Represents an exception that is thrown while a request is being interpreted by the HTTP server.
    /// </summary>
    public sealed class HttpRequestException : Exception {
        internal HttpRequestException ( string message ) : base ( message ) { }
        internal HttpRequestException ( string message, Exception? innerException ) : base ( message, innerException ) { }
    }

    /// <summary>
    /// Represents an HTTP request received by a Sisk server.
    /// </summary>
    public sealed class HttpRequest : IDisposable {

        internal HttpServer baseServer;
        internal IDisposable? streamingEntity;
        internal IPAddress remoteAddr = null!;
        private readonly HttpServerConfiguration contextServerConfiguration;
        private readonly HttpServerEngineContextResponse listenerResponse;
        private readonly HttpServerEngineContextRequest listenerRequest;
        private readonly HttpServerEngineContext context;
        private byte []? contentBytes;
        private HttpHeaderCollection? headers;
        private StringKeyStoreCollection? cookies;
        private StringValueCollection? query;

        private readonly Uri requestUri;
        private readonly HttpMethod requestMethod;

        private int currentFrame;
        private bool disposedValue;

        internal HttpRequest (
            HttpServer server,
            HttpServerEngineContext context ) {

            this.context = context;
            baseServer = server;
            contextServerConfiguration = baseServer.ServerConfiguration;
            listenerResponse = context.Response;
            listenerRequest = context.Request;
            RequestedAt = DateTime.UtcNow.Add ( HttpServer.environmentUtcOffset );

            requestUri = context.Request.Url ?? throw new HttpRequestException ( SR.HttpRequest_Error );
            ContentLength = listenerRequest.ContentLength64;
            requestMethod = new HttpMethod ( listenerRequest.HttpMethod );
        }

        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        string ConvertEncodingCodePage ( string input, Encoding inEnc, Encoding outEnc ) {
            return outEnc.GetString ( inEnc.GetBytes ( input ) );
        }

        internal IPAddress ReadRequestRemoteAddr () {
            if (contextServerConfiguration.ForwardingResolver is { } fr) {
                try {
                    return fr.OnResolveClientAddress ( this, listenerRequest.RemoteEndPoint );
                }
                catch (Exception ex) {
                    throw new TargetInvocationException ( SR.Format ( SR.ForwardingResolverInvocationException, "remote address", fr.GetType ().Name ), ex );
                }
            }
            else {
                return new IPAddress ( listenerRequest.RemoteEndPoint.Address.GetAddressBytes () );
            }
        }

        internal async Task<byte []> ReadRequestStreamContentsAsync ( CancellationToken cancellation = default ) {
            if (contentBytes is null) {
                if (ContentLength > Int32.MaxValue) {
                    throw new InvalidOperationException ( SR.HttpRequest_ContentAbove2G );
                }
                else if (ContentLength > 0) {
                    using (var memoryStream = new MemoryStream ( (int) ContentLength )) {
                        await listenerRequest.InputStream.CopyToAsync ( memoryStream, cancellation ).ConfigureAwait ( false );
                        contentBytes = memoryStream.ToArray ();
                    }
                }
                else if (ContentLength < 0) {
                    using (var memoryStream = new MemoryStream ()) {
                        long maxLength = contextServerConfiguration.MaximumContentLength <= 0 ?
                            Int32.MaxValue :
                            contextServerConfiguration.MaximumContentLength;

                        await StreamUtil.CopyToLimitedAsync ( listenerRequest.InputStream, memoryStream, 81920, maxLength, cancellation ).ConfigureAwait ( false );
                        contentBytes = memoryStream.ToArray ();
                    }
                }
                else // = 0
                {
                    contentBytes = Array.Empty<byte> ();
                }
            }

            return contentBytes;
        }

        byte [] ReadRequestStreamContents () {
            if (contentBytes is null) {
                if (ContentLength > Int32.MaxValue) {
                    throw new InvalidOperationException ( SR.HttpRequest_ContentAbove2G );
                }
                else if (ContentLength > 0) {
                    using (var memoryStream = new MemoryStream ( (int) ContentLength )) {
                        listenerRequest.InputStream.CopyTo ( memoryStream );
                        contentBytes = memoryStream.ToArray ();
                    }
                }
                else if (ContentLength < 0) {
                    using (var memoryStream = new MemoryStream ()) {
                        long maxLength = contextServerConfiguration.MaximumContentLength <= 0 ?
                            Int32.MaxValue :
                            contextServerConfiguration.MaximumContentLength;

                        StreamUtil.CopyToLimited ( listenerRequest.InputStream, memoryStream, 81920, maxLength );
                        contentBytes = memoryStream.ToArray ();
                    }
                }
                else // = 0
                {
                    contentBytes = Array.Empty<byte> ();
                }
            }

            return contentBytes;
        }

        /// <summary>
        /// Gets or sets the default options used for JSON serialization.
        /// </summary>
        /// <remarks>
        /// These options are used by default when serializing or deserializing JSON data through <see cref="GetJsonContent{T}()"/>,
        /// unless custom options are provided. See <see cref="System.Text.Json.JsonSerializerOptions"/> 
        /// for more information on available options.
        /// </remarks>
        public static JsonSerializerOptions? DefaultJsonSerializerOptions { get; set; } = new JsonSerializerOptions ( JsonSerializerDefaults.Web );

        /// <summary>
        /// Gets a unique random ID for this request.
        /// </summary>
        public Guid RequestId { get => listenerRequest.RequestTraceIdentifier; }

        /// <summary>
        /// Gets a boolean indicating whether this request was locally made by an secure
        /// transport context (SSL/TLS) or not.
        /// </summary>
        /// <remarks>
        /// This property brings local request data, so it may not reflect the original client request when used with proxy or CDNs.
        /// </remarks>
        public bool IsSecure {
            get {
                if (contextServerConfiguration.ForwardingResolver is { } fr) {
                    try {
                        return fr.OnResolveSecureConnection ( this, listenerRequest.IsSecureConnection );
                    }
                    catch (Exception ex) {
                        throw new TargetInvocationException ( SR.Format ( SR.ForwardingResolverInvocationException, "encryptation state", fr.GetType ().Name ), ex );
                    }
                }
                else {
                    return listenerRequest.IsSecureConnection;
                }
            }
        }

        /// <summary>
        /// Gets a boolean indicating whether this request has body contents and whether
        /// it has already been read into memory by the server.
        /// </summary>
        public bool IsContentAvailable { get => HasContents && contentBytes is not null; }

        /// <summary>
        /// Gets a boolean indicating whether this request has body contents.
        /// </summary>
        public bool HasContents { get => ContentLength > 0; }

        /// <summary>
        /// Gets the HTTP request headers.
        /// </summary>
        public HttpHeaderCollection Headers {
            get {
                if (headers is null) {
                    if (contextServerConfiguration.NormalizeHeadersEncodings) {
                        headers = new HttpHeaderCollection ();
                        Encoding entryCodepage = Encoding.GetEncoding ( "ISO-8859-1" );
                        foreach (string headerName in listenerRequest.Headers) {
                            string headerValue = listenerRequest.Headers [ headerName ]!;
                            headers.Add (
                                headerName,
                                ConvertEncodingCodePage ( headerValue, entryCodepage, listenerRequest.ContentEncoding )
                            );
                        }
                    }
                    else {
                        headers = new HttpHeaderCollection ();
                        headers.ImportNameValueCollection ( listenerRequest.Headers );
                    }

                    headers.MakeReadOnly ();
                }

                return headers;
            }
        }

        /// <summary>
        /// Gets an <see cref="StringKeyStoreCollection"/> object with all cookies set in this request.
        /// </summary>
        public StringKeyStoreCollection Cookies {
            get {
                if (cookies is null) {
                    string? cookieHeader = listenerRequest.Headers [ HttpKnownHeaderNames.Cookie ];
                    StringKeyStoreCollection store = new StringKeyStoreCollection ();
                    if (cookieHeader is not null) {
                        store.ImportCookieString ( cookieHeader );
                    }
                    store.MakeReadOnly ();
                    cookies = store;
                }

                return cookies;
            }
        }

        /// <summary>
        /// Gets a cancellation token that is signaled when the client disconnects.
        /// </summary>
        public CancellationToken DisconnectToken { get => context.ContextAbortedToken; }

        /// <summary>
        /// Get the requested host (without port) for this <see cref="HttpRequest"/>.
        /// </summary>
        public string? Host { get; internal set; }

        /// <summary>
        /// Gets the managed object which holds data for an entire HTTP session.
        /// </summary>
        /// <remarks>
        /// This property is an shortcut for <see cref="HttpContext.RequestBag"/> property.
        /// </remarks>
        public TypedValueDictionary Bag => Context.RequestBag;

        /// <summary>
        /// Get the requested host header with the port from this HTTP request.
        /// </summary>
        /// <remarks>
        /// This property brings local request data, so it may not reflect the original client request when used with proxy or CDNs.
        /// </remarks>
        public string Authority {
            get => requestUri.Authority;
        }

        /// <summary>
        /// Gets the HTTP request path without the query string.
        /// </summary>
        public string Path {
            get => requestUri.AbsolutePath;
        }

        /// <summary>
        /// Gets the raw, full HTTP request path with the query string.
        /// </summary>
        public string FullPath {
            get => listenerRequest.RawUrl ?? "/";
        }

        /// <summary>
        /// Gets the full URL for this request, with scheme, host, port, path and query.
        /// </summary>
        /// <remarks>
        /// This property brings local request data, so it may not reflect the original client request when used with proxy or CDNs.
        /// </remarks>
        public string FullUrl {
            get => requestUri.ToString ();
        }

        /// <summary>
        /// Gets the <see cref="System.Uri"/> component for this HTTP request requested URL.
        /// </summary>
        public Uri Uri {
            get => requestUri;
        }

        /// <summary>
        /// Gets an string <see cref="Encoding"/> that can be used to decode text in this HTTP request.
        /// </summary>
        public Encoding RequestEncoding {
            get => listenerRequest.ContentEncoding;
        }

        /// <summary>
        /// Gets the HTTP request method.
        /// </summary>
        public HttpMethod Method {
            get => requestMethod;
        }

        /// <summary>
        /// Gets the HTTP request body as string, decoded by the request content encoding.
        /// </summary>
        /// <remarks>
        /// When calling this property, the entire content of the request is read into memory and stored in <see cref="RawBody"/>.
        /// </remarks>
        [DebuggerBrowsable ( DebuggerBrowsableState.Never )]
        public string Body {
            get => listenerRequest.ContentEncoding.GetString ( RawBody );
        }

        /// <summary>
        /// Gets the HTTP request body as a byte array.
        /// </summary>
        /// <remarks>
        /// When calling this property, the entire content of the request is read into memory.
        /// </remarks>
        [DebuggerBrowsable ( DebuggerBrowsableState.Never )]
        public byte [] RawBody {
            get {
                return ReadRequestStreamContents ();
            }
        }

        /// <summary>
        /// Gets the content length in bytes count.
        /// </summary>
        /// <remarks>
        /// This value can be negative if the content length is unknown.
        /// </remarks>
        public long ContentLength { get; }

        /// <summary>
        /// Gets the HTTP request query value collection.
        /// </summary>
        public StringValueCollection Query {
            get {
                if (query is null) {
                    var sv = new StringValueCollection ( "query parameter" );
                    sv.ImportNameValueCollection ( listenerRequest.QueryString );
                    sv.MakeReadOnly ();
                    query = sv;
                }
                return query;
            }
        }

        /// <summary>
        /// Gets the <see cref="StringValueCollection"/> object which represents the current
        /// route parameters.
        /// </summary>
        public StringValueCollection RouteParameters { get; } = new StringValueCollection ( "route parameter" );

        /// <summary>
        /// Gets the HTTP request URL raw query string, including the '?' char.
        /// </summary>
        public string QueryString { get => listenerRequest.Url?.Query ?? string.Empty; }

        /// <summary>
        /// Gets the incoming local IP address from the request.
        /// </summary>
        public IPAddress RemoteAddress {
            get {
                return remoteAddr;
            }
        }

        /// <summary>
        /// Gets the moment which the request was received by the server.
        /// </summary>
        public DateTime RequestedAt { get; private init; }

        /// <summary>
        /// Gets the <see cref="HttpContext"/> for this request.
        /// </summary>
        public HttpContext Context { get; internal set; } = null!;

        /// <summary>
        /// Gets the request contents of the body as a byte array.
        /// </summary>
        /// <returns>A byte array containing the body contents.</returns>
        public byte [] GetBodyContents () => RawBody;

        /// <summary>
        /// Asynchronously reads the request contents as a memory byte array.
        /// </summary>
        /// <param name="cancellation">A <see cref="CancellationToken"/> to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that returns a <see cref="Memory{T}"/> of bytes containing the body contents.</returns>
        public async Task<Memory<byte>> GetBodyContentsAsync ( CancellationToken cancellation = default ) {
            byte [] body = await ReadRequestStreamContentsAsync ( cancellation ).ConfigureAwait ( false );
            return body;
        }

        /// <summary>
        /// Deserializes the request body into an object of type <typeparamref name="T"/> using the provided <see cref="JsonTypeInfo{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize into.</typeparam>
        /// <param name="typeInfo">The <see cref="JsonTypeInfo{T}"/> to use for deserialization.</param>
        /// <returns>The deserialized object, or <c>null</c> if the request body is empty.</returns>
        public T? GetJsonContent<T> ( JsonTypeInfo<T> typeInfo ) {
            if (ContentLength >= 0) {
                var requestStream = GetRequestStream ();
                return JsonSerializer.Deserialize<T> ( requestStream, typeInfo );
            }
            else {
                var content = GetBodyContents ();
                return JsonSerializer.Deserialize<T> ( content, typeInfo );
            }
        }

        /// <summary>
        /// Deserializes the request body into an object of type <typeparamref name="T"/> using the provided <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize into.</typeparam>
        /// <param name="jsonOptions">The <see cref="JsonSerializerOptions"/> to use for deserialization.</param>
        /// <returns>The deserialized object, or <c>null</c> if the request body is empty.</returns>
        [RequiresDynamicCode ( SR.RequiresUnreferencedCode__JsonDeserialize )]
        [RequiresUnreferencedCode ( SR.RequiresUnreferencedCode__JsonDeserialize )]
        public T? GetJsonContent<T> ( JsonSerializerOptions? jsonOptions = null ) {
            if (ContentLength >= 0) {
                var requestStream = GetRequestStream ();
                return JsonSerializer.Deserialize<T> ( requestStream, jsonOptions ?? DefaultJsonSerializerOptions );
            }
            else {
                var content = GetBodyContents ();
                return JsonSerializer.Deserialize<T> ( content, jsonOptions ?? DefaultJsonSerializerOptions );
            }
        }

        /// <summary>
        /// Deserializes the request body into an object of type <typeparamref name="T"/> using the default
        /// <see cref="JsonSerializerOptions"/> from <see cref="DefaultJsonSerializerOptions"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize into.</typeparam>
        /// <returns>The deserialized object, or <c>null</c> if the request body is empty.</returns>
        [RequiresDynamicCode ( SR.RequiresUnreferencedCode__JsonDeserialize )]
        [RequiresUnreferencedCode ( SR.RequiresUnreferencedCode__JsonDeserialize )]
        public T? GetJsonContent<T> () => GetJsonContent<T> ( (JsonSerializerOptions?) null );

        /// <summary>
        /// Asynchronously deserializes the request body into an object of type <typeparamref name="T"/> using the provided <see cref="JsonTypeInfo{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize into.</typeparam>
        /// <param name="typeInfo">The <see cref="JsonTypeInfo{T}"/> to use for deserialization.</param>
        /// <param name="cancellation">A <see cref="CancellationToken"/> to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="ValueTask{T}"/> that represents the asynchronous deserialization operation.</returns>
        public async ValueTask<T?> GetJsonContentAsync<T> ( JsonTypeInfo<T> typeInfo, CancellationToken cancellation = default ) {
            if (ContentLength >= 0) {
                var requestStream = GetRequestStream ();
                return await JsonSerializer.DeserializeAsync<T> ( requestStream, typeInfo, cancellation ).ConfigureAwait ( false );
            }
            else {
                var content = await GetBodyContentsAsync ( cancellation ).ConfigureAwait ( false );
                return JsonSerializer.Deserialize<T> ( content.Span, typeInfo );
            }
        }

        /// <summary>
        /// Asynchronously deserializes the request body into an object of type <typeparamref name="T"/> using the provided <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize into.</typeparam>
        /// <param name="jsonOptions">The <see cref="JsonSerializerOptions"/> to use for deserialization.</param>
        /// <param name="cancellation">A <see cref="CancellationToken"/> to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="ValueTask{T}"/> that represents the asynchronous deserialization operation.</returns>
        [RequiresDynamicCode ( SR.RequiresUnreferencedCode__JsonDeserialize )]
        [RequiresUnreferencedCode ( SR.RequiresUnreferencedCode__JsonDeserialize )]
        public async ValueTask<T?> GetJsonContentAsync<T> ( JsonSerializerOptions? jsonOptions, CancellationToken cancellation = default ) {
            if (ContentLength >= 0) {
                var requestStream = GetRequestStream ();
                return await JsonSerializer.DeserializeAsync<T> ( requestStream, jsonOptions ?? DefaultJsonSerializerOptions, cancellation ).ConfigureAwait ( false );
            }
            else {
                var content = await GetBodyContentsAsync ( cancellation ).ConfigureAwait ( false );
                return JsonSerializer.Deserialize<T> ( content.Span, jsonOptions ?? DefaultJsonSerializerOptions );
            }
        }

        /// <summary>
        /// Asynchronously deserializes the request body into an object of type <typeparamref name="T"/> using the default <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize into.</typeparam>
        /// <param name="cancellation">A <see cref="CancellationToken"/> to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="ValueTask{T}"/> that represents the asynchronous deserialization operation.</returns>
        [RequiresDynamicCode ( SR.RequiresUnreferencedCode__JsonDeserialize )]
        [RequiresUnreferencedCode ( SR.RequiresUnreferencedCode__JsonDeserialize )]
        public ValueTask<T?> GetJsonContentAsync<T> ( CancellationToken cancellation = default ) => GetJsonContentAsync<T> ( (JsonSerializerOptions?) null, cancellation );

        /// <summary>
        /// Reads the request body and obtains a <see cref="MultipartFormCollection"/> from it.
        /// </summary>
        public MultipartFormCollection GetMultipartFormContent () {
            try {
                byte [] body = ReadRequestStreamContents ();
                return MultipartObject.ParseMultipartObjects ( this, body );
            }
            catch (Exception ex) {
                throw new HttpRequestException ( SR.Format ( SR.MultipartFormReader_Exception, ex.Message ), ex );
            }
        }

        /// <summary>
        /// Asynchronously reads the request body and obtains a <see cref="MultipartFormCollection"/> from it.
        /// </summary>
        /// <param name="cancellation">A <see cref="CancellationToken"/> to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, containing a <see cref="MultipartFormCollection"/> instance representing the multipart form content of the request.</returns>
        /// <exception cref="HttpRequestException">If an error occurs while parsing the multipart form content.</exception>
        public async Task<MultipartFormCollection> GetMultipartFormContentAsync ( CancellationToken cancellation = default ) {
            try {
                byte [] body = await ReadRequestStreamContentsAsync ( cancellation ).ConfigureAwait ( false );
                return MultipartObject.ParseMultipartObjects ( this, body, cancellation );
            }
            catch (Exception ex) {
                throw new HttpRequestException ( SR.Format ( SR.MultipartFormReader_Exception, ex.Message ), ex );
            }
        }

        /// <summary>
        /// Reads the request body and extracts form data parameters from it.
        /// </summary>
        public StringKeyStoreCollection GetFormContent () {
            return StringKeyStoreCollection.FromQueryString ( Body );
        }

        /// <summary>
        /// Asynchronously reads the request body and extracts form data parameters from it.
        /// </summary>
        /// <param name="cancellation">A <see cref="CancellationToken"/> to cancel the asynchronous operation.</param>
        public async Task<StringKeyStoreCollection> GetFormContentAsync ( CancellationToken cancellation = default ) {
            byte [] body = await ReadRequestStreamContentsAsync ( cancellation ).ConfigureAwait ( false );
            string bodyContents = Encoding.UTF8.GetString ( body );
            return StringKeyStoreCollection.FromQueryString ( bodyContents );
        }

        /// <summary>
        /// Gets a visual representation of this request.
        /// </summary>
        /// <param name="includeBody">Optional. Defines if the body should be included in the output.</param>
        /// <param name="appendExtraInfo">Optional. Appends extra information, such as request id and date into the output.</param>
        public string GetRawHttpRequest ( bool includeBody = true, bool appendExtraInfo = false ) {
            StringBuilder sb = new StringBuilder ();
            // Method and path
            sb.Append ( Method.ToString ().ToUpperInvariant () + " " );
            sb.Append ( Path + " " );
            sb.Append ( "HTTP/" );
            sb.Append ( listenerRequest.ProtocolVersion.Major + "." );
            sb.Append ( listenerRequest.ProtocolVersion.Minor + "\n" );

            // Headers
            if (appendExtraInfo) {
                sb.AppendLine ( null, $":remote-ip: {RemoteAddress} (was {listenerRequest.RemoteEndPoint})" );
                sb.AppendLine ( null, $":host: {Host} (was {listenerRequest.UserHostName})" );
                sb.AppendLine ( null, $":date: {RequestedAt:s}" );
                sb.AppendLine ( null, $":request-id: {RequestId}" );
                sb.AppendLine ( null, $":request-proto: {(IsSecure ? "https" : "http")}" );
            }
            sb.AppendLine ( Headers.ToString ( null ) );
            sb.AppendLine ();

            // Content
            if (includeBody) {
                if (Body.Length < 8 * SizeHelper.UnitKb) {
                    sb.Append ( Body );
                }
                else {
                    sb.Append ( null, $"| ({SizeHelper.HumanReadableSize ( Body.Length )})" );
                }
            }

            return sb.ToString ();
        }

        /// <summary>
        /// Calls another handler for this request, preserving the current call-stack frame, and then returns the response from
        /// it. This method manages to prevent possible stack overflows.
        /// </summary>
        /// <param name="otherCallback">Defines the <see cref="RouteAction"/> method which will handle this request.</param>
        public object SendTo ( RouteAction otherCallback ) {
            Interlocked.Increment ( ref currentFrame );
            if (currentFrame > 64) {
                throw new OverflowException ( SR.HttpRequest_SendTo_MaxRedirects );
            }
            return otherCallback ( this );
        }

        /// <summary>
        /// Gets the HTTP request content stream. This property is only available while the
        /// content has not been imported by the HTTP server and will invalidate the body content 
        /// cached in this object.
        /// </summary>
        public Stream GetRequestStream () {
            if (contentBytes is not null) {
                throw new InvalidOperationException ( SR.HttpRequest_InputStreamAlreadyLoaded );
            }
            return listenerRequest.InputStream;
        }

        /// <summary>
        /// Gets an HTTP response stream for this HTTP request.
        /// </summary>
        public HttpResponseStreamManager GetResponseStream () {
            if (streamingEntity is not null) {
                throw new InvalidOperationException ( SR.HttpRequest_AlreadyInStreamingState );
            }
            streamingEntity = listenerResponse;
            return new HttpResponseStreamManager ( listenerResponse, listenerRequest, this );
        }

        /// <summary>
        /// Gets an Event Source interface for this request. Calling this method will put this <see cref="HttpRequest"/> instance in it's
        /// event source listening state.
        /// </summary>
        /// <param name="identifier">Optional. Defines an label to the EventStream connection, useful for finding this connection's reference later.</param>
        public HttpRequestEventSource GetEventSource ( string? identifier = null ) {
            if (streamingEntity is not null) {
                throw new InvalidOperationException ( SR.HttpRequest_AlreadyInStreamingState );
            }
            var sse = new HttpRequestEventSource ( identifier, listenerResponse, listenerRequest, this );
            streamingEntity = sse;
            return sse;
        }

        /// <summary>
        /// Asynchronously gets an Event Source interface for this request. Calling this method will put this <see cref="HttpRequest"/> instance in its
        /// event source listening state.
        /// </summary>
        /// <param name="identifier">Optional. Defines a label to the EventStream connection, useful for finding this connection's reference later.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, containing an <see cref="HttpRequestEventSource"/> instance representing the event source for this request.</returns>
        public Task<HttpRequestEventSource> GetEventSourceAsync ( string? identifier = null ) {
            if (streamingEntity is not null) {
                throw new InvalidOperationException ( SR.HttpRequest_AlreadyInStreamingState );
            }
            var sse = new HttpRequestEventSource ( identifier, listenerResponse, listenerRequest, this );
            streamingEntity = sse;
            return Task.FromResult ( sse );
        }

        /// <summary>
        /// Accepts and acquires a websocket for this request. Calling this method will put this <see cref="HttpRequest"/> instance in
        /// streaming state.
        /// </summary>
        /// <param name="subprotocol">Optional. Determines the sub-protocol to plug the websocket in.</param>
        /// <param name="identifier">Optional. Defines an label to the Web Socket connection, useful for finding this connection's reference later.</param>
        public HttpWebSocket GetWebSocket ( string? subprotocol = null, string? identifier = null ) {
            var wsTask = GetWebSocketAsync ( subprotocol, identifier );
            return wsTask.ConfigureAwait ( false ).GetAwaiter ().GetResult ();
        }

        /// <summary>
        /// Asynchronously accepts and acquires a websocket for this request. Calling this method will put this <see cref="HttpRequest"/> instance in
        /// streaming state.
        /// </summary>
        /// <param name="subprotocol">Optional. Determines the sub-protocol to plug the websocket in.</param>
        /// <param name="identifier">Optional. Defines an label to the Web Socket connection, useful for finding this connection's reference later.</param>
        /// <returns>A task that represents the asynchronous operation, returning an instance of <see cref="HttpWebSocket"/> representing the accepted websocket connection.</returns>
        public async Task<HttpWebSocket> GetWebSocketAsync ( string? subprotocol = null, string? identifier = null ) {
            if (streamingEntity is not null) {
                throw new InvalidOperationException ( SR.HttpRequest_AlreadyInStreamingState );
            }
            var accept = await context.AcceptWebSocketAsync ( subprotocol ).ConfigureAwait ( false );
            var ws = new HttpWebSocket ( accept, this, identifier );
            streamingEntity = ws;
            return ws;
        }

        /// <summary>
        /// Immediately closes the connection with the client and does not send any response. 
        /// </summary>
        /// <remarks>
        /// This method returns an <see cref="HttpResponse"/> indicated to exit outside the scope of the request
        /// context. However, when calling this method, the connection is interrupted instantly.
        /// </remarks>
        public HttpResponse Abort () {
            listenerResponse.Abort ();
            return HttpResponse.Refuse ();
        }

        /// <summary>
        /// Gets an string representation of this <see cref="HttpRequest"/> object.
        /// </summary>
        public override string ToString () {
            return $"{Method} {FullPath}";
        }

        private void Dispose ( bool disposing ) {
            if (!disposedValue) {
                if (disposing) {
                    streamingEntity?.Dispose ();
                    streamingEntity = null;

                    contentBytes = null;
                }

                disposedValue = true;
            }
        }

        void IDisposable.Dispose () {
            Dispose ( disposing: true );
            GC.SuppressFinalize ( this );
        }

        ///
        ~HttpRequest () {
            Dispose ( disposing: false );
        }
    }
}