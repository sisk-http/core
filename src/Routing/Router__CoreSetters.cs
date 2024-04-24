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
using System.Runtime.CompilerServices;

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
    /// Gets an boolean indicating if there are any route that matches the specified method and route path.
    /// </summary>
    /// <param name="method">The route method.</param>
    /// <param name="path">The route path.</param>
    /// <definition>
    /// public bool IsDefined(RouteMethod method, string path)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public bool IsDefined(RouteMethod method, string path)
    {
        return GetCollisionRoute(method, path) != null;
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
        foreach (Route r in _routesList)
        {
            if (r.Name == name)
            {
                return r;
            }
        }
        return null;
    }

    /// <summary>
    /// Scans for all types that implements <typeparamref name="TModule"/> and associates an instance of each type to the router. Note that, <typeparamref name="TModule"/> must be an <see cref="RouterModule"/> type and an accessible constructor
    /// for each type must be present.
    /// </summary>
    /// <typeparam name="TModule">An class which implements <see cref="RouterModule"/>, or the router module itself.</typeparam>
    /// <param name="assembly">The assembly where the scanning types are.</param>
    /// <param name="activateInstances">Optional. Determines whether found types should be defined as instances or static members.</param>
    /// <definition>
    /// [RequiresUnreferencedCode(SR.Router_AutoScanModules_RequiresUnreferencedCode)]
    /// public void AutoScanModules{{TModule}}(Assembly assembly, bool activateInstances = true) where TModule : RouterModule
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    [RequiresUnreferencedCode(SR.Router_AutoScanModules_RequiresUnreferencedCode)]
    public void AutoScanModules<TModule>(Assembly assembly, bool activateInstances = true) where TModule : RouterModule
    {
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            throw new NotSupportedException(SR.Router_AutoScanModules_RequiresUnreferencedCode);
        }
        Type tType = typeof(TModule);
        var types = assembly.GetTypes();
        foreach (Type type in types)
        {
            if (type.IsAssignableTo(tType))
            {
                /*
                    When scanning and finding an abstract class, the method checks whether
                    it directly implements RouterModule or whether there is no base-type first.

                    Abstract classes should not be included on the router.
                 */
                if (type.IsAbstract || type == tType)
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
    /// <definition>
    /// public void AutoScanModules{{TModule}}() where T : RouterModule
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    [RequiresUnreferencedCode(SR.Router_AutoScanModules_RequiresUnreferencedCode)]
    public void AutoScanModules<TModule>() where TModule : RouterModule
    {
        if (typeof(TModule) == typeof(RouterModule))
        {
            throw new InvalidOperationException(SR.Router_AutoScanModules_TModuleSameAssembly);
        }
        AutoScanModules<TModule>(typeof(TModule).Assembly);
    }

    /// <summary>
    /// Defines an route with their method, path and action function.
    /// </summary>
    /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    /// <definition>
    /// public void SetRoute(RouteMethod method, string path, RouterCallback action)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void SetRoute(RouteMethod method, string path, RouteAction action)
        => SetRoute(new Route(method, path, action));

    /// <summary>
    /// Defines an route with their method, path, action function and name.
    /// </summary>
    /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    /// <param name="name">The route name.</param>
    /// <definition>
    /// public void SetRoute(RouteMethod method, string path, RouterCallback action, string? name)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
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
    /// <definition>
    /// public void SetRoute(RouteMethod method, string path, RouterCallback action, string? name, IRequestHandler[] middlewares)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void SetRoute(RouteMethod method, string path, RouteAction action, string? name, IRequestHandler[] middlewares)
        => SetRoute(new Route(method, path, name, action, middlewares));

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
    /// Searches in the specified object for static methods marked with routing attributes, such as <see cref="RouteAttribute"/> and optionals <see cref="RequestHandlerAttribute"/>, and creates
    /// routes from them.
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

    /// <summary>
    /// Searches in the specified object for static methods marked with routing attributes, such as <see cref="RouteAttribute"/> and optionals <see cref="RequestHandlerAttribute"/>, and creates
    /// routes from them.
    /// </summary>
    /// <remarks>
    /// This method is an shortcut for <see cref="SetObject(Type)"/>.
    /// </remarks>
    /// <typeparam name="TObject">The type of the class where the static methods are. The routing methods must be static and marked with <see cref="RouteAttribute"/>.</typeparam>
    /// <exception cref="Exception">An exception is thrown when a method has an erroneous signature.</exception>
    /// <definition>
    /// public void SetObject{{TObject}}()
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void SetObject<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TObject>()
    {
        SetObject(typeof(TObject));
    }

    private void SetInternal(MethodInfo[] methods, Type callerType, object? instance)
    {
        RouterModule? rmodule = instance as RouterModule;
        string? prefix;

        if (rmodule?.Prefix == null)
        {
            RoutePrefixAttribute? rPrefix = callerType.GetCustomAttribute<RoutePrefixAttribute>();
            prefix = rPrefix?.Prefix;
        }
        else
        {
            prefix = rmodule.Prefix;
        }

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
                    if (rmodule?.RequestHandlers.Count() > 0)
                    {
                        foreach (IRequestHandler handler in rmodule.RequestHandlers)
                        {
                            methodHandlers.Add(handler);
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

                        string path = atrInstance.Path;
                        if (prefix != null && !atrInstance.UseRegex)
                        {
                            path = PathUtility.CombinePaths(prefix, path);
                        }

                        Route route = new Route()
                        {
                            Method = atrInstance.Method,
                            Path = path,
                            Action = r,
                            Name = atrInstance.Name,
                            RequestHandlers = methodHandlers.ToArray(),
                            LogMode = atrInstance.LogMode,
                            UseCors = atrInstance.UseCors,
                            UseRegex = atrInstance.UseRegex
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
