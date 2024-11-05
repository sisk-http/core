// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerHandlerRepository.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using Sisk.Core.Routing;

namespace Sisk.Core.Http.Handlers;

enum HttpServerHandlerActionEvent
{
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

internal class HttpServerHandlerRepository
{
    private readonly HttpServer parent;
    private readonly List<HttpServerHandler> handlers = new List<HttpServerHandler>();
    internal readonly DefaultHttpServerHandler _default = new DefaultHttpServerHandler();

    public HttpServerHandlerRepository(HttpServer parent)
    {
        this.parent = parent;
        this.RegisterHandler(this._default);
    }

    public void RegisterHandler(HttpServerHandler handler)
    {
        this.handlers.Add(handler);
    }

    private bool IsEventBreakable(HttpServerHandlerActionEvent eventName)
        => eventName == HttpServerHandlerActionEvent.ServerStarting
        || eventName == HttpServerHandlerActionEvent.ServerStarted
        || eventName == HttpServerHandlerActionEvent.SetupRouter;

    private void CallEvery(HandlerActionBase action, HttpServerHandlerActionEvent eventName)
    {
        int c = this.handlers.Count;
        for (int i = 0; i < c; i++)
        {
            var handler = this.handlers[i];

            try
            {
                action.DynamicInvoke(handler);
            }
            catch (Exception ex)
            {
                if (this.parent.ServerConfiguration.ThrowExceptions == false && this.IsEventBreakable(eventName) == false)
                {
                    this.parent.ServerConfiguration.ErrorsLogsStream?.WriteException(ex);
                }
                else throw;
            }
        }
    }

    internal void ServerStarting(HttpServer val) => this.CallEvery(handler => handler.InvokeOnServerStarting(val), HttpServerHandlerActionEvent.ServerStarting);
    internal void ServerStarted(HttpServer val) => this.CallEvery(handler => handler.InvokeOnServerStarted(val), HttpServerHandlerActionEvent.ServerStarted);
    internal void SetupRouter(Router val) => this.CallEvery(handler => handler.InvokeOnSetupRouter(val), HttpServerHandlerActionEvent.SetupRouter);
    internal void ContextBagCreated(TypedValueDictionary val) => this.CallEvery(handler => handler.InvokeOnContextBagCreated(val), HttpServerHandlerActionEvent.ContextBagCreated);
    internal void HttpRequestOpen(HttpRequest val) => this.CallEvery(handler => handler.InvokeOnHttpRequestOpen(val), HttpServerHandlerActionEvent.HttpRequestOpen);
    internal void HttpRequestClose(HttpServerExecutionResult val) => this.CallEvery(handler => handler.InvokeOnHttpRequestClose(val), HttpServerHandlerActionEvent.HttpRequestClose);
    internal void Exception(Exception val) => this.CallEvery(handler => handler.InvokeOnException(val), HttpServerHandlerActionEvent.Exception);
    internal void Stopping(HttpServer val) => this.CallEvery(handler => handler.InvokeOnServerStopping(val), HttpServerHandlerActionEvent.Stopping);
    internal void Stopped(HttpServer val) => this.CallEvery(handler => handler.InvokeOnServerStopped(val), HttpServerHandlerActionEvent.Stopped);

    internal delegate void HandlerActionBase(HttpServerHandler handler);
}
