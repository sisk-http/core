// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Router.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Internal;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

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
    public partial class Router
    {
        internal record RouterExecutionResult(HttpResponse? Response, Route? Route, RouteMatchResult Result, Exception? Exception);

        internal HttpServer? ParentServer { get; private set; }
        internal IList<Route> _routes = new List<Route>();
        private bool throwException = false;
        private IDictionary<Type, Func<object, HttpResponse>> actionHandlers;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal void BindServer(HttpServer server)
        {
            if (ReferenceEquals(server, ParentServer)) return;

            ParentServer = server;
            throwException = server.ServerConfiguration.ThrowExceptions;
            server.handler.SetupRouter(this);

            _routes = _routes.ToImmutableArray();
            actionHandlers = actionHandlers.ToImmutableDictionary();
        }

        /// <summary>
        /// Combines the specified URL paths into one.
        /// </summary>
        /// <param name="paths">The string array which contains parts that will be combined.</param>
        /// <definition>
        /// public static string CombinePaths(params string[] paths)
        /// </definition>
        /// <type>
        /// Static method
        /// </type>
        public static string CombinePaths(params string[] paths)
        {
            return PathUtility.CombinePaths(paths);
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
        public Router()
        {
            actionHandlers = new Dictionary<Type, Func<object, HttpResponse>>();
        }

        /// <summary>
        /// Gets or sets the global requests handlers that will be executed in all matched routes.
        /// </summary>
        /// <definition>
        /// public IRequestHandler[]? GlobalRequestHandlers { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public IRequestHandler[]? GlobalRequestHandlers { get; set; }

        /// <summary>
        /// Gets or sets the Router action exception handler.
        /// </summary>
        /// <definition>
        /// public ExceptionErrorCallback? CallbackErrorHandler { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
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
        public Route[] GetDefinedRoutes() => _routes.ToArray();

        /// <summary>
        /// Register an type handling association to converting it to an <see cref="HttpResponse"/> object.
        /// </summary>
        /// <param name="actionHandler">The function that receives an object of the T and returns an <see cref="HttpResponse"/> response from the informed object.</param>
        /// <definition>
        /// public void RegisterActionAssociation{{T}}(T type, Action{{T, HttpResponse}} actionHandler) where T : Type
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void RegisterValueHandler<T>(Func<object, HttpResponse> actionHandler)
        {
            Type type = typeof(T);
            if (type == typeof(HttpResponse))
            {
                throw new ArgumentException(SR.Router_Handler_HttpResponseRegister);
            }
            if (actionHandlers.ContainsKey(type))
            {
                throw new ArgumentException(SR.Router_Handler_Duplicate);
            }
            actionHandlers.Add(type, actionHandler);
        }

        HttpResponse ResolveAction(object routeResult)
        {
            if (routeResult is null)
            {
                throw new ArgumentNullException(SR.Router_Handler_ActionNullValue);
            }

            Type actionType = routeResult.GetType();
            if (!actionHandlers.TryGetValue(actionType, out var actionHandler))
            {
                throw new InvalidOperationException(string.Format(SR.Router_Handler_UnrecognizedAction, actionType.FullName));
            }

            return actionHandler(routeResult);
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
