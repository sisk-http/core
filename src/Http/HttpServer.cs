// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServer.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections.Concurrent;
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

        private bool _disposed;
        private ListeningHost? _onlyListeningHost;
        private CancellationTokenSource? listenerCancellation;
        internal HttpEventSourceCollection _eventCollection = new HttpEventSourceCollection ();
        internal HttpWebSocketConnectionCollection _wsCollection = new HttpWebSocketConnectionCollection ();
        internal HttpServerHandlerRepository handler;

        internal ConcurrentStack<TaskCompletionSource<HttpServerExecutionResult>> syncCompletionSources = new ();

        static HttpServer () {
            Version assVersion = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Version!;
            PoweredBy = $"Sisk/{assVersion.Major}.{assVersion.Minor}";
            SiskVersion = assVersion;

            environmentUtcOffset = DateTimeOffset.Now.Offset;
        }

        /// <summary>
        /// Gets an <see cref="bool"/> indicating if Sisk can be used with the current environment.
        /// </summary>
        [Obsolete ( "This property is no longer supported. To find out if HttpListener is supported, use HttpListener.IsSupported." )]
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
        public bool IsListening { get => listenerCancellation is { IsCancellationRequested: false } && !_disposed; }

        /// <summary>
        /// Gets an string array containing all URL prefixes which this HTTP server is listening to.
        /// </summary>
        public string [] ListeningPrefixes => ServerConfiguration.Engine.ListeningPrefixes;

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
        /// Waits for the next HTTP request to be processed, with a specified timeout.
        /// </summary>
        /// <param name="timeout">The time span to wait for the next request. If not specified, the default timeout is used.</param>
        /// <returns>The result of the HTTP server execution.</returns>
        public HttpServerExecutionResult WaitNext ( TimeSpan timeout = default ) {
            return WaitNextAsync ( timeout ).GetAwaiter ().GetResult ();
        }

        /// <summary>
        /// Waits for the next HTTP request to be processed, with a specified cancellation token.
        /// </summary>
        /// <param name="cancellation">The cancellation token to signal when the operation should be cancelled.</param>
        /// <returns>The result of the HTTP server execution.</returns>
        public HttpServerExecutionResult WaitNext ( CancellationToken cancellation = default ) {
            return WaitNextAsync ( cancellation ).GetAwaiter ().GetResult ();
        }

        /// <summary>
        /// Asynchronously waits for the next HTTP request to be processed, with a specified timeout.
        /// </summary>
        /// <param name="timeout">The time span to wait for the next request. If not specified, the default timeout is used.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the result of the HTTP server execution.</returns>
        public Task<HttpServerExecutionResult> WaitNextAsync ( TimeSpan timeout = default ) {
            var ctx = new CancellationTokenSource ( timeout );
            return WaitNextAsync ( ctx.Token );
        }

        /// <summary>
        /// Asynchronously waits for the next HTTP request to be processed, with a specified cancellation token.
        /// </summary>
        /// <param name="cancellation">The cancellation token to signal when the operation should be cancelled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the result of the HTTP server execution.</returns>
        public async Task<HttpServerExecutionResult> WaitNextAsync ( CancellationToken cancellation = default ) {
            if (!IsListening)
                Start ();

            var source = new TaskCompletionSource<HttpServerExecutionResult> ();
            syncCompletionSources.Push ( source );

            using (var cancellationSource = cancellation.Register ( () => source.SetCanceled ( cancellation ) )) {
                return await source.Task.ConfigureAwait ( false );
            }
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
            if (listenerCancellation is { IsCancellationRequested: false }) {
                return;
            }
            if (ServerConfiguration.ListeningHosts is null) {
                throw new InvalidOperationException ( SR.Httpserver_NoListeningHost );
            }

            var engine = ServerConfiguration.Engine;
            if (engine is null) {
                throw new InvalidOperationException ( SR.Httpserver_NoEngine );
            }

            ObjectDisposedException.ThrowIf ( _disposed, this );

            engine.SetListeningHosts ( ServerConfiguration.ListeningHosts );

            listenerCancellation = new CancellationTokenSource ();
            engine.IdleConnectionTimeout = ServerConfiguration.IdleConnectionTimeout;
            engine.OnConfiguring ( this, ServerConfiguration );

            handler.ServerStarting ( this );
            BindRouters ();

            if (ServerConfiguration.ListeningHosts.Count == 1) {
                _onlyListeningHost = ServerConfiguration.ListeningHosts [ 0 ];
            }
            else {
                _onlyListeningHost = null;
            }

            engine.StartServer ();

            if (engine.EventLoopMecanism == Engine.HttpServerEngineContextEventLoopMecanism.UnboundAsyncronousGetContext) {
                engine.BeginGetContext ( UnboundAsyncListenerCallback, engine );
            }
            else if (engine.EventLoopMecanism == Engine.HttpServerEngineContextEventLoopMecanism.InlineAsyncronousGetContext) {
                ThreadPool.QueueUserWorkItem ( BoundAsyncListenerEventLoop, engine );
            }

            handler.ServerStarted ( this );
        }

        /// <summary>
        /// Stops the server from listening and stops the request handler.
        /// </summary>
        public void Stop () {
            if (listenerCancellation is null ||
                listenerCancellation is { IsCancellationRequested: true } ||
                _disposed) {
                return;
            }

            handler.Stopping ( this );
            ServerConfiguration.Engine.StopServer ();
            listenerCancellation.Cancel ();

            UnbindRouters ();
            handler.Stopped ( this );
        }

        /// <summary>
        /// Invalidates this class and releases the resources used by it, and
        /// permanently closes the HTTP server.
        /// </summary>
        public void Dispose () {
            if (_disposed)
                return;

            foreach (var waitinSources in syncCompletionSources) {
                waitinSources.TrySetCanceled ();
            }

            if (listenerCancellation is { }) {
                listenerCancellation.Cancel ();
                listenerCancellation.Dispose ();
            }

            ServerConfiguration.Dispose ();

            _disposed = true;
            GC.SuppressFinalize ( this );
        }

        /// <exclude/>
        ~HttpServer () {
            Dispose ();
        }
    }
}
