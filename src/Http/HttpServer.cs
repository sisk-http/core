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
        private bool _isListening = false;
        private bool _isDisposing = false;

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

        private HttpListener httpListener = new HttpListener();
        private string poweredByHeader = "";

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
            Version assVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
            poweredByHeader = $"Sisk/{assVersion.Major}.{assVersion.Minor}";

            if (this.ServerConfiguration.ListeningHosts is null)
            {
                throw new InvalidOperationException("Cannot start the HTTP server with no listening hosts.");
            }

            List<string> listeningPrefixes = new List<string>();
            foreach (ListeningHost listeningHost in this.ServerConfiguration.ListeningHosts)
            {
                // dns checking is made on the server callback
                foreach (ListeningPort port in listeningHost.Ports)
                {
                    string prefix;

                    if (port.Secure)
                    {
                        prefix = $"https://+:{port.Port}/";
                    }
                    else
                    {
                        prefix = $"http://+:{port.Port}/";
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

        private string HumanReadableSize(int size)
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

        private void ListenerCallback(IAsyncResult result)
        {
            if (_isDisposing || !_isListening)
                return;

            httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), httpListener);
            HttpListenerContext context;

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

            HttpRequest request = new HttpRequest(ref baseRequest, ref baseResponse, this.ServerConfiguration);
            HttpResponse? response = null;
            HttpServerExecutionResult? executionResult = new HttpServerExecutionResult()
            {
                Request = request,
                Response = response,
                Status = HttpServerExecutionStatus.NoResponse
            };

            try
            {
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

                if (ServerConfiguration.Verbose == VerboseMode.Normal)
                {
                    verbosePrefix = $"{context.Request.HttpMethod,8} ({baseRequest.Url.Authority}) {context.Request.Url?.AbsolutePath ?? " / "}";
                }
                else if (ServerConfiguration.Verbose == VerboseMode.Detailed)
                {
                    verbosePrefix = $"{context.Request.HttpMethod,8} {DateTime.Now:G} %%STATUS%% {request.Origin} ({baseRequest.Url?.Scheme} {baseRequest.Url!.Authority}) {context.Request.Url?.AbsolutePath ?? "/"}";
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

                if (matchedListeningHost.Router == null || !matchedListeningHost.CanListen)
                {
                    baseResponse.StatusCode = 503; // Service Unavailable
                    executionResult.Status = HttpServerExecutionStatus.ListeningHostNotReady;
                    return;
                }

                baseResponse.Headers.Set("Server", poweredByHeader);
                baseResponse.Headers.Set("X-Powered-By", poweredByHeader);
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

                {
                    string corsAlHd = matchedListeningHost.CrossOriginResourceSharingPolicy.GetAllowHeadersHeader();
                    string corsAlMt = matchedListeningHost.CrossOriginResourceSharingPolicy.GetAllowMethodsHeader();
                    string corsAlOr = matchedListeningHost.CrossOriginResourceSharingPolicy.GetAllowOriginsHeader();
                    string corsMxAg = matchedListeningHost.CrossOriginResourceSharingPolicy.GetMaxAgeHeader();

                    if (!string.IsNullOrEmpty(corsAlHd))
                        baseResponse.Headers.Set("Access-Control-Allow-Headers", corsAlHd);
                    if (!string.IsNullOrEmpty(corsAlMt))
                        baseResponse.Headers.Set("Access-Control-Allow-Methods", corsAlMt);
                    if (!string.IsNullOrEmpty(corsAlOr))
                        baseResponse.Headers.Set("Access-Control-Allow-Origin", corsAlOr);
                    if (!string.IsNullOrEmpty(corsMxAg) && corsMxAg != "0")
                        baseResponse.Headers.Set("Access-Control-Allow-Max-Age", corsMxAg);
                }

                // get response
                matchedListeningHost.Router.ParentServer = this;
                matchedListeningHost.Router.ParentListenerHost = matchedListeningHost;
                response = matchedListeningHost.Router.Execute(request);

                if (response is null)
                {
                    executionResult.Status = HttpServerExecutionStatus.NoResponse;
                    return;
                }
                else if (response?.internalStatus == HttpResponse.HTTPRESPONSE_EVENTSOURCE_CLOSE)
                {
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

                executionResult.Status = HttpServerExecutionStatus.Executed;
                verboseSuffix = $"[{httpStatusVerbose}] {HumanReadableSize((int)incomingSize) + " -> " + HumanReadableSize((int)outcomingSize)}";

                executionResult.RequestSize = incomingSize;
                executionResult.ResponseSize = outcomingSize;
                executionResult.Response = response;

                sw.Stop();
                baseResponse.Close();
                closeStream = false;
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
                    baseResponse.Close();
                }

                if (OnConnectionClose != null)
                {
                    OnConnectionClose(this, executionResult);
                }

                if (ServerConfiguration.Verbose == VerboseMode.Normal)
                {
                    if (!string.IsNullOrEmpty(verbosePrefix))
                    {
                        if (verboseSuffix == "")
                        {
                            Console.WriteLine($"{verbosePrefix} -> {executionResult.Status}");
                        }
                        else
                        {
                            Console.WriteLine($"{verbosePrefix} {verboseSuffix}");
                        }
                    }
                }
                else if (ServerConfiguration.Verbose == VerboseMode.Detailed)
                {
                    if (!string.IsNullOrEmpty(verbosePrefix))
                    {
                        verbosePrefix = verbosePrefix.Replace("%%STATUS%%", executionResult.Status.ToString());
                        Console.WriteLine($"{verbosePrefix} {verboseSuffix} after {sw.ElapsedMilliseconds}ms");
                    }
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
