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
/// Represents an event handler for the <see cref="HttpServer"/>, router, and related events.
/// </summary>
/// <definition>
/// public abstract class HttpServerHandler
/// </definition>
/// <type>
/// Class
/// </type>
public abstract class HttpServerHandler
{
    /// <summary>
    /// Method that is called immediately before starting the <see cref="HttpServer"/>.
    /// </summary>
    /// <param name="server">The Http server entity which is starting.</param>
    /// <definition>
    /// protected virtual void OnSetupHttpServer(HttpServer server)
    /// </definition>
    /// <type>
    /// Virtual method
    /// </type>
    protected virtual void OnServerStarting(HttpServer server) { }
    internal void InvokeOnServerStarting(HttpServer server) => OnServerStarting(server);

    /// <summary>
    /// Method that is called immediately after starting the <see cref="HttpServer"/>, when it's
    /// ready and listening.
    /// </summary>
    /// <param name="server">The Http server entity which is ready.</param>
    /// <definition>
    /// protected virtual void OnServerStarted(HttpServer server)
    /// </definition>
    /// <type>
    /// Virtual method
    /// </type>
    protected virtual void OnServerStarted(HttpServer server) { }
    internal void InvokeOnServerStarted(HttpServer server) => OnServerStarted(server);

    /// <summary>
    /// Method that is called when an <see cref="Router"/> is binded to the Http server.
    /// </summary>
    /// <param name="router">The router entity which is binded.</param>
    /// <definition>
    /// protected virtual void OnSetupRouter(Router router)
    /// </definition>
    /// <type>
    /// Virtual method
    /// </type>
    protected virtual void OnSetupRouter(Router router) { }

    internal void InvokeOnSetupRouter(Router router) => OnSetupRouter(router);

    /// <summary>
    /// Method that is called when an <see cref="HttpContextBagRepository"/> is created within an
    /// <see cref="HttpRequest"/> object.
    /// </summary>
    /// <param name="contextBag">The creating context bag.</param>
    /// <definition>
    /// protected virtual void OnContextBagCreated(HttpContextBagRepository contextBag) 
    /// </definition>
    /// <type>
    /// Virtual method
    /// </type>
    protected virtual void OnContextBagCreated(HttpContextBagRepository contextBag) { }
    internal void InvokeOnContextBagCreated(HttpContextBagRepository contextBag) => OnContextBagCreated(contextBag);

    /// <summary>
    /// Method that is called when an <see cref="HttpRequest"/> is received in the
    /// Http server.
    /// </summary>
    /// <param name="request">The connecting Http request entity.</param>
    /// <definition>
    /// protected virtual void OnHttpRequestOpen(HttpRequest request)
    /// </definition>
    /// <type>
    /// Virtual method
    /// </type>
    protected virtual void OnHttpRequestOpen(HttpRequest request) { }
    internal void InvokeOnHttpRequestOpen(HttpRequest request) => OnHttpRequestOpen(request);

    /// <summary>
    /// Method that is called when an <see cref="HttpRequest"/> is closed in the
    /// Http server.
    /// </summary>
    /// <param name="result">The result of the execution of the request.</param>
    /// <definition>
    /// protected virtual void OnHttpRequestClose(HttpServerExecutionResult result)
    /// </definition>
    /// <type>
    /// Virtual method
    /// </type>
    protected virtual void OnHttpRequestClose(HttpServerExecutionResult result) { }
    internal void InvokeOnHttpRequestClose(HttpServerExecutionResult result) => OnHttpRequestClose(result);

    /// <summary>
    /// Method that is called when an exception is caught in the Http server. This method is called
    /// regardless of whether <see cref="HttpServerConfiguration.ThrowExceptions"/> is enabled or not.
    /// </summary>
    /// <param name="exception">The exception object.</param>
    /// <definition>
    /// protected virtual void OnException(Exception exception)
    /// </definition>
    /// <type>
    /// Virtual method
    /// </type>
    protected virtual void OnException(Exception exception) { }
    internal void InvokeOnException(Exception exception) => OnException(exception);
}
