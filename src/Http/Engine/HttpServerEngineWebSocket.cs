// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerEngineWebSocket.cs
// Repository:  https://github.com/sisk-http/core

using System.Net.WebSockets;

namespace Sisk.Core.Http.Engine;

/// <summary>
/// Provides an abstract base class for WebSocket contexts.
/// </summary>
public abstract class HttpServerEngineWebSocket {

    /// <summary>
    /// Creates a concrete <see cref="HttpServerEngineWebSocket"/> instance from the specified <see cref="WebSocket"/>.
    /// </summary>
    /// <param name="ws">The <see cref="WebSocket"/> to wrap.</param>
    /// <returns>A new <see cref="HttpServerEngineWebSocket"/> instance that represents the supplied WebSocket.</returns>
    public static HttpServerEngineWebSocket CreateFromWebSocket ( WebSocket ws ) {
        return new HttpServerEngineDefaultWebSocket ( ws );
    }

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