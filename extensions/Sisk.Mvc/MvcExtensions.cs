using Sisk.Core.Http.Hosting;

namespace Sisk.Mvc;

public static class MvcExtensions
{
    public static HttpServerHostContextBuilder UseMvc(this HttpServerHostContextBuilder builder)
    {
        return builder.UseHandler<MvcServerHandler>();
    }
}