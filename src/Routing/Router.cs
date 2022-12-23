using Sisk.Core.Http;
using Sisk.Core.Routing.Handlers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace Sisk.Core.Routing
{
    internal class RoutePathMatchResult
    {
        public bool Matched { get; set; }
        public Dictionary<string, string>? RouteParameters { get; set; }
    }

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
        private List<Route> routes = new List<Route>();
        internal HttpServer? ParentServer { get; set; }
        internal ListeningHost ParentListenerHost { get; set; } = null!;

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
        public Route[] GetDefinedRoutes() => routes.ToArray();

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
            foreach (Route r in this.routes)
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
            routes.Add(new Route(method, path, null, callback, null));
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
            routes.Add(new Route(method, path, name, callback, null));
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
            routes.Add(new Route(method, path, name, callback, middlewares));
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
                routes.Insert(0, r);
            }
            else
            {
                routes.Add(r);
            }
        }

        /// <summary>
        /// Searches the object instance for methods with attribute <see cref="RouteAttribute"/> and optionals <see cref="RequestHandlerAttribute"/>, and creates routes from them.
        /// </summary>
        /// <param name="attrClassInstance">The instance of the class where the methods are. The routing methods must be static and marked with <see cref="RouteAttribute"/>.</param>
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
            Type t = attrClassInstance.GetType();
            foreach (var method in t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
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
                        RouterCallback r = (RouterCallback)Delegate.CreateDelegate(typeof(RouterCallback), method);
                        SetRoute(atrInstance.Method, atrInstance.Path, r, atrInstance.Name, methodHandlers.ToArray());
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

        private RoutePathMatchResult TestRouteMatchUsingRegex(string routePath, string requestPath)
        {
            return new RoutePathMatchResult()
            {
                Matched = Regex.IsMatch(requestPath, routePath, RegexOptions.Compiled),
                RouteParameters = null
            };
        }

        private RoutePathMatchResult TestRouteMatchUsingDefaultTemplate(string routePath, string requestPath)
        {
            routePath = Regex.Escape(routePath);
            routePath = routePath.Replace("<", "(?<");
            routePath = routePath.Replace(">", ">[^/\\?#])+[/\\?#]?");
            routePath = $"^{routePath.Trim()}$";

            Dictionary<string, string> routeParams = new Dictionary<string, string>();
            var matches = Regex.Matches(requestPath, routePath, RegexOptions.Compiled);
            foreach (Match match in matches)
            {
                foreach (Group group in match.Groups)
                {
                    if (Int32.TryParse(group.Name, out _))
                        continue;
                    string grpName = group.Name;
                    string grpContent = "";
                    if (grpName.Trim() == "")
                        continue;
                    foreach (Capture capture in group.Captures)
                    {
                        grpContent += capture.Value;
                    }
                    routeParams.Add(grpName, grpContent);
                }
            }

            return new RoutePathMatchResult() { Matched = matches.Count > 0, RouteParameters = routeParams };
        }

        internal HttpResponse? Execute(HttpRequest request)
        {
            HttpContext? context = null;
            Route? matchedRoute = null;
            RouteMatchResult matchResult = RouteMatchResult.NotMatched;

            foreach (Route route in routes)
            {
                // test path
                RoutePathMatchResult pathTest;
                if (route.UseRegex)
                {
                    pathTest = TestRouteMatchUsingRegex(route.Path, request.Path);
                }
                else
                {
                    pathTest = TestRouteMatchUsingDefaultTemplate(route.Path, request.Path);
                }

                if (!pathTest.Matched)
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
                else if (IsMethodMatching(request.Method.Method, route.Method))
                {
                    isMethodMatched = true;
                }

                if (isMethodMatched)
                {
                    if (pathTest.RouteParameters is not null)
                    {
                        foreach (KeyValuePair<string, string> routeParam in pathTest.RouteParameters)
                        {
                            request.Query.Add(routeParam.Key, HttpUtility.UrlDecode(routeParam.Value));
                        }
                    }
                    matchResult = RouteMatchResult.FullyMatched;
                    matchedRoute = route;
                    break;
                }
            }

            if (matchResult == RouteMatchResult.NotMatched && NotFoundErrorHandler is not null)
            {
                return NotFoundErrorHandler();
            }
            else if (matchResult == RouteMatchResult.OptionsMatched)
            {
                HttpResponse corsResponse = new HttpResponse();
                corsResponse.Status = System.Net.HttpStatusCode.OK;

                return corsResponse;
            }
            else if (matchResult == RouteMatchResult.PathMatched && MethodNotAllowedErrorHandler is not null)
            {
                return MethodNotAllowedErrorHandler();
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
                            return handlerResponse;
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
                            return handlerResponse;
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
                        return handlerResponse;
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
                        return handlerResponse;
                    }
                }
            }

            return context?.RouterResponse;
        }
    }

    internal enum RouteMatchResult
    {
        FullyMatched,
        PathMatched,
        OptionsMatched,
        NotMatched
    }
}
