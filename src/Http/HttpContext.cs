// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpContext.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections.Concurrent;
using Sisk.Core.Entity;
using Sisk.Core.Routing;

namespace Sisk.Core.Http {

    /// <summary>
    /// Represents an context that is shared in a entire HTTP session.
    /// </summary>
    public sealed class HttpContext : IDisposable {

        bool _disposed;
        internal readonly static AsyncLocal<HttpContext?> _context = new AsyncLocal<HttpContext?> ();
        internal readonly ConcurrentQueue<Func<Task>> _deferredActions = new ();

        /// <summary>
        /// Gets the current running <see cref="HttpContext"/>.
        /// </summary>
        /// <remarks>
        /// This property is only accessible during an HTTP session, within the executing HTTP code.
        /// </remarks>
        public static HttpContext Current { get => _context.Value ?? throw new InvalidOperationException ( SR.HttpContext_InvalidThreadStaticAccess ); }

        /// <summary>
        /// Gets whether the current thread context is running inside an HTTP context.
        /// </summary>
        public static bool IsRequestContext { get => _context.Value is not null; }

        /// <summary>
        /// Gets the current running <see cref="HttpContext"/>.
        /// </summary>
        /// <returns>The current <see cref="HttpContext"/> instance, otherwise, <see langword="null"/>.</returns>
        public static HttpContext? GetCurrentContext () => _context.Value;

        /// <summary>
        /// Gets or sets an <see cref="HttpHeaderCollection"/> indicating HTTP headers which
        /// will overwrite headers set by CORS, router response or request handlers.
        /// </summary>
        /// <remarks>
        /// This property replaces existing headers in the final response. Use <see cref="ExtraHeaders"/> to
        /// add headers without replacing existing ones.
        /// </remarks>
        public HttpHeaderCollection OverrideHeaders { get; set; } = new HttpHeaderCollection ();

        /// <summary>
        /// Gets or sets the <see cref="HttpHeaderCollection"/> indicating HTTP headers which will
        /// be added (not overwritten) in the final response.
        /// </summary>
        public HttpHeaderCollection ExtraHeaders { get; set; } = new HttpHeaderCollection ();

        /// <summary>
        /// Gets the <see cref="Http.ListeningHost"/> instance of this HTTP context.
        /// </summary>
        public ListeningHost? ListeningHost { get; internal set; }

        /// <summary>
        /// Gets or sets a managed collection for this HTTP context.
        /// </summary>
        public TypedValueDictionary RequestBag { get; set; } = new TypedValueDictionary ();

        /// <summary>
        /// Gets the atomic numeric operations available for values stored in <see cref="RequestBag"/>.
        /// </summary>
        /// <remarks>
        /// Atomicity is guaranteed only between operations performed through this property. Concurrent direct
        /// access to <see cref="RequestBag"/> is not synchronized by these operations.
        /// </remarks>
        public HttpContextInterlocked Interlocked { get; }

        /// <summary>
        /// Gets the context <see cref="Http.HttpServer"/> instance.
        /// </summary>
        public HttpServer HttpServer { get; private set; }

