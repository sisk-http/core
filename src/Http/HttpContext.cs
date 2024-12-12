// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpContext.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using Sisk.Core.Internal;
using Sisk.Core.Routing;
using System.Diagnostics.CodeAnalysis;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents an context that is shared in a entire HTTP session.
    /// </summary>
    public sealed class HttpContext
    {
        internal readonly static AsyncLocal<HttpContext?> _context = new AsyncLocal<HttpContext?>();

        /// <summary>
        /// Gets the current running <see cref="HttpContext"/>.
        /// </summary>
        /// <remarks>
        /// This property is only accessible during an HTTP session, within the executing HTTP code.
        /// </remarks>
        [Experimental(DiagnosticId.Sisk_HttpContext_Current_Experimental)]
        public static HttpContext Current { get => _context.Value ?? throw new InvalidOperationException(SR.HttpContext_InvalidThreadStaticAccess); }

        /// <summary>
        /// Gets whether the current thread context is running inside an HTTP context.
        /// </summary>
        public static bool IsRequestContext { get => _context.Value is not null; }

        /// <summary>
        /// Gets or sets an <see cref="HttpHeaderCollection"/> indicating HTTP headers which
        /// will overwrite headers set by CORS, router response or request handlers.
        /// </summary>
        /// <remarks>
        /// This property replaces existing headers in the final response. Use <see cref="ExtraHeaders"/> to
        /// add headers without replacing existing ones.
        /// </remarks>
        public HttpHeaderCollection OverrideHeaders { get; set; } = new HttpHeaderCollection();

        /// <summary>
        /// Gets or sets the <see cref="HttpHeaderCollection"/> indicating HTTP headers which will
        /// be added (not overwritten) in the final response.
        /// </summary>
        public HttpHeaderCollection ExtraHeaders { get; set; } = new HttpHeaderCollection();

        /// <summary>
        /// Gets the <see cref="Http.ListeningHost"/> instance of this HTTP context.
        /// </summary>
        public ListeningHost? ListeningHost { get; internal set; }

        /// <summary>
        /// Gets or sets a managed collection for this HTTP context.
        /// </summary>
        public TypedValueDictionary RequestBag { get; set; } = new TypedValueDictionary();

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

        internal HttpContext(HttpServer httpServer)
        {
            this.HttpServer = httpServer;
            this.Request = null!; // associated later
            this.Router = null!;// associated later, may be null
            this.ListeningHost = null!; // associated later, may be null
        }
    }
}
