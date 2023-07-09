﻿using Sisk.Core.Http;
using Sisk.Core.Internal;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;

namespace Sisk.Core.Routing;

public partial class Router
{
    private bool IsMethodMatching(string ogRqMethod, RouteMethod method)
    {
        Enum.TryParse(typeof(RouteMethod), ogRqMethod, true, out object? ogRqParsedObj);
        if (ogRqParsedObj is null)
        {
            return false;
        }
        RouteMethod ogRqParsed = (RouteMethod)ogRqParsedObj!;
        return method.HasFlag(ogRqParsed);
    }

    private Internal.HttpStringInternals.PathMatchResult TestRouteMatchUsingRegex(string routePath, string requestPath)
    {
        return new Internal.HttpStringInternals.PathMatchResult
            (Regex.IsMatch(requestPath, routePath, MatchRoutesIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None), new System.Collections.Specialized.NameValueCollection());
    }

    internal HttpResponse? InvokeHandler(IRequestHandler handler, HttpRequest request, HttpContext context, IRequestHandler[]? bypass)
    {
        HttpResponse? result = null;
        if (bypass != null)
        {
            bool isBypassed = false;
            foreach (IRequestHandler bypassed in bypass)
            {
                if (object.ReferenceEquals(bypassed, handler))
                {
                    isBypassed = true;
                    break;
                }
            }
            if (isBypassed) return null;
        }

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
            }
            else throw;
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal RouterExecutionResult Execute(HttpRequest request, HttpListenerRequest baseRequest, ListeningHost matchedHost, ref HttpContext? context)
    {
        Route? matchedRoute = null;
        RouteMatchResult matchResult = RouteMatchResult.NotMatched;

        context = new HttpContext(this.ParentServer, request, matchedRoute, matchedHost);
        HttpServerFlags flag = ParentServer!.ServerConfiguration.Flags;
        request.Context = context;
        bool hasGlobalHandlers = this.GlobalRequestHandlers?.Length > 0;

        foreach (Route route in _routes)
        {
            // test path
            Internal.HttpStringInternals.PathMatchResult pathTest;
            if (route.UseRegex)
            {
                pathTest = TestRouteMatchUsingRegex(route.Path, request.Path);
            }
            else
            {
                pathTest = HttpStringInternals.IsPathMatch(route.Path, request.Path, MatchRoutesIgnoreCase);
            }

            if (!pathTest.IsMatched)
            {
                continue;
            }

            matchResult = RouteMatchResult.PathMatched;
            bool isMethodMatched = false;

            // test method
            if (route.Method == RouteMethod.Any)
            {
                isMethodMatched = true;
            }
            else if (request.Method == HttpMethod.Options)
            {
                matchResult = RouteMatchResult.OptionsMatched;
                break;
            }
            else if (flag.TreatHeadAsGetMethod && (request.Method == HttpMethod.Head && route.Method == RouteMethod.Get))
            {
                isMethodMatched = true;
            }
            else if (IsMethodMatching(request.Method.Method, route.Method))
            {
                isMethodMatched = true;
            }

            if (isMethodMatched)
            {
                foreach (string routeParam in pathTest.Query)
                {
                    request.Query.Add(routeParam, HttpUtility.UrlDecode(pathTest.Query[routeParam]));
                }
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
            return new RouterExecutionResult(MethodNotAllowedErrorHandler(), matchedRoute, matchResult, null);
        }
        else if (matchResult == RouteMatchResult.FullyMatched && matchedRoute != null)
        {
            context.MatchedRoute = matchedRoute;
            HttpResponse? result = null;

            if (flag.ForceTrailingSlash && !matchedRoute.UseRegex && !request.Path.EndsWith('/'))
            {
                HttpResponse res = new HttpResponse();
                res.Status = HttpStatusCode.MovedPermanently;
                res.Headers.Add("Location", request.Path + "/" + (request.QueryString ?? ""));
                return new RouterExecutionResult(res, matchedRoute, matchResult, null);
            }

            #region Before-contents global handlers
            if (hasGlobalHandlers)
            {
                foreach (IRequestHandler handler in this.GlobalRequestHandlers!.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.BeforeContents))
                {
                    var handlerResponse = InvokeHandler(handler, request, context, matchedRoute.BypassGlobalRequestHandlers);
                    if (handlerResponse is not null)
                    {
                        return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult, null);
                    }
                }
            }
            #endregion

            #region Before-contents route-specific handlers
            if (matchedRoute!.RequestHandlers?.Length > 0)
            {
                foreach (IRequestHandler handler in matchedRoute.RequestHandlers.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.BeforeContents))
                {
                    var handlerResponse = InvokeHandler(handler, request, context, matchedRoute.BypassGlobalRequestHandlers);
                    if (handlerResponse is not null)
                    {
                        return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult, null);
                    }
                }
            }
            #endregion

            request.ImportContents(baseRequest.InputStream);

            #region Before-response global handlers
            if (hasGlobalHandlers)
            {
                foreach (IRequestHandler handler in this.GlobalRequestHandlers!.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.BeforeResponse))
                {
                    var handlerResponse = InvokeHandler(handler, request, context, matchedRoute.BypassGlobalRequestHandlers);
                    if (handlerResponse is not null)
                    {
                        return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult, null);
                    }
                }
            }
            #endregion

            #region Before-response route-specific handlers
            if (matchedRoute!.RequestHandlers?.Length > 0)
            {
                foreach (IRequestHandler handler in matchedRoute.RequestHandlers.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.BeforeResponse))
                {
                    var handlerResponse = InvokeHandler(handler, request, context, matchedRoute.BypassGlobalRequestHandlers);
                    if (handlerResponse is not null)
                    {
                        return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult, null);
                    }
                }
            }
            #endregion

            #region Route callback

            if (matchedRoute.Callback is null)
            {
                throw new ArgumentNullException("No route callback was defined to the route " + matchedRoute.ToString());
            }

            try
            {
                context.MatchedRoute = matchedRoute;
                result = matchedRoute.Callback(request);
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
                        return new RouterExecutionResult(new HttpResponse(HttpResponse.HTTPRESPONSE_ERROR), matchedRoute, matchResult, ex);
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
            if (hasGlobalHandlers)
            {
                foreach (IRequestHandler handler in this.GlobalRequestHandlers!.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.AfterResponse))
                {
                    var handlerResponse = InvokeHandler(handler, request, context, matchedRoute.BypassGlobalRequestHandlers);
                    if (handlerResponse is not null)
                    {
                        return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult, null);
                    }
                }
            }
            #endregion

            #region After-response route-specific handlers
            if (matchedRoute!.RequestHandlers?.Length > 0)
            {
                foreach (IRequestHandler handler in matchedRoute.RequestHandlers.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.AfterResponse))
                {
                    var handlerResponse = InvokeHandler(handler, request, context, matchedRoute.BypassGlobalRequestHandlers);
                    if (handlerResponse is not null)
                    {
                        return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult, null);
                    }
                }
            }
            #endregion
        }

        return new RouterExecutionResult(context?.RouterResponse, matchedRoute, matchResult, null);
    }
}