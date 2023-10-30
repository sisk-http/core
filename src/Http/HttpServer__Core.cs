// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServer__Core.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using Sisk.Core.Internal;
using Sisk.Core.Routing;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sisk.Core.Http;

public partial class HttpServer
{
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

    internal static void SetCorsHeaders(HttpServerFlags serverFlags, HttpListenerRequest baseRequest, CrossOriginResourceSharingHeaders cors, HttpListenerResponse baseResponse)
    {
        if (!serverFlags.SendCorsHeaders) return;
        if (cors.AllowHeaders.Length > 0) baseResponse.Headers.Set("Access-Control-Allow-Headers", string.Join(", ", cors.AllowHeaders));
        if (cors.AllowMethods.Length > 0) baseResponse.Headers.Set("Access-Control-Allow-Methods", string.Join(", ", cors.AllowMethods));
        if (cors.AllowOrigin != null) baseResponse.Headers.Set("Access-Control-Allow-Origin", cors.AllowOrigin);
        if (cors.AllowOrigins?.Length > 0)
        {
            string? origin = baseRequest.Headers["Origin"];
            if (origin != null)
            {
                foreach (var definedOrigin in cors.AllowOrigins)
                {
                    if (string.Compare(definedOrigin, origin, true) == 0)
                    {
                        baseResponse.Headers.Set("Access-Control-Allow-Origin", origin);
                        break;
                    }
                }
            }
        }
        if (cors.AllowCredentials != null) baseResponse.Headers.Set("Access-Control-Allow-Credentials", cors.AllowCredentials.ToString()!.ToLower());
        if (cors.ExposeHeaders.Length > 0) baseResponse.Headers.Set("Access-Control-Expose-Headers", string.Join(", ", cors.ExposeHeaders));
        if (cors.MaxAge.TotalSeconds > 0) baseResponse.Headers.Set("Access-Control-Max-Age", cors.MaxAge.TotalSeconds.ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void ListenerCallback(IAsyncResult result)
    {
        #region Init context variables
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
        HttpContext? srContext = null;
        long incomingSize = 0;
        long outcomingSize = 0;
        bool closeStream = true;
        bool useCors = false;
        bool hasAccessLogging = ServerConfiguration.AccessLogsStream != null;
        bool hasErrorLogging = ServerConfiguration.ErrorsLogsStream != null;
        IPAddress otherParty = baseRequest.RemoteEndPoint.Address;
        Uri? connectingUri = baseRequest.Url;
        int responseStatus = 0;
        string responseDescription = "";
        NameValueCollection? reqHeaders = null;
        Router.RouterExecutionResult? routerResult = null;

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

        #endregion

        try
        {
            sw.Start();

            #region Step 1 - DNS/Listening host matching

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

            #endregion

            #region Step 2 - Request validation

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

            // aditional before-router flags
            if (flag.SendSiskHeader)
                baseResponse.Headers.Set("X-Powered-By", poweredByHeader);

            #endregion

            #region Step 3 - Routing and callback

            // get response
            routerResult = matchedListeningHost.Router.Execute(request, baseRequest, matchedListeningHost, ref srContext);
            response = routerResult.Response;
            useCors = routerResult.Route?.UseCors ?? true;

            if (useCors)
            {
                SetCorsHeaders(flag, baseRequest, matchedListeningHost.CrossOriginResourceSharingPolicy, baseResponse);
            }

            if (response is null || response?.internalStatus == HttpResponse.HTTPRESPONSE_EMPTY)
            {
                baseResponse.StatusCode = 204;
                executionResult.Status = HttpServerExecutionStatus.NoResponse;
                goto finishSending;
            }
            else if (response?.internalStatus == HttpResponse.HTTPRESPONSE_ERROR)
            {
                executionResult.Status = HttpServerExecutionStatus.UncaughtExceptionThrown;
                baseResponse.StatusCode = 500;
                if (routerResult.Exception != null)
                    throw routerResult.Exception;
                goto finishSending;
            }
            else if (response?.internalStatus == HttpResponse.HTTPRESPONSE_CLIENT_CLOSE ||
                     response?.internalStatus == HttpResponse.HTTPRESPONSE_SERVER_CLOSE)
            {
                executionResult.Status = HttpServerExecutionStatus.ConnectionClosed;
                baseResponse.StatusCode = (int)response.Status;
                goto finishSending;
            }
            else if (response?.internalStatus == HttpResponse.HTTPRESPONSE_SERVER_REFUSE)
            {
                executionResult.Status = HttpServerExecutionStatus.ConnectionClosed;
                baseResponse.Abort();
                goto finishSending;
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

            baseResponse.KeepAlive = ServerConfiguration.KeepAlive;
            baseResponse.SendChunked = response.SendChunked;

            #endregion

            #region Step 4 - Response computing

            NameValueCollection resHeaders = new NameValueCollection
            {
                response.Headers
            };

            if (srContext?.OverrideHeaders.Count > 0) resHeaders.Add(srContext.OverrideHeaders);

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
                    baseResponse.ContentLength64 = responseBytes.Length;

                if (context.Request.HttpMethod != "HEAD")
                {
                    baseResponse.OutputStream.Write(responseBytes);
                    outcomingSize += responseBytes.Length;
                }
            }

        #endregion

        finishSending:
            #region Step 5 - Close streams and send response

            string httpStatusVerbose = $"{baseResponse.StatusCode} {baseResponse.StatusDescription}";

            executionResult.RequestSize = incomingSize;
            executionResult.ResponseSize = outcomingSize;
            executionResult.Response = response;

            if (executionResult.Status == HttpServerExecutionStatus.NoResponse && response?.internalStatus != HttpResponse.HTTPRESPONSE_EMPTY)
                executionResult.Status = HttpServerExecutionStatus.Executed;

            sw.Stop();
            baseResponse.Close();
            baseRequest.InputStream.Close();

            closeStream = false;
            #endregion
        }
        catch (ObjectDisposedException objException)
        {
            executionResult.Status = HttpServerExecutionStatus.ExceptionThrown;
            executionResult.ServerException = objException;
            hasErrorLogging = false;
        }
        catch (HttpListenerException netException)
        {
            executionResult.Status = HttpServerExecutionStatus.ConnectionClosed;
            executionResult.ServerException = netException;
            hasErrorLogging = false;
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
                // Close() would throw an exception if the sent payload length is greater than
                // content length, so it will force close the connection on the Abort() method
                try
                {
                    baseResponse.Close();
                }
                catch (Exception)
                {
                    baseResponse.Abort();
                }
            }

            if (OnConnectionClose != null)
            {
                executionResult.Request = request;
                OnConnectionClose(this, executionResult);
            }

            LogOutput logMode;

            if (routerResult?.Result == RouteMatchResult.OptionsMatched)
            {
                logMode = flag.OptionsLogMode;
            }
            else
            {
                logMode = srContext?.MatchedRoute?.LogMode ?? LogOutput.Both;
            }

            bool canAccessLog = logMode.HasFlag(LogOutput.AccessLog) && hasAccessLogging;
            bool canErrorLog = logMode.HasFlag(LogOutput.ErrorLog) && hasErrorLogging;

            if (executionResult.ServerException != null && canErrorLog)
            {
                ServerConfiguration.ErrorsLogsStream?.WriteException(executionResult.ServerException);
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
                    sw.ElapsedMilliseconds,
                    baseRequest.HttpMethod);

                string line = ServerConfiguration.AccessLogsFormat;
                formatter.Format(ref line);

                ServerConfiguration.AccessLogsStream?.WriteLine(line);
            }
        }
    }
}
