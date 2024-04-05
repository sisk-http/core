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
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;

namespace Sisk.Core.Routing;

public partial class Router
{
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
        if (route.routeRegex == null)
        {
            route.routeRegex = new Regex(route.Path, MatchRoutesIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }

        var test = route.routeRegex.Match(requestPath);
        if (test.Success)
        {
            NameValueCollection query = new NameValueCollection();
            foreach (Group group in test.Groups)
            {
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

    internal IEnumerable<IRequestHandler> FilterRequestHandlerEnumerate(RequestHandlerExecutionMode mode, params IEnumerable<IRequestHandler>?[] baseLists)
    {
        foreach (var baseList in baseLists)
        {
            if (baseList is null)
            {
                continue;
            }
            foreach (var handler in baseList)
            {
                if (handler.ExecutionMode == mode)
                    yield return handler;
            }
        }
    }

    internal HttpResponse? InvokeHandler(IRequestHandler handler, HttpRequest request, HttpContext context, IRequestHandler[]? bypass)
    {
        if (bypass is not null && bypass.Contains(handler))
        {
            return null;
        }

        HttpResponse? result = null;
        try
        {
            result = handler.Execute(request, context);
        }
        catch (Exception ex)
        {
            if (!throwException)
            {
                if (CallbackErrorHandler is not null)
                {
                    result = CallbackErrorHandler(ex, context);
                }
                else { /* do nothing */ };
            }
            else throw;
        }

        return result;
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Task<> is already included with dynamic dependency.")]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal async Task<RouterExecutionResult> Execute(HttpContext context)
    {
        if (ParentServer == null) throw new InvalidOperationException(SR.Router_NotBinded);

        context.Router = this;
        HttpRequest request = context.Request;
        Route? matchedRoute = null;
        RouteMatchResult matchResult = RouteMatchResult.NotMatched;

        HttpServerFlags flag = ParentServer!.ServerConfiguration.Flags;

        foreach (Route route in _routes)
        {
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
                if (pathTest.Query != null)
                    foreach (string routeParam in pathTest.Query)
                        request.Query.SetItem(routeParam, HttpUtility.UrlDecode(pathTest.Query[routeParam]));

                matchResult = RouteMatchResult.FullyMatched;
                matchedRoute = route;
                break;
            }
        }

        if (matchResult == RouteMatchResult.NotMatched && NotFoundErrorHandler is not null)
        {
            return new RouterExecutionResult(NotFoundErrorHandler(), null, matchResult, null);
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

            ParentServer?.handler.ContextBagCreated(context.RequestBag);

            #region Before-response handlers
            foreach (IRequestHandler handler in FilterRequestHandlerEnumerate(RequestHandlerExecutionMode.BeforeResponse, GlobalRequestHandlers, matchedRoute.RequestHandlers))
            {
                var handlerResponse = InvokeHandler(handler, request, context, matchedRoute.BypassGlobalRequestHandlers);
                if (handlerResponse is not null)
                {
                    return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult, null);
                }
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
                    actionResult = await Unsafe.As<object, Task<object>>(ref actionResult);
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
                if (!throwException && (ex is not HttpListenerException))
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
            foreach (IRequestHandler handler in FilterRequestHandlerEnumerate(RequestHandlerExecutionMode.AfterResponse, GlobalRequestHandlers, matchedRoute.RequestHandlers))
            {
                var handlerResponse = InvokeHandler(handler, request, context, matchedRoute.BypassGlobalRequestHandlers);
                if (handlerResponse is not null)
                {
                    return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult, null);
                }
            }
            #endregion     
        }

        return new RouterExecutionResult(context.RouterResponse, matchedRoute, matchResult, null);
    }
}
