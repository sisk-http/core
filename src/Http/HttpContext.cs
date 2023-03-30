using Sisk.Core.Routing;
using System.Dynamic;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents an context for Http requests.
    /// </summary>
    /// <definition>
    /// public class HttpContext
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class HttpContext
    {
        /// <summary>
        /// Gets or sets a managed object that is accessed and modified by request handlers.
        /// </summary>
        /// <definition>
        /// public Dictionary&lt;string, object?&gt; RequestBag { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Dictionary<string, object?> RequestBag { get; set; }

        /// <summary>
        /// Gets the context Http Server instance.
        /// </summary>
        /// <definition>
        /// public HttpServer? HttpServer { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpServer? HttpServer { get; private set; }

        /// <summary>
        /// Gets or sets the HTTP response for this context. This property is only not null when a post-executing <see cref="IRequestHandler"/> was executed for this router context.
        /// </summary>
        /// <definition>
        /// public HttpResponse? RouterResponse { get; } 
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse? RouterResponse { get; internal set; } = null!;

        /// <summary>
        /// Gets the matched Http Route object from the Router.
        /// </summary>
        /// <definition>
        /// public Route? MatchedRoute { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Route? MatchedRoute { get; private set; }

        internal HttpContext(Dictionary<string, object?> requestBag, HttpServer? httpServer, Route? matchedRoute)
        {
            RequestBag = requestBag;
            HttpServer = httpServer;
            MatchedRoute = matchedRoute;
        }
    }
}
