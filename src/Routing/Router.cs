// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Router.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sisk.Core.Http;
using Sisk.Core.Internal;

record struct RouteDictItem(System.Type type, Delegate lambda);


namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents a collection of <see cref="Route"/> and main executor of actions in the <see cref="HttpServer"/>.
    /// </summary>
    public sealed partial class Router
    {
        internal record RouterExecutionResult(HttpResponse? Response, Route? Route, RouteMatchResult Result, Exception? Exception);

        internal HttpServer? parentServer;
        internal List<Route> _routesList = new();
        internal List<RouteDictItem> _actionHandlersList = new();

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal void BindServer(HttpServer server)
        {
            if (this.parentServer is not null)
            {
                if (ReferenceEquals(server, this.parentServer))
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
                this.parentServer = server;
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
        /// Normalizes and combines the specified file-system paths into one.
        /// </summary>
        /// <param name="allowRelativeReturn">Specifies if relative paths should be merged and ".." returns should be respected.</param>
        /// <param name="separator">Specifies the path separator character.</param>
        /// <param name="paths">Specifies the array of paths to combine.</param>
        public static string FilesystemCombinePaths(bool allowRelativeReturn, char separator, ReadOnlySpan<string> paths)
        {
            return PathUtility.NormalizedCombine(allowRelativeReturn, separator, paths);
        }

        /// <summary>
        /// Normalizes and combines the specified file-system paths into one, using the default environment directory separator char.
        /// </summary>
        /// <param name="paths">Specifies the array of paths to combine.</param>
        public static string FilesystemCombinePaths(params string[] paths)
        {
            return PathUtility.NormalizedCombine(false, Path.DirectorySeparatorChar, paths);
        }

        /// <summary>
        /// Normalize the given path to use the specified directory separator, trim the last separator and
        /// remove empty entries.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <param name="directorySeparator">The directory separator.</param>
        public static string NormalizePath(string path, char directorySeparator = '/')
        {
            string[] parts = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            string result = string.Join(directorySeparator, parts);
            if (path.StartsWith('/') || path.StartsWith('\\')) result = directorySeparator + result;
            return result;
        }

        /// <summary>
        /// Gets an boolean indicating where this <see cref="Router"/> is read-only or not.
        /// </summary>
        public bool IsReadOnly { get => this.parentServer is not null; }

        /// <summary>
        /// Gets or sets whether this <see cref="Router"/> will match routes ignoring case.
        /// </summary>
        public bool MatchRoutesIgnoreCase { get; set; } = false;

        /// <summary>
        /// Creates an new <see cref="Router"/> instance with default values.
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
        public Route[] GetDefinedRoutes() => this._routesList.ToArray();

        /// <summary>
        /// Resolves the specified object into an valid <see cref="HttpResponse"/> using the defined
        /// value handlers or throws an exception if not possible.
        /// </summary>
        /// <param name="result">The object that will be converted to an valid <see cref="HttpResponse"/>.</param>
        /// <remarks>
        /// This method can throw exceptions. To avoid exceptions while trying to convert the specified object
        /// into an <see cref="HttpResponse"/>, consider using <see cref="TryResolveActionResult(object?, out HttpResponse?)"/>.
        /// </remarks>
        public HttpResponse ResolveActionResult(object? result)
        {
            return this.ResolveAction(result);
        }

        /// <summary>
        /// Tries to resolve the specified object into an valid <see cref="HttpResponse"/> using the defined
        /// value handlers.
        /// </summary>
        /// <param name="result">The object that will be converted to an valid <see cref="HttpResponse"/>.</param>
        /// <param name="response">When this method returns, the response object. This parameter is not initialized.</param>
        /// <returns>When this method returns, the <see cref="HttpResponse"/> object.</returns>
        public bool TryResolveActionResult(object? result, [NotNullWhen(true)] out HttpResponse? response)
        {
            if (result is null)
            {
                response = null;
                return false;
            }
            else if (result is HttpResponse httpres)
            {
                response = httpres;
                return true;
            }

            // IsReadOnly garantes that _actionHandlersList and
            // _routesList will be not modified during span reading
            ;
            bool wasLocked = false;
            if (!this.IsReadOnly)
            {
                wasLocked = true;
                Monitor.Enter(this._actionHandlersList);
            }
            try
            {
                Type actionType = result.GetType();

                Span<RouteDictItem> hspan = CollectionsMarshal.AsSpan(this._actionHandlersList);
                ref RouteDictItem pointer = ref MemoryMarshal.GetReference(hspan);
                for (int i = 0; i < hspan.Length; i++)
                {
                    ref RouteDictItem current = ref Unsafe.Add(ref pointer, i);
                    if (actionType.IsAssignableTo(current.type))
                    {
                        var resultObj = current.lambda.DynamicInvoke(result) as HttpResponse;
                        if (resultObj is null)
                        {
                            throw new InvalidOperationException(SR.Format(SR.Router_Handler_HandlerNotHttpResponse, current.type.Name));
                        }
                        response = resultObj;
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
                    Monitor.Exit(this._actionHandlersList);
                }
            }
        }

        /// <summary>
        /// Register an type handling association to converting it to an <see cref="HttpResponse"/> object.
        /// </summary>
        /// <param name="actionHandler">The function that receives an object of the <typeparamref name="T"/> and returns an <see cref="HttpResponse"/> response from the informed object.</param>
        public void RegisterValueHandler<T>(RouterActionHandlerCallback<T> actionHandler) where T : notnull
        {
            if (this.IsReadOnly)
            {
                throw new InvalidOperationException(SR.Router_ReadOnlyException);
            }
            Type type = typeof(T);
            if (type == typeof(HttpResponse))
            {
                throw new ArgumentException(SR.Router_Handler_HttpResponseRegister);
            }
            for (int i = 0; i < this._actionHandlersList!.Count; i++)
            {
                RouteDictItem item = this._actionHandlersList[i];
                if (item.type.Equals(type))
                {
                    throw new ArgumentException(SR.Router_Handler_Duplicate);
                }
            }
            this._actionHandlersList.Add(new RouteDictItem(type, actionHandler));
        }

        HttpResponse ResolveAction(object? routeResult)
        {
            if (routeResult is null)
            {
                throw new ArgumentNullException(nameof(routeResult), SR.Router_Handler_ActionNullValue);
            }
            else if (routeResult is HttpResponse rh)
            {
                return rh;
            }
            else if (this.TryResolveActionResult(routeResult, out HttpResponse? result))
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
            this.parentServer = null;
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
