// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerEngine.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Sisk.Core.Http.Streams;

namespace Sisk.Core.Http.Engine;

using System.Net.WebSockets;

/// <summary>
/// Provides an abstract base class for HTTP server engines.
/// </summary>
public abstract class HttpServerEngine : IDisposable {

    /// <summary>
    /// Gets or sets the timeout for idle connections.
    /// </summary>
    /// <value>
    /// The <see cref="TimeSpan"/> representing the idle connection timeout.
    /// </value>
    public abstract TimeSpan IdleConnectionTimeout { get; set; }

    /// <summary>
    /// Adds a listening prefix to the server.
    /// </summary>
    /// <param name="prefix">The prefix to add.</param>
    public abstract void AddListeningPrefix ( string prefix );

    /// <summary>
    /// Clears all listening prefixes from the server.
    /// </summary>
    public abstract void ClearPrefixes ();

    /// <summary>
    /// Starts the HTTP server.
    /// </summary>
    public abstract void StartServer ();

    /// <summary>
    /// Stops the HTTP server.
    /// </summary>
    public abstract void StopServer ();

    /// <summary>
    /// Begins an asynchronous operation to get an HTTP context.
    /// </summary>
    /// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
    /// <param name="state">An object that provides state information for the asynchronous operation.</param>
    /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous operation.</returns>
    public abstract IAsyncResult BeginGetContext ( AsyncCallback? callback, object? state );

    /// <summary>
    /// Ends an asynchronous operation to get an HTTP context.
    /// </summary>
    /// <param name="asyncResult">The <see cref="IAsyncResult"/> that references the pending asynchronous operation.</param>
    /// <returns>An <see cref="HttpServerEngineContext"/> representing the HTTP context.</returns>
    public abstract HttpServerEngineContext EndGetContext ( IAsyncResult asyncResult );

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public abstract void Dispose ();
}

/// <summary>
/// Provides an abstract base class for HTTP contexts.
/// </summary>
public abstract class HttpServerEngineContext {
    /// <summary>
    /// Gets the HTTP request associated with the context.
    /// </summary>
    /// <value>
    /// The <see cref="HttpServerEngineContextRequest"/> representing the HTTP request.
    /// </value>
    public abstract HttpServerEngineContextRequest Request { get; }

    /// <summary>
    /// Gets the HTTP response associated with the context.
    /// </summary>
    /// <value>
    /// The <see cref="HttpServerEngineContextResponse"/> representing the HTTP response.
    /// </value>
    public abstract HttpServerEngineContextResponse Response { get; }

    /// <summary>
    /// Gets a value that indicates whether the HTTP connection has been aborted.
    /// </summary>
    /// <value>
    /// A <see cref="CancellationToken"/> that can be used to signal that the HTTP connection has been aborted.
    /// </value>
    public virtual CancellationToken ContextAbortedToken { get; } = CancellationToken.None;

    /// <summary>
    /// Accepts a WebSocket connection asynchronously.
    /// </summary>
    /// <param name="subProtocol">The subprotocol to use for the WebSocket connection.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains the <see cref="HttpServerEngineWebSocket"/>.</returns>
    public abstract Task<HttpServerEngineWebSocket> AcceptWebSocketAsync ( string? subProtocol );
}

/// <summary>
/// Provides an abstract base class for WebSocket contexts.
/// </summary>
public abstract class HttpServerEngineWebSocket {
    /// <summary>
    /// Gets the state of the WebSocket.
    /// </summary>
    /// <value>
    /// The <see cref="WebSocketState"/> representing the state of the WebSocket.
    /// </value>
    public abstract WebSocketState State { get; }

    /// <summary>
    /// Closes the output stream of the WebSocket asynchronously.
    /// </summary>
    /// <param name="closeStatus">The status code for closing the WebSocket.</param>
    /// <param name="reason">The reason for closing the WebSocket.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public abstract Task CloseOutputAsync ( WebSocketCloseStatus closeStatus, string? reason, CancellationToken cancellation );

    /// <summary>
    /// Closes the WebSocket connection asynchronously.
    /// </summary>
    /// <param name="closeStatus">The status code for closing the WebSocket.</param>
    /// <param name="reason">The reason for closing the WebSocket.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public abstract Task CloseAsync ( WebSocketCloseStatus closeStatus, string? reason, CancellationToken cancellation );

    /// <summary>
    /// Receives data from the WebSocket asynchronously.
    /// </summary>
    /// <param name="buffer">The buffer to receive the data into.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation. The result contains the <see cref="ValueWebSocketReceiveResult"/>.</returns>
    public abstract ValueTask<ValueWebSocketReceiveResult> ReceiveAsync ( Memory<byte> buffer, CancellationToken cancellationToken );

    /// <summary>
    /// Sends data over the WebSocket asynchronously.
    /// </summary>
    /// <param name="buffer">The buffer containing the data to send.</param>
    /// <param name="messageType">The type of message to send.</param>
    /// <param name="endOfMessage">A value indicating whether this is the end of the message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public abstract ValueTask SendAsync ( ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken );
}

