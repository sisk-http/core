// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerHandler.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;
using Sisk.Core.Entity;
using Sisk.Core.Routing;

namespace Sisk.Core.Http.Handlers;

/// <summary>
/// Represents an event handler for the <see cref="HttpServer"/>, router, and related events.
/// </summary>
public abstract class HttpServerHandler {

    /// <summary>
    /// Gets the priority value that determines the order in which handlers are executed.
    /// Lower values are executed first.
    /// </summary>
    public virtual int Priority { get; }

    /// <summary>
    /// Event that is called immediately before starting the <see cref="HttpServer"/>.
    /// </summary>
    /// <param name="server">The HTTP server entity which is starting.</param>
    protected virtual void OnServerStarting ( HttpServer server ) { }
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void InvokeOnServerStarting ( HttpServer server ) => OnServerStarting ( server );

    /// <summary>
    /// Event that is called immediately after starting the <see cref="HttpServer"/>, when it's
    /// ready and listening.
    /// </summary>
    /// <param name="server">The HTTP server entity which is ready.</param>
    protected virtual void OnServerStarted ( HttpServer server ) { }
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void InvokeOnServerStarted ( HttpServer server ) => OnServerStarted ( server );

    /// <summary>
    /// Event that is called before the <see cref="HttpServer"/> stop, when it is
    /// stopping from listening requests.
    /// </summary>
    /// <param name="server">The HTTP server entity which is stopping.</param>
    protected virtual void OnServerStopping ( HttpServer server ) { }
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void InvokeOnServerStopping ( HttpServer server ) => OnServerStopping ( server );

    /// <summary>
    /// Event that is called after the <see cref="HttpServer"/> is stopped, meaning
    /// it has stopped from listening to requests.
    /// </summary>
    /// <param name="server">The HTTP server entity which has stopped.</param>
    protected virtual void OnServerStopped ( HttpServer server ) { }
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void InvokeOnServerStopped ( HttpServer server ) => OnServerStopped ( server );

    /// <summary>
    /// Event that is called when an <see cref="Router"/> is binded to the HTTP server.
    /// </summary>
    /// <param name="router">The router entity which is binded.</param>
    protected virtual void OnSetupRouter ( Router router ) { }
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void InvokeOnSetupRouter ( Router router ) => OnSetupRouter ( router );

    /// <summary>
    /// Event that is called when an HTTP context is created within an
    /// <see cref="HttpRequest"/> object.
    /// </summary>
    /// <param name="contextBag">The creating context bag.</param>
    protected virtual void OnContextBagCreated ( TypedValueDictionary contextBag ) { }
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void InvokeOnContextBagCreated ( TypedValueDictionary contextBag ) => OnContextBagCreated ( contextBag );

    /// <summary>
    /// Event that is called when an <see cref="HttpRequest"/> is received in the
    /// HTTP server.
    /// </summary>
    /// <param name="request">The connecting HTTP request entity.</param>
    protected virtual void OnHttpRequestOpen ( HttpRequest request ) { }
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void InvokeOnHttpRequestOpen ( HttpRequest request ) => OnHttpRequestOpen ( request );

    /// <summary>
    /// Event that is called when an <see cref="HttpRequest"/> is closed in the
    /// HTTP server.
    /// </summary>
    /// <param name="result">The result of the execution of the request.</param>
    protected virtual void OnHttpRequestClose ( HttpServerExecutionResult result ) { }
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void InvokeOnHttpRequestClose ( HttpServerExecutionResult result ) => OnHttpRequestClose ( result );

    /// <summary>
    /// Event that is called when an exception is caught in the HTTP server. This method is called
    /// regardless of whether <see cref="HttpServerConfiguration.ThrowExceptions"/> is enabled or not.
    /// </summary>
    /// <param name="exception">The exception object.</param>
    protected virtual void OnException ( Exception exception ) { }
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void InvokeOnException ( Exception exception ) => OnException ( exception );
}
