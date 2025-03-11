// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RouterModule.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.Core.Routing;

/// <summary>
/// Indicates that extended class supports router modules, which allows the management of routes,
/// request handlers and prefixes.
/// </summary>
public abstract class RouterModule {
    internal bool _wasSetupCalled;

    /// <summary>
    /// Gets or sets an list of <see cref="IRequestHandler"/> this <see cref="RouterModule"/> runs.
    /// </summary>
    public IList<IRequestHandler> RequestHandlers { get; set; } = new List<IRequestHandler> ();

    /// <summary>
    /// Gets or sets the router prefix for this class. This property overrides any
    /// value defined by <see cref="RoutePrefixAttribute"/> set in this class.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// Registers an <see cref="IRequestHandler"/> on all routes defined by this module.
    /// </summary>
    /// <param name="handler">The <see cref="IRequestHandler"/> instance which will be applied to all registered routes
    /// of this class.</param>
    protected void HasRequestHandler ( IRequestHandler handler ) {
        RequestHandlers.Add ( handler );
    }

    /// <summary>
    /// Specifies a prefix for all routes defined by this module.
    /// </summary>
    /// <param name="prefix">The prefix to be applied to all registered routes of this class.</param>
    /// <remarks>
    /// This method allows for the specification of a common prefix for all routes defined by this module,
    /// which can be useful for organizing and structuring routes in a large application.
    /// </remarks>
    protected void HasPrefix ( string prefix ) {
        Prefix = prefix;
    }

    /// <summary>
    /// Method that is called when an <see cref="Router"/> is defining routes from the current
    /// <see cref="RouterModule"/>.
    /// </summary>
    /// <remarks>
    /// The base method <see cref="RouterModule.OnSetup(Router)"/> is mandatory to be called on all derived methods.
    /// </remarks>
    /// <param name="parentRouter">The <see cref="Router"/> which is defining routes from the current <see cref="RouterModule"/>.</param>
    protected virtual void OnSetup ( Router parentRouter ) {
        _wasSetupCalled = true;
    }

    /// <summary>
    /// This method is called before a route is defined in the router and after it
    /// is created in this class, so its attributes and parameters can be modified. This method must
    /// be overloaded in the extending class and must not be called directly.
    /// </summary>
    /// <param name="configuringRoute">The route being defined on the router.</param>
    protected virtual void OnRouteCreating ( Route configuringRoute ) {
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    internal void CallRouteCreating ( Route configuringRoute ) => OnRouteCreating ( configuringRoute );

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    internal void CallOnSetup ( Router parentRouter ) => OnSetup ( parentRouter );
}