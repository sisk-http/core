using Sisk.Core.Routing;
using System.Collections.Specialized;
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
    public class HttpContext
    {
        /// <summary>
        /// Gets or sets an <see cref="NameValueCollection"/> indicating HTTP headers which
        /// will overwrite headers set by CORS, router response or request handlers.
        /// </summary>
        /// <definition>
        /// public NameValueCollection OverrideHeaders { get; set; }
        /// </definition>
        /// <type> 
        /// Property
        /// </type>
        public NameValueCollection OverrideHeaders { get; set; } = new NameValueCollection();
        
        /// <summary>
        /// Gets the <see cref="ListeningHost"/> instance of this HTTP context.
        /// </summary>
        /// <definition>
        /// public ListeningHost ListeningHost { get; }
        /// </definition>
        /// <type> 
        /// Property
        /// </type>
        public ListeningHost ListeningHost { get; private set; }

        /// <summary>
        /// Gets or sets a managed object that is accessed and modified by request handlers.
        /// </summary>
        /// <definition>
        /// public Dictionary{{string, object?}} RequestBag { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public Dictionary<string, object?> RequestBag { get; set; } = new Dictionary<string, object?>();

        /// <summary>
        /// Gets the context Http Server instance.
        /// </summary>
        /// <definition>
        /// public HttpServer? HttpServer { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
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
        public HttpResponse? RouterResponse { get; internal set; } = null!;

        /// <summary>
        /// Gets the HTTP request which is contained in this HTTP context.
        /// </summary>
        /// <definition>
        /// public HttpRequest Request { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpRequest Request { get; private set; }

        /// <summary>
        /// Gets the matched Http Route object from the Router.
        /// </summary>
        /// <definition>
        /// public Route? MatchedRoute { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public Route? MatchedRoute { get; internal set; }

        internal HttpContext(HttpServer? httpServer, HttpRequest request, Route? matchedRoute, ListeningHost host)
        {
            Request = request;
            HttpServer = httpServer;
            MatchedRoute = matchedRoute;
            ListeningHost = host;
        }
    }
}
