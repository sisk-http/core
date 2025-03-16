// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Router.SetRoute.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Web;
using Sisk.Core.Entity;
using Sisk.Core.Http;
using Sisk.Core.Internal;

namespace Sisk.Core.Routing;

public partial class Router {
    static readonly BindingFlags SetObjectBindingFlag = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    #region AutoScan methods
    /// <summary>
    /// Scans for all types that implements the specified module type and associates an instance of each type to the router.
    /// </summary>
    /// <param name="moduleType">An class which implements <see cref="RouterModule"/>, or the router module itself.</param>
    /// <param name="searchAssembly">The assembly to search the module type in.</param>
    [RequiresUnreferencedCode ( SR.RequiresUnreferencedCode )]
    public void AutoScanModules ( Type moduleType, Assembly searchAssembly ) {
        if (moduleType == typeof ( RouterModule )) {
            throw new InvalidOperationException ( SR.Router_AutoScanModules_TModuleSameAssembly );
        }
        var types = searchAssembly.GetTypes ();
        for (int i = 0; i < types.Length; i++) {
            Type type = types [ i ];
            if (type.IsAssignableTo ( moduleType )) {
                /*
                    When scanning and finding an abstract class, the method checks whether
                    it directly implements RouterModule or whether there is no base-type first.

                    Abstract classes should not be included on the router.
                 */
                if (type == moduleType) {
                    ;
                }
                else if (type.IsAbstract) {
                    if (type.IsSealed) // static
                    {
                        SetObject ( type );
                    }
                }
                else {
                    object? instance = Activator.CreateInstance ( type );
                    if (instance is not null) {
                        SetObject ( instance );
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
    [RequiresUnreferencedCode ( SR.RequiresUnreferencedCode )]
    public void AutoScanModules<TModule> ( Assembly assembly ) where TModule : RouterModule
        => AutoScanModules ( typeof ( TModule ), assembly );

    /// <summary>
    /// Scans for all types that implements <typeparamref name="TModule"/> and associates an instance of each type to the router. Note
    /// that, <typeparamref name="TModule"/> must be an <see cref="RouterModule"/> type and an accessible constructor
    /// for each type must be present.
    /// </summary>
    /// <typeparam name="TModule">An class which implements <see cref="RouterModule"/>, or the router module itself.</typeparam>
    [RequiresUnreferencedCode ( SR.RequiresUnreferencedCode )]
    public void AutoScanModules<TModule> () where TModule : RouterModule
        => AutoScanModules<TModule> ( typeof ( TModule ).Assembly );
    #endregion

    #region Map* methods
    /// <summary>
    /// Maps an GET route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapGet ( string path, Delegate action )
        => SetRoute ( RouteMethod.Get, path, action );

    /// <summary>
    /// Maps an GET route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapGet ( string path, RouteAction action )
        => SetRoute ( RouteMethod.Get, path, action );

    /// <summary>
    /// Maps an POST route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapPost ( string path, Delegate action )
        => SetRoute ( RouteMethod.Post, path, action );

    /// <summary>
    /// Maps an POST route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapPost ( string path, RouteAction action )
        => SetRoute ( RouteMethod.Post, path, action );

    /// <summary>
    /// Maps an PUT route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapPut ( string path, Delegate action )
        => SetRoute ( RouteMethod.Put, path, action );

    /// <summary>
    /// Maps an PUT route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapPut ( string path, RouteAction action )
        => SetRoute ( RouteMethod.Put, path, action );

    /// <summary>
    /// Maps an DELETE route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapDelete ( string path, Delegate action )
        => SetRoute ( RouteMethod.Delete, path, action );

    /// <summary>
    /// Maps an DELETE route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapDelete ( string path, RouteAction action )
        => SetRoute ( RouteMethod.Delete, path, action );

    /// <summary>
    /// Maps an PATCH route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapPatch ( string path, Delegate action )
        => SetRoute ( RouteMethod.Patch, path, action );

    /// <summary>
    /// Maps an PATCH route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapPatch ( string path, RouteAction action )
        => SetRoute ( RouteMethod.Patch, path, action );

    /// <summary>
    /// Maps an route which matches any HTTP method, using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapAny ( string path, Delegate action )
        => SetRoute ( RouteMethod.Any, path, action );

    /// <summary>
    /// Maps an route which matches any HTTP method, using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapAny ( string path, RouteAction action )
        => SetRoute ( RouteMethod.Any, path, action );

    /// <summary>
    /// Maps an OPTIONS route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapOptions ( string path, Delegate action )
        => SetRoute ( RouteMethod.Options, path, action );

    /// <summary>
    /// Maps an OPTIONS route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapOptions ( string path, RouteAction action )
        => SetRoute ( RouteMethod.Options, path, action );

    /// <summary>
    /// Maps an HEAD route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapHead ( string path, Delegate action )
        => SetRoute ( RouteMethod.Head, path, action );

    /// <summary>
    /// Maps an HEAD route using the specified path and action function.
    /// </summary>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void MapHead ( string path, RouteAction action )
        => SetRoute ( RouteMethod.Head, path, action );
    #endregion

    #region SetRoute methods
    /// <summary>
    /// Defines an route with their method, path and action function.
    /// </summary>
    /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void SetRoute ( RouteMethod method, string path, RouteAction action )
        => SetRoute ( new Route ( method, path, action ) );

    /// <summary>
    /// Defines an route with their method, path and action function.
    /// </summary>
    /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    public void SetRoute ( RouteMethod method, string path, Delegate action )
        => SetRoute ( new Route ( method, path, action ) );

    /// <summary>
    /// Defines an route with their method, path, action function and name.
    /// </summary>
    /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    /// <param name="name">The route name.</param>
    public void SetRoute ( RouteMethod method, string path, Delegate action, string? name )
        => SetRoute ( new Route ( method, path, name, action, null ) );

    /// <summary>
    /// Defines an route with their method, path, action function, name and request handlers.
    /// </summary>
    /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
    /// <param name="path">The route path.</param>
    /// <param name="action">The route function to be called after matched.</param>
    /// <param name="name">The route name.</param>
    /// <param name="middlewares">Handlers that run before calling your route action.</param>
    public void SetRoute ( RouteMethod method, string path, Delegate action, string? name, IRequestHandler [] middlewares )
        => SetRoute ( new Route ( method, path, name, action, middlewares ) );

    /// <summary>
    /// Defines an route in this Router instance.
    /// </summary>
    /// <param name="r">The route to be defined in the Router.</param>
    public void SetRoute ( Route r ) {
        if (IsReadOnly) {
            throw new InvalidOperationException ( SR.Router_ReadOnlyException );
        }

        if (!r.UseRegex && Prefix is string prefix) {
            r.Path = PathUtility.CombinePaths ( prefix, r.Path );
        }

        _routesList.Add ( r );
    }
    #endregion

    #region SetObject methods

    /// <summary>
    /// Searches for all instance and static methods that are marked with an attribute of
    /// type <see cref="RouteAttribute"/> in the specified object and creates routes
    /// for these methods.
    /// </summary>
    /// <param name="attrClassInstance">The instance of the class where the methods are. The routing methods must be marked with any <see cref="RouteAttribute"/>.</param>
    /// <exception cref="Exception">An exception is thrown when a method has an erroneous signature.</exception>
    [RequiresUnreferencedCode ( SR.RequiresUnreferencedCode__RouterSetObject )]
    public void SetObject ( object attrClassInstance ) {
        Type attrClassType = attrClassInstance.GetType ();
        MethodInfo [] methods = attrClassType.GetMethods ( SetObjectBindingFlag );
        SetObjectInternal ( methods, attrClassType, attrClassInstance );
    }

    /// <summary>
    /// Searches for all instance and static methods that are marked with an attribute of
    /// type <see cref="RouteAttribute"/> in the specified object and creates routes
    /// for these methods.
    /// </summary>
    /// <param name="attrClassType">The type of the class where the methods are. The routing methods must be marked with any <see cref="RouteAttribute"/>.</param>
    public void SetObject ( [DynamicallyAccessedMembers ( DynamicallyAccessedMemberTypes.All )] Type attrClassType ) {
        MethodInfo [] methods = attrClassType.GetMethods ( SetObjectBindingFlag );
        SetObjectInternal ( methods, attrClassType, null );
    }

    /// <summary>
    /// Searches for all instance and static methods that are marked with an attribute of
    /// type <see cref="RouteAttribute"/> in the specified object and creates routes
    /// for these methods.
    /// </summary>
    /// <param name="attrClassType">The type of the class where the methods are. The routing methods must be marked with any <see cref="RouteAttribute"/>.</param>
    /// <param name="instance">The instance of the object where the route methods are.</param>
    public void SetObject ( [DynamicallyAccessedMembers ( DynamicallyAccessedMemberTypes.All )] Type attrClassType, object instance ) {
        MethodInfo [] methods = attrClassType.GetMethods ( SetObjectBindingFlag );
        SetObjectInternal ( methods, attrClassType, instance );
    }

    /// <summary>
    /// Searches for all instance and static methods that are marked with an attribute of
    /// type <see cref="RouteAttribute"/> in the specified object and creates routes
    /// for these methods.
    /// </summary>
    /// <typeparam name="TObject">The type of the class where the methods are. The routing methods must be marked with any <see cref="RouteAttribute"/>.</typeparam>
    /// <exception cref="Exception">An exception is thrown when a method has an erroneous signature.</exception>
    public void SetObject<[DynamicallyAccessedMembers ( DynamicallyAccessedMemberTypes.All )] TObject> () {
        SetObject ( typeof ( TObject ) );
    }

    /// <summary>
    /// Searches for all instance and static methods that are marked with an attribute of
    /// type <see cref="RouteAttribute"/> in the specified object and creates routes
    /// for these methods.
    /// </summary>
    /// <param name="instance">The instance of <typeparamref name="TObject"/> to invoke the instance methods on.</param>
    /// <typeparam name="TObject">The type of the class where the methods are. The routing methods must be marked with any <see cref="RouteAttribute"/>.</typeparam>
    /// <exception cref="Exception">An exception is thrown when a method has an erroneous signature.</exception>
    public void SetObject<[DynamicallyAccessedMembers ( DynamicallyAccessedMemberTypes.All )] TObject> ( TObject instance ) where TObject : notnull {
        SetObject ( typeof ( TObject ), instance );
    }

    private void SetObjectInternal ( MethodInfo [] methods, Type callerType, object? instance ) {
        RouterModule? rmodule = instance as RouterModule;

        // get caller prefix from RoutePrefix first, or router module
        object [] callerTypeLevelHandlers = callerType.GetCustomAttributes ( true );
        List<IRequestHandler> callerAttrReqHandlers = new List<IRequestHandler> ( callerTypeLevelHandlers.Length );
        string? prefix = rmodule?.Prefix;

        // search for an RoutePrefix handler
        for (int i = 0; i < callerTypeLevelHandlers.Length; i++) {
            object attr = callerTypeLevelHandlers [ i ];

            if (attr is RoutePrefixAttribute rprefix) {
                prefix = rprefix.Prefix;
            }
            else if (attr is RequestHandlerAttribute rhattr) {
                callerAttrReqHandlers.Add ( rhattr.Activate () );
            }
        }

        for (int imethod = 0; imethod < methods.Length; imethod++) {
            MethodInfo method = methods [ imethod ];

            if (instance is null && !method.Attributes.HasFlag ( MethodAttributes.Static )) {
                continue;
            }

            List<RouteAttribute> routeAttributes = new List<RouteAttribute> ();
            object [] methodAttributes = method.GetCustomAttributes ( true );
            List<IRequestHandler> methodAttrReqHandlers = new List<IRequestHandler> ( methodAttributes.Length );

            methodAttrReqHandlers.AddRange ( callerAttrReqHandlers );
            if (rmodule is not null)
                methodAttrReqHandlers.AddRange ( rmodule.RequestHandlers );

            for (int imethodAttribute = 0; imethodAttribute < methodAttributes.Length; imethodAttribute++) {
                object attrInstance = methodAttributes [ imethodAttribute ];

                if (attrInstance is RequestHandlerAttribute reqHandlerAttr) {
                    methodAttrReqHandlers.Add ( reqHandlerAttr.Activate () );
                }
                else if (attrInstance is RouteAttribute routeAttributeItem) {
                    routeAttributes.Add ( routeAttributeItem );
                }
            }

            foreach (var routeAttribute in routeAttributes) {
                try {
                    string path = routeAttribute.Path;

                    if (prefix is not null && !routeAttribute.UseRegex) {
                        path = PathUtility.CombinePaths ( prefix, path );
                    }

                    Route route = new Route () {
                        Method = routeAttribute.Method,
                        Path = path,
                        Name = routeAttribute.Name,
                        RequestHandlers = methodAttrReqHandlers.ToArray (),
                        LogMode = routeAttribute.LogMode,
                        UseCors = routeAttribute.UseCors,
                        UseRegex = routeAttribute.UseRegex
                    };

                    if (!route.TrySetRouteAction ( method, instance, out Exception? ex )) {
                        throw ex;
                    }

                    if (rmodule is not null) {
                        rmodule.CallRouteCreating ( route );

                        if (rmodule._wasSetupCalled == false)
                            rmodule.CallOnSetup ( this );
                    }

                    SetRoute ( route );
                }
                catch (Exception ex) {
                    throw new InvalidOperationException ( SR.Format ( SR.Router_Set_Exception, method.DeclaringType?.FullName, method.Name ), ex );
                }
            }
        }
    }
    #endregion

    private HttpResponse RewriteHandler ( string rewriteInto, HttpRequest request ) {
        string newPath = rewriteInto;
        foreach (StringValue item in request.RouteParameters) {
            newPath = newPath.Replace ( $"<{item.Name}>", HttpUtility.UrlEncode ( item.Value ), MatchRoutesIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal );
        }

        return new HttpResponse () {
            Status = HttpStatusInformation.Found,
            Headers = new HttpHeaderCollection () {
                Location = newPath
            }
        };
    }

    private Route? GetCollisionRoute ( RouteMethod method, string path ) {
        ArgumentNullException.ThrowIfNull ( path );
        HttpStringInternals.AssertRoute ( path );

        for (int i = 0; i < _routesList.Count; i++) {
            Route r = _routesList [ i ];
            bool methodMatch =
                method == RouteMethod.Any || r.Method == RouteMethod.Any ||
                method == r.Method;
            bool pathMatch = HttpStringInternals.IsRoutePatternMatch ( r.Path, path, MatchRoutesIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal );

            if (pathMatch & methodMatch) {
                return r;
            }
        }
        return null;
    }

    /// <exclude/>
    public static Router operator + ( Router r, Route route ) {
        r.SetRoute ( route );
        return r;
    }
}
