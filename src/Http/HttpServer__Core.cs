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
    const long UnitKb = 1024;
    const long UnitMb = UnitKb * 1024;
    const long UnitGb = UnitMb * 1024;
    const long UnitTb = UnitGb * 1024;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string HumanReadableSize(float? size)
    {
        if (size < UnitKb)
        {
            return $"{size:n2} bytes";
        }
        else if (size > UnitKb && size <= UnitMb)
        {
            return $"{size / UnitKb:n2} kb";
        }
        else if (size > UnitMb && size <= UnitGb)
        {
            return $"{size / UnitMb:n2} mb";
        }
        else if (size > UnitGb && size <= UnitTb)
        {
            return $"{size / UnitGb:n2} gb";
        }
        else// if (size > UnitTb)
        {
            return $"{size / UnitTb:n2} tb";
        }
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

    private void BindRouters()
    {
        foreach (var lh in ServerConfiguration.ListeningHosts)
        {
            lh.Router?.BindServer(this);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private async void ListenerCallback(IAsyncResult result)
    {
        if (_isDisposing || !_isListening)
            return;

        var listener = (HttpListener)result.AsyncState!;
        listener.BeginGetContext(_listenerCallback, listener);

        HttpListenerContext context = listener.EndGetContext(result);
        await ProcessRequest(context);
    }

    private async Task ProcessRequest(HttpListenerContext context)
    {
        HttpRequest request = null!;
        HttpResponse? response = null;
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
        Router.RouterExecutionResult? routerResult = null;

        if (ServerConfiguration.DefaultCultureInfo != null)
        {
            Thread.CurrentThread.CurrentCulture = ServerConfiguration.DefaultCultureInfo;
            Thread.CurrentThread.CurrentUICulture = ServerConfiguration.DefaultCultureInfo;
        }

        HttpServerExecutionResult executionResult = new HttpServerExecutionResult()
        {
            Request = request,
            Response = response,
            Status = HttpServerExecutionStatus.NoResponse
        };

        // 38480

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
            ListeningHost? matchedListeningHost = _onlyListeningHost ?? ServerConfiguration.ListeningHosts
                    .GetRequestMatchingListeningHost(dnsSafeHost, baseRequest.LocalEndPoint.Port);

            if (matchedListeningHost is null)
            {
                baseResponse.StatusCode = 400; // Bad Request
                executionResult.Status = HttpServerExecutionStatus.DnsUnknownHost;
                return;
            }

            request = new HttpRequest(this, matchedListeningHost, context);
            srContext = new HttpContext(this, request, null, matchedListeningHost);

            request.Context = srContext;
            executionResult.Request = request;

            if (ServerConfiguration.ResolveForwardedOriginAddress)
            {
                otherParty = request.Origin;
            }

            if (matchedListeningHost.Router == null || !matchedListeningHost.CanListen)
            {
                baseResponse.StatusCode = 503; // Service Unavailable
                executionResult.Status = HttpServerExecutionStatus.ListeningHostNotReady;
                return;
            }

            #endregion // 27668

            #region Step 2 - Request validation

            if (ServerConfiguration.IncludeRequestIdHeader)
                baseResponse.Headers.Set(flag.HeaderNameRequestId, request.RequestId.ToString());
            if (flag.SendSiskHeader)
                baseResponse.Headers.Set("X-Powered-By", poweredByHeader);

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

            if (OnConnectionOpen != null)
                OnConnectionOpen(this, request);

            handler.HttpRequestOpen(request);

            #endregion

            #region Step 3 - Routing and action

            // get response
            var timeout = flag.RouteActionTimeout;
            if (timeout.Ticks > 0)
            {
                var routerTask = matchedListeningHost.Router.Execute(srContext);
                if (await Task.WhenAny(routerTask, Task.Delay(timeout)) == routerTask)
                {
                    routerResult = routerTask.Result;
                }
                else
                {
                    throw new RequestTimeoutException();
                }
            }
            else
            {
                routerResult = await matchedListeningHost.Router.Execute(srContext);
            }

            response = routerResult.Response;
            useCors = routerResult.Route?.UseCors ?? true;

            if (useCors)
            {
                SetCorsHeaders(flag, baseRequest, matchedListeningHost.CrossOriginResourceSharingPolicy, baseResponse);
            }

            if (response is null || response.internalStatus == HttpResponse.HTTPRESPONSE_EMPTY)
            {
                baseResponse.StatusCode = 204;
                executionResult.Status = HttpServerExecutionStatus.NoResponse;
                goto finishSending;
            }
            else if (response.internalStatus == HttpResponse.HTTPRESPONSE_ERROR)
            {
                executionResult.Status = HttpServerExecutionStatus.UncaughtExceptionThrown;
                baseResponse.StatusCode = 500;
                if (routerResult.Exception != null)
                    throw routerResult.Exception;
                goto finishSending;
            }
            else if (response.internalStatus == HttpResponse.HTTPRESPONSE_CLIENT_CLOSE ||
                     response.internalStatus == HttpResponse.HTTPRESPONSE_SERVER_CLOSE)
            {
                executionResult.Status = HttpServerExecutionStatus.ConnectionClosed;
                baseResponse.StatusCode = (int)response.Status;
                goto finishSending;
            }
            else if (response.internalStatus == HttpResponse.HTTPRESPONSE_SERVER_REFUSE)
            {
                executionResult.Status = HttpServerExecutionStatus.ConnectionClosed;
                baseResponse.Abort();
                goto finishSending;
            }

            if (response.CustomStatus != null)
            {
                baseResponse.StatusCode = response.CustomStatus.Value.StatusCode;
                baseResponse.StatusDescription = response.CustomStatus.Value.Description;
            }
            else
            {
                baseResponse.StatusCode = (int)response.Status;
            }

            baseResponse.KeepAlive = ServerConfiguration.KeepAlive;

            #endregion

            #region Step 4 - Response computing
            NameValueCollection resHeaders = new NameValueCollection
            {
                response.Headers
            };

            long? responseContentLength = response.Content?.Headers.ContentLength;
            if (srContext?.OverrideHeaders.Count > 0) resHeaders.Add(srContext.OverrideHeaders);

            foreach (string incameHeader in resHeaders)
            {
                string? value = resHeaders[incameHeader];
                if (string.IsNullOrEmpty(value)) continue;

                baseResponse.AddHeader(incameHeader, value);
            }

            if (response.Content != null && responseContentLength > 0)
            {
                // determines the content type
                baseResponse.ContentType = resHeaders["Content-Type"] ?? response.Content.Headers.ContentType?.ToString();

                // determines if the response should be sent as chunked or normal
                if (response.SendChunked)
                {
                    baseResponse.SendChunked = true;
                }
                else
                {
                    baseResponse.ContentLength64 = responseContentLength.Value;
                }

                // write the output buffer
                if (context.Request.HttpMethod != "HEAD")
                {
                    outcomingSize += responseContentLength.Value;

                    bool isPayloadStreamable =
                        response.Content is StreamContent ||
                        responseContentLength > 1024;

                    if (isPayloadStreamable)
                    {
                        Stream contentStream = await response.Content.ReadAsStreamAsync();
                        contentStream.CopyTo(baseResponse.OutputStream);
                    }
                    else
                    {
                        byte[] contents = await response.Content.ReadAsByteArrayAsync();
                        baseResponse.OutputStream.Write(contents);
                    }
                }
            }

        #endregion

        finishSending:
            #region Step 5 - Close streams and send response

            if (response?.CalculedLength >= 0)
            {
                executionResult.ResponseSize = response.CalculedLength;
            }
            else
            {
                executionResult.ResponseSize = outcomingSize;
            }

            executionResult.RequestSize = incomingSize;
            executionResult.Response = response;

            if (executionResult.Status == HttpServerExecutionStatus.NoResponse && response?.internalStatus != HttpResponse.HTTPRESPONSE_EMPTY)
                executionResult.Status = HttpServerExecutionStatus.Executed;

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
        catch (RequestTimeoutException netException)
        {
            baseResponse.StatusCode = 408;
            executionResult.Status = HttpServerExecutionStatus.RequestTimeout;
            executionResult.ServerException = netException;
            hasErrorLogging = false;
        }
        catch (HttpRequestException requestException)
        {
            baseResponse.StatusCode = 400;
            executionResult.Status = HttpServerExecutionStatus.MalformedRequest;
            executionResult.ServerException = requestException;
            hasErrorLogging = false;
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
            sw.Stop();

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
                OnConnectionClose(this, executionResult);
            }

            handler.HttpRequestClose(executionResult);

            if (executionResult.ServerException != null)
                handler.Exception(executionResult.ServerException);

            LogOutput logMode;

            #region Logging
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
                    request.Headers,
                    baseResponse.StatusCode,
                    baseResponse.StatusDescription,
                    sw.ElapsedMilliseconds,
                    baseRequest.HttpMethod);

                string line = ServerConfiguration.AccessLogsFormat;
                formatter.Format(ref line);

                ServerConfiguration.AccessLogsStream?.WriteLine(line);
            }
            #endregion
        }
    }
}
