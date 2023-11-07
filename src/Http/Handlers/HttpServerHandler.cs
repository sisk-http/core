// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Routing;

namespace Sisk.Core.Http.Handlers;

/// <summary>
/// Represents a handler for the <see cref="HttpServer"/>, router, and related modules.
/// </summary>
/// <definition>
/// public class HttpServerHandler
/// </definition>
/// <type>
/// Class
/// </type>
public class HttpServerHandler
{
    /// <summary>
    /// Method that is called before starting the <see cref="HttpServer"/>.
    /// </summary>
    /// <param name="server">The Http server entity which is starting.</param>
    /// <definition>
    /// public virtual void OnSetupHttpServer(HttpServer server)
    /// </definition>
    /// <type>
    /// Virtual method
    /// </type>
    public virtual void OnSetupHttpServer(HttpServer server) { }

    /// <summary>
    /// Method that is called when an <see cref="Router"/> is binded to the Http server.
    /// </summary>
    /// <param name="router">The router entity which is binded.</param>
    /// <definition>
    /// public virtual void OnSetupRouter(Router router)
    /// </definition>
    /// <type>
    /// Virtual method
    /// </type>
    public virtual void OnSetupRouter(Router router) { }

    /// <summary>
    /// Method that is called when an <see cref="HttpContextBagRepository"/> is created within an
    /// <see cref="HttpRequest"/> object.
    /// </summary>
    /// <param name="contextBag">The creating context bag.</param>
    /// <definition>
    /// public virtual void OnContextBagCreated(HttpContextBagRepository contextBag) 
    /// </definition>
    /// <type>
    /// Virtual method
    /// </type>
    public virtual void OnContextBagCreated(HttpContextBagRepository contextBag) { }

    /// <summary>
    /// Method that is called when an <see cref="HttpRequest"/> is received in the
    /// Http server.
    /// </summary>
    /// <param name="request">The connecting Http request entity.</param>
    /// <definition>
    /// public virtual void OnHttpRequestOpen(HttpRequest request)
    /// </definition>
    /// <type>
    /// Virtual method
    /// </type>
    public virtual void OnHttpRequestOpen(HttpRequest request) { }

    /// <summary>
    /// Method that is called when an <see cref="HttpRequest"/> is closed in the
    /// Http server.
    /// </summary>
    /// <param name="result">The result of the execution of the request.</param>
    /// <definition>
    /// public virtual void OnHttpRequestClose(HttpServerExecutionResult result)
    /// </definition>
    /// <type>
    /// Virtual method
    /// </type>
    public virtual void OnHttpRequestClose(HttpServerExecutionResult result) { }

    /// <summary>
    /// Method that is called when an exception is caught in the Http server.
    /// </summary>
    /// <param name="exception">The exception object.</param>
    /// <definition>
    /// public virtual void OnException(Exception exception)
    /// </definition>
    /// <type>
    /// Virtual method
    /// </type>
    public virtual void OnException(Exception exception) { }
}
