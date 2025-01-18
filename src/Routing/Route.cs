// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Route.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using Sisk.Core.Entity;

namespace Sisk.Core.Routing {

    /// <summary>
    /// Represents an HTTP route to be matched by an <see cref="Router"/>.
    /// </summary>
    public class Route {
        internal RouteAction? _singleParamCallback;
        internal ParameterlessRouteAction? _parameterlessRouteAction;

        internal bool _isAsyncEnumerable;
        internal bool _isAsyncTask;
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
        public TypedValueDictionary Bag { get; set; } = new TypedValueDictionary ( StringComparer.OrdinalIgnoreCase );

        /// <summary>
        /// Gets an boolean indicating if this <see cref="Route"/> action return is an asynchronous <see cref="Task"/>.
        /// </summary>
        public bool IsAsync { get => this._isAsyncTask; }

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
        /// Gets or sets the matching HTTP method.
        /// </summary>
        public RouteMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the path expression that will be interpreted by the router and validated by the requests.
        /// </summary>
        public string Path {
            get {
                return this.path;
            }
            set {
                if (this.UseRegex && this.routeRegex != null) {
                    // routeRegex is created in the router invocation
                    this.routeRegex = null;
                }
                this.path = value;
            }
        }

        /// <summary>
        /// Gets or sets the route name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the function that is called after the route is matched with the request.
        /// </summary>
        public Delegate? Action {
            get {
                if (this._parameterlessRouteAction != null) {
                    return this._parameterlessRouteAction;
                }
                else {
                    return this._singleParamCallback;
                }
            }
            set {
                if (value is null) {
                    this._parameterlessRouteAction = null;
                    this._singleParamCallback = null;
                    this._isAsyncTask = false;
                    this._isAsyncEnumerable = false;
                    return;
                }
                else if (!this.TrySetRouteAction ( value.Method, value.Target, out Exception? ex )) {
                    throw ex;
                }
            }
        }

        internal bool TrySetRouteAction ( MethodInfo method, object? target, [NotNullWhen ( false )] out Exception? ex ) {
            if (Delegate.CreateDelegate ( typeof ( RouteAction ), target, method, false ) is RouteAction raction) {
                this._singleParamCallback = raction;
                this._parameterlessRouteAction = null;
            }
            else if (Delegate.CreateDelegate ( typeof ( ParameterlessRouteAction ), target, method, false ) is ParameterlessRouteAction parameterlessRouteAction) {
                this._singleParamCallback = null;
                this._parameterlessRouteAction = parameterlessRouteAction;
            }
            else {
                ex = new ArgumentException ( SR.Router_Set_InvalidType );
                return false;
            }

            Exception? CheckAsyncReturnParameters ( Type asyncOutType ) {
                if (asyncOutType.GenericTypeArguments.Length == 0) {
                    return new InvalidOperationException ( string.Format ( SR.Route_Action_AsyncMissingGenericType, this ) );
                }
                else {
                    Type genericAssignType = asyncOutType.GenericTypeArguments [ 0 ];
                    if (genericAssignType.IsValueType) {
                        return new NotSupportedException ( SR.Route_Action_ValueTypeSet );
                    }
                }
                return null;
            }

            var retType = method.ReturnType;
            if (retType.IsValueType) {
                ex = new NotSupportedException ( SR.Route_Action_ValueTypeSet );
                return false;
            }
            else if (retType.IsAssignableTo ( typeof ( Task ) )) {
                this._isAsyncTask = true;
                if (CheckAsyncReturnParameters ( retType ) is Exception rex) {
                    ex = rex;
                    return false;
                }
            }
            else if (retType.IsGenericType && retType.GetGenericTypeDefinition () == typeof ( IAsyncEnumerable<> )) {
                this._isAsyncEnumerable = true;
                if (CheckAsyncReturnParameters ( retType ) is Exception rex) {
                    ex = rex;
                    return false;
                }
            }
            else {
                this._isAsyncTask = false;
                this._isAsyncEnumerable = false;
            }

            ex = null;
            return true;
        }

        /// <summary>
        /// Gets or sets the request handlers instances to run before the route's Action.
        /// </summary>
        public IRequestHandler [] RequestHandlers { get; set; } = Array.Empty<IRequestHandler> ();

        /// <summary>
        /// Gets or sets the global request handlers instances that will not run on this route.
        /// </summary>
        public IRequestHandler [] BypassGlobalRequestHandlers { get; set; } = Array.Empty<IRequestHandler> ();

