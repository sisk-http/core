// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequest.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics;
using System.Net;
using System.Text;
using Sisk.Core.Entity;
using Sisk.Core.Helpers;
using Sisk.Core.Http.Streams;
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
    public sealed class HttpRequest {
        internal HttpServer baseServer;
        private readonly HttpServerConfiguration contextServerConfiguration;
        private readonly HttpListenerResponse listenerResponse;
        private readonly HttpListenerRequest listenerRequest;
        private readonly HttpListenerContext context;
        private byte []? contentBytes;
        internal bool isStreaming;
        private HttpRequestEventSource? activeEventSource;
        private HttpHeaderCollection? headers = null;
        private StringKeyStore? cookies = null;
        private StringValueCollection? query = null;

        private IPAddress remoteAddr;
        private HttpMethod requestMethod;

        private int currentFrame;

        internal HttpRequest (
            HttpServer server,
            HttpListenerContext context ) {

            this.context = context;
            this.baseServer = server;
            this.contextServerConfiguration = this.baseServer.ServerConfiguration;
            this.listenerResponse = context.Response;
            this.listenerRequest = context.Request;
            this.RequestedAt = DateTime.UtcNow.Add ( HttpServer.environmentUtcOffset );

            this.ContentLength = this.listenerRequest.ContentLength64;
            this.remoteAddr = this.ReadRequestRemoteAddr ();
            this.requestMethod = new HttpMethod ( this.listenerRequest.HttpMethod );
        }

        internal string mbConvertCodepage ( string input, Encoding inEnc, Encoding outEnc ) {
            byte [] tempBytes;
            tempBytes = inEnc.GetBytes ( input );
            return outEnc.GetString ( tempBytes );
        }

        IPAddress ReadRequestRemoteAddr () {
            if (this.contextServerConfiguration.ForwardingResolver is { } fr) {
                return fr.OnResolveClientAddress ( this, this.listenerRequest.RemoteEndPoint );
            }
            else {
                return new IPAddress ( this.listenerRequest.RemoteEndPoint.Address.GetAddressBytes () );
            }
        }

        byte [] ReadRequestStreamContents () {
            if (this.contentBytes is null) {
                if (this.ContentLength > Int32.MaxValue) {
                    throw new OutOfMemoryException ( SR.HttpRequest_ContentAbove2G );
                }
                else if (this.ContentLength > 0) {
                    using (var memoryStream = new MemoryStream ( (int) this.ContentLength )) {
                        this.listenerRequest.InputStream.CopyTo ( memoryStream );
                        this.contentBytes = memoryStream.ToArray ();
                    }
                }
                else if (this.ContentLength < 0) {
                    this.contentBytes = Array.Empty<byte> ();
                    throw new HttpRequestException ( SR.HttpRequest_NoContentLength );
                }
                else // = 0
                {
                    this.contentBytes = Array.Empty<byte> ();
                }
            }

            return this.contentBytes;
        }

        /// <summary>
        /// Gets a unique random ID for this request.
        /// </summary>
        public Guid RequestId { get => this.listenerRequest.RequestTraceIdentifier; }

        /// <summary>
        /// Gets a boolean indicating whether this request was locally made by an secure
        /// transport context (SSL/TLS) or not.
        /// </summary>
        /// <remarks>
        /// This property brings local request data, so it may not reflect the original client request when used with proxy or CDNs.
        /// </remarks>
        public bool IsSecure {
            get {
                if (this.contextServerConfiguration.ForwardingResolver is { } fr) {
                    return fr.OnResolveSecureConnection ( this, this.listenerRequest.IsSecureConnection );
                }
                else {
                    return this.listenerRequest.IsSecureConnection;
                }
            }
        }

        /// <summary>
        /// Gets a boolean indicating whether this request has body contents and whether
        /// it has already been read into memory by the server.
        /// </summary>
        public bool IsContentAvailable { get => this.HasContents && this.contentBytes is not null; }

        /// <summary>
        /// Gets a boolean indicating whether this request has body contents.
        /// </summary>
        public bool HasContents { get => this.ContentLength > 0; }

        /// <summary>
        /// Gets the HTTP request headers.
        /// </summary>
        public HttpHeaderCollection Headers {
            get {
                if (this.headers is null) {
                    if (this.contextServerConfiguration.Flags.NormalizeHeadersEncodings) {
                        this.headers = new HttpHeaderCollection ();
                        Encoding entryCodepage = Encoding.GetEncoding ( "ISO-8859-1" );
                        foreach (string headerName in this.listenerRequest.Headers) {
                            string headerValue = this.listenerRequest.Headers [ headerName ]!;
                            this.headers.Add (
                                headerName,
                                this.mbConvertCodepage ( headerValue, entryCodepage, this.listenerRequest.ContentEncoding )
                            );
                        }
                    }
                    else {
                        this.headers = new HttpHeaderCollection ();
                        this.headers.ImportNameValueCollection ( (WebHeaderCollection) this.listenerRequest.Headers );
                    }

                    this.headers.MakeReadOnly ();
                }

                return this.headers;
            }
        }

        /// <summary>
        /// Gets an <see cref="StringKeyStore"/> object with all cookies set in this request.
        /// </summary>
        public StringKeyStore Cookies {
            get {
                if (this.cookies is null) {
                    string? cookieHeader = this.listenerRequest.Headers [ HttpKnownHeaderNames.Cookie ];
                    StringKeyStore store = new StringKeyStore ();
                    if (cookieHeader is not null) {
                        store.ImportCookieString ( cookieHeader );
                    }
                    store.MakeReadOnly ();
                    this.cookies = store;
                }

                return this.cookies;
            }
        }

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
        public TypedValueDictionary Bag => this.Context.RequestBag;

        /// <summary>
        /// Get the requested host header with the port from this HTTP request.
        /// </summary>
        /// <remarks>
        /// This property brings local request data, so it may not reflect the original client request when used with proxy or CDNs.
        /// </remarks>
        public string Authority {
            get => this.listenerRequest.Url!.Authority;
        }

        /// <summary>
        /// Gets the HTTP request path without the query string.
        /// </summary>
        public string Path {
            get => this.listenerRequest.Url?.AbsolutePath ?? "/";
        }

        /// <summary>
        /// Gets the raw, full HTTP request path with the query string.
        /// </summary>
        public string FullPath {
            get => this.listenerRequest.RawUrl ?? "/";
        }

        /// <summary>
        /// Gets the full URL for this request, with scheme, host, port, path and query.
        /// </summary>
        /// <remarks>
        /// This property brings local request data, so it may not reflect the original client request when used with proxy or CDNs.
        /// </remarks>
        public string FullUrl {
            get => this.listenerRequest.Url!.ToString ();
        }

        /// <summary>
        /// Gets the <see cref="System.Uri"/> component for this HTTP request requested URL.
        /// </summary>
        public Uri Uri {
            get => this.listenerRequest.Url!;
        }

        /// <summary>
        /// Gets an string <see cref="Encoding"/> that can be used to decode text in this HTTP request.
        /// </summary>
        public Encoding RequestEncoding {
            get => this.listenerRequest.ContentEncoding;
        }

        /// <summary>
        /// Gets the HTTP request method.
        /// </summary>
        public HttpMethod Method {
            get => this.requestMethod;
        }

        /// <summary>
        /// Gets the HTTP request body as string, decoded by the request content encoding.
        /// </summary>
        /// <remarks>
        /// When calling this property, the entire content of the request is read into memory and stored in <see cref="RawBody"/>.
        /// </remarks>
        [DebuggerBrowsable ( DebuggerBrowsableState.Never )]
        public string Body {
            get => this.listenerRequest.ContentEncoding.GetString ( this.RawBody );
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
                return this.ReadRequestStreamContents ();
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
                if (this.query is null) {
                    var sv = new StringValueCollection ( "query parameter" );
                    sv.ImportNameValueCollection ( this.listenerRequest.QueryString );
                    sv.MakeReadOnly ();
                    this.query = sv;
                }
                return this.query;
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
        public string QueryString { get => this.listenerRequest.Url?.Query ?? string.Empty; }

        /// <summary>
        /// Gets the incoming local IP address from the request.
        /// </summary>
        public IPAddress RemoteAddress {
            get {
                return this.remoteAddr;
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
        /// Reads the request body and obtains a <see cref="MultipartFormCollection"/> from it.
        /// </summary>
        public MultipartFormCollection GetMultipartFormContent () {
            try {
                return MultipartObject.ParseMultipartObjects ( this );
            }
            catch (Exception ex) {
                throw new HttpRequestException ( SR.Format ( SR.MultipartFormReader_Exception, ex.Message ), ex );
            }
        }

        /// <summary>
        /// Reads the request body and extracts form data parameters from it.
        /// </summary>
        public StringKeyStore GetFormContent () {
            return StringKeyStore.FromQueryString ( this.Body );
        }

        /// <summary>
        /// Gets a visual representation of this request.
        /// </summary>
        /// <param name="includeBody">Optional. Defines if the body should be included in the output.</param>
        /// <param name="appendExtraInfo">Optional. Appends extra information, such as request id and date into the output.</param>
        public string GetRawHttpRequest ( bool includeBody = true, bool appendExtraInfo = false ) {
            StringBuilder sb = new StringBuilder ();
            // Method and path
            sb.Append ( this.Method.ToString ().ToUpper () + " " );
            sb.Append ( this.Path + " " );
            sb.Append ( "HTTP/" );
            sb.Append ( this.listenerRequest.ProtocolVersion.Major + "." );
            sb.Append ( this.listenerRequest.ProtocolVersion.Minor + "\n" );

            // Headers
            if (appendExtraInfo) {
                sb.AppendLine ( $":remote-ip: {this.RemoteAddress} (was {this.listenerRequest.RemoteEndPoint})" );
                sb.AppendLine ( $":host: {this.Host} (was {this.listenerRequest.UserHostName})" );
                sb.AppendLine ( $":date: {this.RequestedAt:s}" );
                sb.AppendLine ( $":request-id: {this.RequestId}" );
                sb.AppendLine ( $":request-proto: {(this.IsSecure ? "https" : "http")}" );
            }
            sb.AppendLine ( this.Headers.ToString () );
            sb.AppendLine ();

            // Content
            if (includeBody) {
                if (this.Body.Length < 8 * SizeHelper.UnitKb) {
                    sb.Append ( this.Body );
                }
                else {
                    sb.Append ( $"| ({SizeHelper.HumanReadableSize ( this.Body.Length )})" );
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
            Interlocked.Increment ( ref this.currentFrame );
            if (this.currentFrame > 64) {
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
            if (this.contentBytes is not null) {
                throw new InvalidOperationException ( SR.HttpRequest_InputStreamAlreadyLoaded );
            }
            return this.listenerRequest.InputStream;
        }

        /// <summary>
        /// Gets an HTTP response stream for this HTTP request.
        /// </summary>
        public HttpResponseStream GetResponseStream () {
            if (this.isStreaming) {
                throw new InvalidOperationException ( SR.HttpRequest_AlreadyInStreamingState );
            }
            this.isStreaming = true;
            return new HttpResponseStream ( this.listenerResponse, this.listenerRequest, this );
        }

        /// <summary>
        /// Gets an Event Source interface for this request. Calling this method will put this <see cref="HttpRequest"/> instance in it's
        /// event source listening state.
        /// </summary>
        /// <param name="identifier">Optional. Defines an label to the EventStream connection, useful for finding this connection's reference later.</param>
        public HttpRequestEventSource GetEventSource ( string? identifier = null ) {
            if (this.isStreaming) {
                throw new InvalidOperationException ( SR.HttpRequest_AlreadyInStreamingState );
            }
            this.isStreaming = true;
            this.activeEventSource = new HttpRequestEventSource ( identifier, this.listenerResponse, this.listenerRequest, this );
            return this.activeEventSource;
        }

        /// <summary>
        /// Accepts and acquires a websocket for this request. Calling this method will put this <see cref="HttpRequest"/> instance in
        /// streaming state.
        /// </summary>
        /// <param name="subprotocol">Optional. Determines the sub-protocol to plug the websocket in.</param>
        /// <param name="identifier">Optional. Defines an label to the Web Socket connection, useful for finding this connection's reference later.</param>
        public HttpWebSocket GetWebSocket ( string? subprotocol = null, string? identifier = null ) {
            if (this.isStreaming) {
                throw new InvalidOperationException ( SR.HttpRequest_AlreadyInStreamingState );
            }
            this.isStreaming = true;
            var accept = this.context.AcceptWebSocketAsync ( subprotocol ).Result;
            return new HttpWebSocket ( accept, this, identifier );
        }

        /// <summary>
        /// Immediately closes the connection with the client and does not send any response. 
        /// </summary>
        /// <remarks>
        /// This method returns an <see cref="HttpResponse"/> indicated to exit outside the scope of the request
        /// context. However, when calling this method, the connection is interrupted instantly.
        /// </remarks>
        public HttpResponse Abort () {
            this.listenerResponse.Abort ();
            return HttpResponse.Refuse ();
        }

        /// <summary>
        /// Gets an string representation of this <see cref="HttpRequest"/> object.
        /// </summary>
        public override string ToString () {
            return $"{this.Method} {this.FullPath}";
        }
    }
}