// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpWebSocket.cs
// Repository:  https://github.com/sisk-http/core

using System.Net.WebSockets;
using System.Runtime.CompilerServices;

namespace Sisk.Core.Http.Streams {

    /// <summary>
    /// Provides an persistent bi-directional socket between the client and the HTTP server.
    /// </summary>
    public sealed class HttpWebSocket : IDisposable {
        bool isListening = true;
        readonly HttpStreamPingPolicy pingPolicy;

        internal WebSocketMessage? lastMessage;
        internal CancellationTokenSource asyncListenerToken = null!;
        internal ManualResetEvent closeEvent = new ManualResetEvent ( false );
        internal ManualResetEvent waitNextEvent = new ManualResetEvent ( false );
        internal Thread receiveThread;
        internal HttpListenerWebSocketContext ctx;
        internal HttpRequest request;
        internal TimeSpan closeTimeout = TimeSpan.Zero;
        internal bool _isClosed;
        internal bool isWaitingNext;
        internal bool wasServerClosed;
        internal string? _identifier;

        const int BUFFER_LENGTH = 1024;
        int attempt;
        long length;

        /// <summary>
        /// Gets the <see cref="HttpStreamPingPolicy"/> for this HTTP web socket connection.
        /// </summary>
        public HttpStreamPingPolicy PingPolicy => pingPolicy;

        /// <summary>
        /// Gets or sets the maximum wait time for synchronous listener methods like <see cref="WaitNext()"/>.
        /// </summary>
        public TimeSpan WaitTimeout { get; set; } = TimeSpan.FromSeconds ( 60 );

        /// <summary>
        /// Gets or sets the maximum number of attempts to send a failed message before the server closes the connection. Set it to -1 to
        /// don't close the connection on failed attempts.
        /// </summary>
        public int MaxAttempts { get; set; } = 3;

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

        /// <summary>
        /// Represents the event which is called when this web socket receives an message from
        /// remote origin.
        /// </summary>
        public event EventHandler<WebSocketMessage>? OnReceive;

        internal HttpWebSocket ( HttpListenerWebSocketContext ctx, HttpRequest req, string? identifier ) {
            this.ctx = ctx;
            request = req;
            _identifier = identifier;
            pingPolicy = new HttpStreamPingPolicy ( this );

            if (identifier != null) {
                req.baseServer._wsCollection.RegisterWebSocket ( this );
            }

            receiveThread = new Thread ( new ThreadStart ( ReceiveTask ) );
            receiveThread.IsBackground = true;
            receiveThread.Start ();
        }

        void RecreateAsyncToken () {
            asyncListenerToken = new CancellationTokenSource ();
            if (closeTimeout.TotalMilliseconds > 0)
                asyncListenerToken.CancelAfter ( closeTimeout );
            asyncListenerToken.Token.ThrowIfCancellationRequested ();
        }

        void TrimMessage ( WebSocketReceiveResult result, WebSocketMessage message ) {
            if (result.Count < message.Length) {
                byte [] trimmed = new byte [ result.Count ];
                for (int i = 0; i < trimmed.Length; i++) {
                    trimmed [ i ] = message.MessageBytes [ i ];
                }
                message.__msgBytes = trimmed;
            }
            message.IsClose = result.MessageType == WebSocketMessageType.Close;
            message.IsEnd = result.EndOfMessage;

            if (result.MessageType == WebSocketMessageType.Close) {
                _isClosed = true;
                isListening = false;
                closeEvent.Set ();
            }
        }

        internal async void ReceiveTask () {
            while (isListening) {
                RecreateAsyncToken ();
                WebSocketMessage message = new WebSocketMessage ( this, BUFFER_LENGTH );

                var arrSegment = new ArraySegment<byte> ( message.__msgBytes );
                WebSocketReceiveResult result;

                try {
                    result = await ctx.WebSocket.ReceiveAsync ( arrSegment, asyncListenerToken.Token );
                }
                catch (Exception) {
                    if (ctx.WebSocket.State != WebSocketState.Open
                     && ctx.WebSocket.State != WebSocketState.Connecting) {
                        Close ();
                        break;
                    }
                    continue;
                }

                if (result.Count == 0 || result.CloseStatus != null) {
                    Close ();
                    break;
                }

                TrimMessage ( result, message );
                bool isPingMessage = message.GetString () == pingPolicy.DataMessage;

                if (isWaitingNext & !isPingMessage) {
                    isWaitingNext = false;
                    lastMessage = message;
                    waitNextEvent.Set ();
                }
                else {
                    OnReceive?.Invoke ( this, message );
                }
            }
        }

