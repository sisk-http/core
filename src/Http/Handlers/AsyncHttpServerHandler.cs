// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   AsyncHttpServerHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using Sisk.Core.Routing;

namespace Sisk.Core.Http.Handlers;

/// <summary>
/// Represents an asynchronous event handler for the <see cref="HttpServer"/>, router, and related events.
/// </summary>
public abstract class AsyncHttpServerHandler : HttpServerHandler {
    /// <summary>
    /// Method that is called immediately before starting the <see cref="HttpServer"/>.
    /// </summary>
    /// <param name="server">The HTTP server entity which is starting.</param>
    protected virtual Task OnServerStartingAsync ( HttpServer server ) => Task.CompletedTask;

    /// <summary>
    /// Method that is called immediately after starting the <see cref="HttpServer"/>, when it's
    /// ready and listening.
    /// </summary>
    /// <param name="server">The HTTP server entity which is ready.</param>
    protected virtual Task OnServerStartedAsync ( HttpServer server ) => Task.CompletedTask;

    /// <summary>
    /// Method that is called before the <see cref="HttpServer"/> stop, when it is
    /// stopping from listening requests.
    /// </summary>
    /// <param name="server">The HTTP server entity which is stopping.</param>
    protected virtual Task OnServerStoppingAsync ( HttpServer server ) => Task.CompletedTask;

    /// <summary>
    /// Method that is called after the <see cref="HttpServer"/> is stopped, meaning
    /// it has stopped from listening to requests.
    /// </summary>
    /// <param name="server">The HTTP server entity which has stopped.</param>
    protected virtual Task OnServerStoppedAsync ( HttpServer server ) => Task.CompletedTask;

    /// <summary>
    /// Method that is called when an <see cref="Router"/> is binded to the HTTP server.
    /// </summary>
    /// <param name="router">The router entity which is binded.</param>
    protected virtual Task OnSetupRouterAsync ( Router router ) => Task.CompletedTask;

    /// <summary>
    /// Method that is called when an HTTP context is created within an
    /// <see cref="HttpRequest"/> object.
    /// </summary>
    /// <param name="contextBag">The creating context bag.</param>
    protected virtual Task OnContextBagCreatedAsync ( TypedValueDictionary contextBag ) => Task.CompletedTask;

    /// <summary>
    /// Method that is called when an <see cref="HttpRequest"/> is received in the
    /// HTTP server.
    /// </summary>
    /// <param name="request">The connecting HTTP request entity.</param>
    protected virtual Task OnHttpRequestOpenAsync ( HttpRequest request ) => Task.CompletedTask;

    /// <summary>
    /// Method that is called when an <see cref="HttpRequest"/> is closed in the
    /// HTTP server.
    /// </summary>
    /// <param name="result">The result of the execution of the request.</param>
    protected virtual Task OnHttpRequestCloseAsync ( HttpServerExecutionResult result ) => Task.CompletedTask;

    /// <summary>
    /// Method that is called when an exception is caught in the HTTP server. This method is called
    /// regardless of whether <see cref="HttpServerConfiguration.ThrowExceptions"/> is enabled or not.
    /// </summary>
    /// <param name="exception">The exception object.</param>
    protected virtual Task OnExceptionAsync ( Exception exception ) => Task.CompletedTask;

    /// <inheritdoc/>
    protected sealed override void OnContextBagCreated ( TypedValueDictionary contextBag ) {
        this.OnContextBagCreatedAsync ( contextBag ).GetAwaiter ().GetResult ();
    }

    /// <inheritdoc/>
    protected sealed override void OnException ( Exception exception ) {
        this.OnExceptionAsync ( exception ).GetAwaiter ().GetResult ();
    }

    /// <inheritdoc/>
    protected sealed override void OnHttpRequestClose ( HttpServerExecutionResult result ) {
        this.OnHttpRequestCloseAsync ( result ).GetAwaiter ().GetResult ();
    }

    /// <inheritdoc/>
    protected sealed override void OnHttpRequestOpen ( HttpRequest request ) {
        this.OnHttpRequestOpenAsync ( request ).GetAwaiter ().GetResult ();
    }

    /// <inheritdoc/>
    protected sealed override void OnServerStarted ( HttpServer server ) {
        this.OnServerStartedAsync ( server ).GetAwaiter ().GetResult ();
    }

    /// <inheritdoc/>
    protected sealed override void OnServerStarting ( HttpServer server ) {
        this.OnServerStartingAsync ( server ).GetAwaiter ().GetResult ();
    }

    /// <inheritdoc/>
    protected sealed override void OnServerStopped ( HttpServer server ) {
        this.OnServerStoppedAsync ( server ).GetAwaiter ().GetResult ();
    }

    /// <inheritdoc/>
    protected sealed override void OnServerStopping ( HttpServer server ) {
        this.OnServerStoppingAsync ( server ).GetAwaiter ().GetResult ();
    }

    /// <inheritdoc/>
    protected sealed override void OnSetupRouter ( Router router ) {
        this.OnSetupRouterAsync ( router ).GetAwaiter ().GetResult ();
    }
}
