using System.Net;

namespace Sisk.Core.Http
{
    /// <summary>
    /// An <see cref="HttpRequestEventSource"/> instance opens a persistent connection to the request, which sends events in text/event-stream format.
    /// </summary>
    /// <definition>
    /// public class HttpRequestEventSource : IDisposable
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class HttpRequestEventSource : IDisposable
    {
        private ManualResetEvent terminatingMutex = new ManualResetEvent(false);
        private HttpListenerResponse res;
        private HttpListenerRequest req;
        private HttpServer hostServer;
        private List<string> sendQueue = new List<string>();
        bool hasSentData = false;
        int length = 0;
        TimeSpan keepAlive = TimeSpan.Zero;
        DateTime lastSuccessfullMessage = DateTime.Now;

        // 
        // isClosed determines if this instance has some connection or not
        // isDisposed determines if this object was removed from their collection but wasnt collected by gc yet
        // 

        private bool isClosed = false;
        private bool isDisposed = false;

        /// <summary>
        /// Gets an integer indicating the total bytes sent by this instance to the client.
        /// </summary>
        /// <definition>
        /// public int SentContentLength { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public int SentContentLength { get => length; }

        /// <summary>
        /// Gets or sets an label to this EventStream connection, useful for finding this connection's reference later.
        /// </summary>
        /// <definition>
        /// public string? Identifier { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string? Identifier { get; private set; }

        /// <summary>
        /// Gets an boolean indicating if this connection is open and this instance can send messages.
        /// </summary>
        /// <definition>
        /// public bool IsDisposed { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool IsActive { get; private set; }

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
            HttpRequestEventSource? other = obj as HttpRequestEventSource;
            if (other == null) return false;
            return other.Identifier != null && other.Identifier == this.Identifier;
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
            return Identifier?.GetHashCode() ?? 0;
        }

        internal HttpRequestEventSource(string? identifier, HttpListenerResponse res, HttpListenerRequest req, HttpRequest host)
        {
            this.res = res ?? throw new ArgumentNullException(nameof(res));
            this.req = req ?? throw new ArgumentNullException(nameof(req));
            this.Identifier = identifier;
            this.hostServer = host.baseServer;

            hostServer._eventCollection.RegisterEventSource(this);

            IsActive = true;

            res.AddHeader("Cache-Control", "no-store, no-cache");
            res.AddHeader("Content-Type", "text/event-stream");
            res.AddHeader("X-Powered-By", HttpServer.poweredByHeader);
            HttpServer.SetCorsHeaders(host.hostContext.CrossOriginResourceSharingPolicy, res);
        }

        private void keepAliveTask()
        {
            while (IsActive)
            {
                if (lastSuccessfullMessage < DateTime.Now - keepAlive)
                {
                    Dispose();
                    break;
                }
                else
                {
                    Thread.Sleep(3000);
                }
            }
        }

        /// <summary>
        /// Sends an header to the streaming context.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="value">The header value.</param>
        /// <definition>
        /// public void AppendHeader(string name, string value)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void AppendHeader(string name, string value)
        {
            if (hasSentData)
            {
                throw new InvalidOperationException("It's not possible to set headers after a message has been sent in this instance.");
            }
            this.res.AddHeader(name, value);
        }

        /// <summary>
        /// Writes a event message with their data to the event listener and returns an boolean indicating if the message was delivered to the client.
        /// </summary>
        /// <param name="data">The message text.</param>
        /// <definition>
        /// public bool Send(string data)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool Send(string data)
        {
            if (!IsActive)
            {
                return false;
            }
            hasSentData = true;
            sendQueue.Add($"data: {data}\n\n");
            Flush();
            return true;
        }

        /// <summary>
        /// Writes a event message with their data to the event listener and returns an boolean indicating if the message was delivered to the client.
        /// </summary>
        /// <param name="data">The message object.</param>
        /// <definition>
        /// public bool Send(object? data)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool Send(object? data)
        {
            if (!IsActive)
            {
                return false;
            }
            hasSentData = true;
            sendQueue.Add($"data: {data?.ToString()}\n\n");
            Flush();
            return true;
        }

        /// <summary>
        /// Asynchronously waits for the connection to close before continuing execution. This method
        /// is released when either the client or the server reaches an sending failure.
        /// </summary>
        /// <definition>
        /// public void KeepAlive()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void KeepAlive()
        {
            if (!IsActive)
            {
                throw new InvalidOperationException("Cannot keep alive an instance that has it's connection disposed.");
            }
            terminatingMutex.WaitOne();
        }

        /// <summary>
        /// Asynchronously waits for the connection to close before continuing execution with
        /// an maximum keep alive timeout. This method is released when either the client or the server reaches an sending failure.
        /// </summary>
        /// <param name="maximumIdleTolerance">The maximum timeout interval for an idle connection to automatically release this method.</param>
        /// <definition>
        /// public void KeepAlive()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void KeepAlive(TimeSpan maximumIdleTolerance)
        {
            if (!IsActive)
            {
                throw new InvalidOperationException("Cannot keep alive an instance that has it's connection disposed.");
            }
            this.keepAlive = maximumIdleTolerance;
            new Task(keepAliveTask).Start();
            terminatingMutex.WaitOne();
        }

        /// <summary>
        /// Closes the event listener and it's connection.
        /// </summary>
        /// <definition>
        /// public HttpResponse Close()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse Close()
        {
            if (!isClosed)
            {
                isClosed = true;
                Flush();
                Dispose();
                hostServer._eventCollection.UnregisterEventSource(this);
            }
            return new HttpResponse(HttpResponse.HTTPRESPONSE_EVENTSOURCE_CLOSE)
            {
                CalculedLength = length
            };
        }

        /// <summary>
        /// Cancels the sending queue from sending pending messages and clears the queue.
        /// </summary>
        /// <definition>
        /// public void Cancel()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Cancel()
        {
            sendQueue.Clear();
        }

        private void Flush()
        {
            for (int i = 0; i < sendQueue.Count; i++)
            {
                if (isClosed)
                {
                    return;
                }
                string item = sendQueue[i];
                byte[] itemBytes = req.ContentEncoding.GetBytes(item);
                try
                {
                    res.OutputStream.Write(itemBytes);
                    length += itemBytes.Length;
                    sendQueue.RemoveAt(0);
                    lastSuccessfullMessage = DateTime.Now;
                }
                catch (Exception)
                {
                    Dispose();
                }
            }
        }

        /// <summary>
        /// Flushes and releases the used resources of this class instance.
        /// </summary>
        /// <definition>
        /// public void Dispose()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void Dispose()
        {
            if (isDisposed) return;
            Close();
            sendQueue.Clear();
            terminatingMutex.Set();
            IsActive = false;
            isDisposed = true;
        }
    }
}
