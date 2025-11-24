// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerHandlerRepository.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;
using Sisk.Core.Entity;
using Sisk.Core.Routing;

namespace Sisk.Core.Http.Handlers;

enum HttpServerHandlerActionEvent {
    ServerStarting,
    ServerStarted,
    SetupRouter,
    ContextBagCreated,
    HttpRequestOpen,
    HttpRequestClose,
    Exception,
    Stopping,
    Stopped,
}

internal sealed class HttpServerHandlerRepository {
    private readonly HttpServer parent;
    private readonly SortedSet<HttpServerHandler> handlers = new SortedSet<HttpServerHandler> ( HttpServerHandlerSortingComparer.Instance );
    internal readonly DefaultHttpServerHandler _default = new DefaultHttpServerHandler ();

    public HttpServerHandlerRepository ( HttpServer parent ) {
        this.parent = parent;
        RegisterHandler ( _default );
    }

    public void RegisterHandler ( HttpServerHandler handler ) {
        handlers.Add ( handler );
    }

    private bool IsEventBreakable ( HttpServerHandlerActionEvent eventName )
        => eventName == HttpServerHandlerActionEvent.ServerStarting
        || eventName == HttpServerHandlerActionEvent.ServerStarted
        || eventName == HttpServerHandlerActionEvent.SetupRouter;

    private void CallEvery ( HandlerActionBase action, HttpServerHandlerActionEvent eventName ) {

        foreach (var handler in handlers) {
            try {
                action ( handler );
            }
            catch (Exception ex) {
                if (parent.ServerConfiguration.ThrowExceptions == false && IsEventBreakable ( eventName ) == false) {
                    parent.ServerConfiguration.ErrorsLogsStream?.WriteException ( ex );
                }
                else
                    throw;
            }
        }
    }


    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void ServerStarting ( HttpServer val ) => CallEvery ( handler => handler.InvokeOnServerStarting ( val ), HttpServerHandlerActionEvent.ServerStarting );
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void ServerStarted ( HttpServer val ) => CallEvery ( handler => handler.InvokeOnServerStarted ( val ), HttpServerHandlerActionEvent.ServerStarted );
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void SetupRouter ( Router val ) => CallEvery ( handler => handler.InvokeOnSetupRouter ( val ), HttpServerHandlerActionEvent.SetupRouter );
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void ContextBagCreated ( TypedValueDictionary val ) => CallEvery ( handler => handler.InvokeOnContextBagCreated ( val ), HttpServerHandlerActionEvent.ContextBagCreated );
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void HttpRequestOpen ( HttpRequest val ) => CallEvery ( handler => handler.InvokeOnHttpRequestOpen ( val ), HttpServerHandlerActionEvent.HttpRequestOpen );
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void HttpRequestClose ( HttpServerExecutionResult val ) => CallEvery ( handler => handler.InvokeOnHttpRequestClose ( val ), HttpServerHandlerActionEvent.HttpRequestClose );
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void Exception ( Exception val ) => CallEvery ( handler => handler.InvokeOnException ( val ), HttpServerHandlerActionEvent.Exception );
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void Stopping ( HttpServer val ) => CallEvery ( handler => handler.InvokeOnServerStopping ( val ), HttpServerHandlerActionEvent.Stopping );
    [MethodImpl ( MethodImplOptions.AggressiveInlining )] internal void Stopped ( HttpServer val ) => CallEvery ( handler => handler.InvokeOnServerStopped ( val ), HttpServerHandlerActionEvent.Stopped );

    internal delegate void HandlerActionBase ( HttpServerHandler handler );
}
