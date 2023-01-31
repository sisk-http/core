namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents an class that, when applied to a method, will be recognized by a router as a route.
    /// </summary>
    /// <definition>
    /// [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    /// public class RouteAttribute : Attribute
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing
    /// </namespace>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RouteAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.
        /// </summary>
        /// <definition>
        /// public RouteMethod Method { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public RouteMethod Method { get; set; } = RouteMethod.Any;

        /// <summary>
        /// Gets or sets the path expression that will be interpreted by the router and validated by the requests.
        /// </summary>
        /// <definition>
        /// public string Path { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public string Path { get; set; } = null!;

        /// <summary>
        /// Gets or sets the route name. It allows it to be found by other routes and makes it easier to create links.
        /// </summary>
        /// <definition>
        /// public string? Name { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets whether this route should send Cross-Origin Resource Sharing headers in the response.
        /// </summary>
        /// <definition>
        /// public bool UseCors { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public bool UseCors { get; set; } = true;

        /// <summary>
        /// Gets or sets how this route can write messages to log files on the server.
        /// </summary>
        /// <definition>
        /// public LogOutput LogMode { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public LogOutput LogMode { get; set; } = LogOutput.Both;

        /// <summary>
        /// Creates an new <see cref="RouteAttribute"/> instance with given route method and path pattern.
        /// </summary>
        /// <param name="method">The route entry point method.</param>
        /// <param name="path">The route path.</param>
        /// <definition>
        /// public RouteAttribute(RouteMethod method, string path)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public RouteAttribute(RouteMethod method, string path)
        {
            this.Method = method;
            this.Path = path;
        }
    }
}
