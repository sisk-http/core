// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServer__Core.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using Sisk.Core.Internal;
using Sisk.Core.Routing;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace Sisk.Core.Http;

public partial class HttpServer
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void ApplyHttpContentHeaders(HttpListenerResponse response, HttpContentHeaders contentHeaders)
    {
        // content-length is applied outside this method
        // do not include that here

        if (contentHeaders.ContentType?.ToString() is { } ContentType
            && response.Headers.GetValues(HttpKnownHeaderNames.ContentType)?.Length is 0 or null)
            response.ContentType = ContentType;

        if (contentHeaders.ContentRange?.ToString() is { } ContentRange
            && response.Headers.GetValues(HttpKnownHeaderNames.ContentRange)?.Length is 0 or null)
            response.AppendHeader(HttpKnownHeaderNames.ContentRange, ContentRange);

        if (contentHeaders.ContentMD5 is { } ContentMD5
            && response.Headers.GetValues(HttpKnownHeaderNames.ContentMD5)?.Length is 0 or null) // rfc1864#section-2
            response.AppendHeader(HttpKnownHeaderNames.ContentMD5, Convert.ToBase64String(ContentMD5));

        if (contentHeaders.ContentLocation?.ToString() is { } ContentLocation
            && response.Headers.GetValues(HttpKnownHeaderNames.ContentLocation)?.Length is 0 or null)
            response.AppendHeader(HttpKnownHeaderNames.ContentLocation, ContentLocation);

        if (contentHeaders.ContentDisposition?.ToString() is { } ContentDisposition
            && response.Headers.GetValues(HttpKnownHeaderNames.ContentDisposition)?.Length is 0 or null)
            response.AppendHeader(HttpKnownHeaderNames.ContentDisposition, ContentDisposition);

        if (contentHeaders.LastModified is { } LastModified
            && response.Headers.GetValues(HttpKnownHeaderNames.LastModified)?.Length is 0 or null)
            response.AppendHeader(HttpKnownHeaderNames.LastModified, LastModified.ToUniversalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss 'GMT'"));

        if (contentHeaders.Expires is { } Expires
            && response.Headers.GetValues(HttpKnownHeaderNames.Expires)?.Length is 0 or null)
            response.AppendHeader(HttpKnownHeaderNames.Expires, Expires.ToUniversalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss 'GMT'"));

        if (contentHeaders.ContentLanguage.Count > 0
            && response.Headers.GetValues(HttpKnownHeaderNames.ContentLanguage)?.Length is 0 or null)
            response.AppendHeader(HttpKnownHeaderNames.ContentLanguage, string.Join(", ", contentHeaders.ContentLanguage));

        if (contentHeaders.ContentEncoding.Count > 0
            && response.Headers.GetValues(HttpKnownHeaderNames.ContentEncoding)?.Length is 0 or null)
            response.AppendHeader(HttpKnownHeaderNames.ContentEncoding, string.Join(", ", contentHeaders.ContentEncoding));
    }

    internal static void SetCorsHeaders(HttpListenerRequest baseRequest, CrossOriginResourceSharingHeaders? cors, HttpListenerResponse baseResponse)
    {
        if (cors is null)
            return;

        if (cors.AllowHeaders.Length > 0)
            baseResponse.Headers.Set(HttpKnownHeaderNames.AccessControlAllowHeaders, string.Join(", ", cors.AllowHeaders));

        if (cors.AllowMethods.Length > 0)
            baseResponse.Headers.Set(HttpKnownHeaderNames.AccessControlAllowMethods, string.Join(", ", cors.AllowMethods));

        if (cors.AllowOrigins?.Length > 0)
        {
            string? origin = baseRequest.Headers[HttpKnownHeaderNames.Origin];

            if (origin is not null)
            {
                for (int i = 0; i < cors.AllowOrigins.Length; i++)
                {
                    string definedOrigin = cors.AllowOrigins[i];
                    if (string.Compare(definedOrigin, origin, true) == 0)
                    {
                        baseResponse.Headers.Set(HttpKnownHeaderNames.AccessControlAllowOrigin, origin);
                        break;
                    }
                }
            }
        }
        else if (cors.AllowOrigin is not null)
        {
            baseResponse.Headers.Set(HttpKnownHeaderNames.AccessControlAllowOrigin, cors.AllowOrigin);
        }

        if (cors.AllowCredentials == true)
            baseResponse.Headers.Set(HttpKnownHeaderNames.AccessControlAllowCredentials, "true");

        if (cors.ExposeHeaders.Length > 0)
            baseResponse.Headers.Set(HttpKnownHeaderNames.AccessControlExposeHeaders, string.Join(", ", cors.ExposeHeaders));

        if (cors.MaxAge > TimeSpan.Zero)
            baseResponse.Headers.Set(HttpKnownHeaderNames.AccessControlMaxAge, cors.MaxAge.TotalSeconds.ToString());
    }

    private void UnbindRouters()
    {
        for (int i = 0; i < this.ServerConfiguration.ListeningHosts.Count; i++)
        {
            var lh = this.ServerConfiguration.ListeningHosts[i];
            if (lh.Router is { } router && ReferenceEquals(this, router.parentServer))
            {
                router.FreeHttpServer();
            }
        }
    }

    private void BindRouters()
    {
        for (int i = 0; i < this.ServerConfiguration.ListeningHosts.Count; i++)
        {
            var lh = this.ServerConfiguration.ListeningHosts[i];
            if (lh.Router is { } router && router.parentServer is null)
            {
                router.BindServer(this);
            }
        }
    }

    private void ListenerCallback(IAsyncResult result)
    {
        if (this._isDisposing || !this._isListening)
            return;

        this.httpListener.BeginGetContext(this.ListenerCallback, null);
        HttpListenerContext context = this.httpListener.EndGetContext(result);

        this.ProcessRequest(context);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void ProcessRequest(HttpListenerContext context)
    {
        HttpRequest request = null!;
        HttpResponse? response = null;
        HttpServerFlags flag = this.ServerConfiguration.Flags;
        Stopwatch sw = Stopwatch.StartNew();
        HttpListenerResponse baseResponse = context.Response;
        HttpListenerRequest baseRequest = context.Request;
        HttpContext? srContext = new HttpContext(this);
        bool closeStream = true;

        HttpContext._context.Value = srContext;

        bool hasAccessLogging = this.ServerConfiguration.AccessLogsStream is not null;
        bool hasErrorLogging = this.ServerConfiguration.ErrorsLogsStream is not null;

        Router.RouterExecutionResult? routerResult = null;

        if (this.ServerConfiguration.DefaultCultureInfo is not null)
        {
            Thread.CurrentThread.CurrentCulture = this.ServerConfiguration.DefaultCultureInfo;
            Thread.CurrentThread.CurrentUICulture = this.ServerConfiguration.DefaultCultureInfo;
        }

        HttpServerExecutionResult executionResult = new HttpServerExecutionResult()
        {
            Request = request,
            Response = response,
            Context = srContext,
            Status = HttpServerExecutionStatus.NoResponse
        };

        // 38480

        try
        {
            if (!flag.AsyncRequestProcessing)
            {
                Monitor.Enter(this.httpListener);
            }

            #region Step 1 - DNS/Listening host matching

            // context initialization
            request = new HttpRequest(this, context);

            srContext.Request = request;
            request.Context = srContext;

            executionResult.Request = request;
            executionResult.Context = srContext;

            if (baseRequest.Url is null)
            {
                baseResponse.StatusCode = 400;
                executionResult.Status = HttpServerExecutionStatus.DnsFailed;
                return;
            }

            if (baseRequest.IsLocal == false && this.ServerConfiguration.RemoteRequestsAction == RequestListenAction.Drop)
            {
                executionResult.Status = HttpServerExecutionStatus.RemoteRequestDropped;
                baseResponse.Abort();
                return;
            }

            string dnsSafeHost = baseRequest.Url.Host;
            if (this.ServerConfiguration.ForwardingResolver is ForwardingResolver fr)
            {
                dnsSafeHost = fr.OnResolveRequestHost(request, dnsSafeHost);
            }

            // detect the listening host for this listener
            ListeningHost? matchedListeningHost = this._onlyListeningHost
                ?? this.ServerConfiguration.ListeningHosts.GetRequestMatchingListeningHost(dnsSafeHost, baseRequest.Url!.AbsolutePath, baseRequest.LocalEndPoint.Port);

            if (matchedListeningHost is null)
            {
                baseResponse.StatusCode = 400; // Bad Request
                executionResult.Status = HttpServerExecutionStatus.DnsUnknownHost;
                return;
            }
            else
            {
                srContext.ListeningHost = matchedListeningHost;
                request.Host = dnsSafeHost;
            }

            if (matchedListeningHost.Router is null)
            {
                baseResponse.StatusCode = 503; // Service Unavailable
                executionResult.Status = HttpServerExecutionStatus.ListeningHostNotReady;
                return;
            }
            else
            {
                srContext.Router = matchedListeningHost.Router;
                matchedListeningHost.Router.BindServer(this);
            }

            #endregion // 27668

            #region Step 2 - Request validation

            if (this.ServerConfiguration.IncludeRequestIdHeader)
                baseResponse.Headers.Set(flag.HeaderNameRequestId, baseRequest.RequestTraceIdentifier.ToString());

            if (flag.SendSiskHeader)
                baseResponse.Headers.Set(HttpKnownHeaderNames.XPoweredBy, PoweredBy);

            long userMaxContentLength = this.ServerConfiguration.MaximumContentLength;
            bool isContentLenOutsideUserBounds = userMaxContentLength > 0 && baseRequest.ContentLength64 > userMaxContentLength;

            if (isContentLenOutsideUserBounds)
            {
                executionResult.Status = HttpServerExecutionStatus.ContentTooLarge;
                baseResponse.StatusCode = 413;
                return;
            }

            // check for illegal body content requests
            if (flag.ThrowContentOnNonSemanticMethods && (
                   request.Method == HttpMethod.Get
                || request.Method == HttpMethod.Options
                || request.Method == HttpMethod.Head
                || request.Method == HttpMethod.Trace
                || request.Method == HttpMethod.Connect
                ) && context.Request.ContentLength64 > 0)
            {
                executionResult.Status = HttpServerExecutionStatus.ContentServedOnIllegalMethod;
                baseResponse.StatusCode = 400;
                return;
            }

            this.handler.HttpRequestOpen(request);

            #endregion

            #region Step 3 - Routing and action

            // get response
            routerResult = matchedListeningHost.Router.Execute(srContext);
            executionResult.ServerException = routerResult.Exception;

            response = routerResult.Response;
            bool routeAllowCors = routerResult.Route?.UseCors ?? true;

            if (flag.SendCorsHeaders && routeAllowCors)
            {
                SetCorsHeaders(baseRequest, matchedListeningHost.CrossOriginResourceSharingPolicy, baseResponse);
            }

            if (response is null || response.internalStatus == HttpResponse.HTTPRESPONSE_EMPTY)
            {
                baseResponse.StatusCode = 204;
                executionResult.Status = HttpServerExecutionStatus.NoResponse;
                goto finishSending;
            }
            else if (response.internalStatus == HttpResponse.HTTPRESPONSE_UNHANDLED_EXCEPTION)
            {
                executionResult.Status = HttpServerExecutionStatus.UncaughtExceptionThrown;
                baseResponse.StatusCode = 500;
                if (routerResult.Exception is not null)
                    throw routerResult.Exception;
                goto finishSending;
            }
            else if (response.internalStatus == HttpResponse.HTTPRESPONSE_CLIENT_CLOSE ||
                     response.internalStatus == HttpResponse.HTTPRESPONSE_SERVER_CLOSE)
            {
                executionResult.Status = HttpServerExecutionStatus.ConnectionClosed;
                goto finishSending;
            }
            else if (response.internalStatus == HttpResponse.HTTPRESPONSE_SERVER_REFUSE)
            {
                executionResult.Status = HttpServerExecutionStatus.ConnectionClosed;
                baseResponse.Abort();
                goto finishSending;
            }

            baseResponse.StatusCode = response.Status.StatusCode;
            baseResponse.StatusDescription = response.Status.Description;
            baseResponse.KeepAlive = this.ServerConfiguration.KeepAlive;

            #endregion

            #region Step 4 - Response computing
            HttpHeaderCollection responseHeaders = response.Headers;
            responseHeaders.AddRange(srContext.ExtraHeaders);
            responseHeaders.SetRange(srContext.OverrideHeaders);

            for (int i = 0; i < responseHeaders.Count; i++)
            {
                (string, List<string>) incameHeader = responseHeaders.items[i];
                if (string.IsNullOrEmpty(incameHeader.Item1)) continue;

                for (int j = 0; j < incameHeader.Item2.Count; j++)
                    baseResponse.Headers.Add(incameHeader.Item1, incameHeader.Item2[j]);
            }

            try
            {
                bool canServeContent;
                if (flag.PreventResponseContentsInProhibitedMethods)
                {
                    canServeContent = request.Method != HttpMethod.Head && request.Method != HttpMethod.Options;
                }
                else
                {
                    canServeContent = true;
                }

                if (canServeContent)
                {
                    if (response.Content is ByteArrayContent barrayContent)
                    {
                        ApplyHttpContentHeaders(baseResponse, barrayContent.Headers);
                        byte[] contentBytes = ByteArrayAccessors.UnsafeGetContent(barrayContent);
                        int offset = ByteArrayAccessors.UnsafeGetOffset(barrayContent);
                        int count = ByteArrayAccessors.UnsafeGetCount(barrayContent);

                        if (response.SendChunked)
                        {
                            baseResponse.SendChunked = true;
                        }
                        else
                        {
                            baseResponse.SendChunked = false;
                            baseResponse.ContentLength64 = count;
                        }

                        baseResponse.OutputStream.Write(contentBytes, offset, count);
                    }
                    else if (response.Content is HttpContent httpContent)
                    {
                        ApplyHttpContentHeaders(baseResponse, httpContent.Headers);
                        var httpContentStream = httpContent.ReadAsStream(); // the HttpContent.Dispose should dispose this stream

                        if (httpContentStream.CanSeek && !response.SendChunked)
                        {
                            httpContentStream.Seek(0, SeekOrigin.Begin);
                            baseResponse.SendChunked = false;
                            baseResponse.ContentLength64 = httpContentStream.Length;
                        }
                        else
                        {
                            baseResponse.SendChunked = true;
                        }

                        httpContentStream.CopyTo(baseResponse.OutputStream, flag.RequestStreamCopyBufferSize);
                    }
                }
            }
            finally
            {
                response.Content?.Dispose();
            }

        #endregion

        finishSending:
            #region Step 5 - Close streams and send response

            executionResult.ResponseSize = response?.CalculedLength ?? baseResponse.ContentLength64;
            executionResult.RequestSize = baseRequest.ContentLength64;
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
            hasAccessLogging = false;
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
            if (!this.ServerConfiguration.ThrowExceptions)
            {
                baseResponse.StatusCode = 500;
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
            executionResult.Elapsed = sw.Elapsed;

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

            this.handler.HttpRequestClose(executionResult);

            if (executionResult.ServerException is not null)
                this.handler.Exception(executionResult.ServerException);

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

            if (executionResult.ServerException is not null && canErrorLog)
            {
                string entry = LogFormatter.FormatExceptionEntr(executionResult);
                this.ServerConfiguration.ErrorsLogsStream?.WriteLine(entry);
            }

            if (canAccessLog)
            {
                string line = LogFormatter.FormatAccessLogEntry(this.ServerConfiguration.AccessLogsFormat, executionResult);
                this.ServerConfiguration.AccessLogsStream?.WriteLine(line);
            }

            if (this.isWaitingNextEvent)
            {
                this.waitingExecutionResult = executionResult;
                this.waitNextEvent.Set();
                this.isWaitingNextEvent = false;
            }

            if (!flag.AsyncRequestProcessing)
            {
                Monitor.Exit(this.httpListener);
            }
            #endregion
        }
    }
}
