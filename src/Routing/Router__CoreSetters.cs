// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Router__CoreSetters.cs
// Repository:  https://github.com/sisk-http/core

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
    public static Router operator +(Router r, Route route)
    {
        r.SetRoute(route);
        return r;
    }

    /// <summary>
    /// Gets an boolean indicating if there are any route that matches the specified method and route path.
    /// </summary>
    /// <param name="method">The route method.</param>
    /// <param name="path">The route path.</param>
    public bool IsDefined(RouteMethod method, string path)
    {
        return GetCollisionRoute(method, path) is not null;
    }

    /// <summary>
    /// Gets an defined <see cref="Route"/> by their name property.
    /// </summary>
    /// <param name="name">The route name.</param>
    public Route? GetRouteFromName(string name)
    {
        for (int i = 0; i < _routesList.Count; i++)
        {
            Route r = _routesList[i];
            if (string.Compare(name, r.Name) == 0)
            {
                return r;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the first matched <see cref="Route"/> by their HTTP method and path.
    /// </summary>
    /// <param name="method">The HTTP method to match.</param>
    /// <param name="uri">The URL expression.</param>
    public Route? GetRouteFromPath(RouteMethod method, string uri)
    {
        return GetCollisionRoute(method, uri);
    }

    /// <summary>
    /// Gets the first matched <see cref="Route"/> by their URL path.
    /// </summary>
    /// <param name="uri">The URL expression.</param>
    public Route? GetRouteFromPath(string uri) => GetRouteFromPath(RouteMethod.Any, uri);

    /// <summary>
    /// Scans for all types that implements the specified module type and associates an instance of each type to the router.
    /// </summary>
    /// <param name="moduleType">An class which implements <see cref="RouterModule"/>, or the router module itself.</param>
    /// <param name="searchAssembly">The assembly to search the module type in.</param>
    /// <param name="activateInstances">Optional. Determines whether found types should be defined as instances or static members.</param>
    [RequiresUnreferencedCode(SR.Router_AutoScanModules_RequiresUnreferencedCode)]
    public void AutoScanModules(Type moduleType, Assembly searchAssembly, bool activateInstances = true)
    {
        if (moduleType == typeof(RouterModule))
        {
            throw new InvalidOperationException(SR.Router_AutoScanModules_TModuleSameAssembly);
        }
        var types = searchAssembly.GetTypes();
        for (int i = 0; i < types.Length; i++)
        {
            Type type = types[i];
            if (type.IsAssignableTo(moduleType))
            {
                /*
                    When scanning and finding an abstract class, the method checks whether
                    it directly implements RouterModule or whether there is no base-type first.

                    Abstract classes should not be included on the router.
                 */
                if (type.IsAbstract || type == moduleType)
                {
                    continue;
                }
                else
                {
                    if (activateInstances)
                    {
                        var instance = Activator.CreateInstance(type)!;
                        if (instance != null)
                            SetObject(instance);
                    }
                    else
                    {
                        SetObject(type);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Scans for all types that implements <typeparamref name="TModule"/> and associates an instance of each type to the router. Note that, <typeparamref name="TModule"/> must be an <see cref="RouterModule"/> type and an accessible constructor
    /// for each type must be present.
    /// </summary>
    /// <typeparam name="TModule">An class which implements <see cref="RouterModule"/>, or the router module itself.</typeparam>
    /// <param name="assembly">The assembly to search <typeparamref name="TModule"/> in.</param>
    /// <param name="activateInstances">Optional. Determines whether found types should be defined as instances or static members.</param>
    [RequiresUnreferencedCode(SR.Router_AutoScanModules_RequiresUnreferencedCode)]
    public void AutoScanModules<TModule>(Assembly assembly, bool activateInstances = true) where TModule : RouterModule
        => AutoScanModules(typeof(TModule), assembly, activateInstances);

    /// <summary>
    /// Scans for all types that implements <typeparamref name="TModule"/> and associates an instance of each type to the router. Note
    /// that, <typeparamref name="TModule"/> must be an <see cref="RouterModule"/> type and an accessible constructor
    /// for each type must be present.
    /// </summary>
    /// <typeparam name="TModule">An class which implements <see cref="RouterModule"/>, or the router module itself.</typeparam>
    [RequiresUnreferencedCode(SR.Router_AutoScanModules_RequiresUnreferencedCode)]
    public void AutoScanModules<TModule>() where TModule : RouterModule
        => AutoScanModules<TModule>(typeof(TModule).Assembly);

    /// <summary>
    /// Maps an GET route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapGet(string path, RouteAction action)
        => SetRoute(RouteMethod.Get, path, action);

    /// <summary>
    /// Maps an POST route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapPost(string path, RouteAction action)
        => SetRoute(RouteMethod.Post, path, action);

    /// <summary>
    /// Maps an PUT route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapPut(string path, RouteAction action)
        => SetRoute(RouteMethod.Put, path, action);

    /// <summary>
    /// Maps an DELETE route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapDelete(string path, RouteAction action)
        => SetRoute(RouteMethod.Delete, path, action);

    /// <summary>
    /// Maps an PATCH route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapPatch(string path, RouteAction action)
        => SetRoute(RouteMethod.Patch, path, action);

    /// <summary>
    /// Maps an route which matches any HTTP method, using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapAny(string path, RouteAction action)
        => SetRoute(RouteMethod.Any, path, action);

    /// <summary>
    /// Defines an route with their method, path and action function.
    /// </summary>
    /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void SetRoute(RouteMethod method, string path, RouteAction action)
        => SetRoute(new Route(method, path, action));

    /// <summary>
    /// Defines an route with their method, path, action function and name.
    /// </summary>
    /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    /// <param name="name">The route name.</param>
    public void SetRoute(RouteMethod method, string path, RouteAction action, string? name)
        => SetRoute(new Route(method, path, name, action, null));

    /// <summary>
    /// Defines an route with their method, path, action function, name and request handlers.
    /// </summary>
    /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    /// <param name="name">The route name.</param>
    /// <param name="middlewares">Handlers that run before calling your route action.</param>
    public void SetRoute(RouteMethod method, string path, RouteAction action, string? name, IRequestHandler[] middlewares)
        => SetRoute(new Route(method, path, name, action, middlewares));

    /// <summary>
    /// Defines an route in this Router instance.
    /// </summary>
    /// <param name="r">The route to be defined in the Router.</param>
    public void SetRoute(Route r)
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException(SR.Router_ReadOnlyException);
        }
        Route? collisonRoute;
        if (!r.UseRegex && (collisonRoute = GetCollisionRoute(r.Method, r.Path)) != null)
        {
            throw new ArgumentException(string.Format(SR.Router_Set_Collision, r, collisonRoute));
        }

        _routesList!.Add(r);
    }

    /// <summary>
    /// Searches in the specified object for instance methods marked with routing attributes, such as <see cref="RouteAttribute"/> and optionals <see cref="RequestHandlerAttribute"/>, and creates
    /// routes from them. All routes is delegated to the specified instance.
    /// </summary>
    /// <param name="attrClassInstance">The instance of the class where the instance methods are. The routing methods must be instance methods and marked with <see cref="RouteAttribute"/>.</param>
    /// <exception cref="Exception">An exception is thrown when a method has an erroneous signature.</exception>
    public void SetObject(object attrClassInstance)
    {
        Type attrClassType = attrClassInstance.GetType();
        MethodInfo[] methods = attrClassType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        SetInternal(methods, attrClassType, attrClassInstance);
    }

    /// <summary>
    /// Searches in the specified object for static methods marked with routing attributes, such as <see cref="RouteAttribute"/> and optionals <see cref="RequestHandlerAttribute"/>, and creates
    /// routes from them.
    /// </summary>
    /// <param name="attrClassType">The type of the class where the static methods are. The routing methods must be static and marked with <see cref="RouteAttribute"/>.</param>
    /// <exception cref="Exception">An exception is thrown when a method has an erroneous signature.</exception>
    public void SetObject([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type attrClassType)
    {
        MethodInfo[] methods = attrClassType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        SetInternal(methods, attrClassType, null);
    }

    /// <summary>
    /// Searches in the specified object for static methods marked with routing attributes, such as <see cref="RouteAttribute"/> and optionals <see cref="RequestHandlerAttribute"/>, and creates
    /// routes from them.
    /// </summary>
    /// <remarks>
    /// This method is an shortcut for <see cref="SetObject(Type)"/>.
    /// </remarks>
    /// <typeparam name="TObject">The type of the class where the static methods are. The routing methods must be static and marked with <see cref="RouteAttribute"/>.</typeparam>
    /// <exception cref="Exception">An exception is thrown when a method has an erroneous signature.</exception>
    public void SetObject<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TObject>()
    {
        SetObject(typeof(TObject));
    }

    private void SetInternal(MethodInfo[] methods, Type callerType, object? instance)
    {
        RouterModule? rmodule = instance as RouterModule;
        string? prefix;

        if (rmodule?.Prefix is null)
        {
            RoutePrefixAttribute? rPrefix = callerType.GetCustomAttribute<RoutePrefixAttribute>();
            prefix = rPrefix?.Prefix;
        }
        else
        {
            prefix = rmodule.Prefix;
        }

        for (int imethod = 0; imethod < methods.Length; imethod++)
        {
            MethodInfo? method = methods[imethod];

            RouteAttribute? routeAttribute = null;
            object[] methodAttributes = method.GetCustomAttributes(true);
            List<IRequestHandler> methodAttrReqHandlers = new List<IRequestHandler>(methodAttributes.Length);

            for (int imethodAttribute = 0; imethodAttribute < methodAttributes.Length; imethodAttribute++)
            {
                object attrInstance = methodAttributes[imethodAttribute];

                if (attrInstance is RequestHandlerAttribute reqHandlerAttr)
                {
                    IRequestHandler? rhandler = (IRequestHandler?)Activator.CreateInstance(reqHandlerAttr.RequestHandlerType, reqHandlerAttr.ConstructorArguments);
                    if (rhandler is not null)
                        methodAttrReqHandlers.Add(rhandler);
                }
                else if (attrInstance is RouteAttribute routeAttributeItem)
                {
                    routeAttribute = routeAttributeItem;
                }
            }

            if (routeAttribute is not null)
            {
                if (rmodule?.RequestHandlers.Count > 0)
                {
                    for (int imodReqHandler = 0; imodReqHandler < rmodule.RequestHandlers.Count; imodReqHandler++)
                    {
                        IRequestHandler handler = rmodule.RequestHandlers[imodReqHandler];
                        methodAttrReqHandlers.Add(handler);
                    }
                }
                try
                {
                    RouteAction r;

                    if (instance == null)
                    {
                        r = (RouteAction)Delegate.CreateDelegate(typeof(RouteAction), method);
                    }
                    else
                    {
                        r = (RouteAction)Delegate.CreateDelegate(typeof(RouteAction), instance, method);
                    }

                    string path = routeAttribute.Path;

                    if (prefix is not null && !routeAttribute.UseRegex)
                    {
                        path = PathUtility.CombinePaths(prefix, path);
                    }

                    Route route = new Route()
                    {
                        Method = routeAttribute.Method,
                        Path = path,
                        Action = r,
                        Name = routeAttribute.Name,
                        RequestHandlers = methodAttrReqHandlers.ToArray(),
                        LogMode = routeAttribute.LogMode,
                        UseCors = routeAttribute.UseCors,
                        UseRegex = routeAttribute.UseRegex
                    };

                    Route? collisonRoute;
                    if ((collisonRoute = GetCollisionRoute(route.Method, route.Path)) != null)
                    {
                        throw new ArgumentException(string.Format(SR.Router_Set_Collision, route, collisonRoute));
                    }

                    rmodule?.OnRouteCreating(route);
                    SetRoute(route);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(SR.Router_Set_Exception, method.DeclaringType?.FullName, method.Name), ex);
                }
            }
        }
    }
    #endregion

    private Route? GetCollisionRoute(RouteMethod method, string path)
    {
        if (!path.StartsWith('/'))
        {
            throw new ArgumentException(SR.Router_Set_InvalidRouteStart);
        }

        for (int i = 0; i < _routesList.Count; i++)
        {
            Route r = _routesList[i];
            bool methodMatch =
                (method == RouteMethod.Any || r.Method == RouteMethod.Any) ||
                method == r.Method;
            bool pathMatch = HttpStringInternals.PathRouteMatch(r.Path, path, MatchRoutesIgnoreCase);

            if (methodMatch && pathMatch)
            {
                return r;
            }
        }
        return null;
    }
}
