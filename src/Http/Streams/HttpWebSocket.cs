// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpWebSocket.cs
// Repository:  https://github.com/sisk-http/core

using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sisk.Core.Http.Streams
{
    /// <summary>
    /// Provides an persistent bi-directional socket between the client and the HTTP server.
    /// </summary>
    /// <definition>
    /// public sealed class HttpWebSocket
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public sealed class HttpWebSocket
    {
        bool isListening = true;
        HttpStreamPingPolicy pingPolicy;

        internal WebSocketMessage? lastMessage = null;
        internal CancellationTokenSource asyncListenerToken = null!;
        internal ManualResetEvent closeEvent = new ManualResetEvent(false);
        internal ManualResetEvent waitNextEvent = new ManualResetEvent(false);
        internal Thread receiveThread;
        internal HttpListenerWebSocketContext ctx;
        internal HttpRequest request;
        internal TimeSpan closeTimeout = TimeSpan.Zero;
        internal bool isClosed = false;
        internal bool isWaitingNext = false;
        internal bool wasServerClosed = false;
        internal string? identifier = null;

        int attempt = 0;
        int bufferLength = 0;

        /// <summary>
        /// Gets the <see cref="HttpStreamPingPolicy"/> for this HTTP web socket connection.
        /// </summary>
        /// <definition>
        /// public HttpStreamPingPolicy PingPolicy { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpStreamPingPolicy PingPolicy => pingPolicy;

        /// <summary>
        /// Gets or sets the maximum number of attempts to send a failed message before the server closes the connection. Set it to -1 to
        /// don't close the connection on failed attempts.
        /// </summary>
        /// <definition>
        /// public int MaxAttempts { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public int MaxAttempts { get; set; } = 3;

        /// <summary>
        /// Gets or sets an object linked with this <see cref="WebSocket"/> session.
        /// </summary>
        /// <definition>
        /// public object? State { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public object? State { get; set; }

        /// <summary>
        /// Gets the <see cref="Sisk.Core.Http.HttpRequest"/> object which created this Web Socket instance.
        /// </summary>
        /// <definition>
        /// public HttpRequest HttpRequest { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpRequest HttpRequest => request;

        /// <summary>
        /// Gets an boolean indicating if this Web Socket connection is closed.
        /// </summary>
        /// <definition>
        /// public bool IsClosed { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool IsClosed => isClosed;

        /// <summary>
        /// Gets an unique identifier label to this Web Socket connection, useful for finding this connection's reference later.
        /// </summary>
        /// <definition>
        /// public string? Identifier { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string? Identifier => identifier;

        /// <summary>
        /// Represents the event which is called when this web socket receives an message from
        /// remote origin.
        /// </summary>
        /// <definition>
        /// public event WebSocketMessageReceivedEventHandler? OnReceive;
        /// </definition>
        /// <type>
        /// Event
        /// </type>
        public event WebSocketMessageReceivedEventHandler? OnReceive = null;

        internal HttpWebSocket(HttpListenerWebSocketContext ctx, HttpRequest req, string? identifier)
        {
            this.ctx = ctx;
            this.request = req;
            this.bufferLength = request.baseServer.ServerConfiguration.Flags.WebSocketBufferSize;
            this.identifier = identifier;
            this.pingPolicy = new HttpStreamPingPolicy(this);

            if (identifier != null)
            {
                req.baseServer._wsCollection.RegisterWebSocket(this);
            }

            receiveThread = new Thread(new ThreadStart(ReceiveTask));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        void RecreateAsyncToken()
        {
            asyncListenerToken = new CancellationTokenSource();
            if (closeTimeout.TotalMilliseconds > 0)
                asyncListenerToken.CancelAfter(closeTimeout);
            asyncListenerToken.Token.ThrowIfCancellationRequested();
        }

        void TrimMessage(WebSocketReceiveResult result, WebSocketMessage message)
        {
            if (result.Count < message.Length)
            {
                byte[] trimmed = new byte[result.Count];
                for (int i = 0; i < trimmed.Length; i++)
                {
                    trimmed[i] = message.MessageBytes[i];
                }
                message.__msgBytes = trimmed;
            }
            message.IsClose = result.MessageType == WebSocketMessageType.Close;
            message.IsEnd = result.EndOfMessage;

            if (result.MessageType == WebSocketMessageType.Close)
            {
                isClosed = true;
                isListening = false;
                closeEvent.Set();
            }
        }

        internal async void ReceiveTask()
        {
            while (isListening)
            {
                RecreateAsyncToken();
                WebSocketMessage message = new WebSocketMessage(this, bufferLength);

                var arrSegment = new ArraySegment<byte>(message.__msgBytes);
                WebSocketReceiveResult result;

                try
                {
                    result = await ctx.WebSocket.ReceiveAsync(arrSegment, asyncListenerToken.Token);
                }
                catch (Exception)
                {
                    if (ctx.WebSocket.State != WebSocketState.Open
                     || ctx.WebSocket.State != WebSocketState.Connecting)
                    {
                        Close();
                        break;
                    }
                    continue;
                }

                if (result.Count == 0 || result.CloseStatus != null)
                {
                    Close();
                    break;
                }

                TrimMessage(result, message);
                if (isWaitingNext)
                {
                    isWaitingNext = false;
                    lastMessage = message;
                    waitNextEvent.Set();
                }
                else
                {
                    if (OnReceive != null) OnReceive(this, message);
                }
            }
        }

        /// <summary>
        /// Configures the ping policy for this instance of HTTP Web Socket.
        /// </summary>
        /// <param name="act">The method that runs on the ping policy for this HTTP Web Socket.</param>
        /// <definition>
        /// public void WithPing(Action{{HttpStreamPingPolicy}} act)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WithPing(Action<HttpStreamPingPolicy> act)
        {
            act(pingPolicy);
        }

        /// <summary>
        /// Sends an text message to the remote point.
        /// </summary>
        /// <param name="message">The target message which will be as an encoded UTF-8 string.</param>
        /// <definition>
        /// public void Send(string message)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Send(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            ReadOnlyMemory<byte> span = new ReadOnlyMemory<byte>(messageBytes);
            SendInternal(span, WebSocketMessageType.Text);
        }

        /// <summary>
        /// Sends an binary message to the remote point.
        /// </summary>
        /// <param name="buffer">The target byte array.</param>
        /// <definition>
        /// public void Send(byte[] buffer)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Send(byte[] buffer)
        {
            ReadOnlyMemory<byte> span = new ReadOnlyMemory<byte>(buffer);
            SendInternal(span, WebSocketMessageType.Binary);
        }

        /// <summary>
        /// Sends an binary message to the remote point.
        /// </summary>
        /// <param name="buffer">The target byte array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        /// <definition>
        /// public void Send(byte[] buffer, int start, int length)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Send(byte[] buffer, int start, int length)
        {
            ReadOnlyMemory<byte> span = new ReadOnlyMemory<byte>(buffer, start, length);
            SendInternal(span, WebSocketMessageType.Binary);
        }

        /// <summary>
        /// Sends an binary message to the remote point.
        /// </summary>
        /// <param name="buffer">The target byte memory.</param>
        /// <definition>
        /// public void Send(ReadOnlyMemory&lt;byte&gt; buffer)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Send(ReadOnlyMemory<byte> buffer)
        {
            SendInternal(buffer, WebSocketMessageType.Binary);
        }

        /// <summary>
        /// Closes the connection between the client and the server and returns an Http resposne indicating that the connection has been terminated.
        /// This method will not throw an exception if the connection is already closed.
        /// </summary>
        /// <definition>
        /// public HttpResponse Close()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpResponse Close()
        {
            if (!isClosed)
            {
                if (ctx.WebSocket.State != WebSocketState.Closed && ctx.WebSocket.State != WebSocketState.Aborted)
                {
                    // CloseAsync can throw an exception if any party closes the connection
                    // early before completing close handshake
                    // when this happens, the connection is already closed by some party and then release
                    // the resources of this websocket
                    try
                    {
                        ctx.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).Wait();
                    }
                    catch (Exception) {; }
                    finally
                    {
                        wasServerClosed = true;
                    }
                }
                request.baseServer._wsCollection.UnregisterWebSocket(this);
                isListening = false;
                isClosed = true;
                closeEvent.Set();
            }
            return new HttpResponse(wasServerClosed ? HttpResponse.HTTPRESPONSE_SERVER_CLOSE : HttpResponse.HTTPRESPONSE_CLIENT_CLOSE);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SendInternal(ReadOnlyMemory<byte> buffer, WebSocketMessageType msgType)
        {
            if (isClosed) { return; }

            if (closeTimeout.TotalMilliseconds > 0)
                asyncListenerToken?.CancelAfter(closeTimeout);

            try
            {
                int totalLength = buffer.Length;
                int chunks = Math.Max(totalLength / bufferLength, 1);

                for (int i = 0; i < chunks; i++)
                {
                    int ca = i * bufferLength;
                    int cb = Math.Min(ca + bufferLength, buffer.Length);

                    ReadOnlyMemory<byte> chunk = buffer[ca..cb];

                    ctx.WebSocket.SendAsync(chunk, msgType, i + 1 == chunks, CancellationToken.None)
                        .AsTask().Wait();
                }

                attempt = 0;
            } 
            catch (Exception)
            {
                attempt++;
                if (MaxAttempts >= 0 && attempt >= MaxAttempts)
                {
                    Close();
                    return;
                }
            }
        }

        /// <summary>
        /// Blocks the current call stack until the connection is terminated by the client or the server, limited to the maximum
        /// timeout.
        /// </summary>
        /// <param name="timeout">Defines the timeout timer before the connection expires without any message.</param>
        /// <definition>
        /// public void WaitForClose(TimeSpan timeout)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WaitForClose(TimeSpan timeout)
        {
            this.closeTimeout = timeout;
            closeEvent.WaitOne();
        }

        /// <summary>
        /// Blocks the current call stack until the connection is terminated by either the client or the server.
        /// </summary>
        /// <definition>
        /// public void WaitForClose()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WaitForClose()
        {
            closeEvent.WaitOne();
        }

        /// <summary>
        /// Blocks the current thread and waits the next incoming message from this web socket instance.
        /// </summary>
        /// <remarks>
        /// Null is returned if a connection error is thrown.
        /// </remarks>
        /// <definition>
        /// public WebSocketMessage? WaitNext()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public WebSocketMessage? WaitNext()
        {
            waitNextEvent.Reset();
            isWaitingNext = true;
            waitNextEvent.WaitOne();
            return lastMessage;
        }
    }

    /// <summary>
    /// Represents the void that is called when the Web Socket receives an message.
    /// </summary>
    /// <param name="sender">The <see cref="HttpWebSocket"/> object which fired the event.</param>
    /// <param name="message">The Web Socket message information.</param>
    /// <definition>
    /// public delegate void WebSocketMessageReceivedEventHandler(object? sender, WebSocketMessage message);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate void WebSocketMessageReceivedEventHandler(object? sender, WebSocketMessage message);

    /// <summary>
    /// Represents an websocket request message received by an websocket server.
    /// </summary>
    /// <definition>
    /// public sealed class WebSocketMessage
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public sealed class WebSocketMessage
    {
        internal byte[] __msgBytes;

        /// <summary>
        /// Gets an boolean indicating that this message is the last chunk of the message.
        /// </summary>
        /// <definition>
        /// public bool IsEnd { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool IsEnd { get; internal set; }

        /// <summary>
        /// Gets an boolean indicating that this message is an remote closing message.
        /// </summary>
        /// <definition>
        /// public bool IsClose { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool IsClose { get; internal set; }

        /// <summary>
        /// Gets an byte array with the message contents.
        /// </summary>
        /// <definition>
        /// public byte[] MessageBytes { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public byte[] MessageBytes => __msgBytes;

        /// <summary>
        /// Gets the message length in byte count.
        /// </summary>
        /// <definition>
        /// public int Length { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public int Length => __msgBytes.Length;

        /// <summary>
        /// Gets the sender <see cref="HttpWebSocket"/> object instance which received this message.
        /// </summary>
        /// <definition>
        /// public HttpWebSocket Sender { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpWebSocket Sender { get; internal set; }

        /// <summary>
        /// Reads the message bytes as string using the specified encoding.
        /// </summary>
        /// <param name="encoder">The encoding which will be used to decode the message.</param>
        /// <definition>
        /// public string GetString(System.Text.Encoding encoder)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public string GetString(System.Text.Encoding encoder)
        {
            return encoder.GetString(MessageBytes);
        }

        /// <summary>
        /// Reads the message bytes as string using the UTF-8 text encoding.
        /// </summary>
        /// <definition>
        /// public string GetString()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public string GetString()
        {
            return GetString(Encoding.UTF8);
        }

        internal WebSocketMessage(HttpWebSocket httpws, int bufferLen)
        {
            Sender = httpws;
            __msgBytes = new byte[bufferLen];
        }
    }
}
