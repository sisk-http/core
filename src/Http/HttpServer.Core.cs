// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServer.Core.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Sisk.Core.Entity;
using Sisk.Core.Internal;
using Sisk.Core.Routing;

namespace Sisk.Core.Http;

public partial class HttpServer {


    internal static void ApplyHttpContentHeaders ( HttpListenerResponse response, HttpContentHeaders contentHeaders ) {
        // content-length is applied outside this method
        // do not include that here

        // is faster to count if any headers had been defined into HttpListenerResponse
        // before iterating them

        Span<string> definedHeaders;
        if (response.Headers.Count > 0) {
            definedHeaders = response.Headers.AllKeys;
        }
        else {
            definedHeaders = Span<string>.Empty;
        }

        var headerComparer = StringComparer.OrdinalIgnoreCase;

        if (contentHeaders.ContentType?.ToString () is { } ContentType
            && !SpanHelpers.Contains ( definedHeaders, HttpKnownHeaderNames.ContentType, headerComparer ))
            response.ContentType = ContentType;

        if (contentHeaders.ContentRange?.ToString () is { } ContentRange
            && !SpanHelpers.Contains ( definedHeaders, HttpKnownHeaderNames.ContentRange, headerComparer ))
            response.AppendHeader ( HttpKnownHeaderNames.ContentRange, ContentRange );

        if (contentHeaders.ContentMD5 is { } ContentMD5
            && !SpanHelpers.Contains ( definedHeaders, HttpKnownHeaderNames.ContentMD5, headerComparer )) // rfc1864#section-2
            response.AppendHeader ( HttpKnownHeaderNames.ContentMD5, Convert.ToBase64String ( ContentMD5 ) );

        if (contentHeaders.ContentLocation?.ToString () is { } ContentLocation
            && !SpanHelpers.Contains ( definedHeaders, HttpKnownHeaderNames.ContentLocation, headerComparer ))
            response.AppendHeader ( HttpKnownHeaderNames.ContentLocation, ContentLocation );

        if (contentHeaders.ContentDisposition?.ToString () is { } ContentDisposition
            && !SpanHelpers.Contains ( definedHeaders, HttpKnownHeaderNames.ContentDisposition, headerComparer ))
            response.AppendHeader ( HttpKnownHeaderNames.ContentDisposition, ContentDisposition );

        if (contentHeaders.LastModified is { } LastModified
            && !SpanHelpers.Contains ( definedHeaders, HttpKnownHeaderNames.LastModified, headerComparer ))
            response.AppendHeader ( HttpKnownHeaderNames.LastModified, LastModified.ToUniversalTime ().ToString ( "dddd, dd MMMM yyyy HH:mm:ss 'GMT'", formatProvider: null ) );

        if (contentHeaders.Expires is { } Expires
            && !SpanHelpers.Contains ( definedHeaders, HttpKnownHeaderNames.Expires, headerComparer ))
            response.AppendHeader ( HttpKnownHeaderNames.Expires, Expires.ToUniversalTime ().ToString ( "dddd, dd MMMM yyyy HH:mm:ss 'GMT'", formatProvider: null ) );

        if (contentHeaders.ContentLanguage.Count > 0
            && !SpanHelpers.Contains ( definedHeaders, HttpKnownHeaderNames.ContentLanguage, headerComparer ))
            response.AppendHeader ( HttpKnownHeaderNames.ContentLanguage, string.Join ( ", ", contentHeaders.ContentLanguage ) );

        if (contentHeaders.ContentEncoding.Count > 0
            && !SpanHelpers.Contains ( definedHeaders, HttpKnownHeaderNames.ContentEncoding, headerComparer ))
            response.AppendHeader ( HttpKnownHeaderNames.ContentEncoding, string.Join ( ", ", contentHeaders.ContentEncoding ) );
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    internal static void SetCorsHeaders ( HttpListenerRequest baseRequest, CrossOriginResourceSharingHeaders? cors, HttpListenerResponse baseResponse ) {
        if (cors is null)
            return;

        if (cors.AllowHeaders.Length > 0)
            baseResponse.Headers.Set ( HttpKnownHeaderNames.AccessControlAllowHeaders, string.Join ( ", ", cors.AllowHeaders ) );

        if (cors.AllowMethods.Length > 0)
            baseResponse.Headers.Set ( HttpKnownHeaderNames.AccessControlAllowMethods, string.Join ( ", ", cors.AllowMethods ) );

        if (cors.AllowOrigin is { } allowOriginValue) {

            if (allowOriginValue == CrossOriginResourceSharingHeaders.AutoAllowOrigin) {

                string? requestOrigin = baseRequest.Headers [ HttpKnownHeaderNames.Origin ];
                if (requestOrigin != null)
                    baseResponse.Headers.Set ( HttpKnownHeaderNames.AccessControlAllowOrigin, requestOrigin );
            }
            else {
                baseResponse.Headers.Set ( HttpKnownHeaderNames.AccessControlAllowOrigin, cors.AllowOrigin );
            }
        }
        else if (cors.AllowOrigins?.Length > 0) {
            string? origin = baseRequest.Headers [ HttpKnownHeaderNames.Origin ];

            if (origin is not null) {
                for (int i = 0; i < cors.AllowOrigins.Length; i++) {
                    string definedOrigin = cors.AllowOrigins [ i ];
                    if (string.Equals ( definedOrigin, origin, StringComparison.OrdinalIgnoreCase )) {
                        baseResponse.Headers.Set ( HttpKnownHeaderNames.AccessControlAllowOrigin, origin );
                        break;
                    }
                }
            }
        }

        if (cors.AllowCredentials == true)
            baseResponse.Headers.Set ( HttpKnownHeaderNames.AccessControlAllowCredentials, "true" );

        if (cors.ExposeHeaders.Length > 0)
            baseResponse.Headers.Set ( HttpKnownHeaderNames.AccessControlExposeHeaders, string.Join ( ", ", cors.ExposeHeaders ) );

        if (cors.MaxAge > TimeSpan.Zero)
            baseResponse.Headers.Set ( HttpKnownHeaderNames.AccessControlMaxAge, cors.MaxAge.TotalSeconds.ToString ( provider: null ) );
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private void UnbindRouters () {
        for (int i = 0; i < ServerConfiguration.ListeningHosts.Count; i++) {
            var lh = ServerConfiguration.ListeningHosts [ i ];
            if (lh.Router is { } router && ReferenceEquals ( this, router.parentServer )) {
                router.FreeHttpServer ();
            }
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private void BindRouters () {
        for (int i = 0; i < ServerConfiguration.ListeningHosts.Count; i++) {
            var lh = ServerConfiguration.ListeningHosts [ i ];
            if (lh.Router is { } router && router.parentServer is null) {
                router.BindServer ( this );
            }
        }
    }

    private void ListenerCallback ( IAsyncResult result ) {
        if (_isDisposing || !_isListening)
            return;

        httpListener.BeginGetContext ( ListenerCallback, null );
        HttpListenerContext context = httpListener.EndGetContext ( result );

        ProcessRequest ( context );
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    private void ProcessRequest ( HttpListenerContext context ) {
        HttpRequest request = null!;
        HttpResponse? response = null;

        Stopwatch sw = Stopwatch.StartNew ();

        HttpListenerResponse baseResponse = context.Response;
        HttpListenerRequest baseRequest = context.Request;

        HttpContext? srContext = new HttpContext ( this );
        bool closeStream = true;

        HttpContext._context.Value = srContext;

        var currentConfig = ServerConfiguration;
        bool hasAccessLogging = currentConfig.AccessLogsStream is not null;
        bool hasErrorLogging = currentConfig.ErrorsLogsStream is not null;

        Router.RouterExecutionResult? routerResult = null;

        HttpContent? servedContent = null;
        HttpServerExecutionResult executionResult = new HttpServerExecutionResult () {
            Context = srContext,
            Status = HttpServerExecutionStatus.NoResponse
        };

        try {

            if (currentConfig.AsyncRequestProcessing == false) {
                Monitor.Enter ( httpListener );
            }

            #region Step 1 - DNS/Listening host matching

            // context initialization
            request = new HttpRequest ( this, context );

            srContext.Request = request;
            request.Context = srContext;

            if (currentConfig.RemoteRequestsAction == RequestListenAction.Drop && baseRequest.IsLocal == false) {
                executionResult.Status = HttpServerExecutionStatus.RemoteRequestDropped;
                baseResponse.Abort ();
                return;
            }

            string dnsSafeHost = request.Uri.Host;
            if (currentConfig.ForwardingResolver is ForwardingResolver fr) {
                dnsSafeHost = fr.OnResolveRequestHost ( request, dnsSafeHost );
            }

            // detect the listening host for this listener
            ListeningHost? matchedListeningHost = _onlyListeningHost
                ?? currentConfig.ListeningHosts.GetRequestMatchingListeningHost ( dnsSafeHost, baseRequest.Url!.AbsolutePath, baseRequest.LocalEndPoint.Port );

            if (matchedListeningHost is null) {
                baseResponse.StatusCode = 400; // Bad Request
                executionResult.Status = HttpServerExecutionStatus.DnsUnknownHost;
                return;
            }
            else {
                srContext.ListeningHost = matchedListeningHost;
                request.Host = dnsSafeHost;
            }

            if (matchedListeningHost.Router is null) {
                baseResponse.StatusCode = 503; // Service Unavailable
                executionResult.Status = HttpServerExecutionStatus.ListeningHostNotReady;
                return;
            }
            else {
                srContext.Router = matchedListeningHost.Router;
                matchedListeningHost.Router.BindServer ( this );
            }

            #endregion // 27668

            #region Step 2 - Request validation

            if (currentConfig.IncludeRequestIdHeader)
                baseResponse.Headers.Set ( HttpKnownHeaderNames.XRequestID, baseRequest.RequestTraceIdentifier.ToString () );
            if (currentConfig.SendSiskHeader)
                baseResponse.Headers.Set ( HttpKnownHeaderNames.XPoweredBy, PoweredBy );

            long userMaxContentLength = currentConfig.MaximumContentLength;
            bool isContentLenOutsideUserBounds = userMaxContentLength > 0 && request.ContentLength > userMaxContentLength;

            if (isContentLenOutsideUserBounds) {
                executionResult.Status = HttpServerExecutionStatus.ContentTooLarge;
                baseResponse.StatusCode = 413;
                return;
            }

            handler.HttpRequestOpen ( request );

            #endregion

            #region Step 3 - Routing and action

            // get response
            routerResult = matchedListeningHost.Router.Execute ( srContext );

            executionResult.ServerException = routerResult.Exception;
            response = routerResult.Response;
            servedContent = response?.Content;

            bool routeAllowCors = routerResult.Route?.UseCors ?? true;

            if (routeAllowCors) {
                SetCorsHeaders ( baseRequest, matchedListeningHost.CrossOriginResourceSharingPolicy, baseResponse );
            }

            if (response is null || response.internalStatus == HttpResponse.HTTPRESPONSE_EMPTY) {
                baseResponse.StatusCode = 204/*NoContent*/;
                executionResult.Status = HttpServerExecutionStatus.NoResponse;
                goto finishSending;
            }
            else if (response.internalStatus == HttpResponse.HTTPRESPONSE_UNHANDLED_EXCEPTION) {
                executionResult.Status = HttpServerExecutionStatus.UncaughtExceptionThrown;
                baseResponse.StatusCode = 500/*InternalServerError*/;

                if (routerResult.Exception is not null)
                    throw routerResult.Exception;

                goto finishSending;
            }
            else if (response.internalStatus == HttpResponse.HTTPRESPONSE_CLIENT_CLOSE ||
                     response.internalStatus == HttpResponse.HTTPRESPONSE_SERVER_CLOSE) {
                executionResult.Status = HttpServerExecutionStatus.ConnectionClosed;
                goto finishSending;
            }
            else if (response.internalStatus == HttpResponse.HTTPRESPONSE_SERVER_REFUSE) {
                executionResult.Status = HttpServerExecutionStatus.ConnectionClosed;
                baseResponse.Abort ();
                goto finishSending;
            }

            baseResponse.StatusCode = response.Status.StatusCode;
            baseResponse.StatusDescription = response.Status.Description;
            baseResponse.KeepAlive = currentConfig.KeepAlive;

            #endregion

            #region Step 4 - Response computing
            HttpHeaderCollection responseHeaders = response.Headers;
            responseHeaders.AddRange ( srContext.ExtraHeaders );
            responseHeaders.SetRange ( srContext.OverrideHeaders );

            for (int i = 0; i < responseHeaders.Count; i++) {
                (string, List<string>) incameHeader = responseHeaders.items [ i ];
                if (string.IsNullOrEmpty ( incameHeader.Item1 ))
                    continue;

                for (int j = 0; j < incameHeader.Item2.Count; j++)
                    baseResponse.Headers.Add ( incameHeader.Item1, incameHeader.Item2 [ j ] );
            }
            
            if (currentConfig.EnableAutomaticResponseCompression
                && servedContent is { } and not CompressedContent
                && request.Headers.AcceptEncoding is { } acceptedEncodings) {

                if (acceptedEncodings.Contains ( "br" )) {
                    servedContent = new BrotliContent ( servedContent );
                }
                else if (acceptedEncodings.Contains ( "gzip" )) {
                    servedContent = new GZipContent ( servedContent );
                }
                else if (acceptedEncodings.Contains ( "deflate" )) {
                    servedContent = new DeflateContent ( servedContent );
                }
            }

            if (servedContent is ByteArrayContent barrayContent) {
                ApplyHttpContentHeaders ( baseResponse, barrayContent.Headers );
                ref byte [] contentBytes = ref ByteArrayAccessors.UnsafeGetContent ( barrayContent );
                ref int offset = ref ByteArrayAccessors.UnsafeGetOffset ( barrayContent );
                ref int count = ref ByteArrayAccessors.UnsafeGetCount ( barrayContent );

                if (response.SendChunked) {
                    baseResponse.SendChunked = true;
                }
                else {
                    baseResponse.SendChunked = false;
                    baseResponse.ContentLength64 = count;
                }

                baseResponse.OutputStream.Write ( contentBytes, offset, count );
            }
            else if (servedContent is HttpContent httpContent) {
                ApplyHttpContentHeaders ( baseResponse, httpContent.Headers );
                var httpContentStream = httpContent.ReadAsStream (); // the HttpContent.Dispose should dispose this stream

                if (httpContentStream.CanSeek && !response.SendChunked) {
                    httpContentStream.Seek ( 0, SeekOrigin.Begin );
                    baseResponse.SendChunked = false;
                    baseResponse.ContentLength64 = httpContentStream.Length;
                }
                else {
                    baseResponse.SendChunked = true;
                }

                httpContentStream.CopyTo ( baseResponse.OutputStream );
            }
#endregion

finishSending:
#region Step 5 - Close streams and send response
            baseResponse.Close ();
            closeStream = false;

            if (executionResult.Status == HttpServerExecutionStatus.NoResponse && response?.internalStatus != HttpResponse.HTTPRESPONSE_EMPTY)
                executionResult.Status = HttpServerExecutionStatus.Executed;
            #endregion
        }
        catch (ObjectDisposedException objException) {
            executionResult.Status = HttpServerExecutionStatus.ExceptionThrown;
            executionResult.ServerException = objException;
            hasErrorLogging = false;
        }
        catch (HttpListenerException netException) {
            // often raised when the client connection is closed during content streaming
            // it's not a real error and the server should deal with this as an client disconnect
            executionResult.Status = HttpServerExecutionStatus.ConnectionClosed;
            executionResult.ServerException = netException;
            hasErrorLogging = false;
            hasAccessLogging = false;
        }
        catch (HttpRequestException requestException) {
            baseResponse.StatusCode = 400/*BadRequest*/;
            baseResponse.StatusDescription = HttpStatusInformation.BadRequest.Description;
            executionResult.Status = HttpServerExecutionStatus.MalformedRequest;
            executionResult.ServerException = requestException;
            hasErrorLogging = false;
        }
        catch (Exception ex) {
            if (!currentConfig.ThrowExceptions) {
                baseResponse.StatusCode = 500/*InternalServerError*/;
                baseResponse.StatusDescription = HttpStatusInformation.InternalServerError.Description;
                executionResult.ServerException = ex;
                executionResult.Status = HttpServerExecutionStatus.ExceptionThrown;
            }
            else {
                throw;
            }
        }
        finally {

            sw.Stop ();

            executionResult.ResponseSize = response?.CalculedLength ?? baseResponse.ContentLength64;
            executionResult.RequestSize = request.ContentLength;
            executionResult.Elapsed = sw.Elapsed;

            servedContent?.Dispose ();

            if (currentConfig.DisposeDisposableContextValues)
                foreach (var value in srContext.RequestBag.Values)
                    (value as IDisposable)?.Dispose ();

            if (closeStream) {
                // Close() would throw an exception if the sent payload length is greater than
                // content length, so it will force close the connection on the Abort() method
                try {
                    baseResponse.Close ();
                }
                catch (Exception) {
                    baseResponse.Abort ();
                }
            }

            handler.HttpRequestClose ( executionResult );

            if (executionResult.ServerException is not null)
                handler.Exception ( executionResult.ServerException );

            LogOutput logMode;

            #region Logging
            if (routerResult?.Result == RouteMatchResult.OptionsMatched) {
                logMode = currentConfig.OptionsLogMode;
            }
            else {
                logMode = srContext?.MatchedRoute?.LogMode ?? LogOutput.Both;
            }

            bool canAccessLog = hasAccessLogging && logMode.HasFlag ( LogOutput.AccessLog );
            bool canErrorLog = hasErrorLogging && logMode.HasFlag ( LogOutput.ErrorLog );

            if (executionResult.ServerException is not null && canErrorLog) {
                string entry = LogFormatter.FormatExceptionEntr ( executionResult );
                currentConfig.ErrorsLogsStream?.WriteLine ( entry );
            }

            if (canAccessLog) {
                string line = LogFormatter.FormatAccessLogEntry ( currentConfig.AccessLogsFormat, executionResult );
                ServerConfiguration.AccessLogsStream?.WriteLine ( line );
            }

            if (isWaitingNextEvent) {
                waitingExecutionResult = executionResult;
                waitNextEvent.Set ();
                isWaitingNextEvent = false;
            }

            if (!currentConfig.AsyncRequestProcessing) {
                Monitor.Exit ( httpListener );
            }

            (request as IDisposable)?.Dispose ();
            #endregion
        }
    }
}
