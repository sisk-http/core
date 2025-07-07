// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpWebSocket.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Sisk.Core.Internal;

namespace Sisk.Core.Http.Streams {

    /// <summary>
    /// Provides an persistent bi-directional socket between the client and the HTTP server.
    /// </summary>
    public sealed class HttpWebSocket : IDisposable {
        bool isDisposed;
        long length;
        readonly HttpStreamPingPolicy pingPolicy;

        internal byte [] receiveBuffer = new byte [ 131072 ];
        internal SemaphoreSlim sendSemaphore = new SemaphoreSlim ( 1 );
        internal SemaphoreSlim receiveSemaphore = new SemaphoreSlim ( 1 );
        internal HttpListenerWebSocketContext ctx;
        internal HttpRequest request;
        internal bool _isClosed;
        internal bool wasServerClosed;
        internal string? _identifier;

        /// <summary>
        /// Gets the <see cref="HttpStreamPingPolicy"/> for this HTTP web socket connection.
        /// </summary>
        public HttpStreamPingPolicy PingPolicy => pingPolicy;

        /// <summary>
        /// Gets or sets an object linked with this <see cref="WebSocket"/> session.
        /// </summary>
        public object? State { get; set; }

        /// <summary>
        /// Gets the <see cref="Sisk.Core.Http.HttpRequest"/> object which created this Web Socket instance.
        /// </summary>
        public HttpRequest HttpRequest => request;

        /// <summary>
        /// Gets an boolean indicating if this Web Socket connection is closed.
        /// </summary>
        public bool IsClosed => _isClosed;

        /// <summary>
        /// Gets an unique identifier label to this Web Socket connection, useful for finding this connection's reference later.
        /// </summary>
        public string? Identifier => _identifier;

        internal HttpWebSocket ( HttpListenerWebSocketContext ctx, HttpRequest req, string? identifier ) {
            this.ctx = ctx;
            request = req;
            _identifier = identifier;
            pingPolicy = new HttpStreamPingPolicy ( this );

            if (identifier != null) {
                req.baseServer._wsCollection.RegisterWebSocket ( this );
            }
        }

        /// <summary>
        /// Sends an asynchronous text message to the WebSocket endpoint.
        /// </summary>
        /// <param name="message">The text message to send.</param>
        /// <param name="cancellation">The <see cref="CancellationToken"/> to use for cancellation.</param>
        /// <returns>A <see cref="ValueTask{T}"/> that represents the asynchronous send operation, 
        /// which returns <see langword="true"/> if the message was sent successfully; otherwise, <see langword="false"/>.</returns>
        public ValueTask<bool> SendAsync ( string message, CancellationToken cancellation = default ) {
            ArgumentNullException.ThrowIfNull ( message );
            return SendInternalAsync ( Encoding.UTF8.GetBytes ( message ), WebSocketMessageType.Text, cancellation );
        }

        /// <summary>
        /// Sends an asynchronous binary message to the WebSocket endpoint.
        /// </summary>
        /// <param name="buffer">The binary data to send.</param>
        /// <param name="cancellation">The <see cref="CancellationToken"/> to use for cancellation.</param>
        /// <returns>A <see cref="ValueTask{T}"/> that represents the asynchronous send operation, 
        /// which returns <see langword="true"/> if the message was sent successfully; otherwise, <see langword="false"/>.</returns>
        public ValueTask<bool> SendAsync ( ReadOnlyMemory<byte> buffer, CancellationToken cancellation = default ) {
            return SendInternalAsync ( buffer, WebSocketMessageType.Binary, cancellation );
        }

        /// <summary>
        /// Closes the WebSocket connection asynchronously.
        /// </summary>
        /// <param name="cancellation">The <see cref="CancellationToken"/> to use for cancellation.</param>
        /// <returns>A <see cref="Task{T}"/> that represents the asynchronous close operation, 
        /// which returns an <see cref="HttpResponse"/> indicating the result of the close operation.</returns>
        public async Task<HttpResponse> CloseAsync ( CancellationToken cancellation = default ) {
            if (!_isClosed) {
                if (ctx.WebSocket.State != WebSocketState.Closed && ctx.WebSocket.State != WebSocketState.Aborted) {
                    try {
                        await ctx.WebSocket.CloseOutputAsync ( WebSocketCloseStatus.NormalClosure, null, cancellation );
                    }
                    catch (Exception) {
                        ;
                    }
                    finally {
                        wasServerClosed = true;
                    }
                }
                request.baseServer._wsCollection.UnregisterWebSocket ( this );
                _isClosed = true;
            }
            return new HttpResponse ( wasServerClosed ? HttpResponse.HTTPRESPONSE_SERVER_CLOSE : HttpResponse.HTTPRESPONSE_CLIENT_CLOSE ) {
                CalculedLength = length
            };
        }

