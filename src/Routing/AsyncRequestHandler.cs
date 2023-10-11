// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
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
/// <definition>
/// public abstract class AsyncRequestHandler : IRequestHandler
/// </definition>
/// <type>
/// Class
/// </type>
public abstract class AsyncRequestHandler : IRequestHandler
{
    /// <summary>
    /// Gets or sets when this RequestHandler should run.
    /// </summary>
    /// <definition>
    /// public abstract RequestHandlerExecutionMode ExecutionMode { get; init; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public abstract RequestHandlerExecutionMode ExecutionMode { get; init; }

    /// <summary>
    /// This method is called by the <see cref="Router"/> before executing a request when the <see cref="Route"/> instantiates an object that implements this interface. If it returns
    /// a <see cref="HttpResponse"/> object, the route callback is not called and all execution of the route is stopped. If it returns "null", the execution is continued.
    /// </summary>
    /// <param name="request">The entry HTTP request.</param>
    /// <param name="context">The HTTP request context. It may contain information from other <see cref="IRequestHandler"/>.</param>
    /// <returns></returns>
    /// <definition>
    /// public abstract Task{{HttpResponse?}} ExecuteAsync(HttpRequest request, HttpContext context)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public abstract Task<HttpResponse?> ExecuteAsync(HttpRequest request, HttpContext context);

    /// <inheritdoc/>
    /// <nodocs/>
    public HttpResponse? Execute(HttpRequest request, HttpContext context)
    {
        return this.ExecuteAsync(request, context).GetAwaiter().GetResult();
    }
}
