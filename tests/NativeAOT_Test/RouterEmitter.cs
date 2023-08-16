using Sisk.Core.Routing;
using System.Collections.Specialized;

namespace NativeAOT_Test;
internal class RouterEmitter : RouterFactory
{
    public override void Bootstrap()
    {
        ;
    }

    public override Router BuildRouter()
    {
        Router r = new Router();
        r.SetObject(typeof(Callbacks));
        return r;
    }

    public override void Setup(NameValueCollection setupParameters)
    {
        ;
    }
}
