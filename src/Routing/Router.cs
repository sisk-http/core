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
    public partial class Router
    {
        internal record RouterExecutionResult(HttpResponse? Response, Route? Route, RouteMatchResult Result, Exception? Exception);
        internal HttpServer? ParentServer { get; private set; }
        private HashSet<Route> _routes = new HashSet<Route>();
        private bool throwException = false;
        private Dictionary<Type, Func<object, HttpResponse>> actionHandlers;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal void BindServer(HttpServer server)
        {
            if (object.ReferenceEquals(server, this.ParentServer)) return;
            this.ParentServer = server;
            this.throwException = server.ServerConfiguration.ThrowExceptions;
        }

        /// <summary>
        /// Concats the two routes paths into one.
        /// </summary>
        /// <param name="path1">The first path to concat;</param>
        /// <param name="path2">The second path to concat.</param>
        /// <definition>
        /// public static string CombinePaths(string path1, string path2)
        /// </definition>
        /// <type>
        /// Static method
        /// </type>
        public static string CombinePaths(string path1, string path2)
        {
            if (path1 == null) throw new ArgumentNullException(nameof(path1));
            if (path2 == null) throw new ArgumentNullException(nameof(path2));

            return Internal.HttpStringInternals.CombineRoutePaths(path1, path2);
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
        /// Gets or sets the Router callback exception handler.
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
                throw new ArgumentException("Cannot register HttpResponse as an valid type to the action handler.");
            }
            if (actionHandlers.ContainsKey(type))
            {
                throw new ArgumentException("The specified type is already defined in this router instance.");
            }
            actionHandlers.Add(type, actionHandler);
        }

        HttpResponse ResolveAction(object routeResult)
        {
            Type actionType = routeResult.GetType();
            if (routeResult == null)
            {
                throw new ArgumentNullException("Action result values cannot be null values.");
            }
            Type? matchedType = null;
            foreach (Type tkey in actionHandlers.Keys)
            {
                if (actionType.IsAssignableTo(tkey))
                {
                    matchedType = tkey;
                    break;
                }
            }
            if (matchedType == null)
            {
                throw new InvalidOperationException($"Action of type \"{actionType.FullName}\" doens't have an action handler registered on the router that issued it.");
            }

            var actionHandler = actionHandlers[matchedType];

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
