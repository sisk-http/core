using Sisk.Core.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Sisk.Core.Routing;

public partial class Router
{
    #region "Route setters"
    /// <summary>
    /// Defines an route to an router.
    /// </summary>
    /// <param name="r">The router instance which the route is being set.</param>
    /// <param name="route">The route to be defined in the router.</param>
    /// <definition>
    /// public static Router operator +(Router r, Route route)
    /// </definition>
    /// <type>
    /// Operator
    /// </type>
    public static Router operator +(Router r, Route route)
    {
        r.SetRoute(route);
        return r;
    }

    /// <summary>
    /// Gets an route object by their name that is defined in this Router.
    /// </summary>
    /// <param name="name">The route name.</param>
    /// <returns></returns>
    /// <definition>
    /// public Route? GetRouteFromName(string name)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public Route? GetRouteFromName(string name)
    {
        foreach (Route r in this._routes)
        {
            if (r.Name == name)
            {
                return r;
            }
        }
        return null;
    }

    /// <summary>
    /// Defines an route with their method, path and callback function.
    /// </summary>
    /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
    /// <param name="path">The route path.</param>
    /// <param name="callback">The route function to be called after matched.</param>
    /// <definition>
    /// public void SetRoute(RouteMethod method, string path, RouterCallback callback)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void SetRoute(RouteMethod method, string path, RouterCallback callback)
    {
        Route newRoute = new Route(method, path, null, callback, null);
        Route? collisonRoute;
        if ((collisonRoute = GetCollisionRoute(newRoute.Method, newRoute.Path)) != null)
        {
            throw new ArgumentException($"A possible route collision could happen between route {newRoute} and route {collisonRoute}. Please review the methods and paths of these routes.");
        }
        _routes.Add(newRoute);
    }

    /// <summary>
    /// Defines an route with their method, path, callback function and name.
    /// </summary>
    /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
    /// <param name="path">The route path.</param>
    /// <param name="callback">The route function to be called after matched.</param>
    /// <param name="name">The route name.</param>
    /// <definition>
    /// public void SetRoute(RouteMethod method, string path, RouterCallback callback, string? name)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void SetRoute(RouteMethod method, string path, RouterCallback callback, string? name)
    {
        Route newRoute = new Route(method, path, name, callback, null);
        Route? collisonRoute;
        if ((collisonRoute = GetCollisionRoute(newRoute.Method, newRoute.Path)) != null)
        {
            throw new ArgumentException($"A possible route collision could happen between route {newRoute} and route {collisonRoute}. Please review the methods and paths of these routes.");
        }
        _routes.Add(newRoute);
    }

    /// <summary>
    /// Defines an route with their method, path, callback function, name and request handlers.
    /// </summary>
    /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
    /// <param name="path">The route path.</param>
    /// <param name="callback">The route function to be called after matched.</param>
    /// <param name="name">The route name.</param>
    /// <param name="middlewares">Handlers that run before calling your route callback.</param>
    /// <definition>
    /// public void SetRoute(RouteMethod method, string path, RouterCallback callback, string? name, IRequestHandler[] middlewares)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void SetRoute(RouteMethod method, string path, RouterCallback callback, string? name, IRequestHandler[] middlewares)
    {
        Route newRoute = new Route(method, path, name, callback, middlewares);
        Route? collisonRoute;
        if ((collisonRoute = GetCollisionRoute(newRoute.Method, newRoute.Path)) != null)
        {
            throw new ArgumentException($"A possible route collision could happen between route {newRoute} and route {collisonRoute}. Please review the methods and paths of these routes.");
        }
        _routes.Add(newRoute);
    }

    /// <summary>
    /// Defines an route in this Router instance.
    /// </summary>
    /// <param name="r">The route to be defined in the Router.</param>
    /// <definition>
    /// public void SetRoute(Route r)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void SetRoute(Route r)
    {
        Route? collisonRoute;
        if (!r.UseRegex && (collisonRoute = GetCollisionRoute(r.Method, r.Path)) != null)
        {
            throw new ArgumentException($"A possible route collision could happen between route {r} and route {collisonRoute}. Please review the methods and paths of these routes.");
        }
        _routes.Add(r);
    }

