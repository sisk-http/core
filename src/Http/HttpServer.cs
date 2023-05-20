using Sisk.Core.Entity;
using Sisk.Core.Http.Streams;
using Sisk.Core.Internal;
using Sisk.Core.Routing;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class HttpServer : IDisposable
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void Stop()
        {
            _isListening = false;
            httpListener.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static string HumanReadableSize(float? size)
        {
            if (size == null) return "";
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }
            return string.Format("{0:0.##}{1}", size, sizes[order]);
        }

        private void TryCloseStream(HttpListenerResponse response)
        {
            try
            {
                response.Close();
            }
            catch (Exception)
            {
                ;
            }
        }

        internal static void SetCorsHeaders(CrossOriginResourceSharingHeaders cors, HttpListenerResponse baseResponse)
        {
            if (cors.AllowHeaders.Length > 0) baseResponse.Headers.Set("Access-Control-Allow-Headers", string.Join(", ", cors.AllowHeaders));
            if (cors.AllowMethods.Length > 0) baseResponse.Headers.Set("Access-Control-Allow-Methods", string.Join(", ", cors.AllowMethods));
            if (cors.AllowOrigin != null) baseResponse.Headers.Set("Access-Control-Allow-Origin", cors.AllowOrigin);
            if (cors.AllowCredentials != null) baseResponse.Headers.Set("Access-Control-Allow-Credentials", cors.AllowCredentials.ToString()!.ToLower());
            if (cors.ExposeHeaders.Length > 0) baseResponse.Headers.Set("Access-Control-Expose-Headers", string.Join(", ", cors.ExposeHeaders));
            if (cors.MaxAge.TotalSeconds > 0) baseResponse.Headers.Set("Access-Control-Max-Age", cors.MaxAge.TotalSeconds.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void ListenerCallback(IAsyncResult result)
        {
            if (_isDisposing || !_isListening)
                return;

            httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), httpListener);
            HttpListenerContext context;
            HttpRequest request = null!;
            HttpResponse? response = null;

            try
            {
                context = httpListener.EndGetContext(result);
            }
            catch (Exception)
            {
                return;
            }

            HttpServerFlags flag = ServerConfiguration.Flags;
            Stopwatch sw = new Stopwatch();
            HttpListenerResponse baseResponse = context.Response;
            HttpListenerRequest baseRequest = context.Request;
            long incomingSize = 0;
            long outcomingSize = 0;
            bool closeStream = true;
            bool useCors = false;
            bool hasAccessLogging = ServerConfiguration.AccessLogsStream != null;
            bool hasErrorLogging = ServerConfiguration.ErrorsLogsStream != null;
            LogOutput logMode = LogOutput.Both;
            IPAddress otherParty = baseRequest.RemoteEndPoint.Address;
            Uri? connectingUri = baseRequest.Url;
            int responseStatus = 0;
            string responseDescription = "";
            NameValueCollection? reqHeaders = null;

            if (ServerConfiguration.DefaultCultureInfo != null)
            {
                Thread.CurrentThread.CurrentCulture = ServerConfiguration.DefaultCultureInfo;
                Thread.CurrentThread.CurrentUICulture = ServerConfiguration.DefaultCultureInfo;
            }

            HttpServerExecutionResult? executionResult = new HttpServerExecutionResult()
            {
                Request = request,
                Response = response,
                Status = HttpServerExecutionStatus.NoResponse
            };

            try
            {
                sw.Start();

                if (connectingUri is null)
                {
                    baseResponse.StatusCode = 400;
                    executionResult.Status = HttpServerExecutionStatus.DnsFailed;
                    return;
                }

                string dnsSafeHost = connectingUri.DnsSafeHost;
                string? forwardedHost = baseRequest.Headers["X-Forwarded-Host"];
                if (ServerConfiguration.ResolveForwardedOriginHost && forwardedHost != null)
                {
                    dnsSafeHost = forwardedHost;
                }

                // detect the listening host for this listener
                ListeningHost? matchedListeningHost = ServerConfiguration.ListeningHosts
                    .GetRequestMatchingListeningHost(dnsSafeHost, baseRequest.LocalEndPoint.Port);

                if (matchedListeningHost is null)
                {
                    baseResponse.StatusCode = 400; // Bad Request
                    executionResult.Status = HttpServerExecutionStatus.DnsUnknownHost;
                    return;
                }
                else
                {
                    request = new HttpRequest(baseRequest, baseResponse, this, matchedListeningHost, context);
                    reqHeaders = baseRequest.Headers;
                    if (ServerConfiguration.ResolveForwardedOriginAddress || ServerConfiguration.ResolveForwardedOriginHost)
                    {
                        otherParty = request.Origin;
                    }
                }

                if (matchedListeningHost.Router == null || !matchedListeningHost.CanListen)
                {
                    baseResponse.StatusCode = 503; // Service Unavailable
                    executionResult.Status = HttpServerExecutionStatus.ListeningHostNotReady;
                    return;
                }

                if (ServerConfiguration.IncludeRequestIdHeader)
                {
                    baseResponse.Headers.Set(flag.HeaderNameRequestId, request.RequestId.ToString());
                }

                if (OnConnectionOpen != null)
                    OnConnectionOpen(this, request);

                long requestMaxSize = ServerConfiguration.MaximumContentLength;
                if (requestMaxSize > 0 && baseRequest.ContentLength64 > requestMaxSize)
                {
                    executionResult.Status = HttpServerExecutionStatus.ContentTooLarge;
                    baseResponse.StatusCode = 413;
                    return;
                }

                // imports the request contents
                incomingSize += request.CalcRequestSize();

                // check for illegal body content requests
                if (flag.ThrowContentOnNonSemanticMethods && (
                       request.Method == HttpMethod.Get
                    || request.Method == HttpMethod.Options
                    || request.Method == HttpMethod.Head
                    || request.Method == HttpMethod.Trace
                    ) && context.Request.ContentLength64 > 0)
                {
                    executionResult.Status = HttpServerExecutionStatus.ContentServedOnIllegalMethod;
                    baseResponse.StatusCode = 400;
                    return;
                }

                // bind
                matchedListeningHost.Router.BindServer(this);

                // get response
                var routerResult = matchedListeningHost.Router.Execute(request, baseRequest);
                response = routerResult.Response;
                logMode = routerResult.Route?.LogMode ?? LogOutput.Both;
                useCors = routerResult.Route?.UseCors ?? true;

                if (flag.SendSiskHeader)
                    baseResponse.Headers.Set("X-Powered-By", poweredByHeader);

                if (response is null)
                {
                    executionResult.Status = HttpServerExecutionStatus.NoResponse;
                    return;
                }
                else if (response?.internalStatus == HttpResponse.HTTPRESPONSE_STREAM_CLOSE)
                {
                    executionResult.Status = HttpServerExecutionStatus.StreamClosed;
                    baseResponse.StatusCode = (int)response.Status;
                    return;
                }
                else if (response?.internalStatus == HttpResponse.HTTPRESPONSE_EMPTY)
                {
                    executionResult.Status = HttpServerExecutionStatus.NoResponse;
                    return;
                }
                else if (response?.internalStatus == HttpResponse.HTTPRESPONSE_ERROR)
                {
                    executionResult.Status = HttpServerExecutionStatus.UncaughtExceptionThrown;
                    executionResult.ServerException = routerResult.Exception;
                    baseResponse.StatusCode = 500;

                    return;
                }
                else if (response?.internalStatus == HttpResponse.HTTPRESPONSE_CLOSE)
                {
                    executionResult.Status = HttpServerExecutionStatus.ClosedStream;
                    baseResponse.Close();
                    return;
                }

                if (useCors && flag.SendCorsHeaders)
                {
                    SetCorsHeaders(matchedListeningHost.CrossOriginResourceSharingPolicy, baseResponse);
                }
                if (routerResult.Result == RouteMatchResult.OptionsMatched)
                {
                    logMode = flag.OptionsLogMode;
                }

                byte[] responseBytes = response!.Content?.ReadAsByteArrayAsync().Result ?? new byte[] { };

                if (response.CustomStatus != null)
                {
                    baseResponse.StatusCode = response.CustomStatus.Value.StatusCode;
                    baseResponse.StatusDescription = response.CustomStatus.Value.Description;
                    responseStatus = response.CustomStatus.Value.StatusCode;
                    responseDescription = response.CustomStatus.Value.Description;
                }
                else
                {
                    baseResponse.StatusCode = (int)response.Status;
                    responseStatus = baseResponse.StatusCode;
                    responseDescription = baseResponse.StatusDescription;
                }
                baseResponse.SendChunked = response.SendChunked;

                NameValueCollection resHeaders = new NameValueCollection
                {
                    response.Headers
                };

                foreach (string incameHeader in resHeaders)
                {
                    baseResponse.AddHeader(incameHeader, resHeaders[incameHeader] ?? "");
                }

                if (responseBytes.Length > 0)
                {
                    baseResponse.ContentType = resHeaders["Content-Type"] ?? response.Content?.Headers.ContentType?.ToString();

                    if (resHeaders["Content-Encoding"] != null)
                    {
                        baseResponse.ContentEncoding = Encoding.GetEncoding(resHeaders["Content-Encoding"]!);
                    }
                    else
                    {
                        baseResponse.ContentEncoding = ServerConfiguration.DefaultEncoding;
                    }

                    if (!response.SendChunked)
                    {
                        baseResponse.ContentLength64 = responseBytes.Length;
                    }

                    if (context.Request.HttpMethod != "HEAD")
                    {
                        baseResponse.OutputStream.Write(responseBytes);
                        outcomingSize += responseBytes.Length;
                    }
                }

                string httpStatusVerbose = $"{(int)response.Status} {response.Status}";

                executionResult.RequestSize = incomingSize;
                executionResult.ResponseSize = outcomingSize;
                executionResult.Response = response;

                sw.Stop();
                baseResponse.Close();
                baseRequest.InputStream.Close();

                closeStream = false;
                executionResult.Status = HttpServerExecutionStatus.Executed;
            }
            catch (ObjectDisposedException objException)
            {
                executionResult.Status = HttpServerExecutionStatus.ExceptionThrown;
                executionResult.ServerException = objException;
            }
            catch (HttpListenerException netException)
            {
                executionResult.Status = HttpServerExecutionStatus.ExceptionThrown;
                executionResult.ServerException = netException;
            }
            catch (HttpRequestException requestException)
            {
                baseResponse.StatusCode = 400;
                executionResult.Status = HttpServerExecutionStatus.MalformedRequest;
                executionResult.ServerException = requestException;
            }
            catch (Exception ex)
            {
                if (!ServerConfiguration.ThrowExceptions)
                {
                    executionResult.ServerException = ex;
                    executionResult.Status = HttpServerExecutionStatus.ExceptionThrown;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (closeStream)
                {
                    baseRequest.InputStream.Close();
                    TryCloseStream(baseResponse);
                }

                if (OnConnectionClose != null)
                {
                    // the "Request" variable was pointing to an null value before
                    // this line
                    executionResult.Request = request;
                    OnConnectionClose(this, executionResult);
                }

                bool canAccessLog = logMode.HasFlag(LogOutput.AccessLog) && hasAccessLogging;
                bool canErrorLog = logMode.HasFlag(LogOutput.ErrorLog) && hasErrorLogging;

                if (executionResult.ServerException != null && canErrorLog)
                {
                    StringBuilder exceptionStr = new StringBuilder();
                    exceptionStr.AppendLine($"Exception thrown at {DateTime.Now:R}");
                    exceptionStr.AppendLine($"-------------\nRequest:");
                    exceptionStr.AppendLine(request.GetRawHttpRequest(false));
                    exceptionStr.AppendLine($"\n-------------\nError contents:");
                    exceptionStr.AppendLine(executionResult.ServerException.ToString());

                    if (executionResult.ServerException.InnerException != null)
                    {
                        exceptionStr.AppendLine($"\n-------------\nInner exception:");
                        exceptionStr.AppendLine(executionResult.ServerException.InnerException.ToString());
                    }

                    ServerConfiguration.ErrorsLogsStream?.WriteLine(exceptionStr.ToString());
                }
                if (canAccessLog)
                {
                    var formatter = new LoggingFormatter(
                        executionResult,
                        DateTime.Now,
                        connectingUri,
                        otherParty,
                        reqHeaders,
                        responseStatus,
                        responseDescription,
                        incomingSize,
                        outcomingSize,
                        sw.ElapsedMilliseconds);

                    string line = ServerConfiguration.AccessLogsFormat;
                    formatter.Format(ref line);

                    ServerConfiguration.AccessLogsStream?.WriteLine(line);
                }
            }
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
