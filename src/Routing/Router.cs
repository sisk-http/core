// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Router.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sisk.Core.Http;
using Sisk.Core.Internal;

sealed class ActionHandler {
    public Type MatchingType { get; set; }
    public Func<object, HttpResponse> Handler { get; set; }

    public ActionHandler ( Type matchingType, Func<object, HttpResponse> handler ) {
        MatchingType = matchingType;
        Handler = handler;
    }
}


namespace Sisk.Core.Routing {

    /// <summary>
    /// Represents a collection of <see cref="Route"/> and main executor of actions in the <see cref="HttpServer"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Naming", "CA1710:Identifiers should not have incorrect suffix",
        Justification = "Breaking change. Not going forward on this one." )]
    public sealed partial class Router : IReadOnlyCollection<Route> {
        internal sealed record RouterExecutionResult ( HttpResponse? Response, Route? Route, RouteMatchResult Result, Exception? Exception );

        internal HttpServer? parentServer;
        internal List<Route> _routesList = new ();
        internal List<ActionHandler> _actionHandlersList = new ();

        [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
        internal void BindServer ( HttpServer server ) {
            if (parentServer is not null) {
                if (ReferenceEquals ( server, parentServer )) {
                    return;
                }
                else {
                    throw new InvalidOperationException ( SR.Router_BindException );
                }
            }
            else {
                server.handler.SetupRouter ( this );
                parentServer = server;

                if (CheckForRouteCollisions)
                    CheckForRouteCollisionsCore ();
            }
        }

        /// <summary>
        /// Gets an boolean indicating where this <see cref="Router"/> is read-only or not.
        /// </summary>
        public bool IsReadOnly { get => parentServer is not null; }

        /// <summary>
        /// Gets or sets whether this <see cref="Router"/> will match routes ignoring case.
        /// </summary>
        public bool MatchRoutesIgnoreCase { get; set; }

        /// <summary>
        /// Gets or sets whether this <see cref="Router"/> should check for possible routing
        /// collisions before starting the HTTP server.
        /// </summary>
        public bool CheckForRouteCollisions { get; set; } = true;

        /// <summary>
        /// Gets or sets the prefix which will be applied to all next defining routes in this
        /// router.
        /// </summary>
        public string? Prefix { get; set; }

        /// <summary>
        /// Creates an new <see cref="Router"/> instance with default values.
        /// </summary>
        public Router () {
        }

        /// <summary>
        /// Creates an new <see cref="Router"/> instance with given route collection.
        /// </summary>
        /// <param name="routes">The route collection to import in this router.</param>
#if NET9_0_OR_GREATER
        public Router ( params IEnumerable<Route> routes )
#else
        public Router ( IEnumerable<Route> routes )
#endif
        {
            foreach (var route in routes)
                SetRoute ( route );
        }

        /// <summary>
        /// Gets or sets the global requests handlers that will be executed in all matched routes.
        /// </summary>
        public IRequestHandler [] GlobalRequestHandlers { get; set; } = Array.Empty<IRequestHandler> ();

        /// <summary>
        /// Gets or sets the Router action exception handler. The response handler for this property will
        /// send an HTTP response to the client when an exception is caught during execution. This property
        /// is only called when <see cref="HttpServerConfiguration.ThrowExceptions"/> is disabled.
        /// </summary>
        public ExceptionErrorCallback? CallbackErrorHandler { get; set; }

        /// <summary>
        /// Gets or sets the Router "404 Not Found" handler.
        /// </summary>
        public RoutingErrorCallback? NotFoundErrorHandler { get; set; } = new RoutingErrorCallback (
            ( context ) => {
                string? accept = context.Request.Headers.Accept;

                if (accept?.Contains ( "text/html", StringComparison.Ordinal ) == true) {
                    return DefaultMessagePage.Instance.CreateMessageHtml ( HttpStatusCode.NotFound, SR.HttpResponse_404_DefaultMessage );
                }
                else {
                    return new HttpResponse ( HttpStatusCode.NotFound );
                }
            } );

        /// <summary>
        /// Gets or sets the Router "405 Method Not Allowed" handler.
        /// </summary>
        public RoutingErrorCallback? MethodNotAllowedErrorHandler { get; set; } = new RoutingErrorCallback (
            ( context ) => {
                string? accept = context.Request.Headers.Accept;

                if (accept?.Contains ( "text/html", StringComparison.Ordinal ) == true) {
                    return DefaultMessagePage.Instance.CreateMessageHtml ( HttpStatusCode.MethodNotAllowed, SR.HttpResponse_405_DefaultMessage );
                }
                else {
                    return new HttpResponse ( HttpStatusCode.MethodNotAllowed );
                }
            } );

        /// <inheritdoc/>
        public int Count => ((IReadOnlyCollection<Route>) _routesList).Count;

        /// <summary>
        /// Gets all routes defined on this router instance.
        /// </summary>
        public Route [] GetDefinedRoutes () => _routesList.ToArray ();

        /// <summary>
        /// Resolves the specified object into an valid <see cref="HttpResponse"/> using the defined
        /// value handlers or throws an exception if not possible.
        /// </summary>
        /// <param name="result">The object that will be converted to an valid <see cref="HttpResponse"/>.</param>
        /// <remarks>
        /// This method can throw exceptions. To avoid exceptions while trying to convert the specified object
        /// into an <see cref="HttpResponse"/>, consider using <see cref="TryResolveActionResult(object?, out HttpResponse?)"/>.
        /// </remarks>
        public HttpResponse ResolveActionResult ( object? result ) {
            return ResolveAction ( result );
        }

        /// <summary>
        /// Tries to resolve the specified object into an valid <see cref="HttpResponse"/> using the defined
        /// value handlers.
        /// </summary>
        /// <param name="result">The object that will be converted to an valid <see cref="HttpResponse"/>.</param>
        /// <param name="response">When this method returns, the response object. This parameter is not initialized.</param>
        /// <returns>When this method returns, the <see cref="HttpResponse"/> object.</returns>
        public bool TryResolveActionResult ( object? result, [NotNullWhen ( true )] out HttpResponse? response ) {
            if (result is null) {
                response = null;
                return false;
            }
            else if (result is HttpResponse httpres) {
                response = httpres;
                return true;
            }

            // IsReadOnly garantes that _actionHandlersList and
            // _routesList will be not modified during span reading
            ;
            bool wasLocked = false;
            if (!IsReadOnly) {
                wasLocked = true;
                Monitor.Enter ( _actionHandlersList );
            }
            try {
                Type actionType = result.GetType ();

                Span<ActionHandler> hspan = CollectionsMarshal.AsSpan ( _actionHandlersList );
                ref ActionHandler pointer = ref MemoryMarshal.GetReference ( hspan );
                for (int i = 0; i < hspan.Length; i++) {
                    ref ActionHandler current = ref Unsafe.Add ( ref pointer, i );

                    if (actionType.IsAssignableTo ( current.MatchingType )) {
                        var resultObj = current.Handler ( result )
                            ?? throw new InvalidOperationException ( SR.Format ( SR.Router_Handler_HandlerNotHttpResponse, current.MatchingType.Name ) );
                        response = resultObj;
                        return true;
                    }
                }

                response = null;
                return false;
            }
            finally {
                if (wasLocked) {
                    Monitor.Exit ( _actionHandlersList );
                }
            }
        }

        /// <summary>
        /// Register an type handling association to converting it to an <see cref="HttpResponse"/> object.
        /// </summary>
        /// <param name="actionHandler">The function that receives an object of the <typeparamref name="T"/> and returns an <see cref="HttpResponse"/> response from the informed object.</param>
        public void RegisterValueHandler<T> ( RouterActionHandlerCallback<T> actionHandler ) where T : notnull {
            if (IsReadOnly) {
                throw new InvalidOperationException ( SR.Router_ReadOnlyException );
            }
            Type type = typeof ( T );
            if (type == typeof ( HttpResponse )) {
                throw new ArgumentException ( SR.Router_Handler_HttpResponseRegister );
            }
            for (int i = 0; i < _actionHandlersList!.Count; i++) {
                ActionHandler item = _actionHandlersList [ i ];
                if (item.MatchingType.Equals ( type )) {
                    throw new ArgumentException ( SR.Router_Handler_Duplicate );
                }
            }
            _actionHandlersList.Add ( new ActionHandler ( type, ( obj ) => actionHandler ( (T) obj ) ) );
        }

        HttpResponse ResolveAction ( object? routeResult ) {
            if (routeResult is null) {
                throw new ArgumentNullException ( nameof ( routeResult ), SR.Router_Handler_ActionNullValue );
            }
            else if (routeResult is HttpResponse rh) {
                return rh;
            }
            else if (TryResolveActionResult ( routeResult, out HttpResponse? result )) {
                return result;
            }
            else {
                throw new InvalidOperationException ( SR.Format ( SR.Router_Handler_UnrecognizedAction, routeResult.GetType ().FullName ) );
            }
        }

        void CheckForRouteCollisionsCore () {

            for (int i = 0; i < _routesList.Count; i++) {
                Route I = _routesList [ i ];

                for (int j = 0; j < _routesList.Count; j++) {
                    Route J = _routesList [ j ];

                    bool methodMatched =
                        I.Method == RouteMethod.Any ||
                        J.Method == RouteMethod.Any ||
                        I.Method.HasFlag ( J.Method ) ||
                        J.Method.HasFlag ( I.Method );

                    if (!ReferenceEquals ( I, J ) && methodMatched && HttpStringInternals.IsRoutePatternMatch ( I.Path, J.Path,
                        MatchRoutesIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal )) {
                        throw new ArgumentException ( SR.Format ( SR.Router_Set_Collision, I, J ) );
                    }
                }
            }
        }

        internal void FreeHttpServer () {
            parentServer = null;
        }

        /// <inheritdoc/>
        public IEnumerator<Route> GetEnumerator () {
            return ((IEnumerable<Route>) _routesList).GetEnumerator ();
        }

        IEnumerator IEnumerable.GetEnumerator () {
            return ((IEnumerable) _routesList).GetEnumerator ();
        }
    }

    internal enum RouteMatchResult {
        FullyMatched,
        PathMatched,
        OptionsMatched,
        NotMatched
    }
}
