// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestEventSource.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Text;
using Sisk.Core.Http.Engine;

namespace Sisk.Core.Http.Streams {
    /// <summary>
    /// An <see cref="HttpRequestEventSource"/> instance opens a persistent connection to the request, which sends events in text/event-stream format.
    /// </summary>
    public sealed class HttpRequestEventSource : IDisposable {
        readonly ManualResetEvent terminatingMutex = new ManualResetEvent ( false );
        readonly HttpStreamPingPolicy pingPolicy;
        readonly HttpServerEngineContextResponse res;
        readonly HttpServerEngineContextRequest req;
        readonly HttpRequest reqObj;
        readonly HttpServer hostServer;
        TimeSpan keepAlive = TimeSpan.Zero;
        DateTime lastSuccessfullMessage = DateTime.Now;
        int length;

        internal Queue<string> sendQueue = new Queue<string> ();
        internal bool hasSentData;

        // 
        // isClosed determines if this instance has some connection or not
        // isDisposed determines if this object was removed from their collection but wasnt collected by gc yet
        //

        private bool isClosed;
        private bool isDisposed;

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
        public bool IsActive { get => !isClosed && !isDisposed; }

        internal HttpRequestEventSource ( string? identifier, HttpServerEngineContextResponse res, HttpServerEngineContextRequest req, HttpRequest host ) {
            this.res = res ?? throw new ArgumentNullException ( nameof ( res ) );
            this.req = req ?? throw new ArgumentNullException ( nameof ( req ) );
            Identifier = identifier;
            hostServer = host.baseServer;
            reqObj = host;
            pingPolicy = new HttpStreamPingPolicy ( this );

            hostServer._eventCollection.RegisterEventSource ( this );

            res.AppendHeader ( HttpKnownHeaderNames.CacheControl, "no-store, no-cache" );
            res.AppendHeader ( HttpKnownHeaderNames.ContentType, "text/event-stream; charset=utf-8" );
            if (host.baseServer.ServerConfiguration.SendSiskHeader)
                res.AppendHeader ( HttpKnownHeaderNames.XPoweredBy, HttpServer.PoweredBy );

            if (host.Context.MatchedRoute?.UseCors == true)
                HttpServer.SetCorsHeaders ( req, host.Context.ListeningHost?.CrossOriginResourceSharingPolicy, res );
        }

        private void KeepAliveTask () {
            while (IsActive) {
                if (lastSuccessfullMessage < DateTime.Now - keepAlive) {
                    Dispose ();
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
            act ( pingPolicy );
        }

        /// <summary>
        /// Sends an header to the streaming context.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="value">The header value.</param>
        public void AppendHeader ( string name, string value ) {
            if (hasSentData) {
                throw new InvalidOperationException ( SR.Httpserver_Commons_HeaderAfterContents );
            }
            res.AppendHeader ( name, value );
        }

        /// <summary>
        /// Sends an event to the client over the HTTP connection.
        /// </summary>
        /// <param name="data">The data to be sent as part of the event.</param>
        /// <param name="fieldName">The field name for the event data. Defaults to "data".</param>
        /// <returns>True if the event was sent successfully, false otherwise.</returns>
        public bool Send ( string? data, string fieldName = "data" ) {

            ArgumentNullException.ThrowIfNull ( fieldName, nameof ( fieldName ) );

            if (!IsActive) {
                return false;
            }
            hasSentData = true;
            sendQueue.Enqueue ( $"{fieldName}: {data}\n\n" );
            Flush ();
            return IsActive;
        }

        /// <summary>
        /// Asynchronously sends an event to the client over the HTTP connection.
        /// </summary>
        /// <param name="data">The data to be sent as part of the event.</param>
        /// <param name="fieldName">The field name for the event data. Defaults to "data".</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation. The result is true if the event was sent successfully, false otherwise.</returns>
        public async ValueTask<bool> SendAsync ( string? data, string fieldName = "data" ) {

            ArgumentNullException.ThrowIfNull ( fieldName, nameof ( fieldName ) );

            if (!IsActive) {
                return false;
            }
            hasSentData = true;
            sendQueue.Enqueue ( $"{fieldName}: {data}\n\n" );
            await FlushAsync ();
            return IsActive;
        }

        /// <summary>
        /// Asynchronously waits for the connection to close before continuing execution. This method
        /// is released when either the client or the server reaches an sending failure.
        /// </summary>
        public void KeepAlive () {
            if (!IsActive) {
                throw new InvalidOperationException ( SR.HttpRequestEventSource_KeepAliveDisposed );
            }
            terminatingMutex.WaitOne ();
        }

        /// <summary>
        /// Asynchronously waits for the connection to close before continuing execution with
        /// an maximum keep alive timeout. This method is released when either the client or the server reaches an sending failure.
        /// </summary>
        /// <param name="maximumIdleTolerance">The maximum timeout interval for an idle connection to automatically release this method.</param>
        public void WaitForFail ( TimeSpan maximumIdleTolerance ) {
            if (!IsActive) {
                throw new InvalidOperationException ( SR.HttpRequestEventSource_KeepAliveDisposed );
            }
            keepAlive = maximumIdleTolerance;

            new Task ( KeepAliveTask ).Start ();

            terminatingMutex.WaitOne ();
        }

        /// <summary>
        /// Closes the event listener and it's connection.
        /// </summary>
        public HttpResponse Close () {
            if (!isClosed) {
                isClosed = true;
                Flush ();
                Dispose ();
                hostServer._eventCollection.UnregisterEventSource ( this );
            }
            return new HttpResponse ( HttpResponse.HTTPRESPONSE_SERVER_CLOSE ) {
                CalculedLength = length
            };
        }

        /// <summary>
        /// Cancels the sending queue from sending pending messages and clears the queue.
        /// </summary>
        public void Cancel () {
            sendQueue.Clear ();
        }

        internal void Flush () {
            while (sendQueue.TryDequeue ( out string? item )) {
                byte [] itemBytes = Encoding.UTF8.GetBytes ( item );
                try {
                    res.OutputStream.Write ( itemBytes );
                    length += itemBytes.Length;
                    lastSuccessfullMessage = DateTime.Now;
                }
                catch (Exception) {
                    Dispose ();
                }
            }
        }

        internal async ValueTask FlushAsync () {
            while (sendQueue.TryDequeue ( out string? item )) {
                byte [] itemBytes = Encoding.UTF8.GetBytes ( item );
                try {
                    await res.OutputStream.WriteAsync ( itemBytes );
                    length += itemBytes.Length;
                    lastSuccessfullMessage = DateTime.Now;
                }
                catch (Exception) {
                    Dispose ();
                }
            }
        }

        /// <summary>
        /// Flushes and releases the used resources of this class instance.
        /// </summary>
        public void Dispose () {
            if (isDisposed)
                return;

            GC.SuppressFinalize ( this );

            Close ();
            sendQueue.Clear ();
            terminatingMutex.Dispose ();
            isDisposed = true;
        }

        /// <exclude/>
        ~HttpRequestEventSource () {
            Dispose ();
        }
    }
}