using Sisk.Core.Entity;
using Sisk.Core.Routing;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Net;
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
        private static TimeSpan currentTimezoneDiff = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);

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

        private static string HumanReadableSize(float? size)
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

        internal static string FormatAccessLog(
            [In] string format,
            [In] HttpServerExecutionResult res,
            [In] DateTime d,
            [In] Uri? bReqUri,
            [In] IPAddress bReqIpAddr,
            [In] NameValueCollection? reqHeaders,
            [In] int bResStatusCode,
            [In] string bResStatusDescr,
            [In] float? incomingSize,
            [In] float? outcomingSize,
            [In] long execTime)
        {
            void replaceEntity(ref string format, string piece, Func<string?> result)
            {
                if (format.Contains(piece))
                {
                    string? repl = result();
                    format = format.Replace(piece, repl);
                }
            }

            void replaceHeaders(ref string format)
            {
                int pos = 0;
                while ((pos = format.IndexOf("%{")) > 0)
                {
                    int end = format.IndexOf('}');
                    string headerName = format.Substring(pos + 2, end - pos - 2);
                    string? headerValue = reqHeaders?[headerName];
                    format = format.Replace($"%{{{headerName}}}", headerValue);
                }
            }

            Dictionary<string, Func<string?>> staticReplacements = new Dictionary<string, Func<string?>>()
            {
                { "%dd", () => $"{d.Day:D2}" },
                { "%dmmm", () => $"{d:MMMM}" },
                { "%dmm", () => $"{d:MMM}" },
                { "%dm", () => $"{d.Month:D2}" },
                { "%dy", () => $"{d.Year:D4}" },
                { "%th", () => $"{d:hh}" },
                { "%tH", () => $"{d:HH}" },
                { "%ti", () => $"{d.Minute:D2}" },
                { "%ts", () => $"{d.Second:D2}" },
                { "%tm", () => $"{d.Millisecond:D3}" },// 
                { "%tz", () => $"{currentTimezoneDiff.TotalHours:00}00" },
                { "%ri", () => bReqIpAddr.ToString() },
                { "%rs", () => bReqUri?.Scheme },
                { "%ra", () => bReqUri?.Authority },
                { "%rh", () => bReqUri?.Host },
                { "%rp", () => bReqUri?.Port.ToString() },
                { "%rz", () => bReqUri?.AbsolutePath ?? "/" },
                { "%rq", () => bReqUri?.Query },
                { "%sc", () => bResStatusCode.ToString() },
                { "%sd", () => bResStatusDescr },
                { "%lin", () => HumanReadableSize(incomingSize) },
                { "%lou", () => HumanReadableSize(outcomingSize) },
                { "%lms", () => execTime.ToString() },
                { "%ls", () => res.Status.ToString() }
            };

            foreach (var k in staticReplacements)
            {
                replaceEntity(ref format, k.Key, k.Value);
            }

            replaceHeaders(ref format);

            return format;
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
                    request = new HttpRequest(baseRequest, baseResponse, this.ServerConfiguration, matchedListeningHost);
                    reqHeaders = new NameValueCollection(baseRequest.Headers);
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

                // get response
                matchedListeningHost.Router.ParentServer = this;
                matchedListeningHost.Router.ParentListenerHost = matchedListeningHost;

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

                    baseResponse.ContentLength64 = responseBytes.Length;
                    if (context.Request.HttpMethod != "HEAD")
                    {
                        baseResponse.OutputStream.Write(responseBytes);
                        outcomingSize += responseBytes.Length;
                    }
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

                bool canAccessLog = logMode.HasFlag(LogOutput.AccessLog) && hasAccessLogging;
                bool canErrorLog = logMode.HasFlag(LogOutput.ErrorLog) && hasAccessLogging;

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
                    string line = FormatAccessLog(ServerConfiguration.AccessLogsFormat,
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
                    ServerConfiguration.AccessLogsStream?.WriteLine(line);
                }

                /*if (executionResult.ServerException != null && canErrorLog)
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
                }*/
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
