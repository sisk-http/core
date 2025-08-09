﻿// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServer.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using Sisk.Core.Http.Handlers;
using Sisk.Core.Http.Hosting;
using Sisk.Core.Http.Streams;
using Sisk.Core.Routing;

namespace Sisk.Core.Http {
    /// <summary>
    /// Provides an lightweight HTTP server powered by Sisk.
    /// </summary>
    public sealed partial class HttpServer : IDisposable {

        // DateTime.Now invokes an system internal call everytime we call it, so it's more performatic
        // to cache the timezone offset and add it to DateTime.UtcNow everytime we need it
        internal static TimeSpan environmentUtcOffset;

        /// <summary>
        /// Gets the X-Powered-By Sisk header value.
        /// </summary>
        public static string PoweredBy { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the current Sisk version.
        /// </summary>
        public static Version SiskVersion { get; private set; } = null!;

        private bool _isListening;
        private bool _isDisposing;
        private readonly HttpListener httpListener = new HttpListener ();
        private ListeningHost? _onlyListeningHost;
        internal HttpEventSourceCollection _eventCollection = new HttpEventSourceCollection ();
        internal HttpWebSocketConnectionCollection _wsCollection = new HttpWebSocketConnectionCollection ();
        internal HashSet<string>? _listeningPrefixes;
        internal HttpServerHandlerRepository handler;

        internal AutoResetEvent waitNextEvent = new AutoResetEvent ( false );
        internal bool isWaitingNextEvent;
        internal HttpServerExecutionResult? waitingExecutionResult;

        static HttpServer () {
            Version assVersion = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Version!;
            PoweredBy = $"Sisk/{assVersion.Major}.{assVersion.Minor}";
            SiskVersion = assVersion;

            environmentUtcOffset = DateTimeOffset.Now.Offset;
        }

        /// <summary>
        /// Gets an <see cref="bool"/> indicating if Sisk can be used with the current environment.
        /// </summary>
        public static bool IsSupported { get => HttpListener.IsSupported; }

        /// <summary>
        /// Gets an <see cref="bool"/> indicating if the current environment supports dynamic code or it's running in
        /// an AOT assembly.
        /// </summary>
        public static bool IsDynamicCodeSupported { get => System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported; }

        /// <summary>
        /// Builds an <see cref="HttpServerHostContext"/> context invoking the handler on it.
        /// </summary>
        /// <param name="handler">The action which will configure the host context.</param>
        public static HttpServerHostContextBuilder CreateBuilder ( Action<HttpServerHostContextBuilder> handler ) {
            var builder = new HttpServerHostContextBuilder ();
            handler ( builder );
            return builder;
        }

        /// <summary>
        /// Builds an empty <see cref="HttpServerHostContext"/> context with predefined listening port.
        /// </summary>
        public static HttpServerHostContextBuilder CreateBuilder ( ushort port ) {
            var builder = new HttpServerHostContextBuilder ();
            builder.UseListeningPort ( port );
            return builder;
        }

        /// <summary>
        /// Builds an empty <see cref="HttpServerHostContext"/> context with predefined listening host string.
        /// </summary>
        public static HttpServerHostContextBuilder CreateBuilder ( string listeningHost ) {
            var builder = new HttpServerHostContextBuilder ();
            builder.UseListeningPort ( listeningHost );
            return builder;
        }

        /// <summary>
        /// Builds an empty <see cref="HttpServerHostContext"/> context.
        /// </summary>
        public static HttpServerHostContextBuilder CreateBuilder () {
            var builder = new HttpServerHostContextBuilder ();
            return builder;
        }

        /// <summary>
        /// Gets an listening and running HTTP server in an random port.
        /// </summary>
        public static HttpServer CreateListener () => CreateListener ( ListeningPort.GetRandomPort ().Port, out _, out _, out _ );

        /// <summary>
        /// Gets an listening and running HTTP server in the specified port.
        /// </summary>
        /// <param name="port">The listening port of the HTTP server.</param>
        public static HttpServer CreateListener ( ushort port ) => CreateListener ( port, out _, out _, out _ );

        /// <summary>
        /// Gets an listening and running HTTP server in the specified port.
        /// </summary>
        /// <param name="insecureHttpPort">The insecure port where the HTTP server will listen.</param>
        /// <param name="configuration">The <see cref="HttpServerConfiguration"/> object issued from this method.</param>
        /// <param name="host">The <see cref="ListeningHost"/> object issued from this method.</param>
        /// <param name="router">The <see cref="Router"/> object issued from this method.</param>
        public static HttpServer CreateListener (
            ushort insecureHttpPort,
            out HttpServerConfiguration configuration,
            out ListeningHost host,
            out Router router
        ) {
            var s = Emit ( insecureHttpPort, out configuration, out host, out router );
            s.Start ();
            return s;
        }

        /// <summary>
        /// Gets an non-listening HTTP server with configuration, listening host, and router.
        /// </summary>
        /// <param name="insecureHttpPort">The insecure port where the HTTP server will listen.</param>
        /// <param name="configuration">The <see cref="HttpServerConfiguration"/> object issued from this method.</param>
        /// <param name="host">The <see cref="ListeningHost"/> object issued from this method.</param>
        /// <param name="router">The <see cref="Router"/> object issued from this method.</param>
        public static HttpServer Emit (
            ushort insecureHttpPort,
            out HttpServerConfiguration configuration,
            out ListeningHost host,
            out Router router
        ) {
            router = new Router ();
            if (insecureHttpPort == 0) {
                host = new ListeningHost {
                    Router = router,
                    Ports = [ ListeningPort.GetRandomPort () ]
                };
            }
            else {
                host = new ListeningHost {
                    Router = router,
                    Ports = [ new ListeningPort ( false, "localhost", insecureHttpPort ) ]
                };
            }
            configuration = new HttpServerConfiguration ();
            configuration.ListeningHosts.Add ( host );

            HttpServer server = new HttpServer ( configuration );
            return server;
        }

        /// <summary>
        /// Gets or sets the Server Configuration object.
        /// </summary>
        public HttpServerConfiguration ServerConfiguration { get; set; } = new HttpServerConfiguration ();

        /// <summary>
        /// Gets an boolean indicating if this HTTP server is running and listening.
        /// </summary>
        public bool IsListening { get => _isListening && !_isDisposing; }

        /// <summary>
        /// Gets an string array containing all URL prefixes which this HTTP server is listening to.
        /// </summary>
        public string [] ListeningPrefixes => _listeningPrefixes?.ToArray () ?? Array.Empty<string> ();

        /// <summary>
        /// Gets an <see cref="HttpEventSourceCollection"/> with active event source connections in this HTTP server.
        /// </summary>
        public HttpEventSourceCollection EventSources { get => _eventCollection; }

        /// <summary>
        /// Gets an <see cref="HttpWebSocketConnectionCollection"/> with active Web Sockets connections in this HTTP server.
        /// </summary>
        public HttpWebSocketConnectionCollection WebSockets { get => _wsCollection; }

        /// <summary>
        /// Creates an new <see cref="HttpServer"/> instance with no predefined configuration.
        /// </summary>
        public HttpServer () {
            ServerConfiguration = new HttpServerConfiguration ();
            ServerConfiguration.ListeningHosts.Add ( new ListeningHost () {
                Ports = [
                    ListeningPort.GetRandomPort()
                ],
                Router = new Router ()
            } );
            handler = new HttpServerHandlerRepository ( this );
        }

        /// <summary>
        /// Creates a new default configuration <see cref="Sisk.Core.Http.HttpServer"/> instance with the given Route and server configuration.
        /// </summary>
        /// <param name="configuration">The configuration object of the server.</param>
        public HttpServer ( HttpServerConfiguration configuration ) {
            ServerConfiguration = configuration;
            handler = new HttpServerHandlerRepository ( this );
        }

        /// <summary>
        /// Associate an <see cref="HttpServerHandler"/> in this HttpServer to handle functions such as requests, routers and contexts.
        /// </summary>
        /// <typeparam name="T">The handler which implements <see cref="HttpServerHandler"/>.</typeparam>
        public void RegisterHandler<T> () where T : HttpServerHandler, new() {
            handler.RegisterHandler ( new T () );
        }

        /// <summary>
        /// Associate an <see cref="HttpServerHandler"/> in this HttpServer to handle functions such as requests, routers and contexts.
        /// </summary>
        /// <param name="obj">The instance of the server handler.</param>
        public void RegisterHandler ( HttpServerHandler obj ) {
            handler.RegisterHandler ( obj );
        }

        /// <summary>
        /// Waits for the next execution result from the server. This method obtains the next completed context from the HTTP server,
        /// both with the request and its response. This method does not interrupt the asynchronous processing of requests.
        /// </summary>
        /// <remarks>
        /// Calling this method, it starts the HTTP server if it ins't started yet.
        /// </remarks>
        public HttpServerExecutionResult WaitNext () {
            if (!IsListening)
                Start ();
            if (isWaitingNextEvent)
                throw new InvalidOperationException ( SR.Httpserver_WaitNext_Race_Condition );

            waitingExecutionResult = null;
            isWaitingNextEvent = true;
            waitNextEvent.WaitOne ();

            return waitingExecutionResult!;
        }

        /// <summary>
        /// Waits for the next execution result from the server asynchronously. This method obtains the next completed context from the HTTP server,
        /// both with the request and its response. This method does not interrupt the asynchronous processing of requests.
        /// </summary>
        /// <remarks>
        /// Calling this method, it starts the HTTP server if it ins't started yet.
        /// </remarks>
        public async Task<HttpServerExecutionResult> WaitNextAsync () {
            return await Task.Run ( WaitNext ).ConfigureAwait ( false );
        }


        /// <summary>
        /// Restarts this HTTP server, sending all processing responses and starting them again, reading the listening ports again.
        /// </summary>
        public void Restart () {
            Stop ();
            Start ();
        }

        /// <summary>
        /// Starts listening to the set port and handling requests on this server.
        /// </summary>
        public void Start () {
            if (ServerConfiguration.ListeningHosts is null) {
                throw new InvalidOperationException ( SR.Httpserver_NoListeningHost );
            }

            ObjectDisposedException.ThrowIf ( _isDisposing, this );

            _listeningPrefixes = new HashSet<string> ();

            for (int i = 0; i < ServerConfiguration.ListeningHosts.Count; i++) {
                ListeningHost listeningHost = ServerConfiguration.ListeningHosts [ i ];
                listeningHost.EnsureReady ();

                for (int j = 0; j < listeningHost.Ports.Count; j++) {
                    var port = listeningHost.Ports [ j ];

                    _listeningPrefixes.Add ( port.ToString ( true ) );
                }
            }

            httpListener.Prefixes.Clear ();
            foreach (string prefix in _listeningPrefixes)
                httpListener.Prefixes.Add ( prefix );

            _isListening = true;
            httpListener.IgnoreWriteExceptions = true;
            httpListener.TimeoutManager.IdleConnection = ServerConfiguration.IdleConnectionTimeout;

            handler.ServerStarting ( this );
            BindRouters ();

            if (ServerConfiguration.ListeningHosts.Count == 1) {
                _onlyListeningHost = ServerConfiguration.ListeningHosts [ 0 ];
            }
            else {
                _onlyListeningHost = null;
            }

            httpListener.Start ();
            httpListener.BeginGetContext ( ListenerCallback, null );

            handler.ServerStarted ( this );
        }

        /// <summary>
        /// Stops the server from listening and stops the request handler.
        /// </summary>
        public void Stop () {
            handler.Stopping ( this );
            _isListening = false;
            httpListener.Stop ();

            UnbindRouters ();
            handler.Stopped ( this );
        }

        /// <summary>
        /// Invalidates this class and releases the resources used by it, and
        /// permanently closes the HTTP server.
        /// </summary>
        public void Dispose () {
            _isDisposing = true;
            httpListener.Close ();
            ServerConfiguration.Dispose ();
            waitNextEvent.Set ();
            waitNextEvent.Dispose ();
        }
    }
}