        /// <summary>
        /// Gets the <see cref="Http.HttpResponse"/> for this context. This property acessible when a post-executing
        /// <see cref="IRequestHandler"/> was executed for this router context.
        /// </summary>
        public HttpResponse? RouterResponse { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Http.HttpRequest"/> which is contained in this HTTP context.
        /// </summary>
        public HttpRequest Request { get; internal set; }

        /// <summary>
        /// Gets the matched <see cref="Routing.Route"/> for this context.
        /// </summary>
        public Route? MatchedRoute { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Routing.Router"/> where this context was
        /// created.
        /// </summary>
        public Router? Router { get; internal set; }

        /// <summary>
        /// Gets or sets an <see cref="LogOutput"/> mode for this context, which will overwrite the
        /// matched route log mode option.
        /// </summary>
        public LogOutput? LogMode { get; set; }

        /// <summary>
        /// Enqueues an action that will be executed after the response is sent to the client.
        /// This action runs within the same context, with access to all current context properties before disposal.
        /// </summary>
        /// <param name="action">The synchronous action to execute.</param>
        public void EnqueueDeferredAction ( Action action ) {
            _deferredActions.Enqueue ( new Func<Task> ( () => {
                action ();
                return Task.CompletedTask;
            } ) );
        }

        /// <summary>
        /// Enqueues an asynchronous action that will be executed after the response is sent to the client.
        /// This action runs within the same context, with access to all current context properties before disposal.
        /// </summary>
        /// <param name="action">The asynchronous action to execute.</param>
        public void EnqueueDeferredAction ( Func<Task> action ) {
            _deferredActions.Enqueue ( action );
        }

        /// <summary>
        /// Enqueues an asynchronous action that will be executed after the response is sent to the client.
        /// This action runs within the same context, with access to all current context properties before disposal.
        /// </summary>
        /// <param name="action">The asynchronous action to execute.</param>
        /// <param name="cancellation">The cancellation token to observe.</param>
        public void EnqueueDeferredAction ( Func<CancellationToken, Task> action, CancellationToken cancellation = default ) {
            _deferredActions.Enqueue ( new Func<Task> ( async () => {
                if (cancellation.IsCancellationRequested) {
                    return;
                }
                await action ( cancellation );
            } ) );
        }

        /// <summary>
        /// Enqueues an asynchronous action that will be executed after the response is sent to the client,
        /// with a timeout after which the action will be cancelled.
        /// This action runs within the same context, with access to all current context properties before disposal.
        /// </summary>
        /// <param name="action">The asynchronous action to execute.</param>
        /// <param name="timeout">The timeout after which the action will be cancelled.</param>
        public void EnqueueDeferredAction ( Func<CancellationToken, Task> action, TimeSpan timeout = default ) {
            var cts = new CancellationTokenSource ( timeout );
            _deferredActions.Enqueue ( new Func<Task> ( async () => {
                if (cts.IsCancellationRequested) {
                    return;
                }
                await action ( cts.Token );
            } ) );
            _deferredActions.Enqueue ( () => {
                cts.Dispose ();
                return Task.CompletedTask;
            } );
        }

        /// <inheritdoc/>
        void IDisposable.Dispose () {
            if (_disposed)
                return;

            _disposed = true;
        }

        internal HttpContext ( HttpServer httpServer ) {
            HttpServer = httpServer;
            Request = null!; // associated later
            Router = null!;// associated later, may be null
            ListeningHost = null!; // associated later, may be null
            Interlocked = new HttpContextInterlocked ( this );
        }

        /// <summary>
        /// Provides atomic operations for numeric values stored by name in an <see cref="HttpContext.RequestBag"/>.
        /// </summary>
        /// <remarks>
        /// Values created by this type are stored as <see cref="double"/>. Operations are atomic with respect to
        /// other operations performed by this instance, but not with respect to direct access to the context bag.
        /// </remarks>
        public sealed class HttpContextInterlocked {

            readonly HttpContext _context;

            internal HttpContextInterlocked ( HttpContext inner ) {
                _context = inner;
            }

            /// <summary>
            /// Atomically adds an integer to the value associated with the specified name.
            /// </summary>
            /// <param name="name">The name of the value.</param>
            /// <param name="value">The value to add.</param>
            /// <remarks>If <paramref name="name"/> is not present, it is created with <paramref name="value"/>.</remarks>
            public void Add ( string name, Int32 value ) {
                Add ( name, (double) value );
            }

            /// <summary>
            /// Atomically adds a number to the value associated with the specified name.
            /// </summary>
            /// <param name="name">The name of the value.</param>
            /// <param name="value">The value to add.</param>
            /// <remarks>If <paramref name="name"/> is not present, it is created with <paramref name="value"/>.</remarks>
            public void Add ( string name, double value ) {
                var requestBag = _context.RequestBag;
                lock (requestBag._values) {
                    if (requestBag.TryGetValue ( name, out double currentValue )) {
                        requestBag [ name ] = currentValue + value;
                    }
                    else {
                        requestBag [ name ] = value;
                    }
                }
            }

            /// <summary>
            /// Atomically compares the value associated with the specified name and replaces it when equal.
            /// </summary>
            /// <param name="name">The name of the value.</param>
            /// <param name="value">The replacement value.</param>
            /// <param name="comparand">The value to compare with the stored value.</param>
            /// <param name="notFound">The value returned when <paramref name="name"/> is not present.</param>
            /// <returns>The original stored value, or <paramref name="notFound"/> when the name is not present.</returns>
            public double CompareExchange ( string name, Int64 value, Int64 comparand, Int64 notFound = -1 ) {
                return CompareExchange ( name, (double) value, (double) comparand, (double) notFound );
            }

            /// <summary>
            /// Atomically compares the value associated with the specified name and replaces it when equal.
            /// </summary>
            /// <param name="name">The name of the value.</param>
            /// <param name="value">The replacement value.</param>
            /// <param name="comparand">The value to compare with the stored value.</param>
            /// <param name="notFound">The value returned when <paramref name="name"/> is not present.</param>
            /// <returns>The original stored value, or <paramref name="notFound"/> when the name is not present.</returns>
            public double CompareExchange ( string name, double value, double comparand, double notFound = -1 ) {
                var requestBag = _context.RequestBag;
                lock (requestBag._values) {
                    if (requestBag.TryGetValue ( name, out double currentValue )) {
                        if (currentValue == comparand) {
                            requestBag [ name ] = value;
                        }
                        return currentValue;
                    }
                    return notFound;
                }
            }

            /// <summary>
            /// Atomically replaces the value associated with the specified name.
            /// </summary>
            /// <param name="name">The name of the value.</param>
            /// <param name="value">The replacement value.</param>
            /// <returns>The original stored value, or <paramref name="value"/> when the name was not present.</returns>
            public double Exchange ( string name, Int64 value ) {
                return Exchange ( name, (double) value );
            }

            /// <summary>
            /// Atomically replaces the value associated with the specified name.
            /// </summary>
            /// <param name="name">The name of the value.</param>
            /// <param name="value">The replacement value.</param>
            /// <returns>The original stored value, or <paramref name="value"/> when the name was not present.</returns>
            public double Exchange ( string name, double value ) {
                var requestBag = _context.RequestBag;
                lock (requestBag._values) {
                    if (requestBag.TryGetValue ( name, out double currentValue )) {
                        requestBag [ name ] = value;
                        return currentValue;
                    }
                    else {
                        requestBag [ name ] = value;
                        return value;
                    }
                }
            }

            /// <summary>
            /// Atomically reads the value associated with the specified name.
            /// </summary>
            /// <param name="name">The name of the value.</param>
            /// <returns>The stored value, or <see langword="null"/> when the name is not present.</returns>
            public double? Inspect ( string name ) {
                var requestBag = _context.RequestBag;
                lock (requestBag._values) {
                    if (requestBag.TryGetValue ( name, out double currentValue )) {
                        return currentValue;
                    }
                    return null;
                }
            }
        }
    }
}
