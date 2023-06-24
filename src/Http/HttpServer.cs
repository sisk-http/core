using Sisk.Core.Entity;
using Sisk.Core.Http.Streams;
using Sisk.Core.Internal;
using Sisk.Core.Routing;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

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
        internal static string poweredByHeader = "";
        internal HttpEventSourceCollection _eventCollection = new HttpEventSourceCollection();
        internal HttpWebSocketConnectionCollection _wsCollection = new HttpWebSocketConnectionCollection();
        internal List<string>? listeningPrefixes;

        static HttpServer()
        {
            Version assVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
            poweredByHeader = $"Sisk/{assVersion.Major}.{assVersion.Minor}";
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
        /// public static HttpServer Emit(in int insecureHttpPort, out HttpServerConfiguration configuration, out ListeningHost host, out Router router)
        /// </definition>
        /// <type>
        /// Static method 
        /// </type>
        public static HttpServer Emit(
                        in int insecureHttpPort,
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
        /// <type>
        /// Property
        /// </type>
        public event ServerExecutionEventHandler? OnConnectionClose;

        /// <summary>
        /// Event that is called when this <see cref="HttpServer"/> receives an request.
        /// </summary>
        /// <definition>
        /// public event ReceiveRequestEventHandler? OnConnectionOpen;
        /// </definition>
        /// <type>
        /// Property
        /// </type>
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
            this.ServerConfiguration = configuration;
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
                throw new InvalidOperationException("Cannot start the HTTP server with no listening hosts.");
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
            httpListener.Start();
            httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), httpListener);
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
