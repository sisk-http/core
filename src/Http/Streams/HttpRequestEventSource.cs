// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestEventSource.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;

namespace Sisk.Core.Http.Streams {
    /// <summary>
    /// An <see cref="HttpRequestEventSource"/> instance opens a persistent connection to the request, which sends events in text/event-stream format.
    /// </summary>
    public sealed class HttpRequestEventSource : IDisposable {
        readonly ManualResetEvent terminatingMutex = new ManualResetEvent ( false );
        readonly HttpStreamPingPolicy pingPolicy;
        readonly HttpListenerResponse res;
        readonly HttpListenerRequest req;
        readonly HttpRequest reqObj;
        readonly HttpServer hostServer;
        TimeSpan keepAlive = TimeSpan.Zero;
        DateTime lastSuccessfullMessage = DateTime.Now;
        int length = 0;

        internal List<string> sendQueue = new List<string> ();
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
        public HttpStreamPingPolicy PingPolicy { get => this.pingPolicy; }

        /// <summary>
        /// Gets the <see cref="Http.HttpRequest"/> object which created this Event Source instance.
        /// </summary>
        public HttpRequest HttpRequest => this.reqObj;

        /// <summary>
        /// Gets an integer indicating the total bytes sent by this instance to the client.
        /// </summary>
        public int SentContentLength { get => this.length; }

        /// <summary>
        /// Gets an unique identifier label to this EventStream connection, useful for finding this connection's reference later.
        /// </summary>
        public string? Identifier { get; private set; }

        /// <summary>
        /// Gets an boolean indicating if this connection is open and this instance can send messages.
        /// </summary>
        public bool IsActive { get; private set; }

        internal HttpRequestEventSource ( string? identifier, HttpListenerResponse res, HttpListenerRequest req, HttpRequest host ) {
            this.res = res ?? throw new ArgumentNullException ( nameof ( res ) );
            this.req = req ?? throw new ArgumentNullException ( nameof ( req ) );
            this.Identifier = identifier;
            this.hostServer = host.baseServer;
            this.reqObj = host;
            this.pingPolicy = new HttpStreamPingPolicy ( this );

            this.hostServer._eventCollection.RegisterEventSource ( this );

            this.IsActive = true;

            res.AddHeader ( HttpKnownHeaderNames.CacheControl, "no-store, no-cache" );
            res.AddHeader ( HttpKnownHeaderNames.ContentType, "text/event-stream" );
            if (host.baseServer.ServerConfiguration.SendSiskHeader)
                res.AddHeader ( HttpKnownHeaderNames.XPoweredBy, HttpServer.PoweredBy );

            if (host.Context.MatchedRoute?.UseCors == true)
                HttpServer.SetCorsHeaders ( req, host.Context.ListeningHost?.CrossOriginResourceSharingPolicy, res );
        }

        private void KeepAliveTask () {
            while (this.IsActive) {
                if (this.lastSuccessfullMessage < DateTime.Now - this.keepAlive) {
                    this.Dispose ();
                    break;
                }
                else {
                    Thread.Sleep ( 1000 );
                }
            }
        }

        /// <summary>
        /// Configures the ping policy for this instance of HTTP Event Source.
        /// </summary>
        /// <param name="act">The method that runs on the ping policy for this HTTP Event Source.</param>
        public void WithPing ( Action<HttpStreamPingPolicy> act ) {
            act ( this.pingPolicy );
        }

        /// <summary>
        /// Sends an header to the streaming context.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="value">The header value.</param>
        public void AppendHeader ( string name, string value ) {
            if (this.hasSentData) {
                throw new InvalidOperationException ( SR.Httpserver_Commons_HeaderAfterContents );
            }
            this.res.AddHeader ( name, value );
        }

        /// <summary>
        /// Writes a event message with their data to the event listener and returns an boolean indicating if the message was delivered to the client.
        /// </summary>
        /// <param name="data">The message text.</param>
        public bool Send ( string data ) {
            if (!this.IsActive) {
                return false;
            }
            this.hasSentData = true;
            this.sendQueue.Add ( $"data: {data}\n\n" );
            this.Flush ();
            return true;
        }

        /// <summary>
        /// Writes a event message with their data to the event listener and returns an boolean indicating if the message was delivered to the client.
        /// </summary>
        /// <param name="data">The message object.</param>
        public bool Send ( object? data ) {
            if (!this.IsActive) {
                return false;
            }
            this.hasSentData = true;
            this.sendQueue.Add ( $"data: {data?.ToString ()}\n\n" );
            this.Flush ();
            return true;
        }

        /// <summary>
        /// Asynchronously waits for the connection to close before continuing execution. This method
        /// is released when either the client or the server reaches an sending failure.
        /// </summary>
        public void KeepAlive () {
            if (!this.IsActive) {
                throw new InvalidOperationException ( SR.HttpRequestEventSource_KeepAliveDisposed );
            }
            this.terminatingMutex.WaitOne ();
        }

        /// <summary>
        /// Asynchronously waits for the connection to close before continuing execution with
        /// an maximum keep alive timeout. This method is released when either the client or the server reaches an sending failure.
        /// </summary>
        /// <param name="maximumIdleTolerance">The maximum timeout interval for an idle connection to automatically release this method.</param>
        public void WaitForFail ( TimeSpan maximumIdleTolerance ) {
            if (!this.IsActive) {
                throw new InvalidOperationException ( SR.HttpRequestEventSource_KeepAliveDisposed );
            }
            this.keepAlive = maximumIdleTolerance;

            new Task ( this.KeepAliveTask ).Start ();

            this.terminatingMutex.WaitOne ();
        }

        /// <summary>
        /// Closes the event listener and it's connection.
        /// </summary>
        public HttpResponse Close () {
            if (!this.isClosed) {
                this.isClosed = true;
                this.Flush ();
                this.Dispose ();
                this.hostServer._eventCollection.UnregisterEventSource ( this );
            }
            return new HttpResponse ( HttpResponse.HTTPRESPONSE_SERVER_CLOSE ) {
                CalculedLength = this.length
            };
        }

        /// <summary>
        /// Cancels the sending queue from sending pending messages and clears the queue.
        /// </summary>
        public void Cancel () {
            this.sendQueue.Clear ();
        }

        internal void Flush () {
            for (int i = 0; i < this.sendQueue.Count; i++) {
                if (this.isClosed) {
                    return;
                }
                string item = this.sendQueue [ i ];
                byte [] itemBytes = this.req.ContentEncoding.GetBytes ( item );
                try {
                    this.res.OutputStream.Write ( itemBytes );
                    this.length += itemBytes.Length;
                    this.sendQueue.RemoveAt ( 0 );
                    this.lastSuccessfullMessage = DateTime.Now;
                }
                catch (Exception) {
                    this.Dispose ();
                }
            }
        }

        /// <summary>
        /// Flushes and releases the used resources of this class instance.
        /// </summary>
        public void Dispose () {
            if (this.isDisposed)
                return;
            this.Close ();
            this.sendQueue.Clear ();
            this.terminatingMutex.Set ();
            this.IsActive = false;
            this.isDisposed = true;
        }
    }
}
