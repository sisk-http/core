// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Router__CoreInvoker.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Internal;
using System.Collections.Specialized;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web;

namespace Sisk.Core.Routing;

public partial class Router
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsMethodMatching(string ogRqMethod, RouteMethod method)
    {
        if (method == RouteMethod.Any) return true;
        if (Enum.TryParse(ogRqMethod, true, out RouteMethod ogRqParsed))
        {
            return method.HasFlag(ogRqParsed);
        }
        return false;
    }

    private Internal.HttpStringInternals.PathMatchResult TestRouteMatchUsingRegex(Route route, string requestPath)
    {
        if (route.routeRegex is null)
        {
            route.routeRegex = new Regex(route.Path, MatchRoutesIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }

        var test = route.routeRegex.Match(requestPath);
        if (test.Success)
        {
            NameValueCollection query = new NameValueCollection();
            for (int i = 0; i < test.Groups.Count; i++)
            {
                Group group = test.Groups[i];
                if (group.Index.ToString() == group.Name) continue;
                query.Add(group.Name, group.Value);
            }
            return new HttpStringInternals.PathMatchResult(true, query);
        }
        else
        {
            return new HttpStringInternals.PathMatchResult(false, new NameValueCollection());
        }
    }

    internal bool InvokeRequestHandlerGroup(RequestHandlerExecutionMode mode, IRequestHandler[] baseLists, IRequestHandler[]? bypassList, HttpRequest request, HttpContext context, out HttpResponse? result, out Exception? exception)
    {
        ref IRequestHandler pointer = ref MemoryMarshal.GetArrayDataReference(baseLists);
        for (int i = 0; i < baseLists.Length; i++)
        {
            var rh = Unsafe.Add(ref pointer, i);
            if (rh.ExecutionMode == mode)
            {
                HttpResponse? response = InvokeHandler(rh, request, context, bypassList, out exception);
                if (response is not null)
                {
                    result = response;
                    return true;
                }
            }
        }
        result = null;
        exception = null;
        return false;
    }

    internal HttpResponse? InvokeHandler(IRequestHandler handler, HttpRequest request, HttpContext context, IRequestHandler[]? bypass, out Exception? exception)
    {
        if (bypass is not null && bypass.Contains(handler))
        {
            exception = null;
            return null;
        }

        HttpResponse? result = null;
        try
        {
            result = handler.Execute(request, context);
        }
        catch (Exception ex)
        {
            exception = ex;
            if (!parentServer!.ServerConfiguration.ThrowExceptions)
            {
                if (CallbackErrorHandler is not null)
                {
                    result = CallbackErrorHandler(ex, context);
                }
                else { /* do nothing */ };
            }
            else throw;
        }

        exception = null;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal RouterExecutionResult Execute(HttpContext context)
    {
        if (parentServer == null) throw new InvalidOperationException(SR.Router_NotBinded);

        context.Router = this;
        HttpRequest request = context.Request;
        Route? matchedRoute = null;
        RouteMatchResult matchResult = RouteMatchResult.NotMatched;

        HttpServerFlags flag = parentServer!.ServerConfiguration.Flags;

        Span<Route> rspan = CollectionsMarshal.AsSpan(_routesList);
        ref Route rPointer = ref MemoryMarshal.GetReference(rspan);
        for (int i = 0; i < rspan.Length; i++)
        {
            Route route = Unsafe.Add(ref rPointer, i);

            // test path
            HttpStringInternals.PathMatchResult pathTest;
            string reqUrlTest;

            if (flag.UnescapedRouteMatching)
            {
                reqUrlTest = HttpUtility.UrlDecode(request.Path);
            }
            else
            {
                reqUrlTest = request.Path;
            }

            if (route.UseRegex)
            {
                pathTest = TestRouteMatchUsingRegex(route, reqUrlTest);
            }
            else
            {
                pathTest = HttpStringInternals.IsPathMatch(route.Path, reqUrlTest, MatchRoutesIgnoreCase);
            }

            if (!pathTest.IsMatched)
            {
                continue;
            }

            matchResult = RouteMatchResult.PathMatched;
            bool isMethodMatched = false;

            // test method
            if (request.Method == HttpMethod.Options)
            {
                matchResult = RouteMatchResult.OptionsMatched;
                break;
            }
            else if (flag.TreatHeadAsGetMethod && request.Method == HttpMethod.Head && route.Method == RouteMethod.Get)
            {
                isMethodMatched = true;
            }
            else if (IsMethodMatching(request.Method.Method, route.Method))
            {
                isMethodMatched = true;
            }

            if (isMethodMatched)
            {
                if (pathTest.Query is not null)
                {
                    var keys = pathTest.Query.Keys;

                    for (int j = 0; j < keys.Count; j++)
                    {
                        string? queryItem = keys[j];
                        if (string.IsNullOrEmpty(queryItem)) continue;

                        string? value = pathTest.Query[queryItem];
                        if (string.IsNullOrEmpty(value)) continue;

                        request.Query.SetItem(queryItem, HttpUtility.UrlDecode(pathTest.Query[queryItem]));
                    }
                }

                matchResult = RouteMatchResult.FullyMatched;
                matchedRoute = route;
                break;
            }
        }

        if (matchResult == RouteMatchResult.NotMatched && NotFoundErrorHandler is not null)
        {
            return new RouterExecutionResult(NotFoundErrorHandler(context), null, matchResult, null);
        }
        else if (matchResult == RouteMatchResult.OptionsMatched)
        {
            HttpResponse corsResponse = new HttpResponse();
            corsResponse.Status = System.Net.HttpStatusCode.OK;

            return new RouterExecutionResult(corsResponse, null, matchResult, null);
        }
        else if (matchResult == RouteMatchResult.PathMatched && MethodNotAllowedErrorHandler is not null)
        {
            context.MatchedRoute = matchedRoute;
            return new RouterExecutionResult(MethodNotAllowedErrorHandler(context), matchedRoute, matchResult, null);
        }
        else if (matchResult == RouteMatchResult.FullyMatched && matchedRoute != null)
        {
            context.MatchedRoute = matchedRoute;
            HttpResponse? result = null;

            if (flag.ForceTrailingSlash && !matchedRoute.UseRegex && !request.Path.EndsWith('/') && request.Method == HttpMethod.Get)
            {
                HttpResponse res = new HttpResponse();
                res.Status = HttpStatusCode.TemporaryRedirect;
                res.Headers.Add("Location", request.Path + "/" + (request.QueryString ?? ""));
                return new RouterExecutionResult(res, matchedRoute, matchResult, null);
            }

            parentServer?.handler.ContextBagCreated(context.RequestBag);

            #region Before-response handlers
            HttpResponse? rhResponse;
            Exception? rhException;
            if (GlobalRequestHandlers is not null && InvokeRequestHandlerGroup(RequestHandlerExecutionMode.BeforeResponse, GlobalRequestHandlers, matchedRoute.BypassGlobalRequestHandlers, request, context, out rhResponse, out rhException))
            {
                return new RouterExecutionResult(rhResponse, matchedRoute, matchResult, rhException);
            }
            if (matchedRoute.RequestHandlers is not null && InvokeRequestHandlerGroup(RequestHandlerExecutionMode.BeforeResponse, matchedRoute.RequestHandlers, null, request, context, out rhResponse, out rhException))
            {
                return new RouterExecutionResult(rhResponse, matchedRoute, matchResult, rhException);
            }
            #endregion

            #region Route action

            if (matchedRoute.Action is null)
            {
                throw new ArgumentNullException(string.Format(SR.Router_NoRouteActionDefined, matchedRoute));
            }

            try
            {
                context.MatchedRoute = matchedRoute;
                object actionResult = matchedRoute.Action(request);

                if (matchedRoute.isReturnTypeTask)
                {
                    ref Task<object> actionTask = ref Unsafe.As<object, Task<object>>(ref actionResult);
                    actionResult = actionTask.GetAwaiter().GetResult();
                }

                if (actionResult is HttpResponse httpres)
                {
                    result = httpres;
                }
                else
                {
                    result = ResolveAction(actionResult);
                }
            }
            catch (Exception ex)
            {
                if (!parentServer!.ServerConfiguration.ThrowExceptions && (ex is not HttpListenerException))
                {
                    if (CallbackErrorHandler is not null)
                    {
                        result = CallbackErrorHandler(ex, context);
                    }
                    else
                    {
                        result = new HttpResponse(HttpResponse.HTTPRESPONSE_ERROR);
                        return new RouterExecutionResult(result, matchedRoute, matchResult, ex);
                    }
                }
                else throw;
            }
            finally
            {
                context.RouterResponse = result;
            }
            #endregion

            #region After-response global handlers
            if (GlobalRequestHandlers is not null && InvokeRequestHandlerGroup(RequestHandlerExecutionMode.AfterResponse, GlobalRequestHandlers, matchedRoute.BypassGlobalRequestHandlers, request, context, out rhResponse, out rhException))
            {
                return new RouterExecutionResult(rhResponse, matchedRoute, matchResult, rhException);
            }
            if (matchedRoute.RequestHandlers is not null && InvokeRequestHandlerGroup(RequestHandlerExecutionMode.AfterResponse, matchedRoute.RequestHandlers, null, request, context, out rhResponse, out rhException))
            {
                return new RouterExecutionResult(rhResponse, matchedRoute, matchResult, rhException);
            }
            #endregion     
        }

        return new RouterExecutionResult(context.RouterResponse, matchedRoute, matchResult, null);
    }
}
