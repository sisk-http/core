using Sisk.Core.Entity;
using Sisk.Core.Routing;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
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
        private Mutex _accessLogMutex = new Mutex();
        private Mutex _errorLogMutex = new Mutex();
        private bool _isListening = false;
        private bool _isDisposing = false;
        private HttpListener httpListener = new HttpListener();
        internal static string poweredByHeader = "";

        static HttpServer()
        {
            Version assVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
            poweredByHeader = $"Sisk/{assVersion.Major}.{assVersion.Minor}";
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

            List<string> listeningPrefixes = new List<string>();
            foreach (ListeningHost listeningHost in this.ServerConfiguration.ListeningHosts)
            {
                foreach (ListeningPort port in listeningHost.Ports)
                {
                    string prefix;

                    if (port.Secure)
                    {
                        prefix = $"https://{listeningHost.Hostname}:{port.Port}/";
                    }
                    else
                    {
                        prefix = $"http://{listeningHost.Hostname}:{port.Port}/";
                    }

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

        private string HumanReadableSize(float size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }
            return String.Format("{0:0.##}{1}", size, sizes[order]);
        }

        private void TryCloseStream(HttpListenerResponse response)
        {
            try
            {
                response.Headers.Set("X-Powered-By", poweredByHeader);
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
            if (cors.AllowOrigins.Length > 0) baseResponse.Headers.Set("Access-Control-Allow-Origin", string.Join(", ", cors.AllowOrigins));
            if (cors.AllowCredentials != null) baseResponse.Headers.Set("Access-Control-Allow-Credentials", cors.AllowCredentials.ToString()!.ToLower());
            if (cors.ExposeHeaders.Length > 0) baseResponse.Headers.Set("Access-Control-Expose-Headers", string.Join(", ", cors.ExposeHeaders));
            if (cors.MaxAge.TotalSeconds > 0) baseResponse.Headers.Set("Access-Control-Max-Age", cors.MaxAge.TotalSeconds.ToString());
        }

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

            Stopwatch sw = new Stopwatch();
            HttpListenerResponse baseResponse = context.Response;
            HttpListenerRequest baseRequest = context.Request;
            string verbosePrefix = "";
            string verboseSuffix = "";
            long incomingSize = 0;
            long outcomingSize = 0;
            bool closeStream = true;
            LogOutput logMode = LogOutput.Both;

            HttpServerExecutionResult? executionResult = new HttpServerExecutionResult()
            {
                Request = request,
                Response = response,
                Status = HttpServerExecutionStatus.NoResponse
            };

            try
            {
                verbosePrefix = $"{DateTime.Now:g} %%STATUS%% {baseRequest.RemoteEndPoint.Address.ToString().TrimEnd('\0')} ({baseRequest.Url?.Scheme} {baseRequest.Url!.Authority}) {baseRequest.HttpMethod.ToUpper()} {baseRequest.Url?.AbsolutePath ?? "/"}";
                sw.Start();

                if (baseRequest.Url is null)
                {
                    baseResponse.StatusCode = 400;
                    executionResult.Status = HttpServerExecutionStatus.DnsFailed;
                    return;
                }

                string dnsSafeHost = baseRequest.Url.DnsSafeHost;
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
                    request = new HttpRequest(baseRequest, baseResponse, this.ServerConfiguration, matchedListeningHost);
                }

                if (matchedListeningHost.Router == null || !matchedListeningHost.CanListen)
                {
                    baseResponse.StatusCode = 503; // Service Unavailable
                    executionResult.Status = HttpServerExecutionStatus.ListeningHostNotReady;
                    return;
                }

                if (ServerConfiguration.IncludeRequestIdHeader)
                {
                    baseResponse.Headers.Set("X-Request-Id", request.RequestId.ToString());
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
                request.ImportContents(baseRequest.InputStream);
                incomingSize += request.CalcRequestSize();

                // check for illegal body content requests
                if ((
                       request.Method == HttpMethod.Get
                    || request.Method == HttpMethod.Options
                    || request.Method == HttpMethod.Head
                    || request.Method == HttpMethod.Trace
                    ) && context.Request.ContentLength64 > 0)
                {
                    executionResult.Status = HttpServerExecutionStatus.ContentServedOnNotSupportedMethod;
                    baseResponse.StatusCode = 400;
                    return;
                }

                // get response
                matchedListeningHost.Router.ParentServer = this;
                matchedListeningHost.Router.ParentListenerHost = matchedListeningHost;

                var routerResult = matchedListeningHost.Router.Execute(request);
                response = routerResult.Response;
                logMode = routerResult.Route?.LogMode ?? LogOutput.Both;

                if ((routerResult.Result == RouteMatchResult.OptionsMatched || (routerResult.Route?.UseCors ?? false))
                    && response?.internalStatus != HttpResponse.HTTPRESPONSE_EVENTSOURCE_CLOSE)
                {
                    var cors = matchedListeningHost.CrossOriginResourceSharingPolicy;
                    SetCorsHeaders(cors, baseResponse);
                }

                if (response is null)
                {
                    executionResult.Status = HttpServerExecutionStatus.NoResponse;
                    return;
                }
                else if (response?.internalStatus == HttpResponse.HTTPRESPONSE_EVENTSOURCE_CLOSE)
                {
                    verboseSuffix = $"{HumanReadableSize((int)incomingSize) + " -> " + HumanReadableSize(response.CalculedLength)}";
                    executionResult.Status = HttpServerExecutionStatus.EventSourceClosed;
                    baseResponse.StatusCode = 200;
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
                    baseResponse.StatusCode = 500;
                    return;
                }

                byte[] responseBytes = response!.Content?.ReadAsByteArrayAsync().Result ?? new byte[] { };

                baseResponse.StatusCode = (int)response.Status;
                baseResponse.SendChunked = response.SendChunked;

                NameValueCollection resHeaders = new NameValueCollection
                    {
                        response.Headers
                    };
                foreach (string incameHeader in resHeaders)
                {
                    baseResponse.AddHeader(incameHeader, resHeaders[incameHeader] ?? "");
                }

                if (responseBytes.Length > 0 && context.Request.HttpMethod != "HEAD")
                {
                    baseResponse.ContentType = resHeaders["Content-Type"] ?? response.Content!.Headers.ContentType!.MediaType ?? "text/plain";

                    if (resHeaders["Content-Encoding"] != null)
                    {
                        baseResponse.ContentEncoding = Encoding.GetEncoding(resHeaders["Content-Encoding"]!);
                    }
                    else
                    {
                        baseResponse.ContentEncoding = ServerConfiguration.DefaultEncoding;
                    }

                    baseResponse.ContentLength64 = responseBytes.Length;
                    baseResponse.OutputStream.Write(responseBytes);
                    outcomingSize += responseBytes.Length;
                }

                outcomingSize += response.CalcHeadersSize();

                string httpStatusVerbose = $"{(int)response.Status} {response.Status}";

                executionResult.RequestSize = incomingSize;
                executionResult.ResponseSize = outcomingSize;
                executionResult.Response = response;

                sw.Stop();
                baseResponse.Close();

                closeStream = false;
                executionResult.Status = HttpServerExecutionStatus.Executed;
                verboseSuffix = $"[{httpStatusVerbose}] {HumanReadableSize((int)incomingSize) + " -> " + HumanReadableSize((int)outcomingSize)}";
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
                    TryCloseStream(baseResponse);
                }

                if (OnConnectionClose != null)
                {
                    OnConnectionClose(this, executionResult);
                }

                bool canAccessLog = logMode == LogOutput.AccessLog || logMode == LogOutput.Both;
                bool canErrorLog = logMode == LogOutput.ErrorLog || logMode == LogOutput.Both;

                if (executionResult.ServerException != null && canErrorLog)
                {
                    _errorLogMutex.WaitOne();
                    ServerConfiguration.ErrorsLogsStream?.WriteLine($"Exception thrown at {DateTime.Now:R}");
                    ServerConfiguration.ErrorsLogsStream?.WriteLine(executionResult.ServerException);
                    if (executionResult.ServerException.InnerException != null)
                    {
                        ServerConfiguration.ErrorsLogsStream?.WriteLine(executionResult.ServerException.InnerException);
                    }
                    _errorLogMutex.ReleaseMutex();
                }

                if (ServerConfiguration.AccessLogsStream != null && canAccessLog)
                {
                    _accessLogMutex.WaitOne();
                    verbosePrefix = verbosePrefix.Replace("%%STATUS%%", executionResult.Status.ToString());
                    if (!string.IsNullOrEmpty(verboseSuffix))
                    {
                        ServerConfiguration.AccessLogsStream.WriteLine($"{verbosePrefix} {verboseSuffix} after {sw.ElapsedMilliseconds}ms");
                    }
                    else
                    {
                        ServerConfiguration.AccessLogsStream.WriteLine($"{verbosePrefix}");
                    }
                    _accessLogMutex.ReleaseMutex();
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
