// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   AsyncRequestHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;

namespace Sisk.Core.Routing;

/// <summary>
/// Represents a class that implements <see cref="IRequestHandler"/> and its execution method is asynchronous.
/// </summary>
public abstract class AsyncRequestHandler : IRequestHandler
{
    /// <inheritdoc/>
    public virtual RequestHandlerExecutionMode ExecutionMode { get; init; } = RequestHandlerExecutionMode.BeforeResponse;

    /// <summary>
    /// This method is called by the <see cref="Router"/> before executing a request when the <see cref="Route"/> instantiates an object that implements this interface. If it returns
    /// a <see cref="HttpResponse"/> object, the route action is not called and all execution of the route is stopped. If it returns "null", the execution is continued.
    /// </summary>
    /// <param name="request">The entry HTTP request.</param>
    /// <param name="context">The HTTP request context. It may contain information from other <see cref="IRequestHandler"/>.</param>
    public abstract Task<HttpResponse?> ExecuteAsync(HttpRequest request, HttpContext context);

    HttpResponse? IRequestHandler.Execute(HttpRequest request, HttpContext context)
    {
        return this.ExecuteAsync(request, context).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets an inline <see cref="AsyncRequestHandler"/> that resolves to the specified function.
    /// </summary>
    /// <param name="execute">The function that the <see cref="AsyncRequestHandler"/> will run.</param>
    /// <param name="executionMode">Optional. Determines where the request handler will be executed.</param>
    public static AsyncRequestHandler Create(Func<HttpRequest, HttpContext, Task<HttpResponse?>> execute, RequestHandlerExecutionMode executionMode = RequestHandlerExecutionMode.BeforeResponse)
    {
        return new InlineAsyncRequestHandler(execute, executionMode);
    }
}

class InlineAsyncRequestHandler : AsyncRequestHandler
{
    public Func<HttpRequest, HttpContext, Task<HttpResponse?>> Handler { get; set; }

    public InlineAsyncRequestHandler(Func<HttpRequest, HttpContext, Task<HttpResponse?>> handler, RequestHandlerExecutionMode mode)
    {
        this.Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        base.ExecutionMode = mode;
    }

    public override async Task<HttpResponse?> ExecuteAsync(HttpRequest request, HttpContext context)
    {
        return await this.Handler(request, context);
    }
}