// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MvcServerHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Http.Handlers;
using Sisk.Core.Routing;

namespace Sisk.Mvc;

public sealed class MvcServerHandler : HttpServerHandler
{
    protected override void OnSetupRouter(Router router)
    {
        RegexRoute rgxRoute = new RegexRoute(RouteMethod.Get, @"/assets/(?<filename>.*)", StaticRouter.ServeStaticAsset);
        router.SetRoute(rgxRoute);
    }

    protected override void OnServerStarting(HttpServer server)
    {
        ViewManager.InitializePartials();
    }
}