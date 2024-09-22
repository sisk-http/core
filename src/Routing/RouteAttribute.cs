// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RouteAttribute.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;

namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents an class that, when applied to a method, will be recognized by a router as a route.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.
        /// </summary>
        public RouteMethod Method { get; set; } = RouteMethod.Any;

        /// <summary>
        /// Gets or sets the path expression that will be interpreted by the router and validated by the requests.
        /// </summary>
        public string Path { get; set; } = null!;

        /// <summary>
        /// Gets or sets the route name. It allows it to be found by other routes and makes it easier to create links.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets whether this route should send Cross-Origin Resource Sharing headers in the response.
        /// </summary>

        public bool UseCors { get; set; } = true;

        /// <summary>
        /// Gets or sets how this route can write messages to log files on the server.
        /// </summary>
        public LogOutput LogMode { get; set; } = LogOutput.Both;

        /// <summary>
        /// Get or sets if this route should use regex to be interpreted instead of predefined templates.
        /// </summary>
        public bool UseRegex { get; set; } = false;

        /// <summary>
        /// Creates an new <see cref="RouteAttribute"/> instance with given route method and path pattern.
        /// </summary>
        /// <param name="method">The route entry point method.</param>
        /// <param name="path">The route path.</param>
        public RouteAttribute(RouteMethod method, string path)
        {
            this.Method = method;
            this.Path = path;
        }
    }

    /// <summary>
    /// Represents a mapping to an route, which it's path is defined by an regular expression. This attribute
    /// is an shorthand from <see cref="RouteAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RegexRouteAttribute : RouteAttribute
    {
        /// <summary>
        /// Creates an new <see cref="RouteGetAttribute"/> attribute instance with given path.
        /// </summary>
        /// <param name="method">The route entry point method.</param>
        /// <param name="pattern">The Regex pattern which will match the route.</param>
        public RegexRouteAttribute(RouteMethod method, [StringSyntax(StringSyntaxAttribute.Regex)] string pattern) : base(method, pattern)
        {
            base.UseRegex = true;
        }
    }

    /// <summary>
    /// Represents a mapping to an HTTP GET route. This attribute is an shorthand from <see cref="RouteAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RouteGetAttribute : RouteAttribute
    {
        /// <summary>
        /// Creates an new <see cref="RouteGetAttribute"/> attribute instance with given path.
        /// </summary>
        /// <param name="path">The GET route path.</param>
        public RouteGetAttribute(string path) : base(RouteMethod.Get, path) { }

        /// <summary>
        /// Creates an new <see cref="RouteGetAttribute"/> attribute instance with an root path (/).
        /// </summary>
        public RouteGetAttribute() : base(RouteMethod.Get, "/") { }
    }

    /// <summary>
    /// Represents a mapping to an HTTP POST route. This attribute is an shorthand from <see cref="RouteAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RoutePostAttribute : RouteAttribute
    {
        /// <summary>
        /// Creates an new <see cref="RoutePostAttribute"/> attribute instance with given path.
        /// </summary>
        /// <param name="path">The POST route path.</param>
        public RoutePostAttribute(string path) : base(RouteMethod.Post, path) { }

        /// <summary>
        /// Creates an new <see cref="RoutePostAttribute"/> attribute instance with an root path (/).
        /// </summary>
        public RoutePostAttribute() : base(RouteMethod.Post, "/") { }
    }

    /// <summary>
    /// Represents a mapping to an HTTP PUT route. This attribute is an shorthand from <see cref="RouteAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RoutePutAttribute : RouteAttribute
    {
        /// <summary>
        /// Creates an new <see cref="RoutePutAttribute"/> attribute instance with given path.
        /// </summary>
        /// <param name="path">The PUT route path.</param>
        public RoutePutAttribute(string path) : base(RouteMethod.Put, path) { }

        /// <summary>
        /// Creates an new <see cref="RoutePutAttribute"/> attribute instance with an root path (/).
        /// </summary>
        public RoutePutAttribute() : base(RouteMethod.Put, "/") { }
    }

    /// <summary>
    /// Represents a mapping to an HTTP PATCH route. This attribute is an shorthand from <see cref="RouteAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RoutePatchAttribute : RouteAttribute
    {
        /// <summary>
        /// Creates an new <see cref="RoutePatchAttribute"/> attribute instance with given path.
        /// </summary>
        /// <param name="path">The PATCH route path.</param>
        public RoutePatchAttribute(string path) : base(RouteMethod.Patch, path) { }

        /// <summary>
        /// Creates an new <see cref="RoutePatchAttribute"/> attribute instance with an root path (/).
        /// </summary>
        public RoutePatchAttribute() : base(RouteMethod.Patch, "/") { }
    }

    /// <summary>
    /// Represents a mapping to an HTTP DELETE route. This attribute is an shorthand from <see cref="RouteAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RouteDeleteAttribute : RouteAttribute
    {
        /// <summary>
        /// Creates an new <see cref="RouteDeleteAttribute"/> attribute instance with given path.
        /// </summary>
        /// <param name="path">The DELETE route path.</param>
        public RouteDeleteAttribute(string path) : base(RouteMethod.Delete, path) { }

        /// <summary>
        /// Creates an new <see cref="RouteDeleteAttribute"/> attribute instance with an root path (/).
        /// </summary>
        public RouteDeleteAttribute() : base(RouteMethod.Delete, "/") { }
    }
}
