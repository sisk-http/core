﻿// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
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
    /// <returns></returns>
    /// <definition>
    /// public delegate object RouterCallback(HttpRequest request);
    /// </definition>
    /// <type> 
    /// Delegate
    /// </type>
    public delegate object RouteAction(HttpRequest request);

    /// <summary>
    /// Represents the function that is called after no route is matched with the request.
    /// </summary>
    /// <returns></returns>
    /// <definition>
    /// public delegate HttpResponse NoMatchedRouteErrorCallback();
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate HttpResponse NoMatchedRouteErrorCallback();

    /// <summary>
    /// Represents the function that is called after the route action threw an exception.
    /// </summary>
    /// <returns></returns>
    /// <definition>
    /// public delegate HttpResponse ExceptionErrorCallback(Exception ex, HttpContext context);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate HttpResponse ExceptionErrorCallback(Exception ex, HttpContext context);
}