        /// <summary>
        /// Creates an new <see cref="Route"/> instance with given parameters.
        /// </summary>
        /// <param name="method">The matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.</param>
        /// <param name="path">The path expression that will be interpreted by the router and validated by the requests.</param>
        /// <param name="action">The function that is called after the route is matched with the request.</param>
        public Route ( RouteMethod method, string path, Delegate? action ) {
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
        public Route ( RouteMethod method, string path, string? name, Delegate? action, IRequestHandler []? beforeCallback ) {
            this.Method = method;
            this.path = path;
            this.Name = name;
            this.Action = action;
            this.RequestHandlers = beforeCallback ?? Array.Empty<IRequestHandler> ();
        }

        /// <summary>
        /// Creates an new <see cref="Route"/> instance with no parameters.
        /// </summary>
        public Route () {
            this.path = "/";
        }

        /// <summary>
        /// Gets an string notation for this <see cref="Route"/> object.
        /// </summary>
        public override string ToString () {
            return $"[{this.Method.ToString ().ToUpper ()} {this.path}] {this.Name ?? this.Action?.Method.Name ?? "<no action>"}";
        }
        #region Helper constructors

        /// <summary>
        /// Creates a route that responds to HTTP GET requests.
        /// </summary>
        /// <param name="path">The URL path for the route.</param>
        /// <param name="action">The action to be executed when the route is matched.</param>
        /// <returns>A <see cref="Route"/> object configured for GET requests.</returns>
        public static Route Get ( string path, RouteAction action ) => new Route ( RouteMethod.Get, path, action );

        /// <summary>
        /// Creates a route that responds to HTTP POST requests.
        /// </summary>
        /// <param name="path">The URL path for the route.</param>
        /// <param name="action">The action to be executed when the route is matched.</param>
        /// <returns>A <see cref="Route"/> object configured for POST requests.</returns>
        public static Route Post ( string path, RouteAction action ) => new Route ( RouteMethod.Post, path, action );

        /// <summary>
        /// Creates a route that responds to HTTP PUT requests.
        /// </summary>
        /// <param name="path">The URL path for the route.</param>
        /// <param name="action">The action to be executed when the route is matched.</param>
        /// <returns>A <see cref="Route"/> object configured for PUT requests.</returns>
        public static Route Put ( string path, RouteAction action ) => new Route ( RouteMethod.Put, path, action );

        /// <summary>
        /// Creates a route that responds to HTTP PATCH requests.
        /// </summary>
        /// <param name="path">The URL path for the route.</param>
        /// <param name="action">The action to be executed when the route is matched.</param>
        /// <returns>A <see cref="Route"/> object configured for PATCH requests.</returns>
        public static Route Patch ( string path, RouteAction action ) => new Route ( RouteMethod.Patch, path, action );

        /// <summary>
        /// Creates a route that responds to HTTP HEAD requests.
        /// </summary>
        /// <param name="path">The URL path for the route.</param>
        /// <param name="action">The action to be executed when the route is matched.</param>
        /// <returns>A <see cref="Route"/> object configured for HEAD requests.</returns>
        public static Route Head ( string path, RouteAction action ) => new Route ( RouteMethod.Head, path, action );

        /// <summary>
        /// Creates a route that responds to any HTTP request method.
        /// </summary>
        /// <param name="path">The URL path for the route.</param>
        /// <param name="action">The action to be executed when the route is matched.</param>
        /// <returns>A <see cref="Route"/> object configured for any request method.</returns>
        public static Route Any ( string path, RouteAction action ) => new Route ( RouteMethod.Any, path, action );

        /// <summary>
        /// Creates a route that responds to HTTP DELETE requests.
        /// </summary>
        /// <param name="path">The URL path for the route.</param>
        /// <param name="action">The action to be executed when the route is matched.</param>
        /// <returns>A <see cref="Route"/> object configured for DELETE requests.</returns>
        public static Route Delete ( string path, RouteAction action ) => new Route ( RouteMethod.Delete, path, action );

        /// <summary>
        /// Creates a route that responds to HTTP OPTIONS requests.
        /// </summary>
        /// <param name="path">The URL path for the route.</param>
        /// <param name="action">The action to be executed when the route is matched.</param>
        /// <returns>A <see cref="Route"/> object configured for OPTIONS requests.</returns>
        public static Route Options ( string path, RouteAction action ) => new Route ( RouteMethod.Options, path, action );
        #endregion
    }

    /// <summary>
    /// Determines the way the server can write log messages. This enumerator is for giving permissions for certain contexts
    /// to be able or not to write to the server logs, such as <see cref="Http.HttpServerConfiguration.AccessLogsStream"/> and <see cref="Http.HttpServerConfiguration.ErrorsLogsStream"/>.
    /// </summary>
    [Flags]
    public enum LogOutput {
        /// <summary>
        /// Determines that the context or the route can write log messages only to the access logs through <see cref="Http.HttpServerConfiguration.AccessLogsStream"/>.
        /// </summary>
        AccessLog = 1 << 1,

        /// <summary>
        /// Determines that the context or the route can write error messages only to the error logs through <see cref="Http.HttpServerConfiguration.ErrorsLogsStream"/>.
        /// </summary>
        ErrorLog = 1 << 2,

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