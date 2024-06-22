// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerHandlerRepository.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Routing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sisk.Core.Http.Handlers;

internal class HttpServerHandlerRepository
{
    private readonly HttpServer parent;
    private readonly List<HttpServerHandler> handlers = new List<HttpServerHandler>();
    internal readonly DefaultHttpServerHandler _default = new DefaultHttpServerHandler();

    public HttpServerHandlerRepository(HttpServer parent)
    {
        this.parent = parent;
        RegisterHandler(_default);
    }

    public void RegisterHandler(HttpServerHandler handler)
    {
        handlers.Add(handler);
    }

    private void CallEvery(Action<HttpServerHandler> action)
    {
        Span<HttpServerHandler> hspan = CollectionsMarshal.AsSpan(handlers);
        ref HttpServerHandler hpointer = ref MemoryMarshal.GetReference(hspan);
        for (int i = 0; i < hspan.Length; i++)
        {
            HttpServerHandler handler = Unsafe.Add(ref hpointer, i);

            try
            {
                action(handler);
            }
            catch (Exception ex)
            {
                if (!parent.ServerConfiguration.ThrowExceptions)
                {
                    parent.ServerConfiguration.ErrorsLogsStream?.WriteException(ex);
                }
                else throw;
            }
        }
    }

    internal void ServerStarting(HttpServer val) => CallEvery(handler => handler.InvokeOnServerStarting(val));
    internal void ServerStarted(HttpServer val) => CallEvery(handler => handler.InvokeOnServerStarted(val));
    internal void SetupRouter(Router val) => CallEvery(handler => handler.InvokeOnSetupRouter(val));
    internal void ContextBagCreated(HttpContextBagRepository val) => CallEvery(handler => handler.InvokeOnContextBagCreated(val));
    internal void HttpRequestOpen(HttpRequest val) => CallEvery(handler => handler.InvokeOnHttpRequestOpen(val));
    internal void HttpRequestClose(HttpServerExecutionResult val) => CallEvery(handler => handler.InvokeOnHttpRequestClose(val));
    internal void Exception(Exception val) => CallEvery(handler => handler.InvokeOnException(val));
    internal void Stopping(HttpServer val) => CallEvery(handler => handler.InvokeOnServerStopping(val));
    internal void Stopped(HttpServer val) => CallEvery(handler => handler.InvokeOnServerStopped(val));
}
