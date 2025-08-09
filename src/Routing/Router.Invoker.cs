﻿// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Router.Invoker.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web;
using Sisk.Core.Http;
using Sisk.Core.Internal;

namespace Sisk.Core.Routing;

public partial class Router {

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private bool IsMethodMatching ( in string ogRqMethod, RouteMethod method ) {

        if (ogRqMethod.Equals ( "GET", StringComparison.Ordinal )) {
            return method.HasFlag ( RouteMethod.Get );
        }
        else if (ogRqMethod.Equals ( "POST", StringComparison.Ordinal )) {
            return method.HasFlag ( RouteMethod.Post );
        }
        else if (ogRqMethod.Equals ( "PUT", StringComparison.Ordinal )) {
            return method.HasFlag ( RouteMethod.Put );
        }
        else if (ogRqMethod.Equals ( "PATCH", StringComparison.Ordinal )) {
            return method.HasFlag ( RouteMethod.Patch );
        }
        else if (ogRqMethod.Equals ( "OPTIONS", StringComparison.Ordinal )) {
            return method.HasFlag ( RouteMethod.Options );
        }
        else if (ogRqMethod.Equals ( "HEAD", StringComparison.Ordinal )) {
            return method.HasFlag ( RouteMethod.Head ) || method.HasFlag ( RouteMethod.Get );
        }
        else if (ogRqMethod.Equals ( "DELETE", StringComparison.Ordinal )) {
            return method.HasFlag ( RouteMethod.Delete );
        }
        else {
            return method.HasFlag ( RouteMethod.Any );
        }
    }

    private RouteMatch TestRouteMatchUsingRegex ( Route route, string requestPath ) {
        route.routeRegex ??= new Regex ( route.Path, MatchRoutesIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None );

        var test = route.routeRegex.Match ( requestPath );
        if (test.Success) {
            NameValueCollection query = new NameValueCollection ();
            for (int i = 0; i < test.Groups.Count; i++) {
                Group group = test.Groups [ i ];
                if (group.Index.ToString ( provider: null ) == group.Name)
                    continue;
                query.Add ( group.Name, group.Value );
            }
            return new RouteMatch ( true, query );
        }
        else {
            return new RouteMatch ( false, null );
        }
    }

    internal bool InvokeRequestHandlerGroup ( RequestHandlerExecutionMode mode, Span<IRequestHandler> baseLists, Span<IRequestHandler> bypassList, HttpRequest request, HttpContext context, out HttpResponse? result, out Exception? exception ) {
        for (int i = 0; i < baseLists.Length; i++) {
            var rh = baseLists [ i ];

            if (rh.ExecutionMode.HasFlag ( mode )) {
                HttpResponse? response = InvokeHandler ( rh, request, context, bypassList, out exception );
                if (response is not null) {
                    result = response;
                    return true;
                }
            }
        }
        result = null;
        exception = null;
        return false;
    }

