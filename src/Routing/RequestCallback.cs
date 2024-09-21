// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RequestCallback.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;

namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents the function that is called after the route is matched with the request.
    /// </summary>
    /// <param name="request">The received request on the router.</param>
    public delegate object RouteAction(HttpRequest request);

    /// <summary>
    /// Represents the function that is called when an request reaches an error on the 
    /// router.
    /// </summary>
    public delegate HttpResponse RoutingErrorCallback(HttpContext context);

    /// <summary>
    /// Represents the function that is called after the route action threw an exception.
    /// </summary>
    public delegate HttpResponse ExceptionErrorCallback(Exception ex, HttpContext context);

    /// <summary>
    /// Represents the function that receives an object of the <typeparamref name="T"/> and returns an <see cref="HttpResponse"/> response from the informed object.
    /// </summary>
    /// <typeparam name="T">The input object type. Cannot be nullable.</typeparam>
    /// <param name="input">The result router object.</param>
    public delegate HttpResponse RouterActionHandlerCallback<T>(T input) where T : notnull;
}
