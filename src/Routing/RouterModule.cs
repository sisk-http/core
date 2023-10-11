// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
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
/// <definition>
/// public abstract class RouterModule
/// </definition>
/// <type>
/// Class
/// </type>
public abstract class RouterModule
{
    /// <summary>
    /// Gets or sets the request handlers this class has implemented.
    /// </summary>
    /// <definition>
    /// public List{{IRequestHandler}} RequestHandlers { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public List<IRequestHandler> RequestHandlers { get; set; } = new List<IRequestHandler>();

    /// <summary>
    /// Gets or sets the router prefix for this class. This property overrides any
    /// value defined by <see cref="RoutePrefixAttribute"/> set in this class.
    /// </summary>
    /// <definition>
    /// public string? Prefix { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? Prefix { get; set; }

    /// <summary>
    /// Registers an <see cref="IRequestHandler"/> on all routes of this class.
    /// </summary>
    /// <param name="handler">The <see cref="IRequestHandler"/> instance which will be applied to all registered routes
    /// of this class.</param>
    /// <definition>
    /// public void HasRequestHandler(IRequestHandler handler)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void HasRequestHandler(IRequestHandler handler)
    {
        RequestHandlers.Add(handler);
    }

    /// <summary>
    /// This method is called before a route is defined in the router and after it
    /// is created in this class, so its attributes and parameters can be modified. This method must
    /// be overloaded in the extending class and must not be called directly.
    /// </summary>
    /// <param name="configuringRoute">The route being defined on the router.</param>
    /// <definition>
    /// public virtual void OnRouteCreating(Route configuringRoute)
    /// </definition>
    /// <type>
    /// Virtual method
    /// </type>
    public virtual void OnRouteCreating(Route configuringRoute)
    {
    }
}
