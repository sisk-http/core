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
    public sealed class HttpWebSocket {
        bool isListening = true;
        readonly HttpStreamPingPolicy pingPolicy;

        internal WebSocketMessage? lastMessage = null;
        internal CancellationTokenSource asyncListenerToken = null!;
        internal ManualResetEvent closeEvent = new ManualResetEvent ( false );
        internal ManualResetEvent waitNextEvent = new ManualResetEvent ( false );
        internal Thread receiveThread;
        internal HttpListenerWebSocketContext ctx;
        internal HttpRequest request;
        internal TimeSpan closeTimeout = TimeSpan.Zero;
        internal bool isClosed = false;
        internal bool isWaitingNext = false;
        internal bool wasServerClosed = false;
        internal string? identifier = null;

        int attempt = 0;
        readonly int bufferLength = 0;
        long length = 0;

        /// <summary>
        /// Gets the <see cref="HttpStreamPingPolicy"/> for this HTTP web socket connection.
        /// </summary>
        public HttpStreamPingPolicy PingPolicy => this.pingPolicy;

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
        public HttpRequest HttpRequest => this.request;

        /// <summary>
        /// Gets an boolean indicating if this Web Socket connection is closed.
        /// </summary>
        public bool IsClosed => this.isClosed;

        /// <summary>
        /// Gets an unique identifier label to this Web Socket connection, useful for finding this connection's reference later.
        /// </summary>
        public string? Identifier => this.identifier;

        /// <summary>
        /// Represents the event which is called when this web socket receives an message from
        /// remote origin.
        /// </summary>
        public event WebSocketMessageReceivedEventHandler? OnReceive;

        internal HttpWebSocket ( HttpListenerWebSocketContext ctx, HttpRequest req, string? identifier ) {
            this.ctx = ctx;
            this.request = req;
            this.bufferLength = this.request.baseServer.ServerConfiguration.Flags.WebSocketBufferSize;
            this.identifier = identifier;
            this.pingPolicy = new HttpStreamPingPolicy ( this );

            if (identifier != null) {
                req.baseServer._wsCollection.RegisterWebSocket ( this );
            }

            this.receiveThread = new Thread ( new ThreadStart ( this.ReceiveTask ) );
            this.receiveThread.IsBackground = true;
            this.receiveThread.Start ();
        }

        void RecreateAsyncToken () {
            this.asyncListenerToken = new CancellationTokenSource ();
            if (this.closeTimeout.TotalMilliseconds > 0)
                this.asyncListenerToken.CancelAfter ( this.closeTimeout );
            this.asyncListenerToken.Token.ThrowIfCancellationRequested ();
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
                this.isClosed = true;
                this.isListening = false;
                this.closeEvent.Set ();
            }
        }

        internal async void ReceiveTask () {
            while (this.isListening) {
                this.RecreateAsyncToken ();
                WebSocketMessage message = new WebSocketMessage ( this, this.bufferLength );

                var arrSegment = new ArraySegment<byte> ( message.__msgBytes );
                WebSocketReceiveResult result;

                try {
                    result = await this.ctx.WebSocket.ReceiveAsync ( arrSegment, this.asyncListenerToken.Token );
                }
                catch (Exception) {
                    if (this.ctx.WebSocket.State != WebSocketState.Open
                     && this.ctx.WebSocket.State != WebSocketState.Connecting) {
                        this.Close ();
                        break;
                    }
                    continue;
                }

                if (result.Count == 0 || result.CloseStatus != null) {
                    this.Close ();
                    break;
                }

                this.TrimMessage ( result, message );
                bool isPingMessage = message.GetString () == this.pingPolicy.DataMessage;

                if (this.isWaitingNext & !isPingMessage) {
                    this.isWaitingNext = false;
                    this.lastMessage = message;
                    this.waitNextEvent.Set ();
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
            act ( this.pingPolicy );
            return this;
        }

        /// <summary>
        /// Configures the ping policy for this instance of HTTP Web Socket.
        /// </summary>
        /// <param name="probeMessage">The payload/probe message that is sent to the client.</param>
        /// <param name="interval">The sending interval for each probe message.</param>
        public HttpWebSocket WithPing ( string probeMessage, TimeSpan interval ) {
            this.PingPolicy.DataMessage = probeMessage;
            this.PingPolicy.Interval = interval;
            this.PingPolicy.Start ();
            return this;
        }

        /// <summary>
        /// Asynchronously sends an message to the remote point.
        /// </summary>
        /// <param name="message">The target message which will be as an encoded UTF-8 string.</param>
        public Task<bool> SendAsync ( object message ) {
            return Task.FromResult ( this.Send ( message ) );
        }

        /// <summary>
        /// Asynchronously sends an text message to the remote point.
        /// </summary>
        /// <param name="message">The target message which will be as an encoded UTF-8 string.</param>
        public Task<bool> SendAsync ( string message ) {
            return Task.FromResult ( this.Send ( message ) );
        }

        /// <summary>
        /// Asynchronously sends an binary message to the remote point.
        /// </summary>
        /// <param name="buffer">The target message which will be as an encoded UTF-8 string.</param>
        public Task<bool> SendAsync ( byte [] buffer ) {
            return Task.FromResult ( this.Send ( buffer ) );
        }

        /// <summary>
        /// Sends an text message to the remote point.
        /// </summary>
        /// <param name="message">The target message which will be as an encoded UTF-8 string.</param>
        public bool Send ( object message ) {
            string? t = message.ToString ();
            if (t is null)
                throw new ArgumentNullException ( nameof ( message ) );

            return this.Send ( t );
        }

        /// <summary>
        /// Sends an text message to the remote point.
        /// </summary> 
        /// <param name="message">The target message which will be as an encoded using the request preferred encoding.</param>
        public bool Send ( string message ) {
            ArgumentNullException.ThrowIfNull ( message );

            byte [] messageBytes = this.request.RequestEncoding.GetBytes ( message );
            return this.SendInternal ( messageBytes, WebSocketMessageType.Text );
        }

        /// <summary>
        /// Sends an binary message to the remote point.
        /// </summary>
        /// <param name="buffer">The target byte array.</param>
        public bool Send ( byte [] buffer ) => this.Send ( buffer, 0, buffer.Length );

        /// <summary>
        /// Sends an binary message to the remote point.
        /// </summary>
        /// <param name="buffer">The target byte array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        public bool Send ( byte [] buffer, int start, int length ) {
            ReadOnlyMemory<byte> span = new ReadOnlyMemory<byte> ( buffer, start, length );
            return this.SendInternal ( span, WebSocketMessageType.Binary );
        }

        /// <summary>
        /// Sends an binary message to the remote point.
        /// </summary>
        /// <param name="buffer">The target byte memory.</param>
        public bool Send ( ReadOnlyMemory<byte> buffer ) {
            return this.SendInternal ( buffer, WebSocketMessageType.Binary );
        }

        /// <summary>
        /// Closes the connection between the client and the server and returns an HTTP response indicating that the connection has been terminated.
        /// This method will not throw an exception if the connection is already closed.
        /// </summary>
        public HttpResponse Close () {
            if (!this.isClosed) {
                if (this.ctx.WebSocket.State != WebSocketState.Closed && this.ctx.WebSocket.State != WebSocketState.Aborted) {
                    // CloseAsync can throw an exception if any party closes the connection
                    // early before completing close handshake
                    // when this happens, the connection is already closed by some party and then release
                    // the resources of this websocket
                    try {
                        this.ctx.WebSocket.CloseOutputAsync ( WebSocketCloseStatus.NormalClosure, null, CancellationToken.None )
                            .Wait ();
                    }
                    catch (Exception) {
                        ;
                    }
                    finally {
                        this.wasServerClosed = true;
                    }
                }
                this.request.baseServer._wsCollection.UnregisterWebSocket ( this );
                this.isListening = false;
                this.isClosed = true;
                this.closeEvent.Set ();
            }
            return new HttpResponse ( this.wasServerClosed ? HttpResponse.HTTPRESPONSE_SERVER_CLOSE : HttpResponse.HTTPRESPONSE_CLIENT_CLOSE ) {
                CalculedLength = this.length
            };
        }

        [MethodImpl ( MethodImplOptions.Synchronized )]
        private bool SendInternal ( ReadOnlyMemory<byte> buffer, WebSocketMessageType msgType ) {
            if (this.isClosed) { return false; }

            if (this.closeTimeout.TotalMilliseconds > 0)
                this.asyncListenerToken?.CancelAfter ( this.closeTimeout );

            try {
                int totalLength = buffer.Length;
                int chunks = (int) Math.Ceiling ( (double) totalLength / this.bufferLength );

                for (int i = 0; i < chunks; i++) {
                    int ca = i * this.bufferLength;
                    int cb = Math.Min ( ca + this.bufferLength, buffer.Length );

                    ReadOnlyMemory<byte> chunk = buffer [ ca..cb ];

                    this.ctx.WebSocket.SendAsync ( chunk, msgType, i + 1 == chunks, CancellationToken.None )
                        .AsTask ().Wait ();

                    this.length += chunk.Length;
                }

                this.attempt = 0;
            }
            catch (Exception) {
                this.attempt++;
                if (this.MaxAttempts >= 0 && this.attempt >= this.MaxAttempts) {
                    this.Close ();
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
            this.closeTimeout = timeout;
            this.closeEvent.WaitOne ();
        }

        /// <summary>
        /// Blocks the current call stack until the connection is terminated by either the client or the server.
        /// </summary>
        public void WaitForClose () {
            this.closeEvent.WaitOne ();
        }

        /// <summary>
        /// Blocks the current thread and waits the next incoming message from this web socket instance.
        /// </summary>
        /// <remarks>
        /// Null is returned if a connection error is thrown.
        /// </remarks>
        public WebSocketMessage? WaitNext () {
            return this.WaitNext ( this.WaitTimeout );
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
            this.waitNextEvent.Reset ();
            this.isWaitingNext = true;

            this.waitNextEvent.WaitOne ( timeout );

            return this.lastMessage;
        }
    }

    /// <summary>
    /// Represents the void that is called when the Web Socket receives an message.
    /// </summary>
    /// <param name="sender">The <see cref="HttpWebSocket"/> object which fired the event.</param>
    /// <param name="message">The Web Socket message information.</param>
    public delegate void WebSocketMessageReceivedEventHandler ( object? sender, WebSocketMessage message );

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
        public byte [] MessageBytes => this.__msgBytes;

        /// <summary>
        /// Gets the message length in byte count.
        /// </summary>
        public int Length => this.__msgBytes.Length;

        /// <summary>
        /// Gets the sender <see cref="HttpWebSocket"/> object instance which received this message.
        /// </summary>
        public HttpWebSocket Sender { get; internal set; }

        /// <summary>
        /// Reads the message bytes as string using the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding which will be used to decode the message.</param>
        public string GetString ( System.Text.Encoding encoding ) {
            return encoding.GetString ( this.MessageBytes );
        }

        /// <summary>
        /// Reads the message bytes as string using the HTTP request encoding.
        /// </summary>
        public string GetString () {
            return this.GetString ( this.Sender.HttpRequest.RequestEncoding );
        }

        internal WebSocketMessage ( HttpWebSocket httpws, int bufferLen ) {
            this.Sender = httpws;
            this.__msgBytes = new byte [ bufferLen ];
        }
    }
}