    /// <summary>
    /// Searches the object instance for methods with attribute <see cref="RouteAttribute"/> and optionals <see cref="RequestHandlerAttribute"/>, and creates routes from them.
    /// </summary>
    /// <param name="attrClassInstance">The instance of the class where the instance methods are. The routing methods must be instance methods and marked with <see cref="RouteAttribute"/>.</param>
    /// <exception cref="Exception">An exception is thrown when a method has an erroneous signature.</exception>
    /// <definition>
    /// public void SetObject(object attrClassInstance)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void SetObject(object attrClassInstance)
    {
        Type attrClassType = attrClassInstance.GetType();
        MethodInfo[] methods = attrClassType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        SetInternal(methods, attrClassType, attrClassInstance);
    }

    /// <summary>
    /// Searches the object instance for methods with attribute <see cref="RouteAttribute"/> and optionals <see cref="RequestHandlerAttribute"/>, and creates routes from them.
    /// </summary>
    /// <param name="attrClassType">The type of the class where the static methods are. The routing methods must be static and marked with <see cref="RouteAttribute"/>.</param>
    /// <exception cref="Exception">An exception is thrown when a method has an erroneous signature.</exception>
    /// <definition>
    /// public void SetObject(Type attrClassType)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void SetObject([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type attrClassType)
    {
        MethodInfo[] methods = attrClassType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        SetInternal(methods, attrClassType, null);
    }

    private void SetInternal(MethodInfo[] methods, Type callerType, object? instance)
    {
        RoutePrefixAttribute? rPrefix = callerType.GetCustomAttribute<RoutePrefixAttribute>();
        string? prefix = rPrefix?.Prefix;

        foreach (var method in methods)
        {
            IEnumerable<RouteAttribute> routesAttributes = method.GetCustomAttributes<RouteAttribute>();
            foreach (var atrInstance in routesAttributes)
            {
                IEnumerable<RequestHandlerAttribute> handlersInstances = method.GetCustomAttributes<RequestHandlerAttribute>();
                if (atrInstance != null)
                {
                    List<IRequestHandler> methodHandlers = new List<IRequestHandler>();
                    if (handlersInstances.Count() > 0)
                    {
                        foreach (RequestHandlerAttribute atr in handlersInstances)
                        {
                            IRequestHandler rhandler = (IRequestHandler)Activator.CreateInstance(atr.RequestHandlerType, atr.ConstructorArguments)!;
                            methodHandlers.Add(rhandler);
                        }
                    }

                    try
                    {
                        RouterCallback r;

                        if (instance == null)
                        {
                            r = (RouterCallback)Delegate.CreateDelegate(typeof(RouterCallback), method);
                        }
                        else
                        {
                            r = (RouterCallback)Delegate.CreateDelegate(typeof(RouterCallback), instance, method);
                        }

                        string path = atrInstance.Path;
                        if (prefix != null)
                        {
                            path = HttpStringInternals.CombineRoutePaths(prefix, path);
                        }

                        Route route = new Route()
                        {
                            Method = atrInstance.Method,
                            Path = path,
                            Callback = r,
                            Name = atrInstance.Name,
                            RequestHandlers = methodHandlers.ToArray(),
                            LogMode = atrInstance.LogMode,
                            UseCors = atrInstance.UseCors
                        };

                        Route? collisonRoute;
                        if ((collisonRoute = GetCollisionRoute(route.Method, route.Path)) != null)
                        {
                            throw new ArgumentException($"A possible route collision could happen between the route {route} at {method.Name} with route {collisonRoute}. Please review the methods and paths of these routes.");
                        }

                        SetRoute(route);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Couldn't set method {method.Name} as an route. See inner exception.", ex);
                    }
                }
            }
        }
    }
    #endregion

    private Route? GetCollisionRoute(RouteMethod method, string path)
    {
        if (!path.StartsWith('/'))
        {
            throw new ArgumentException("Route paths must start with /.");
        }
        foreach (Route r in this._routes)
        {
            bool methodMatch = method != RouteMethod.Any && method == r.Method;
            bool pathMatch = HttpStringInternals.IsPathMatch(r.Path, path, MatchRoutesIgnoreCase).IsMatched;

            if (methodMatch && pathMatch)
            {
                return r;
            }
        }
        return null;
    }
}
