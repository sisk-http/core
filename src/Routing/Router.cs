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
