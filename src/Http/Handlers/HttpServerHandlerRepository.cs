// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerHandlerRepository.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Routing;

namespace Sisk.Core.Http.Handlers;

internal class HttpServerHandlerRepository
{
    private List<HttpServerHandler> handlers = new List<HttpServerHandler>();

    public void RegisterHandler(HttpServerHandler handler)
    {
        handlers.Add(handler);
    }

    private void CallEvery(Action<HttpServerHandler> action)
    {
        foreach (HttpServerHandler handler in handlers)
            action(handler);
    }

    internal void ServerStarting(HttpServer val) => CallEvery(handler => handler.InvokeOnServerStarting(val));
    internal void ServerStarted(HttpServer val) => CallEvery(handler => handler.InvokeOnServerStarted(val));
    internal void SetupRouter(Router val) => CallEvery(handler => handler.InvokeOnSetupRouter(val));
    internal void ContextBagCreated(HttpContextBagRepository val) => CallEvery(handler => handler.InvokeOnContextBagCreated(val));
    internal void HttpRequestOpen(HttpRequest val) => CallEvery(handler => handler.InvokeOnHttpRequestOpen(val));
    internal void HttpRequestClose(HttpServerExecutionResult val) => CallEvery(handler => handler.InvokeOnHttpRequestClose(val));
    internal void Exception(Exception val) => CallEvery(handler => handler.InvokeOnException(val));
}