    internal HttpResponse? InvokeHandler ( IRequestHandler handler, HttpRequest request, HttpContext context, Span<IRequestHandler> bypass, out Exception? exception ) {
        for (int i = 0; i < bypass.Length; i++) {
            if (ReferenceEquals ( handler, bypass [ i ] )) {
                exception = null;
                return null;
            }
        }

        HttpResponse? result = null;
        try {
            result = handler.Execute ( request, context );
        }
        catch (Exception ex) {
            exception = ex;
            if (parentServer!.ServerConfiguration.ThrowExceptions == false) {
                if (CallbackErrorHandler is not null) {
                    result = CallbackErrorHandler ( ex, context );
                }
            }
            else
                throw;
        }

        exception = null;
        return result;
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    internal RouterExecutionResult Execute ( HttpContext context ) {
        // the line below ensures that _routesList will not be modified in this method
        if (parentServer is null)
            throw new InvalidOperationException ( SR.Router_NotBinded );

        HttpRequest request = context.Request;
        HttpServerConfiguration currentConfig = parentServer!.ServerConfiguration;

        Route? matchedRoute = null;
        RouteMatchResult matchResult = RouteMatchResult.NotMatched;
        Exception? handledException = null;

        // IsReadOnly ensures that no route will be added or removed from the list during the
        // span iteration
        // 
        Span<Route> rspan = CollectionsMarshal.AsSpan ( _routesList );
        ref Route rPointer = ref MemoryMarshal.GetReference ( rspan );
        for (int i = 0; i < rspan.Length; i++) {
            ref Route route = ref Unsafe.Add ( ref rPointer, i );

            // test path
            RouteMatch pathTest;
            string reqUrlTest = request.Path;

            if (route.UseRegex) {
                pathTest = TestRouteMatchUsingRegex ( route, reqUrlTest );
            }
            else {
                pathTest = HttpStringInternals.IsReqPathMatch ( route.Path, reqUrlTest,
                    MatchRoutesIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal );
            }

            if (!pathTest.Success) {
                continue;
            }

            matchResult = RouteMatchResult.PathMatched;
            bool isMethodMatched = false;

            // test method
            if (IsMethodMatching ( request.Method.Method, route.Method )) {
                isMethodMatched = true;
            }
            else if (request.Method == HttpMethod.Options) {
                matchResult = RouteMatchResult.OptionsMatched;
                break;
            }

            if (isMethodMatched) {
                if (pathTest.Parameters is not null) {
                    var keys = pathTest.Parameters.Keys;

                    for (int j = 0; j < keys.Count; j++) {
                        string? name = keys [ j ];
                        if (string.IsNullOrEmpty ( name ))
                            continue;

                        string? value = pathTest.Parameters [ name ];
                        if (string.IsNullOrEmpty ( value ))
                            continue;

                        string valueDecoded = HttpUtility.UrlDecode ( value );
                        request.RouteParameters.SetItemInternal ( name, valueDecoded );
                    }

                    request.RouteParameters.MakeReadOnly ();
                }

                matchResult = RouteMatchResult.FullyMatched;
                matchedRoute = route;
                break;
            }
        }

        if (matchResult == RouteMatchResult.NotMatched) {
            if (NotFoundErrorHandler is not null) {
                return new RouterExecutionResult ( NotFoundErrorHandler ( context ), null, matchResult, null );
            }
            else {
                return new RouterExecutionResult ( new HttpResponse ( HttpStatusCode.NotFound ), null, matchResult, null );
            }
        }
        else if (matchResult == RouteMatchResult.OptionsMatched) {
            return new RouterExecutionResult ( new HttpResponse ( HttpStatusCode.OK ), null, matchResult, null );
        }
        else if (matchResult == RouteMatchResult.PathMatched) {
            context.MatchedRoute = matchedRoute;
            if (MethodNotAllowedErrorHandler is not null) {
                return new RouterExecutionResult ( MethodNotAllowedErrorHandler ( context ), matchedRoute, matchResult, null );
            }
            else {
                return new RouterExecutionResult ( new HttpResponse ( HttpStatusCode.MethodNotAllowed ), matchedRoute, matchResult, null );
            }
        }
        else if (matchResult == RouteMatchResult.FullyMatched && matchedRoute is not null) {
            context.MatchedRoute = matchedRoute;
            HttpResponse? result = null;

            if (currentConfig.ForceTrailingSlash && !matchedRoute.UseRegex && !request.Path.EndsWith ( '/' ) && request.Method == HttpMethod.Get) {
                HttpResponse res = new HttpResponse () {
                    Status = HttpStatusInformation.TemporaryRedirect,
                    Headers = new () {
                        Location = $"{request.Path}/{request.QueryString}"
                    }
                };

                return new RouterExecutionResult ( res, matchedRoute, matchResult, null );
            }

            parentServer?.handler.ContextBagCreated ( context.RequestBag );

            #region Before-response handlers
            if (InvokeRequestHandlerGroup ( RequestHandlerExecutionMode.BeforeResponse, GlobalRequestHandlers, matchedRoute.BypassGlobalRequestHandlers, request, context, out HttpResponse? rhResponse, out Exception? rhException )) {
                return new RouterExecutionResult ( rhResponse, matchedRoute, matchResult, rhException );
            }
            if (InvokeRequestHandlerGroup ( RequestHandlerExecutionMode.BeforeResponse, matchedRoute.RequestHandlers, null, request, context, out rhResponse, out rhException )) {
                return new RouterExecutionResult ( rhResponse, matchedRoute, matchResult, rhException );
            }
            #endregion

            #region Route action

            try {
                context.MatchedRoute = matchedRoute;
                object? actionResult;

                if (matchedRoute._parameterlessRouteAction != null) {
                    actionResult = matchedRoute._parameterlessRouteAction ();
                }
                else if (matchedRoute._singleParamCallback != null) {
                    actionResult = matchedRoute._singleParamCallback ( request );
                }
                else {
                    throw new ArgumentException ( SR.Format ( SR.Router_NoRouteActionDefined, matchedRoute ) );
                }

                if (matchedRoute._isAsyncTask) {
                    if (actionResult is null) {
                        throw new ArgumentException ( SR.Router_Handler_ActionNullValue );
                    }

                    ref Task<object> actionTask = ref Unsafe.As<object, Task<object>> ( ref actionResult );
                    actionResult = actionTask.ConfigureAwait ( false ).GetAwaiter ().GetResult ();
                }
                else if (matchedRoute._isAsyncEnumerable) {
                    if (currentConfig.ConvertIAsyncEnumerableIntoEnumerable) {
                        ref IAsyncEnumerable<object> asyncEnumerable = ref Unsafe.As<object, IAsyncEnumerable<object>> ( ref actionResult );
                        actionResult = asyncEnumerable.ToBlockingEnumerable ();
                    }
                }

                if (actionResult is IEnumerable enumerableAction && actionResult is not string) {

                    // Since enumeration can occur outside the router's context (e.g., while reading the HttpContent),
                    // errors might be thrown outside the router's context, preventing application-side error capture.
                    // The ToArray() below forces reading the enumerator into memory, ensuring any enumeration errors
                    // are thrown within the router's context.
                    ArrayList tempArray = new ArrayList ();
                    var enumerator = enumerableAction.GetEnumerator ();
                    while (enumerator.MoveNext ()) {
                        tempArray.Add ( enumerator.Current );
                    }

                    actionResult = tempArray.ToArray ();
                }

                result = ResolveAction ( actionResult );
            }
            catch (Exception ex) {
                if (parentServer!.ServerConfiguration.ThrowExceptions == false
                    && (ex is not HttpListenerException) /*the HttpServer__Core should handle HttpListenerException exceptions*/) {

                    if (CallbackErrorHandler is not null) {
                        handledException = ex;
                        result = CallbackErrorHandler ( ex, context );
                    }
                    else {
                        result = new HttpResponse ( HttpResponse.HTTPRESPONSE_UNHANDLED_EXCEPTION );
                        return new RouterExecutionResult ( result, matchedRoute, matchResult, ex );
                    }
                }
                else
                    throw;
            }
            finally {
                context.RouterResponse = result;
            }
            #endregion

            #region After-response global handlers
            if (InvokeRequestHandlerGroup ( RequestHandlerExecutionMode.AfterResponse, GlobalRequestHandlers, matchedRoute.BypassGlobalRequestHandlers, request, context, out rhResponse, out rhException )) {
                return new RouterExecutionResult ( rhResponse, matchedRoute, matchResult, rhException );
            }
            if (InvokeRequestHandlerGroup ( RequestHandlerExecutionMode.AfterResponse, matchedRoute.RequestHandlers, null, request, context, out rhResponse, out rhException )) {
                return new RouterExecutionResult ( rhResponse, matchedRoute, matchResult, rhException );
            }
            #endregion     
        }

        return new RouterExecutionResult ( context.RouterResponse, matchedRoute, matchResult, handledException );
    }
}
