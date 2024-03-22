// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   DefaultHttpServerHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Routing;

namespace Sisk.Core.Http.Handlers;

internal class DefaultHttpServerHandler : HttpServerHandler
{
    internal static Action<Router>? _routerSetup;
    internal static Action? _serverBootstraping;

    protected override void OnServerStarting(HttpServer server)
    {
        base.OnServerStarting(server);
        if (_serverBootstraping != null) _serverBootstraping();
    }

    protected override void OnSetupRouter(Router router)
    {
        base.OnSetupRouter(router);
        if (_routerSetup != null) _routerSetup(router);
    }
}
