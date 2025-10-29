// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerEngineContext.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http.Engine;


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