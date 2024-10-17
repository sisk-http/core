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