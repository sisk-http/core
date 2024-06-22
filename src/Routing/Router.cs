// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Router.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Internal;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

record struct RouteDictItem(System.Type type, Delegate lambda);

namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents a collection of <see cref="Route"/> and main executor of actions in the <see cref="HttpServer"/>.
    /// </summary>
    public sealed partial class Router
    {
        internal record RouterExecutionResult(HttpResponse? Response, Route? Route, RouteMatchResult Result, Exception? Exception);
        internal HttpServer? ParentServer { get; private set; }

        internal List<Route> _routesList = new();
        internal List<RouteDictItem> _actionHandlersList = new();

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal void BindServer(HttpServer server)
        {
            if (ParentServer is not null)
            {
                if (ReferenceEquals(server, ParentServer))
                {
                    return;
                }
                else
                {
                    throw new InvalidOperationException(SR.Router_BindException);
                }
            }
            else
            {
                server.handler.SetupRouter(this);
                ParentServer = server;
            }
        }

        /// <summary>
        /// Combines the specified URL paths into one.
        /// </summary>
        /// <param name="paths">The string array which contains parts that will be combined.</param>
        public static string CombinePaths(params string[] paths)
        {
            return PathUtility.CombinePaths(paths);
        }

        /// <summary>
        /// Gets an boolean indicating where this <see cref="Router"/> is read-only or not.
        /// </summary>
        public bool IsReadOnly { get => ParentServer is not null; }

        /// <summary>
        /// Gets or sets whether this <see cref="Router"/> will match routes ignoring case.
        /// </summary>
        public bool MatchRoutesIgnoreCase { get; set; } = false;

        /// <summary>
        /// Creates an new <see cref="Router"/> instance with default properties values.
        /// </summary>
        public Router()
        {
        }

        /// <summary>
        /// Gets or sets the global requests handlers that will be executed in all matched routes.
        /// </summary>
        public IRequestHandler[]? GlobalRequestHandlers { get; set; }

        /// <summary>
        /// Gets or sets the Router action exception handler.
        /// </summary>
        public ExceptionErrorCallback? CallbackErrorHandler { get; set; }

        /// <summary>
        /// Gets or sets the Router "404 Not Found" handler.
        /// </summary>
        public RoutingErrorCallback? NotFoundErrorHandler { get; set; } = new RoutingErrorCallback(
            (c) => new HttpResponse(System.Net.HttpStatusCode.NotFound));

        /// <summary>
        /// Gets or sets the Router "405 Method Not Allowed" handler.
        /// </summary>
        public RoutingErrorCallback? MethodNotAllowedErrorHandler { get; set; } = new RoutingErrorCallback(
            (c) => new HttpResponse(System.Net.HttpStatusCode.MethodNotAllowed));

        /// <summary>
        /// Gets all routes defined on this router instance.
        /// </summary>
        public Route[] GetDefinedRoutes() => _routesList.ToArray();

        /// <summary>
        /// Tries to resolve the specified object into an valid <see cref="HttpResponse"/> using the defined
        /// value handlers.
        /// </summary>
        /// <param name="result">The object that will be converted to an valid <see cref="HttpResponse"/>.</param>
        /// <param name="response">When this method returns, the response object. This parameter is not initialized.</param>
        /// <returns>When this method returns, the <see cref="HttpResponse"/> object.</returns>
        public bool TryResolveActionResult(object? result, [NotNullWhen(true)] out HttpResponse? response)
        {
            bool wasLocked = false;
            if (!IsReadOnly)
            {
                wasLocked = true;
                Monitor.Enter(_actionHandlersList);
            }
            try
            {
                if (result is null)
                {
                    response = null;
                    return false;
                }

                Type actionType = result.GetType();

                Span<RouteDictItem> hspan = CollectionsMarshal.AsSpan(_actionHandlersList);
                ref RouteDictItem pointer = ref MemoryMarshal.GetReference(hspan);
                for (int i = 0; i < hspan.Length; i++)
                {
                    ref RouteDictItem current = ref Unsafe.Add(ref pointer, i);
                    if (actionType.IsAssignableTo(current.type))
                    {
                        response = (HttpResponse)current.lambda.DynamicInvoke(result)!;
                        return true;
                    }
                }

                response = null;
                return false;
            }
            finally
            {
                if (wasLocked)
                {
                    Monitor.Exit(_actionHandlersList);
                }
            }
        }

        /// <summary>
        /// Register an type handling association to converting it to an <see cref="HttpResponse"/> object.
        /// </summary>
        /// <param name="actionHandler">The function that receives an object of the T and returns an <see cref="HttpResponse"/> response from the informed object.</param>
        public void RegisterValueHandler<T>(Func<T, HttpResponse> actionHandler)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(SR.Router_ReadOnlyException);
            }
            Type type = typeof(T);
            if (type == typeof(HttpResponse))
            {
                throw new ArgumentException(SR.Router_Handler_HttpResponseRegister);
            }
            for (int i = 0; i < _actionHandlersList!.Count; i++)
            {
                RouteDictItem item = _actionHandlersList[i];
                if (item.type.Equals(type))
                {
                    throw new ArgumentException(SR.Router_Handler_Duplicate);
                }
            }
            _actionHandlersList.Add(new RouteDictItem(type, actionHandler));
        }

        HttpResponse ResolveAction(object routeResult)
        {
            if (routeResult is null)
            {
                throw new ArgumentNullException(SR.Router_Handler_ActionNullValue);
            }
            else if (TryResolveActionResult(routeResult, out HttpResponse? result))
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException(string.Format(SR.Router_Handler_UnrecognizedAction, routeResult.GetType().FullName));
            }
        }

        internal void FreeHttpServer()
        {
            ParentServer = null;
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