        private async ValueTask<WebSocketMessage?> ReceiveInternalAsync ( CancellationToken cancellation ) {
            ArraySegment<byte> buffer = new ArraySegment<byte> ( receiveBuffer );
            WebSocketReceiveResult? result = null;

            if (ctx.WebSocket.State != WebSocketState.Open)
                return null;

            using (var ms = new MemoryStream ()) {

                await receiveSemaphore.WaitAsync ( cancellation );

waitNextMessage:
                if (cancellation.IsCancellationRequested)
                    return null;

                try {
                    do {
                        result = await ctx.WebSocket.ReceiveAsync ( buffer, cancellation );
                        ms.Write ( buffer.Array!, buffer.Offset, result.Count );
                    } while (!result.EndOfMessage);

                    ms.Seek ( 0, SeekOrigin.Begin );

                    if (result.MessageType == WebSocketMessageType.Close) {
                        await ctx.WebSocket.CloseAsync ( WebSocketCloseStatus.NormalClosure, string.Empty, cancellation );
                        await CloseAsync ( cancellation );

                        if (result.Count == 0) {
                            return null;
                        }
                    }

                    var wsmessage = new WebSocketMessage ( this, ms.ToArray () );

                    if (wsmessage.GetString () == pingPolicy.DataMessage) {
                        // ignore this message
                        goto waitNextMessage;
                    }

                    return wsmessage;
                }
                catch {
                    return null;
                }
                finally {
                    receiveSemaphore.Release ();
                }
            }
        }

        private async ValueTask<bool> SendInternalAsync ( ReadOnlyMemory<byte> buffer, WebSocketMessageType msgType, CancellationToken cancellation ) {
            if (_isClosed)
                return false;
            if (ctx.WebSocket.State != WebSocketState.Open && ctx.WebSocket.State != WebSocketState.CloseSent)
                return false;

            await sendSemaphore.WaitAsync ( cancellation );
            try {
                await ctx.WebSocket.SendAsync ( buffer, msgType, true, cancellation );
                length += buffer.Length;
                return true;
            }
            catch {
                await CloseAsync ( cancellation );
                return false;
            }
            finally {
                sendSemaphore.Release ();
            }
        }

        /// <summary>
        /// Receives a message from the WebSocket endpoint asynchronously.
        /// </summary>
        /// <param name="cancellation">The <see cref="CancellationToken"/> to use for cancellation.</param>
        /// <returns>A <see cref="ValueTask{T}"/> that represents the asynchronous receive operation, 
        /// which returns a <see cref="WebSocketMessage"/> if a message is received; otherwise, <c>null</c>.</returns>
        public ValueTask<WebSocketMessage?> ReceiveMessageAsync ( CancellationToken cancellation = default ) {
            return ReceiveInternalAsync ( cancellation );
        }

        /// <summary>
        /// Receives a message from the WebSocket endpoint asynchronously with a specified timeout.
        /// </summary>
        /// <param name="timeout">The time to wait for a message before timing out.</param>
        /// <returns>A <see cref="ValueTask{T}"/> that represents the asynchronous receive operation, 
        /// which returns a <see cref="WebSocketMessage"/> if a message is received; otherwise, <c>null</c>.</returns>
        public ValueTask<WebSocketMessage?> ReceiveMessageAsync ( TimeSpan timeout ) {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource ( timeout );
            return ReceiveInternalAsync ( cancellationTokenSource.Token );
        }

        /// <summary>
        /// Receives a message from the WebSocket endpoint asynchronously with a default timeout of 30 seconds.
        /// </summary>
        /// <returns>A <see cref="ValueTask{T}"/> that represents the asynchronous receive operation, 
        /// which returns a <see cref="WebSocketMessage"/> if a message is received; otherwise, <c>null</c>.</returns>
        public ValueTask<WebSocketMessage?> ReceiveMessageAsync () => ReceiveMessageAsync ( TimeSpan.FromSeconds ( 30 ) );

        /// <inheritdoc/>
        public void Dispose () {
            if (isDisposed)
                return;

            GC.SuppressFinalize ( this );

            pingPolicy.Dispose ();
            receiveSemaphore.Dispose ();
            sendSemaphore.Dispose ();

            isDisposed = true;
            _isClosed = true;
        }

        /// <exclude/>
        ~HttpWebSocket () {
            Dispose ();
        }
    }

    /// <summary>
    /// Represents an websocket request message received by an websocket server.
    /// </summary>
    public sealed class WebSocketMessage {
        internal byte [] __msgBytes;

        /// <summary>
        /// Gets an byte array with the message contents.
        /// </summary>
        public byte [] MessageBytes => __msgBytes;

        /// <summary>
        /// Gets the message length in byte count.
        /// </summary>
        public int Length => __msgBytes.Length;

        /// <summary>
        /// Gets the sender <see cref="HttpWebSocket"/> object instance which received this message.
        /// </summary>
        public HttpWebSocket Sender { get; }

        /// <summary>
        /// Reads the message bytes as string using the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding which will be used to decode the message.</param>
        public string GetString ( Encoding encoding ) {
            return encoding.GetString ( MessageBytes );
        }

        /// <summary>
        /// Reads the message bytes as string using the HTTP request encoding.
        /// </summary>
        public string GetString () {
            return GetString ( Sender.HttpRequest.RequestEncoding );
        }

        internal WebSocketMessage ( HttpWebSocket httpws, byte [] msgBytes ) {
            Sender = httpws;
            __msgBytes = msgBytes;
        }
    }
}
