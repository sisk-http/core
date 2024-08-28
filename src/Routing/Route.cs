// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Route.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using System.Text.RegularExpressions;

namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents an HTTP route to be matched by an <see cref="Router"/> object.
    /// </summary>
    public class Route
    {
        internal RouteAction? _callback { get; set; }
        internal bool isReturnTypeTask;
        internal Regex? routeRegex;
        private string path;

        /// <summary>
        /// Represents an route path which captures any URL path.
        /// </summary>
        public const string AnyPath = "/<<ANY>>";

        /// <summary>
        /// Gets or sets an <see cref="TypedValueDictionary"/> for this route, which can hold contextual variables
        /// for this <see cref="Route"/> object.
        /// </summary>
        public TypedValueDictionary Bag { get; set; } = new TypedValueDictionary(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets an boolean indicating if this <see cref="Route"/> action return is an asynchronous <see cref="Task"/>.
        /// </summary>
        public bool IsAsync { get => this.isReturnTypeTask; }

        /// <summary>
        /// Gets or sets how this route can write messages to log files on the server.
        /// </summary>
        public LogOutput LogMode { get; set; } = LogOutput.Both;

        /// <summary>
        /// Get or sets if this route should use regex to be interpreted instead of predefined templates.
        /// </summary>
        public bool UseRegex { get; set; }

        /// <summary>
        /// Gets or sets whether this route should send Cross-Origin Resource Sharing headers in the response.
        /// </summary>
        public bool UseCors { get; set; } = true;

        /// <summary>
        /// Gets or sets the matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.
        /// </summary>
        public RouteMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the path expression that will be interpreted by the router and validated by the requests.
        /// </summary>
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                if (this.UseRegex && this.routeRegex != null)
                {
                    this.routeRegex = null;
                }
                this.path = value;
            }
        }

        /// <summary>
        /// Gets or sets the route name. It allows it to be found by other routes and makes it easier to create links.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the function that is called after the route is matched with the request.
        /// </summary>
        public RouteAction? Action
        {
            get => this._callback;
            set
            {
                this._callback = value;
                if (value != null)
                {
                    var memberInfo = value.Method;
                    var retType = memberInfo.ReturnType;

                    if (retType.IsValueType)
                    {
                        throw new NotSupportedException(SR.Route_Action_ValueTypeSet);
                    }
                    else if (retType.IsAssignableTo(typeof(Task)))
                    {
                        this.isReturnTypeTask = true;
                        if (retType.GenericTypeArguments.Length == 0)
                        {
                            throw new InvalidOperationException(string.Format(SR.Route_Action_AsyncMissingGenericType, this));
                        }
                        else
                        {
                            Type genericAssignType = retType.GenericTypeArguments[0];
                            if (genericAssignType.IsValueType)
                            {
                                throw new NotSupportedException(SR.Route_Action_ValueTypeSet);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the request handlers instances to run before the route's Action.
        /// </summary>
        public IRequestHandler[] RequestHandlers { get; set; } = Array.Empty<IRequestHandler>();

        /// <summary>
        /// Gets or sets the global request handlers instances that will not run on this route.
        /// </summary>
        public IRequestHandler[] BypassGlobalRequestHandlers { get; set; } = Array.Empty<IRequestHandler>();

        /// <summary>
        /// Creates an new <see cref="Route"/> instance with given parameters.
        /// </summary>
        /// <param name="method">The matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.</param>
        /// <param name="path">The path expression that will be interpreted by the router and validated by the requests.</param>
        /// <param name="action">The function that is called after the route is matched with the request.</param>
        public Route(RouteMethod method, string path, RouteAction action)
        {
            this.Method = method;
            this.path = path;
            this.Action = action;
        }

        /// <summary>
        /// Creates an new <see cref="Route"/> instance with given parameters.
        /// </summary>
        /// <param name="method">The matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.</param>
        /// <param name="path">The path expression that will be interpreted by the router and validated by the requests.</param>
        /// <param name="name">The route name. It allows it to be found by other routes and makes it easier to create links.</param>
        /// <param name="action">The function that is called after the route is matched with the request.</param>
        /// <param name="beforeCallback">The RequestHandlers to run before the route's Action.</param>
        public Route(RouteMethod method, string path, string? name, RouteAction action, IRequestHandler[]? beforeCallback)
        {
            this.Method = method;
            this.path = path;
            this.Name = name;
            this.Action = action;
            this.RequestHandlers = beforeCallback ?? Array.Empty<IRequestHandler>();
        }

        /// <summary>
        /// Creates an new <see cref="Route"/> instance with no parameters.
        /// </summary>
        public Route()
        {
            this.path = "/";
        }

        /// <summary>
        /// Gets an string notation for this <see cref="Route"/> object.
        /// </summary>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return $"{{Method = {this.Method}, Path = {this.Path}}}";
            }
            else
            {
                return $"{{Method = {this.Method}, Path = {this.Path}, Name={this.Name}}}";
            }
        }
    }

    /// <summary>
    /// Determines the way the server can write log messages. This enumerator is for giving permissions for certain contexts
    /// to be able or not to write to the server logs, such as <see cref="Http.HttpServerConfiguration.AccessLogsStream"/> and <see cref="Http.HttpServerConfiguration.ErrorsLogsStream"/>.
    /// </summary>
    [Flags]
    public enum LogOutput
    {
        /// <summary>
        /// Determines that the context or the route can write log messages only to the access logs through <see cref="Http.HttpServerConfiguration.AccessLogsStream"/>.
        /// </summary>
        AccessLog = 1,

        /// <summary>
        /// Determines that the context or the route can write error messages only to the error logs through <see cref="Http.HttpServerConfiguration.ErrorsLogsStream"/>.
        /// </summary>
        ErrorLog = 2,

        /// <summary>
        /// Determines that the context or the route can write log messages to both error and access logs.
        /// </summary>
        Both = AccessLog | ErrorLog,

        /// <summary>
        /// Determines that the context or the route cannot write any log messages.
        /// </summary>
        None = 0
    }
}