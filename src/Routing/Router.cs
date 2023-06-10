using Sisk.Core.Http;
using Sisk.Core.Internal;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents a collection of Routes and main executor of callbacks in an <see cref="HttpServer"/>.
    /// </summary>
    /// <definition>
    /// public class Router
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing
    /// </namespace>
    public class Router
    {
        internal record RouterExecutionResult(HttpResponse? Response, Route? Route, RouteMatchResult Result, Exception? Exception);
        private List<Route> _routes = new List<Route>();
        internal HttpServer? ParentServer { get; private set; }
        private bool throwException = false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void BindServer(HttpServer server)
        {
            if (object.ReferenceEquals(server, this.ParentServer)) return;
            this.ParentServer = server;
            this.throwException = server.ServerConfiguration.ThrowExceptions;
        }

        /// <summary>
        /// Gets or sets whether this <see cref="Router"/> will match routes ignoring case.
        /// </summary>
        /// <definition>
        /// public bool MatchRoutesIgnoreCase { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public bool MatchRoutesIgnoreCase { get; set; } = false;

        /// <summary>
        /// Creates an new <see cref="Router"/> instance with default properties values.
        /// </summary>
        /// <definition>
        /// public Router()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public Router() { }

        /// <summary>
        /// Gets or sets the global requests handlers that will be executed in all matched routes.
        /// </summary>
        /// <definition>
        /// public IRequestHandler[]? GlobalRequestHandlers { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public IRequestHandler[]? GlobalRequestHandlers { get; set; }

        /// <summary>
        /// Gets or sets the Router callback exception handler.
        /// </summary>
        /// <definition>
        /// public ExceptionErrorCallback? CallbackErrorHandler { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public ExceptionErrorCallback? CallbackErrorHandler { get; set; }

        /// <summary>
        /// Gets or sets the Router "404 Not Found" handler.
        /// </summary>
        /// <definition>
        /// public NoMatchedRouteErrorCallback? NotFoundErrorHandler { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public NoMatchedRouteErrorCallback? NotFoundErrorHandler { get; set; } = new NoMatchedRouteErrorCallback(
            () => new HttpResponse(System.Net.HttpStatusCode.NotFound));

        /// <summary>
        /// Gets or sets the Router "405 Method Not Allowed" handler.
        /// </summary>
        /// <definition>
        /// public NoMatchedRouteErrorCallback? MethodNotAllowedErrorHandler { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public NoMatchedRouteErrorCallback? MethodNotAllowedErrorHandler { get; set; } = new NoMatchedRouteErrorCallback(
            () => new HttpResponse(System.Net.HttpStatusCode.MethodNotAllowed));

        /// <summary>
        /// Gets all routes defined on this router instance.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public Route[] GetDefinedRoutes()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public Route[] GetDefinedRoutes() => _routes.ToArray();

        #region "Route setters"
        /// <summary>
        /// Defines an route to an router.
        /// </summary>
        /// <param name="r">The router instance which the route is being set.</param>
        /// <param name="route">The route to be defined in the router.</param>
        /// <definition>
        /// public static Router operator +(Router r, Route route)
        /// </definition>
        /// <type>
        /// Operator
        /// </type>
        public static Router operator +(Router r, Route route)
        {
            r.SetRoute(route);
            return r;
        }

        /// <summary>
        /// Gets an route object by their name that is defined in this Router.
        /// </summary>
        /// <param name="name">The route name.</param>
        /// <returns></returns>
        /// <definition>
        /// public Route? GetRouteFromName(string name)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public Route? GetRouteFromName(string name)
        {
            foreach (Route r in this._routes)
            {
                if (r.Name == name)
                {
                    return r;
                }
            }
            return null;
        }

        /// <summary>
        /// Defines an route with their method, path and callback function.
        /// </summary>
        /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
        /// <param name="path">The route path.</param>
        /// <param name="callback">The route function to be called after matched.</param>
        /// <definition>
        /// public void SetRoute(RouteMethod method, string path, RouterCallback callback)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing 
        /// </namespace>
        public void SetRoute(RouteMethod method, string path, RouterCallback callback)
        {
            Route newRoute = new Route(method, path, null, callback, null);
            Route? collisonRoute;
            if ((collisonRoute = GetCollisionRoute(newRoute.Method, newRoute.Path)) != null)
            {
                throw new ArgumentException($"A possible route collision could happen between route {newRoute} and route {collisonRoute}. Please review the methods and paths of these routes.");
            }
            _routes.Add(newRoute);
        }

        /// <summary>
        /// Defines an route with their method, path, callback function and name.
        /// </summary>
        /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
        /// <param name="path">The route path.</param>
        /// <param name="callback">The route function to be called after matched.</param>
        /// <param name="name">The route name.</param>
        /// <definition>
        /// public void SetRoute(RouteMethod method, string path, RouterCallback callback, string? name)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public void SetRoute(RouteMethod method, string path, RouterCallback callback, string? name)
        {
            Route newRoute = new Route(method, path, name, callback, null);
            Route? collisonRoute;
            if ((collisonRoute = GetCollisionRoute(newRoute.Method, newRoute.Path)) != null)
            {
                throw new ArgumentException($"A possible route collision could happen between route {newRoute} and route {collisonRoute}. Please review the methods and paths of these routes.");
            }
            _routes.Add(newRoute);
        }

        /// <summary>
        /// Defines an route with their method, path, callback function, name and request handlers.
        /// </summary>
        /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
        /// <param name="path">The route path.</param>
        /// <param name="callback">The route function to be called after matched.</param>
        /// <param name="name">The route name.</param>
        /// <param name="middlewares">Handlers that run before calling your route callback.</param>
        /// <definition>
        /// public void SetRoute(RouteMethod method, string path, RouterCallback callback, string? name, IRequestHandler[] middlewares)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public void SetRoute(RouteMethod method, string path, RouterCallback callback, string? name, IRequestHandler[] middlewares)
        {
            Route newRoute = new Route(method, path, name, callback, middlewares);
            Route? collisonRoute;
            if ((collisonRoute = GetCollisionRoute(newRoute.Method, newRoute.Path)) != null)
            {
                throw new ArgumentException($"A possible route collision could happen between route {newRoute} and route {collisonRoute}. Please review the methods and paths of these routes.");
            }
            _routes.Add(newRoute);
        }

        /// <summary>
        /// Defines an route in this Router instance.
        /// </summary>
        /// <param name="r">The route to be defined in the Router.</param>
        /// <definition>
        /// public void SetRoute(Route r)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public void SetRoute(Route r)
        {
            Route? collisonRoute;
            if (!r.UseRegex && (collisonRoute = GetCollisionRoute(r.Method, r.Path)) != null)
            {
                throw new ArgumentException($"A possible route collision could happen between route {r} and route {collisonRoute}. Please review the methods and paths of these routes.");
            }
            _routes.Add(r);
        }

        /// <summary>
        /// Searches the object instance for methods with attribute <see cref="RouteAttribute"/> and optionals <see cref="RequestHandlerAttribute"/>, and creates routes from them.
        /// </summary>
        /// <param name="attrClassInstance">The instance of the class where the instance methods are. The routing methods must be instance methods and marked with <see cref="RouteAttribute"/>.</param>
        /// <exception cref="Exception">An exception is thrown when a method has an erroneous signature.</exception>
        /// <definition>
        /// public void SetObject(object attrClassInstance)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public void SetObject(object attrClassInstance)
        {
            Type attrClassType = attrClassInstance.GetType();
            MethodInfo[] methods = attrClassType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            SetInternal(methods, attrClassInstance);
        }

        /// <summary>
        /// Searches the object instance for methods with attribute <see cref="RouteAttribute"/> and optionals <see cref="RequestHandlerAttribute"/>, and creates routes from them.
        /// </summary>
        /// <param name="attrClassType">The type of the class where the static methods are. The routing methods must be static and marked with <see cref="RouteAttribute"/>.</param>
        /// <exception cref="Exception">An exception is thrown when a method has an erroneous signature.</exception>
        /// <definition>
        /// public void SetObject(Type attrClassType)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public void SetObject([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type attrClassType)
        {
            MethodInfo[] methods = attrClassType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            SetInternal(methods, null);
        }

        private void SetInternal(MethodInfo[] methods, object? instance)
        {
            foreach (var method in methods)
            {
                RouteAttribute? atrInstance = method.GetCustomAttribute<RouteAttribute>();
                IEnumerable<RequestHandlerAttribute> handlersInstances = method.GetCustomAttributes<RequestHandlerAttribute>();

                if (atrInstance != null)
                {
                    List<IRequestHandler> methodHandlers = new List<IRequestHandler>();
                    if (handlersInstances.Count() > 0)
                    {
                        foreach (RequestHandlerAttribute atr in handlersInstances)
                        {
                            IRequestHandler rhandler = (IRequestHandler)Activator.CreateInstance(atr.RequestHandlerType, atr.ConstructorArguments)!;
                            methodHandlers.Add(rhandler);
                        }
                    }

                    try
                    {
                        RouterCallback r;

                        if (instance == null)
                        {
                            r = (RouterCallback)Delegate.CreateDelegate(typeof(RouterCallback), method);
                        }
                        else
                        {
                            r = (RouterCallback)Delegate.CreateDelegate(typeof(RouterCallback), instance, method);
                        }

                        Route route = new Route()
                        {
                            Method = atrInstance.Method,
                            Path = atrInstance.Path,
                            Callback = r,
                            Name = atrInstance.Name,
                            RequestHandlers = methodHandlers.ToArray(),
                            LogMode = atrInstance.LogMode,
                            UseCors = atrInstance.UseCors
                        };

                        Route? collisonRoute;
                        if ((collisonRoute = GetCollisionRoute(route.Method, route.Path)) != null)
                        {
                            throw new ArgumentException($"A possible route collision could happen between the route {route} at {method.Name} with route {collisonRoute}. Please review the methods and paths of these routes.");
                        }

                        SetRoute(route);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Couldn't set method {method.Name} as an route. See inner exception.", ex);
                    }
                }
            }
        }
        #endregion

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

        private Internal.WildcardMatching.PathMatchResult TestRouteMatchUsingRegex(string routePath, string requestPath)
        {
            return new Internal.WildcardMatching.PathMatchResult
                (Regex.IsMatch(requestPath, routePath, MatchRoutesIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None), new System.Collections.Specialized.NameValueCollection());
        }

        private Route? GetCollisionRoute(RouteMethod method, string path)
        {
            if (!path.StartsWith('/'))
            {
                throw new ArgumentException("Route paths must start with /.");
            }
            foreach (Route r in this._routes)
            {
                bool methodMatch = method != RouteMethod.Any && method == r.Method;
                bool pathMatch = WildcardMatching.IsPathMatch(r.Path, path, MatchRoutesIgnoreCase).IsMatched;

                if (methodMatch && pathMatch)
                {
                    return r;
                }
            }
            return null;
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
                        result = CallbackErrorHandler(ex, request);
                    }
                }
                else throw;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal RouterExecutionResult Execute(HttpRequest request, HttpListenerRequest baseRequest)
        {
            Route? matchedRoute = null;
            RouteMatchResult matchResult = RouteMatchResult.NotMatched;
            HttpContext? context = new HttpContext(new Dictionary<string, object?>(), this.ParentServer, matchedRoute);
            HttpServerFlags flag = ParentServer!.ServerConfiguration.Flags;
            request.Context = context;
            bool hasGlobalHandlers = this.GlobalRequestHandlers?.Length > 0;

            foreach (Route route in _routes)
            {
                // test path
                Internal.WildcardMatching.PathMatchResult pathTest;
                if (route.UseRegex)
                {
                    pathTest = TestRouteMatchUsingRegex(route.Path, request.Path);
                }
                else
                {
                    pathTest = WildcardMatching.IsPathMatch(route.Path, request.Path, MatchRoutesIgnoreCase);
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
                    result = matchedRoute.Callback(request);
                }
                catch (Exception ex)
                {
                    if (!throwException)
                    {
                        if (CallbackErrorHandler is not null)
                        {
                            result = CallbackErrorHandler(ex, request);
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

    internal enum RouteMatchResult
    {
        FullyMatched,
        PathMatched,
        OptionsMatched,
        HeadMatched,
        NotMatched
    }
}
