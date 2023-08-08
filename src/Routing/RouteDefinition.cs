using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Routing;

internal class RouteDefinition
{
    public RouteMethod Method { get; set; }
    public string Path { get; set; }

    public RouteDefinition(RouteMethod method, String path)
    {
        Method = method;
        Path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public static RouteDefinition GetFromCallback(RouterCallback callback)
    {
        RouteAttribute? callbackType = callback.GetMethodInfo().GetCustomAttribute<RouteAttribute>(true);
        if (callbackType == null)
        {
            throw new InvalidOperationException("No route definition was found for the given callback. It may be possible that the " +
                "informed method does not implement the RouteAttribute attribute.");
        }
        else
        {
            return new RouteDefinition(callbackType.Method, callbackType.Path);
        }
    }
}
