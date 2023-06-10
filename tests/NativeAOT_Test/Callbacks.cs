using Sisk.Core.Http;
using Sisk.Core.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NativeAOT_Test;

static class Callbacks
{
    [Route(RouteMethod.Get, "/")]
    public static HttpResponse Index(HttpRequest request)
    {
        StringContent content = new StringContent("<h1>Hello, world!</h1>", Encoding.UTF8, "text/html");
        return new HttpResponse(content);
    }
}