        /// <summary>
        /// Configures the ping policy for this instance of HTTP Web Socket.
        /// </summary>
        /// <param name="act">The method that runs on the ping policy for this HTTP Web Socket.</param>
        public HttpWebSocket WithPing ( Action<HttpStreamPingPolicy> act ) {
            act ( pingPolicy );
            return this;
        }

        /// <summary>
        /// Configures the ping policy for this instance of HTTP Web Socket.
        /// </summary>
        /// <param name="probeMessage">The payload/probe message that is sent to the client.</param>
        /// <param name="interval">The sending interval for each probe message.</param>
        public HttpWebSocket WithPing ( string probeMessage, TimeSpan interval ) {
            PingPolicy.DataMessage = probeMessage;
            PingPolicy.Interval = interval;
            PingPolicy.Start ();
            return this;
        }

        /// <summary>
        /// Asynchronously sends an message to the remote point.
        /// </summary>
        /// <param name="message">The target message which will be as an encoded UTF-8 string.</param>
        public Task<bool> SendAsync ( object message ) {
            return Task.FromResult ( Send ( message ) );
        }

        /// <summary>
        /// Asynchronously sends an text message to the remote point.
        /// </summary>
        /// <param name="message">The target message which will be as an encoded UTF-8 string.</param>
        public Task<bool> SendAsync ( string message ) {
            return Task.FromResult ( Send ( message ) );
        }

        /// <summary>
        /// Asynchronously sends an binary message to the remote point.
        /// </summary>
        /// <param name="buffer">The target message which will be as an encoded UTF-8 string.</param>
        public Task<bool> SendAsync ( byte [] buffer ) {
            return Task.FromResult ( Send ( buffer ) );
        }

        /// <summary>
        /// Sends an text message to the remote point.
        /// </summary>
        /// <param name="message">The target message which will be as an encoded UTF-8 string.</param>
        public bool Send ( object message ) {
            string? t = message.ToString ();
            if (t is null)
                throw new ArgumentNullException ( nameof ( message ) );

            return Send ( t );
        }

        /// <summary>
        /// Sends an text message to the remote point.
        /// </summary> 
        /// <param name="message">The target message which will be as an encoded using the request preferred encoding.</param>
        public bool Send ( string message ) {
            ArgumentNullException.ThrowIfNull ( message );

            byte [] messageBytes = request.RequestEncoding.GetBytes ( message );
            return SendInternal ( messageBytes, WebSocketMessageType.Text );
        }

        /// <summary>
        /// Sends an binary message to the remote point.
        /// </summary>
        /// <param name="buffer">The target byte array.</param>
        public bool Send ( byte [] buffer ) => Send ( buffer, 0, buffer.Length );

        /// <summary>
        /// Sends an binary message to the remote point.
        /// </summary>
        /// <param name="buffer">The target byte array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        public bool Send ( byte [] buffer, int start, int length ) {
            ReadOnlyMemory<byte> span = new ReadOnlyMemory<byte> ( buffer, start, length );
            return SendInternal ( span, WebSocketMessageType.Binary );
        }

        /// <summary>
        /// Sends an binary message to the remote point.
        /// </summary>
        /// <param name="buffer">The target byte memory.</param>
        public bool Send ( ReadOnlyMemory<byte> buffer ) {
            return SendInternal ( buffer, WebSocketMessageType.Binary );
        }

        /// <summary>
        /// Closes the connection between the client and the server and returns an HTTP response indicating that the connection has been terminated.
        /// This method will not throw an exception if the connection is already closed.
        /// </summary>
        public HttpResponse Close () {
            if (!_isClosed) {
                if (ctx.WebSocket.State != WebSocketState.Closed && ctx.WebSocket.State != WebSocketState.Aborted) {
                    // CloseAsync can throw an exception if any party closes the connection
                    // early before completing close handshake
                    // when this happens, the connection is already closed by some party and then release
                    // the resources of this websocket
                    try {
                        ctx.WebSocket.CloseOutputAsync ( WebSocketCloseStatus.NormalClosure, null, CancellationToken.None )
                            .Wait ();
                    }
                    catch (Exception) {
                        ;
                    }
                    finally {
                        wasServerClosed = true;
                    }
                }
                request.baseServer._wsCollection.UnregisterWebSocket ( this );
                isListening = false;
                _isClosed = true;
                closeEvent.Set ();
            }
            return new HttpResponse ( wasServerClosed ? HttpResponse.HTTPRESPONSE_SERVER_CLOSE : HttpResponse.HTTPRESPONSE_CLIENT_CLOSE ) {
                CalculedLength = length
            };
        }

