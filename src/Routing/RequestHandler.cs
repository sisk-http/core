// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RequestHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;

namespace Sisk.Core.Routing;

/// <summary>
/// Represents an abstract class which implements <see cref="IRequestHandler"/>.
/// </summary>
public abstract class RequestHandler : IRequestHandler {
    /// <inheritdoc/>
    /// <exclude/>
    public virtual RequestHandlerExecutionMode ExecutionMode { get; init; } = RequestHandlerExecutionMode.BeforeResponse;

    /// <inheritdoc/>
    /// <exclude/>
    public abstract HttpResponse? Execute ( HttpRequest request, HttpContext context );

    /// <summary>
    /// Returns an null <see cref="HttpResponse"/> reference, which points to the next request handler or route action.
    /// </summary>
    public HttpResponse? Next () {
        return null;
    }

    /// <summary>
    /// Gets an inline <see cref="RequestHandler"/> that resolves to the specified function.
    /// </summary>
    /// <param name="execute">The function that the <see cref="RequestHandler"/> will run.</param>
    /// <param name="executionMode">Optional. Determines where the request handler will be executed.</param>
    public static RequestHandler Create ( Func<HttpRequest, HttpContext, HttpResponse?> execute, RequestHandlerExecutionMode executionMode = RequestHandlerExecutionMode.BeforeResponse ) {
        return new InlineRequestHandler ( execute, executionMode );
    }
}

sealed class InlineRequestHandler : RequestHandler {
    public Func<HttpRequest, HttpContext, HttpResponse?> Handler { get; set; }

    public InlineRequestHandler ( Func<HttpRequest, HttpContext, HttpResponse?> handler, RequestHandlerExecutionMode mode ) {
        Handler = handler ?? throw new ArgumentNullException ( nameof ( handler ) );
        base.ExecutionMode = mode;
    }

    public override HttpResponse? Execute ( HttpRequest request, HttpContext context ) {
        return Handler ( request, context );
    }
}