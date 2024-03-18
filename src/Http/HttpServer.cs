// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServer.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http.Handlers;
using Sisk.Core.Http.Hosting;
using Sisk.Core.Http.Streams;
using Sisk.Core.Routing;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides an lightweight HTTP server powered by Sisk.
    /// </summary>
    /// <definition>
    /// public class HttpServer : IDisposable
    /// </definition> 
    /// <type>
    /// Class
    /// </type>
    public partial class HttpServer : IDisposable
    {
        private bool _isListening = false;
        private bool _isDisposing = false;
        private HttpListener httpListener = new HttpListener();
        private AsyncCallback _listenerCallback;
        private ListeningHost? _onlyListeningHost;
        internal static string poweredByHeader = "";
        internal HttpEventSourceCollection _eventCollection = new HttpEventSourceCollection();
        internal HttpWebSocketConnectionCollection _wsCollection = new HttpWebSocketConnectionCollection();
        internal List<string>? listeningPrefixes;
        internal HttpServerHandlerRepository handler = new HttpServerHandlerRepository();

        static HttpServer()
        {
            Version assVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
            poweredByHeader = $"Sisk/{assVersion.Major}.{assVersion.Minor}";
        }

        /// <summary>
        /// Builds an <see cref="HttpServerHostContext"/> context invoking the handler on it.
        /// </summary>
        /// <param name="handler">The action which will configure the host context.</param>
        /// <definition>
        /// public static HttpServerHostContext CreateBuilder(Action{{HttpServerHostContextBuilder}} handler)
        /// </definition>
        /// <type>
        /// Static method 
        /// </type>
        public static HttpServerHostContext CreateBuilder(Action<HttpServerHostContextBuilder> handler)
        {
            var builder = new HttpServerHostContextBuilder();
            handler(builder);
            return builder.Build();
        }

        /// <summary>
        /// Builds an empty <see cref="HttpServerHostContext"/> context.
        /// </summary>
        /// <definition>
        /// public static HttpServerHostContext CreateBuilder()
        /// </definition>
        /// <type>
        /// Static method 
        /// </type>
        public static HttpServerHostContext CreateBuilder()
        {
            var builder = new HttpServerHostContextBuilder();
            return builder.Build();
        }

        /// <summary>
        /// Outputs an non-listening HTTP server with configuration, listening host, and router.
        /// </summary>
        /// <remarks>This method is not appropriate to running production servers.</remarks>
        /// <param name="insecureHttpPort">The insecure port where the HTTP server will listen.</param>
        /// <param name="configuration">The <see cref="HttpServerConfiguration"/> object issued from this method.</param>
        /// <param name="host">The <see cref="ListeningHost"/> object issued from this method.</param>
        /// <param name="router">The <see cref="Router"/> object issued from this method.</param>
        /// <returns></returns>
        /// <definition>
        /// public static HttpServer Emit(in ushort insecureHttpPort, out HttpServerConfiguration configuration, out ListeningHost host, out Router router)
        /// </definition>
        /// <type>
        /// Static method 
        /// </type>
        public static HttpServer Emit(
            in ushort insecureHttpPort,
            out HttpServerConfiguration configuration,
            out ListeningHost host,
            out Router router
        )
        {
            router = new Router();
            if (insecureHttpPort == 0)
            {
                host = new ListeningHost();
                host.Router = router;
                host.Ports = new ListeningPort[]
                {
                    ListeningPort.GetRandomPort()
                };
            }
            else
            {
                host = new ListeningHost();
                host.Router = router;
                host.Ports = new ListeningPort[]
                {
                    new ListeningPort(false, "localhost", insecureHttpPort)
                };
            }
            configuration = new HttpServerConfiguration();
            configuration.ListeningHosts.Add(host);

            HttpServer server = new HttpServer(configuration);
            return server;
        }

        /// <summary>
        /// Gets or sets the Server Configuration object.
        /// </summary>
        /// <definition>
        /// public HttpServerConfiguration ServerConfiguration { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpServerConfiguration ServerConfiguration { get; set; } = new HttpServerConfiguration();

        /// <summary>
        /// Gets an boolean indicating if this HTTP server is running and listening.
        /// </summary>
        /// <definition>
        /// public bool IsListening { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool IsListening { get => _isListening && !_isDisposing; }

        /// <summary>
        /// Gets an string array containing all URL prefixes which this HTTP server is listening to.
        /// </summary>
        /// <definition>
        /// public string ListeningPrefixes { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string[] ListeningPrefixes => listeningPrefixes?.ToArray() ?? Array.Empty<string>();

        /// <summary>
        /// Gets an <see cref="HttpEventSourceCollection"/> with active event source connections in this HTTP server.
        /// </summary>
        /// <definition>
        /// public HttpEventSourceCollection EventSources { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpEventSourceCollection EventSources { get => _eventCollection; }

        /// <summary>
        /// Gets an <see cref="HttpWebSocketConnectionCollection"/> with active Web Sockets connections in this HTTP server.
        /// </summary>
        /// <definition>
        /// public HttpWebSocketConnectionCollection WebSockets { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpWebSocketConnectionCollection WebSockets { get => _wsCollection; }

        /// <summary>
        /// Event that is called when this <see cref="HttpServer"/> computes an request and it's response.
        /// </summary>
        /// <definition>
        /// public event ServerExecutionEventHandler? OnConnectionClose;
        /// </definition>
        /// <remarks>
        /// This event is now obsolete and will be removed in later Sisk versions. Use HttpServerHandlers instead.
        /// </remarks>
        /// <type>
        /// Property
        /// </type>
        [Obsolete("This event is now obsolete and will be removed in later Sisk versions. Use HttpServerHandlers instead.")]
        public event ServerExecutionEventHandler? OnConnectionClose;

        /// <summary>
        /// Event that is called when this <see cref="HttpServer"/> receives an request.
        /// </summary>
        /// <definition>
        /// public event ReceiveRequestEventHandler? OnConnectionOpen;
        /// </definition>
        /// <remarks>
        /// This event is now obsolete and will be removed in later Sisk versions. Use HttpServerHandlers instead.
        /// </remarks>
        /// <type>
        /// Property
        /// </type>
        [Obsolete("This event is now obsolete and will be removed in later Sisk versions. Use HttpServerHandlers instead.")]
        public event ReceiveRequestEventHandler? OnConnectionOpen;

        /// <summary>
        /// Get Sisk version label.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public string GetVersion()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public string GetVersion() => poweredByHeader;

        /// <summary>
        /// Creates a new default configuration <see cref="Sisk.Core.Http.HttpServer"/> instance with the given Route and server configuration.
        /// </summary>
        /// <param name="configuration">The configuration object of the server.</param>
        /// <definition>
        /// public HttpServer(HttpServerConfiguration configuration)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpServer(HttpServerConfiguration configuration)
        {
            this._listenerCallback = new AsyncCallback(ListenerCallback);
            this.ServerConfiguration = configuration;
            this.handler.RegisterHandler(new DefaultHttpServerHandler());
        }

        /// <summary>
        /// Associate an <see cref="HttpServerHandler"/> in this HttpServer to handle functions such as requests, routers and contexts.
        /// </summary>
        /// <typeparam name="T">The handler which implements <see cref="HttpServerHandler"/>.</typeparam>
        /// <definition>
        /// public void RegisterHandler{{[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T}}() where T : HttpServerHandler, new()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void RegisterHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>() where T : HttpServerHandler, new()
        {
            handler.RegisterHandler(new T());
        }

        /// <summary>
        /// Restarts this HTTP server, sending all processing responses and starting them again, reading the listening ports again.
        /// </summary>
        /// <definition>
        /// public void Restart()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public void Restart()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// Starts listening to the set port and handling requests on this server.
        /// </summary>
        /// <definition>
        /// public void Start()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Start()
        {
            if (this.ServerConfiguration.ListeningHosts is null)
            {
                throw new InvalidOperationException(SR.Httpserver_NoListeningHost);
            }

            listeningPrefixes = new List<string>();
            foreach (ListeningHost listeningHost in this.ServerConfiguration.ListeningHosts)
            {
                foreach (ListeningPort port in listeningHost.Ports)
                {
                    string prefix = port.ToString();
                    if (!listeningPrefixes.Contains(prefix)) listeningPrefixes.Add(prefix);
                }
            }

            httpListener.Prefixes.Clear();
            foreach (string prefix in listeningPrefixes)
                httpListener.Prefixes.Add(prefix);

            _isListening = true;
            httpListener.IgnoreWriteExceptions = true;
            httpListener.TimeoutManager.IdleConnection = ServerConfiguration.Flags.IdleConnectionTimeout;

            handler.ServerStarting(this);
            BindRouters();

            if (ServerConfiguration.ListeningHosts.Count == 1)
            {
                _onlyListeningHost = ServerConfiguration.ListeningHosts[0];
            }
            else
            {
                _onlyListeningHost = null;
            }

            httpListener.Start();
            httpListener.BeginGetContext(_listenerCallback, httpListener);

            handler.ServerStarted(this);
        }

        /// <summary>
        /// Stops the server from listening and stops the request handler.
        /// </summary>
        /// <definition>
        /// public void Stop()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Stop()
        {
            _isListening = false;
            httpListener.Stop();
        }

        /// <summary>
        /// Invalidates this class and releases the resources used by it, and permanently closes the HTTP server.
        /// </summary>
        /// <definition>
        /// public void Dispose()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Dispose()
        {
            _isDisposing = true;
            this.httpListener.Close();
            this.ServerConfiguration.Dispose();
        }

        private enum StreamMethodCallback
        {
            Nothing,
            Abort,
            Close
        }
    }
}
