// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RouterModule.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Routing;

/// <summary>
/// Indicates that extended class supports router modules, which allows the management of routes,
/// request handlers and prefixes.
/// </summary>
public abstract class RouterModule
{
    /// <summary>
    /// Gets or sets an list of <see cref="IRequestHandler"/> this <see cref="RouterModule"/> runs.
    /// </summary>
    public IList<IRequestHandler> RequestHandlers { get; set; } = new List<IRequestHandler>();

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
    protected void HasRequestHandler(IRequestHandler handler)
    {
        this.RequestHandlers.Add(handler);
    }

    /// <summary>
    /// This method is called before a route is defined in the router and after it
    /// is created in this class, so its attributes and parameters can be modified. This method must
    /// be overloaded in the extending class and must not be called directly.
    /// </summary>
    /// <param name="configuringRoute">The route being defined on the router.</param>
    protected virtual void OnRouteCreating(Route configuringRoute)
    {
    }

    internal void CallRouteCreating(Route configuringRoute)
    {
        this.OnRouteCreating(configuringRoute);
    }
}