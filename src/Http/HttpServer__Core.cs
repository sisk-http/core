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

namespace Sisk.Core.Http;

public partial class HttpServer
{
    internal const long UnitKb = 1024;
    internal const long UnitMb = UnitKb * 1024;
    internal const long UnitGb = UnitMb * 1024;
    internal const long UnitTb = UnitGb * 1024;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string HumanReadableSize(in float size)
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
                for (int i = 0; i < cors.AllowOrigins.Length; i++)
                {
                    string definedOrigin = cors.AllowOrigins[i];
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

    private void UnbindRouters()
    {
        for (int i = 0; i < ServerConfiguration.ListeningHosts.Count; i++)
        {
            var lh = ServerConfiguration.ListeningHosts[i];
            if (lh.Router is { } router && ReferenceEquals(this, router.parentServer))
            {
                router.FreeHttpServer();
            }
        }
    }

    private void BindRouters()
    {
        for (int i = 0; i < ServerConfiguration.ListeningHosts.Count; i++)
        {
            var lh = ServerConfiguration.ListeningHosts[i];
            if (lh.Router is { } router && router.parentServer is null)
            {
                router.BindServer(this);
            }
        }
    }

    private void ListenerCallback(IAsyncResult result)
    {
        if (_isDisposing || !_isListening)
            return;

        httpListener.BeginGetContext(ListenerCallback, null);
        HttpListenerContext context = httpListener.EndGetContext(result);
        ProcessRequest(context);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void ProcessRequest(HttpListenerContext context)
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

#pragma warning disable CS0219
        // only used to debug where an request dies in http1.1
        string _debugState = "begin";
#pragma warning restore CS0219

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
            if (!flag.AsyncRequestProcessing)
            {
                Monitor.Enter(httpListener);
            }

            sw.Start();

            _debugState = "host_matching";
            #region Step 1 - DNS/Listening host matching

            if (connectingUri is null)
            {
                baseResponse.StatusCode = 400;
                executionResult.Status = HttpServerExecutionStatus.DnsFailed;
                return;
            }

            if (!baseRequest.IsLocal && ServerConfiguration.RemoteRequestsAction == RequestListenAction.Drop)
            {
                executionResult.Status = HttpServerExecutionStatus.RemoteRequestDropped;
                baseResponse.Abort();
                return;
            }

            request = new HttpRequest(this, context);
            string dnsSafeHost = baseRequest.UserHostName;

            if (ServerConfiguration.ForwardingResolver is ForwardingResolver fr)
            {
                dnsSafeHost = fr.OnResolveRequestHost(request, dnsSafeHost);
            }

            // detect the listening host for this listener
            ListeningHost? matchedListeningHost = _onlyListeningHost
                ?? ServerConfiguration.ListeningHosts.GetRequestMatchingListeningHost(dnsSafeHost, baseRequest.LocalEndPoint.Port);

            if (matchedListeningHost is null)
            {
                baseResponse.StatusCode = 400; // Bad Request
                executionResult.Status = HttpServerExecutionStatus.DnsUnknownHost;
                return;
            }

            srContext = new HttpContext(this, request, null, matchedListeningHost);

            request.Host = dnsSafeHost;
            request.Context = srContext;
            executionResult.Request = request;
            otherParty = request.RemoteAddress;

            if (matchedListeningHost.Router is null)
            {
                baseResponse.StatusCode = 503; // Service Unavailable
                executionResult.Status = HttpServerExecutionStatus.ListeningHostNotReady;
                return;
            }
            else
            {
                matchedListeningHost.Router.BindServer(this);
            }

            #endregion // 27668

            #region Step 2 - Request validation

            if (ServerConfiguration.IncludeRequestIdHeader)
                baseResponse.Headers.Set(flag.HeaderNameRequestId, request.RequestId.ToString());

            if (flag.SendSiskHeader)
                baseResponse.Headers.Set(HttpKnownHeaderNames.XPoweredBy, PoweredBy);

            int userMaxContentLength = ServerConfiguration.MaximumContentLength;
            bool isContentLenOutsideUserBounds = userMaxContentLength > 0 && baseRequest.ContentLength64 > userMaxContentLength;
            bool isContentLenOutsideSystemBounds = baseRequest.ContentLength64 > Int32.MaxValue;

            if (isContentLenOutsideUserBounds || isContentLenOutsideSystemBounds)
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
#if NET7_0_OR_GREATER
                || request.Method == HttpMethod.Connect
#endif
                ) && context.Request.ContentLength64 > 0)
            {
                executionResult.Status = HttpServerExecutionStatus.ContentServedOnIllegalMethod;
                baseResponse.StatusCode = 400;
                return;
            }

