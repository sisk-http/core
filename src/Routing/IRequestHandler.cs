// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IRequestHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;

namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents an interface that is executed before a request.
    /// </summary>
    public interface IRequestHandler
    {
        /// <summary>
        /// This method is called by the <see cref="Router"/> before executing a request when the <see cref="Route"/> instantiates an object that implements this interface. If it returns
        /// a <see cref="HttpResponse"/> object, the route action is not called and all execution of the route is stopped. If it returns "null", the execution is continued.
        /// </summary>
        /// <param name="request">The entry HTTP request.</param>
        /// <param name="context">The HTTP request context. It may contain information from other <see cref="IRequestHandler"/>.</param>
        HttpResponse? Execute(HttpRequest request, HttpContext context);

        /// <summary>
        /// Gets or sets when this RequestHandler should run.
        /// </summary>
        RequestHandlerExecutionMode ExecutionMode { get; init; }
    }

    /// <summary>
    /// Defines when the <see cref="IRequestHandler"/> object should be executed.
    /// </summary>
    public enum RequestHandlerExecutionMode
    {
        /// <summary>
        /// Indicates that the handler must be executed before the router calls the route action and after request contents is loaded.
        /// </summary>
        BeforeResponse,

        /// <summary>
        /// Indicates that the handler must be executed after the route action execution.
        /// </summary>
        AfterResponse
    }
}