/// <summary>
/// Provides an abstract base class for HTTP requests.
/// </summary>
public abstract class HttpServerEngineContextRequest {
    /// <summary>
    /// Gets a value indicating whether the request is from the local machine.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the request is from the local machine; otherwise, <see langword="false"/>.
    /// </value>
    public abstract bool IsLocal { get; }

    /// <summary>
    /// Gets the raw URL of the request.
    /// </summary>
    /// <value>
    /// The raw URL of the request.
    /// </value>
    public abstract string? RawUrl { get; }

    /// <summary>
    /// Gets the query string collection.
    /// </summary>
    /// <value>
    /// The <see cref="NameValueCollection"/> containing the query string parameters.
    /// </value>
    public abstract NameValueCollection QueryString { get; }

    /// <summary>
    /// Gets the HTTP protocol version.
    /// </summary>
    /// <value>
    /// The <see cref="Version"/> representing the HTTP protocol version.
    /// </value>
    public abstract Version ProtocolVersion { get; }

    /// <summary>
    /// Gets the host name of the user.
    /// </summary>
    /// <value>
    /// The host name of the user.
    /// </value>
    public abstract string UserHostName { get; }

    /// <summary>
    /// Gets the URL of the request.
    /// </summary>
    /// <value>
    /// The <see cref="Uri"/> representing the URL of the request.
    /// </value>
    public abstract Uri? Url { get; }

    /// <summary>
    /// Gets the HTTP method of the request.
    /// </summary>
    /// <value>
    /// The HTTP method of the request.
    /// </value>
    public abstract string HttpMethod { get; }

    /// <summary>
    /// Gets the local endpoint of the request.
    /// </summary>
    /// <value>
    /// The <see cref="IPEndPoint"/> representing the local endpoint.
    /// </value>
    public abstract IPEndPoint LocalEndPoint { get; }

    /// <summary>
    /// Gets the remote endpoint of the request.
    /// </summary>
    /// <value>
    /// The <see cref="IPEndPoint"/> representing the remote endpoint.
    /// </value>
    public abstract IPEndPoint RemoteEndPoint { get; }

    /// <summary>
    /// Gets the request trace identifier.
    /// </summary>
    /// <value>
    /// The <see cref="Guid"/> representing the request trace identifier.
    /// </value>
    public abstract Guid RequestTraceIdentifier { get; }

    /// <summary>
    /// Gets the HTTP headers.
    /// </summary>
    /// <value>
    /// The <see cref="WebHeaderCollection"/> containing the HTTP headers.
    /// </value>
    public abstract NameValueCollection Headers { get; }

    /// <summary>
    /// Gets the input stream of the request.
    /// </summary>
    /// <value>
    /// The <see cref="Stream"/> representing the input stream.
    /// </value>
    public abstract Stream InputStream { get; }

    /// <summary>
    /// Gets the content length of the request.
    /// </summary>
    /// <value>
    /// The content length of the request.
    /// </value>
    public abstract long ContentLength64 { get; }

    /// <summary>
    /// Gets a value indicating whether the connection is secure.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the connection is secure; otherwise, <see langword="false"/>.
    /// </value>
    public abstract bool IsSecureConnection { get; }

    /// <summary>
    /// Gets the content encoding of the request.
    /// </summary>
    /// <value>
    /// The <see cref="Encoding"/> representing the content encoding.
    /// </value>
    public abstract Encoding ContentEncoding { get; }
}

/// <summary>
/// Provides an abstract base class for HTTP responses.
/// </summary>
public abstract class HttpServerEngineContextResponse : IDisposable {
    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    /// <value>
    /// The HTTP status code.
    /// </value>
    public abstract int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the status description.
    /// </summary>
    /// <value>
    /// The status description.
    /// </value>
    public abstract string StatusDescription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the connection should be kept alive.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the connection should be kept alive; otherwise, <see langword="false"/>.
    /// </value>
    public abstract bool KeepAlive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether chunked transfer encoding is used.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if chunked transfer encoding is used; otherwise, <see langword="false"/>.
    /// </value>
    public abstract bool SendChunked { get; set; }

    /// <summary>
    /// Gets or sets the content length of the response.
    /// </summary>
    /// <value>
    /// The content length of the response.
    /// </value>
    public abstract long ContentLength64 { get; set; }

    /// <summary>
    /// Gets or sets the content type of the response.
    /// </summary>
    /// <value>
    /// The content type of the response.
    /// </value>
    public abstract string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the HTTP headers.
    /// </summary>
    /// <value>
    /// The <see cref="IHttpEngineHeaderList"/> containing the HTTP headers.
    /// </value>
    public abstract IHttpEngineHeaderList Headers { get; }

    /// <summary>
    /// Appends a header to the response.
    /// </summary>
    /// <param name="name">The name of the header.</param>
    /// <param name="value">The value of the header.</param>
    public abstract void AppendHeader ( string name, string value );

    /// <summary>
    /// Aborts the response.
    /// </summary>
    public abstract void Abort ();

    /// <summary>
    /// Closes the response.
    /// </summary>
    public abstract void Close ();

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public abstract void Dispose ();

    /// <summary>
    /// Gets the output stream of the response.
    /// </summary>
    /// <value>
    /// The <see cref="Stream"/> representing the output stream.
    /// </value>
    public abstract Stream OutputStream { get; }
}