            _debugState = "request_create";
            handler.HttpRequestOpen(request);

            #endregion

            #region Step 3 - Routing and action

            // get response
            routerResult = matchedListeningHost.Router.Execute(srContext);

            _debugState = "receive_response";
            response = routerResult.Response;
            useCors = routerResult.Route?.UseCors ?? true;

            if (useCors)
            {
                SetCorsHeaders(flag, baseRequest, matchedListeningHost.CrossOriginResourceSharingPolicy, baseResponse);
            }

            _debugState = "check_response";
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

            _debugState = "send_status";
            baseResponse.StatusCode = response.StatusInformation.StatusCode;
            baseResponse.StatusDescription = response.StatusInformation.Description;
            baseResponse.KeepAlive = ServerConfiguration.KeepAlive;

            #endregion

            #region Step 4 - Response computing
            NameValueCollection resHeaders = new NameValueCollection
            {
                response.Headers
            };

            long? responseContentLength = response.Content?.Headers.ContentLength;
            if (srContext?.OverrideHeaders.Count > 0) resHeaders.Add(srContext.OverrideHeaders);

            for (int i = 0; i < resHeaders.Count; i++)
            {
                string? incameHeader = resHeaders.Keys[i];
                if (string.IsNullOrEmpty(incameHeader)) continue;

                string? value = resHeaders[incameHeader];
                if (string.IsNullOrEmpty(value)) continue;

                baseResponse.Headers[incameHeader] = value;
            }

            _debugState = "sent_headers";
            if (response.Content is not null)
            {
                Stream? contentStream = null;
                // determines the content type
                baseResponse.ContentType = resHeaders[HttpKnownHeaderNames.ContentType] ?? response.Content.Headers.ContentType?.ToString();

                // determines if the response should be sent as chunked or normal
                if (response.SendChunked)
                {
                    baseResponse.SendChunked = true;
                }
                else if (responseContentLength is long contentLength)
                {
                    baseResponse.ContentLength64 = contentLength;
                }
                else if (response.Content is StreamContent stmContent)
                {
                    contentStream = stmContent.ReadAsStream();
                    if (!contentStream.CanSeek)
                    {
                        baseResponse.SendChunked = true;
                    }
                    else
                    {
                        contentStream.Position = 0;
                        baseResponse.ContentLength64 = contentStream.Length;
                    }
                }
                else
                {
                    // the content-length wasn't informed and the user didn't set the request to
                    // send as chunked. so the server will send the response by chunked encoding
                    // mode by default
                    baseResponse.SendChunked = true;
                }

                // write the output buffer
                if (context.Request.HttpMethod != "HEAD")
                {
                    outcomingSize += responseContentLength ?? -1;

                    bool isPayloadStreamable =
                        response.Content is StreamContent ||
                        responseContentLength > flag.RequestStreamCopyBufferSize;

                    if (isPayloadStreamable)
                    {
                        if (contentStream is null)
                            contentStream = response.Content.ReadAsStream();

                        contentStream.CopyTo(baseResponse.OutputStream, flag.RequestStreamCopyBufferSize);
                        _debugState = "send_streamable_end_write";
                    }
                    else
                    {
                        byte[] contents = response.Content.ReadAsByteArrayAsync().Result;
                        baseResponse.OutputStream.Write(contents);
                        _debugState = "send_payload_end_write";
                    }
                }
            }

        #endregion

        finishSending:
            #region Step 5 - Close streams and send response

            _debugState = "content_sent";
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

            _debugState = "define_result";
            baseResponse.Close();
            baseRequest.InputStream.Close();

            closeStream = false;
            _debugState = "close_streams";
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
            hasErrorLogging = false;
        }
        catch (Exception ex)
        {
            if (!ServerConfiguration.ThrowExceptions)
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

            if (isWaitingNextEvent)
            {
                waitingExecutionResult = executionResult;
                waitNextEvent.Set();
                isWaitingNextEvent = false;
            }

            if (!flag.AsyncRequestProcessing)
            {
                Monitor.Exit(httpListener);
            }
            #endregion
        }
    }
}
