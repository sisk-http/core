using Sisk.Core.Http;
using Sisk.Core.Routing.Handlers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

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
        internal record RouterExecutionResult(HttpResponse? Response, Route? Route, RouteMatchResult Result);
        private Internal.WildcardMatching _pathMatcher = new Internal.WildcardMatching();
        private List<Route> _routes = new List<Route>();
        internal HttpServer? ParentServer { get; set; }
        internal ListeningHost ParentListenerHost { get; set; } = null!;

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
            _routes.Add(new Route(method, path, null, callback, null));
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
            _routes.Add(new Route(method, path, name, callback, null));
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
            _routes.Add(new Route(method, path, name, callback, middlewares));
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
            if (r.Path == "*")
            {
                _routes.Insert(0, r);
            }
            else
            {
                _routes.Add(r);
            }
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
            this.SetTypeRoutes(attrClassInstance, true);
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
        public void SetObject(Type attrClassType)
        {
            this.SetTypeRoutes(attrClassType, false);
        }

        private void SetTypeRoutes(object t, bool isInstance)
        {
            MethodInfo[] methods;
            if (isInstance)
            {
                methods = t.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }
            else
            {
                methods = ((Type)t).GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            }
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
                        if (isInstance)
                        {
                            r = (RouterCallback)Delegate.CreateDelegate(typeof(RouterCallback), t, method);
                        }
                        else
                        {
                            r = (RouterCallback)Delegate.CreateDelegate(typeof(RouterCallback), method);
                        }
                        Route route = new Route()
                        {
                            Method = atrInstance.Method,
                            Path = atrInstance.Path,
                            Callback = r,
                            Name = atrInstance.Name,
                            RequestHandlers = methodHandlers.ToArray(),
                            LogMode = atrInstance.LogMode
                        };
                        SetRoute(route);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Couldn't set method {method.Name} as an route. See inner exception.", ex);
                    }
                }
            }
        }

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

        internal RouterExecutionResult Execute(HttpRequest request)
        {
            HttpContext? context = null;
            Route? matchedRoute = null;
            RouteMatchResult matchResult = RouteMatchResult.NotMatched;

            try
            {
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
                        pathTest = _pathMatcher.IsPathMatch(route.Path, request.Path, MatchRoutesIgnoreCase);
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
                    else if (ParentServer!.ServerConfiguration.Flags.TreatHeadAsGetMethod && (request.Method == HttpMethod.Head && route.Method == RouteMethod.Get))
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
                    return new RouterExecutionResult(NotFoundErrorHandler(), null, matchResult);
                }
                else if (matchResult == RouteMatchResult.OptionsMatched)
                {
                    HttpResponse corsResponse = new HttpResponse();
                    corsResponse.Status = System.Net.HttpStatusCode.OK;

                    return new RouterExecutionResult(corsResponse, null, matchResult);
                }
                else if (matchResult == RouteMatchResult.PathMatched && MethodNotAllowedErrorHandler is not null)
                {
                    return new RouterExecutionResult(MethodNotAllowedErrorHandler(), matchedRoute, matchResult);
                }
                else if (matchResult == RouteMatchResult.FullyMatched)
                {
                    HttpResponse? result = null;
                    context = new HttpContext(new Dictionary<string, object?>(), this.ParentServer, matchedRoute);
                    request.Context = context;

                    // (BEFORE) global handlers
                    if (this.GlobalRequestHandlers is not null)
                    {
                        foreach (IRequestHandler handler in this.GlobalRequestHandlers.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.BeforeResponse))
                        {
                            bool isBypassed = false;
                            foreach (IRequestHandler bypassed in matchedRoute!.BypassGlobalRequestHandlers ?? new IRequestHandler[] { })
                            {
                                if (bypassed.Identifier == handler.Identifier)
                                {
                                    isBypassed = true;
                                }
                            }
                            if (isBypassed)
                                continue;
                            HttpResponse? handlerResponse = handler.Execute(request, context);
                            if (handlerResponse is not null)
                            {
                                return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult);
                            }
                        }
                    }

                    // specific before handlers for route
                    if (matchedRoute!.RequestHandlers is not null)
                    {
                        foreach (IRequestHandler handler in matchedRoute.RequestHandlers.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.BeforeResponse))
                        {
                            HttpResponse? handlerResponse = handler.Execute(request, context);
                            if (handlerResponse is not null)
                            {
                                return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult);
                            }
                        }
                    }

                    if (matchedRoute.Callback is null)
                    {
                        throw new ArgumentNullException(nameof(matchedRoute.Callback));
                    }

                    if (CallbackErrorHandler is not null)
                    {
                        try
                        {
                            result = matchedRoute.Callback(request);
                        }
                        catch (Exception ex)
                        {
                            result = CallbackErrorHandler(ex, request);
                        }
                    }
                    else
                    {
                        result = matchedRoute.Callback(request);
                    }

                    context.RouterResponse = result;
                }

                // after GLOBAL request handlers
                if (this.GlobalRequestHandlers is not null && context != null)
                {
                    foreach (IRequestHandler handler in this.GlobalRequestHandlers.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.AfterResponse))
                    {
                        bool isBypassed = false;
                        foreach (IRequestHandler bypassed in matchedRoute!.BypassGlobalRequestHandlers ?? new IRequestHandler[] { })
                        {
                            if (bypassed.Identifier == handler.Identifier)
                            {
                                isBypassed = true;
                            }
                        }

                        if (isBypassed)
                            continue;

                        HttpResponse? handlerResponse = handler.Execute(request, context);

                        if (handlerResponse is not null)
                        {
                            return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult);
                        }
                    }
                }

                // after SPECIFIC request handlers
                if (matchedRoute!.RequestHandlers is not null && context != null)
                {
                    foreach (IRequestHandler handler in matchedRoute.RequestHandlers.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.AfterResponse))
                    {
                        HttpResponse? handlerResponse = handler.Execute(request, context);

                        if (handlerResponse is not null)
                        {
                            return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult);
                        }
                    }
                }

                return new RouterExecutionResult(context?.RouterResponse, matchedRoute, matchResult);
            }
            catch (Exception)
            {
                if (ParentServer?.ServerConfiguration.ThrowExceptions ?? false)
                {
                    throw;
                }
                else
                {
                    return new RouterExecutionResult(new HttpResponse(HttpResponse.HTTPRESPONSE_ERROR), matchedRoute, matchResult);
                }
            }
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
