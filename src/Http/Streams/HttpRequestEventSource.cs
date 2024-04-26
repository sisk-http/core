// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestEventSource.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;

namespace Sisk.Core.Http.Streams
{
    /// <summary>
    /// An <see cref="HttpRequestEventSource"/> instance opens a persistent connection to the request, which sends events in text/event-stream format.
    /// </summary>
    public class HttpRequestEventSource : IDisposable
    {
        readonly ManualResetEvent terminatingMutex = new ManualResetEvent(false);
        readonly HttpStreamPingPolicy pingPolicy;
        readonly HttpListenerResponse res;
        readonly HttpListenerRequest req;
        readonly HttpRequest reqObj;
        readonly HttpServer hostServer;
        TimeSpan keepAlive = TimeSpan.Zero;
        DateTime lastSuccessfullMessage = DateTime.Now;
        int length = 0;

        internal List<string> sendQueue = new List<string>();
        internal bool hasSentData = false;

        // 
        // isClosed determines if this instance has some connection or not
        // isDisposed determines if this object was removed from their collection but wasnt collected by gc yet
        // 

        private bool isClosed = false;
        private bool isDisposed = false;

        /// <summary>
        /// Gets the <see cref="HttpStreamPingPolicy"/> for this HTTP event source connection.
        /// </summary>
        public HttpStreamPingPolicy PingPolicy { get => pingPolicy; }

        /// <summary>
        /// Gets the <see cref="Http.HttpRequest"/> object which created this Event Source instance.
        /// </summary>
        public HttpRequest HttpRequest => reqObj;

        /// <summary>
        /// Gets an integer indicating the total bytes sent by this instance to the client.
        /// </summary>
        public int SentContentLength { get => length; }

        /// <summary>
        /// Gets an unique identifier label to this EventStream connection, useful for finding this connection's reference later.
        /// </summary>
        public string? Identifier { get; private set; }

        /// <summary>
        /// Gets an boolean indicating if this connection is open and this instance can send messages.
        /// </summary>
        public bool IsActive { get; private set; }

        internal HttpRequestEventSource(string? identifier, HttpListenerResponse res, HttpListenerRequest req, HttpRequest host)
        {
            this.res = res ?? throw new ArgumentNullException(nameof(res));
            this.req = req ?? throw new ArgumentNullException(nameof(req));
            Identifier = identifier;
            hostServer = host.baseServer;
            reqObj = host;
            pingPolicy = new HttpStreamPingPolicy(this);

            hostServer._eventCollection.RegisterEventSource(this);

            IsActive = true;

            res.AddHeader("Cache-Control", "no-store, no-cache");
            res.AddHeader("Content-Type", "text/event-stream");
            res.AddHeader("X-Powered-By", HttpServer.PoweredBy);

            HttpServer.SetCorsHeaders(host.baseServer.ServerConfiguration.Flags, req, host.hostContext.CrossOriginResourceSharingPolicy, res);
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
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// Configures the ping policy for this instance of HTTP Event Source.
        /// </summary>
        /// <param name="act">The method that runs on the ping policy for this HTTP Event Source.</param>
        public void WithPing(Action<HttpStreamPingPolicy> act)
        {
            act(pingPolicy);
        }

        /// <summary>
        /// Sends an header to the streaming context.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="value">The header value.</param>
        public void AppendHeader(string name, string value)
        {
            if (hasSentData)
            {
                throw new InvalidOperationException(SR.Httpserver_Commons_HeaderAfterContents);
            }
            res.AddHeader(name, value);
        }

        /// <summary>
        /// Writes a event message with their data to the event listener and returns an boolean indicating if the message was delivered to the client.
        /// </summary>
        /// <param name="data">The message text.</param>
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
        public void KeepAlive()
        {
            if (!IsActive)
            {
                throw new InvalidOperationException(SR.HttpRequestEventSource_KeepAliveDisposed);
            }
            terminatingMutex.WaitOne();
        }

        /// <summary>
        /// Asynchronously waits for the connection to close before continuing execution with
        /// an maximum keep alive timeout. This method is released when either the client or the server reaches an sending failure.
        /// </summary>
        /// <param name="maximumIdleTolerance">The maximum timeout interval for an idle connection to automatically release this method.</param>
        public void WaitForFail(TimeSpan maximumIdleTolerance)
        {
            if (!IsActive)
            {
                throw new InvalidOperationException(SR.HttpRequestEventSource_KeepAliveDisposed);
            }
            keepAlive = maximumIdleTolerance;
            new Task(keepAliveTask).Start();
            terminatingMutex.WaitOne();
        }

        /// <summary>
        /// Closes the event listener and it's connection.
        /// </summary>
        public HttpResponse Close()
        {
            if (!isClosed)
            {
                isClosed = true;
                Flush();
                Dispose();
                hostServer._eventCollection.UnregisterEventSource(this);
            }
            return new HttpResponse(HttpResponse.HTTPRESPONSE_SERVER_CLOSE)
            {
                CalculedLength = length
            };
        }

        /// <summary>
        /// Cancels the sending queue from sending pending messages and clears the queue.
        /// </summary>
        public void Cancel()
        {
            sendQueue.Clear();
        }

        internal void Flush()
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
