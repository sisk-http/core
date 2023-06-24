using Sisk.Core.Http;

namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents the function that is called after the route is matched with the request.
    /// </summary>
    /// <param name="request">The received request on the router.</param>
    /// <returns></returns>
    /// <definition>
    /// public delegate HttpResponse RouterCallback(HttpRequest request);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate HttpResponse RouterCallback(HttpRequest request);

    /// <summary>
    /// Represents the function that is called after no route is matched with the request.
    /// </summary>
    /// <returns></returns>
    /// <definition>
    /// public delegate HttpResponse NoMatchedRouteErrorCallback();
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate HttpResponse NoMatchedRouteErrorCallback();

    /// <summary>
    /// Represents the function that is called after the route callback threw an exception.
    /// </summary>
    /// <returns></returns>
    /// <definition>
    /// public delegate HttpResponse ExceptionErrorCallback(Exception ex, HttpContext context);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate HttpResponse ExceptionErrorCallback(Exception ex, HttpContext context);
}
