// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Route.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System.Text.RegularExpressions;

namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents an HTTP route to be matched by an <see cref="Router"/> object.
    /// </summary>
    /// <definition>
    /// public class Route
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public class Route
    {
        internal RouteAction? _callback { get; set; }
        internal bool isReturnTypeTask;
        internal Regex? routeRegex;
        private string path;

        /// <summary>
        /// Gets an boolean indicating if this <see cref="Route"/> action return is an asynchronous <see cref="Task"/>.
        /// </summary>
        /// <definition>
        /// public bool IsAsync { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <since>0.16</since>
        public bool IsAsync { get => isReturnTypeTask; }

        /// <summary>
        /// Gets or sets how this route can write messages to log files on the server.
        /// </summary>
        /// <definition>
        /// public LogOutput LogMode { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public LogOutput LogMode { get; set; } = LogOutput.Both;

        /// <summary>
        /// Get or sets if this route should use regex to be interpreted instead of predefined templates.
        /// </summary>
        /// <definition>
        /// public bool UseRegex { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool UseRegex { get; set; }

        /// <summary>
        /// Gets or sets whether this route should send Cross-Origin Resource Sharing headers in the response.
        /// </summary>
        /// <definition>
        /// public bool UseCors { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool UseCors { get; set; } = true;

        /// <summary>
        /// Gets or sets the matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.
        /// </summary>
        /// <definition>
        /// public RouteMethod Method { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public RouteMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the path expression that will be interpreted by the router and validated by the requests.
        /// </summary>
        /// <definition>
        /// public string Path { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                if (UseRegex && routeRegex != null)
                {
                    routeRegex = null;
                }
                path = value;
            }
        }

        /// <summary>
        /// Gets or sets the route name. It allows it to be found by other routes and makes it easier to create links.
        /// </summary>
        /// <definition>
        /// public string? Name { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the function that is called after the route is matched with the request.
        /// </summary>
        /// <definition>
        /// public RouteAction? Action
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public RouteAction? Action
        {
            get => _callback;
            set
            {
                _callback = value;
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
                        isReturnTypeTask = true;
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
        /// Gets or sets the RequestHandlers to run before the route's Action.
        /// </summary>
        /// <definition>
        /// public IRequestHandler[]? RequestHandlers { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public IRequestHandler[]? RequestHandlers { get; set; }

        /// <summary>
        /// Gets or sets the global request handlers that will not run on this route. The verification is given by the identifier of the instance of an <see cref="IRequestHandler"/>.
        /// </summary>
        /// <definition>
        /// public IRequestHandler[]? BypassGlobalRequestHandlers { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public IRequestHandler[]? BypassGlobalRequestHandlers { get; set; }

        /// <summary>
        /// Creates an new <see cref="Route"/> instance with given parameters.
        /// </summary>
        /// <param name="method">The matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.</param>
        /// <param name="path">The path expression that will be interpreted by the router and validated by the requests.</param>
        /// <param name="action">The function that is called after the route is matched with the request.</param>
        /// <definition>
        /// public Route(RouteMethod method, string path, RouterCallback action)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public Route(RouteMethod method, string path, RouteAction action)
        {
            Method = method;
            this.path = path;
            Action = action;
        }

        /// <summary>
        /// Creates an new <see cref="Route"/> instance with given parameters.
        /// </summary>
        /// <param name="method">The matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.</param>
        /// <param name="path">The path expression that will be interpreted by the router and validated by the requests.</param>
        /// <param name="name">The route name. It allows it to be found by other routes and makes it easier to create links.</param>
        /// <param name="action">The function that is called after the route is matched with the request.</param>
        /// <param name="beforeCallback">The RequestHandlers to run before the route's Action.</param>
        /// <definition>
        /// public Route(RouteMethod method, string path, string? name, RouterCallback action, IRequestHandler[]? beforeCallback)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public Route(RouteMethod method, string path, string? name, RouteAction action, IRequestHandler[]? beforeCallback)
        {
            Method = method;
            this.path = path;
            Name = name;
            Action = action;
            RequestHandlers = beforeCallback;
        }

        /// <summary>
        /// Creates an new <see cref="Route"/> instance with no parameters.
        /// </summary>
        /// <definition>
        /// public Route()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public Route()
        {
            this.path = "/";
        }

        /// <summary>
        /// Gets an string notation for this <see cref="Route"/> object.
        /// </summary>
        /// <definition>
        /// public override string ToString()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return $"[Method={Method}, Path={Path}]";
            }
            else
            {
                return $"[Method={Method}, Name={Name}]";
            }
        }
    }

    /// <summary>
    /// Determines the way the server can write log messages. This enumerator is for giving permissions for certain contexts to be able or not to write to the logs.
    /// </summary>
    /// <definition>
    /// public enum LogOutput
    /// </definition>
    /// <type>
    /// Enum
    /// </type>

    [Flags]
    public enum LogOutput
    {
        /// <summary>
        /// Determines that the context or the route can write log messages only to the access logs.
        /// </summary>
        AccessLog = 1,
        /// <summary>
        /// Determines that the context or the route can write error messages only to the error logs.
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
