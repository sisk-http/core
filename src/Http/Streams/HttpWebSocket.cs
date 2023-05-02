using System.Net.WebSockets;
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
        internal string? identifier = null;
        internal HttpListenerWebSocketContext ctx;
        internal HttpRequest request;
        bool isListening = true;
        internal bool isClosed = false;

        internal ManualResetEvent closeEvent = new ManualResetEvent(false);
        internal Thread receiveThread;

        int bufferLength = 0;

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
        /// Determines if another object is equals to this class instance.
        /// </summary>
        /// <param name="obj">The another object which will be used to compare.</param>
        /// <returns></returns>
        /// <definition>
        /// public override bool Equals(object? obj)
        /// </definition>
        /// <type>
        /// Method
        /// </type> 
        public override bool Equals(object? obj)
        {
            HttpWebSocket? other = obj as HttpWebSocket;
            if (other == null) return false;
            if (other.identifier == null) return false;
            return other.identifier == identifier;
        }

        /// <summary>
        /// Gets the hash code for this event source.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public override int GetHashCode()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public override int GetHashCode()
        {
            return identifier?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Represents the event which is called when this websocket receives an message from
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
            request = req;
            bufferLength = request.baseServer.ServerConfiguration.Flags.WebSocketBufferSize;
            this.identifier = identifier;

            if (identifier != null)
            {
                req.baseServer._wsCollection.RegisterWebSocket(this);
            }

            receiveThread = new Thread(new ThreadStart(ReceiveTask));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        internal async void ReceiveTask()
        {
            while (isListening)
            {
                WebSocketMessage message = new WebSocketMessage(this, bufferLength);

                var arrSegment = new ArraySegment<byte>(message.__msgBytes);
                var result = await ctx.WebSocket.ReceiveAsync(arrSegment, CancellationToken.None);

                if (OnReceive != null)
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

                    OnReceive(this, message);
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    isClosed = true;
                    isListening = false;
                    closeEvent.Set();
                }
            }
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
        public unsafe void Send(string message)
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
        public unsafe void Send(byte[] buffer)
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
        public unsafe void Send(byte[] buffer, int start, int length)
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
        public unsafe void Send(ReadOnlyMemory<byte> buffer)
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
                ctx.WebSocket.SendAsync(new byte[] { 0 }, WebSocketMessageType.Close, true, CancellationToken.None);
                isListening = false;
                isClosed = true;
                closeEvent.Set();
            }
            request.baseServer._wsCollection.UnregisterWebSocket(this);
            return new HttpResponse(HttpResponse.HTTPRESPONSE_STREAM_CLOSE);
        }

        private void SendInternal(ReadOnlyMemory<byte> buffer, WebSocketMessageType msgType)
        {
            if (isClosed) { return; }

            int totalLength = buffer.Length;
            int chunks = Math.Max(totalLength / bufferLength, 1);

            for (int i = 0; i < chunks; i++)
            {
                int ca = i * bufferLength;
                int cb = Math.Min(ca + bufferLength, buffer.Length);

                ReadOnlyMemory<byte> chunk = buffer[ca..cb];

                ctx.WebSocket.SendAsync(chunk, msgType, i + 1 == chunks, CancellationToken.None);
            }
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