        [MethodImpl ( MethodImplOptions.Synchronized )]
        private bool SendInternal ( ReadOnlyMemory<byte> buffer, WebSocketMessageType msgType ) {
            if (_isClosed) { return false; }

            if (closeTimeout.TotalMilliseconds > 0)
                asyncListenerToken?.CancelAfter ( closeTimeout );

            try {
                int totalLength = buffer.Length;
                int chunks = (int) Math.Ceiling ( (double) totalLength / BUFFER_LENGTH );

                for (int i = 0; i < chunks; i++) {
                    int ca = i * BUFFER_LENGTH;
                    int cb = Math.Min ( ca + BUFFER_LENGTH, buffer.Length );

                    ReadOnlyMemory<byte> chunk = buffer [ ca..cb ];

                    var sendVt = ctx.WebSocket.SendAsync ( chunk, msgType, i + 1 == chunks, asyncListenerToken?.Token ?? default );
                    if (!sendVt.IsCompleted) {
                        sendVt.AsTask ().GetAwaiter ().GetResult ();
                    }

                    length += chunk.Length;
                }

                attempt = 0;
            }
            catch (Exception) {
                attempt++;
                if (MaxAttempts >= 0 && attempt >= MaxAttempts) {
                    Close ();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Blocks the current call stack until the connection is terminated by the client or the server, limited to the maximum
        /// timeout.
        /// </summary>
        /// <param name="timeout">Defines the timeout timer before the connection expires without any message.</param>
        public void WaitForClose ( TimeSpan timeout ) {
            closeTimeout = timeout;
            closeEvent.WaitOne ();
        }

        /// <summary>
        /// Blocks the current call stack until the connection is terminated by either the client or the server.
        /// </summary>
        public void WaitForClose () {
            closeEvent.WaitOne ();
        }

        /// <summary>
        /// Blocks the current thread and waits the next incoming message from this web socket instance.
        /// </summary>
        /// <remarks>
        /// Null is returned if a connection error is thrown.
        /// </remarks>
        public WebSocketMessage? WaitNext () {
            return WaitNext ( WaitTimeout );
        }

        /// <summary>
        /// Blocks the current thread and waits the next incoming message from this web socket instance within
        /// the maximum defined timeout.
        /// </summary>
        /// <param name="timeout">The maximum time to wait until the next message.</param>
        /// <remarks>
        /// Null is returned if a connection error is thrown.
        /// </remarks>
        public WebSocketMessage? WaitNext ( TimeSpan timeout ) {
            waitNextEvent.Reset ();
            isWaitingNext = true;

            waitNextEvent.WaitOne ( timeout );

            return lastMessage;
        }

        /// <inheritdoc/>
        public void Dispose () {
            Close ();
            pingPolicy.Dispose ();
            closeEvent.Dispose ();
            waitNextEvent.Dispose ();
            receiveThread.Join ();
        }
    }

    /// <summary>
    /// Represents an websocket request message received by an websocket server.
    /// </summary>
    public sealed class WebSocketMessage {
        internal byte [] __msgBytes;

        /// <summary>
        /// Gets an boolean indicating that this message is the last chunk of the message.
        /// </summary>
        public bool IsEnd { get; internal set; }

        /// <summary>
        /// Gets an boolean indicating that this message is an remote closing message.
        /// </summary>
        public bool IsClose { get; internal set; }

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
        public HttpWebSocket Sender { get; internal set; }

        /// <summary>
        /// Reads the message bytes as string using the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding which will be used to decode the message.</param>
        public string GetString ( System.Text.Encoding encoding ) {
            return encoding.GetString ( MessageBytes );
        }

        /// <summary>
        /// Reads the message bytes as string using the HTTP request encoding.
        /// </summary>
        public string GetString () {
            return GetString ( Sender.HttpRequest.RequestEncoding );
        }

        internal WebSocketMessage ( HttpWebSocket httpws, int bufferLen ) {
            Sender = httpws;
            __msgBytes = new byte [ bufferLen ];
        }
    }
}
