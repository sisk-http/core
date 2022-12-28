using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

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
            if (httpListener.Prefixes.Count == 0)
            {
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
            }

            foreach (string prefix in listeningPrefixes)
                httpListener.Prefixes.Add(prefix);

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

        private bool HostMatchWildcard(string listeningHostPattern, string dns)
        {
            string lhostPatternRpl = listeningHostPattern
                .Replace(".", @"(\.)?")
                .Replace("*", @"(.*)?");
            return Regex.IsMatch(dns, $@"^{lhostPatternRpl}$");
        }

        private void ListenerCallback(IAsyncResult result)
        {
            if (!_isDisposing && httpListener.IsListening)
            {
                httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), httpListener);

                HttpListenerContext context;
                try
                {
                    context = httpListener.EndGetContext(result);
                }
                catch (Exception)
                {
                    // Requested to shut down the server while processing the request.
                    return;
                }

                Stopwatch sw = new Stopwatch();
                HttpListenerResponse baseResponse = context.Response;
                HttpListenerRequest baseRequest = context.Request;
                string verbosePrefix = "";
                string verboseSuffix = "";

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
                        verbosePrefix = $"&e{context.Request.HttpMethod,8} &8(&7{baseRequest.Url.Authority}&8) &f{context.Request.Url?.AbsolutePath ?? " / "}";
                    }
                    else if (ServerConfiguration.Verbose == VerboseMode.Detailed)
                    {
                        verbosePrefix = $"&e{context.Request.HttpMethod,8} &7{DateTime.Now:G} %%STATUS%% &e{request.Origin} &8(&7{baseRequest.Url?.Scheme} &e{baseRequest.Url!.Authority}&8) &f{context.Request.Url?.AbsolutePath ?? "/"}";
                    }

                    // detect the listening host for this listener
                    ListeningHost? matchedHost = ServerConfiguration.ListeningHosts?.Where(
                        lh => HostMatchWildcard(lh.Hostname, dnsSafeHost) && lh._numericPorts.Contains(baseRequest.LocalEndPoint.Port)).FirstOrDefault();

                    if (matchedHost is null)
                    {
                        baseResponse.StatusCode = 400;
                        executionResult.Status = HttpServerExecutionStatus.DnsUnknownHost;
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
                        string corsAlHd = matchedHost.CrossOriginResourceSharingPolicy.GetAllowHeadersHeader();
                        string corsAlMt = matchedHost.CrossOriginResourceSharingPolicy.GetAllowMethodsHeader();
                        string corsAlOr = matchedHost.CrossOriginResourceSharingPolicy.GetAllowOriginsHeader();
                        string corsMxAg = matchedHost.CrossOriginResourceSharingPolicy.GetMaxAgeHeader();

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
                    matchedHost.Router.ParentServer = this;
                    matchedHost.Router.ParentListenerHost = matchedHost;
                    response = matchedHost.Router.Execute(request);

                    if (response?.isEmpty ?? false)
                    {
                        if (request.isServingEventSourceEvents)
                        {
                            executionResult.Status = HttpServerExecutionStatus.EventSourceClosed;
                        }
                        else
                        {
                            executionResult.Status = HttpServerExecutionStatus.NoResponse;
                        }

                        baseResponse.StatusCode = 200;
                        return;
                    }

                    if (response is null)
                    {
                        // when the application returned a empty response, the
                        // socket terminates immediately
                        executionResult.Status = HttpServerExecutionStatus.NoResponse;
                        baseResponse.StatusCode = 510;
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
                    }

                    executionResult.Status = HttpServerExecutionStatus.Executed;
                    verboseSuffix = $"&8[&b{(int)response.Status} &3{response.Status}&8] &7{HumanReadableSize((int)context.Request.ContentLength64) + " &8-> &7" + HumanReadableSize(responseBytes.Length)}";

                    sw.Stop();
                    baseResponse.Close();
                }
                catch (Exception ex)
                {
                    if (ServerConfiguration.ThrowExceptions)
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
                    if (OnConnectionClose != null)
                    {
                        OnConnectionClose(this, executionResult);
                    }

                    bool canWriteColor = ServerConfiguration.EnableVerboseColors;

                    if (ServerConfiguration.Verbose == VerboseMode.Normal)
                    {
                        if (!string.IsNullOrEmpty(verbosePrefix))
                        {
                            if (verboseSuffix == "")
                            {
                                Util.WriteColorfulLine($"{verbosePrefix} -> {executionResult.Status}", canWriteColor);
                            }
                            else
                            {
                                Util.WriteColorfulLine($"{verbosePrefix} {verboseSuffix}", canWriteColor);
                            }
                        }
                    }
                    else if (ServerConfiguration.Verbose == VerboseMode.Detailed)
                    {
                        if (!string.IsNullOrEmpty(verbosePrefix))
                        {
                            string statusStr = executionResult.Status.ToString();
                            if (executionResult.IsSuccessStatus)
                            {
                                statusStr = "&b" + statusStr;
                            }
                            else
                            {
                                statusStr = "&c" + statusStr;
                            }
                            verbosePrefix = verbosePrefix.Replace("%%STATUS%%", statusStr);
                            Util.WriteColorfulLine($"{verbosePrefix} {verboseSuffix} &8after &7{sw.ElapsedMilliseconds}ms", canWriteColor);
                        }
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
    }
}
