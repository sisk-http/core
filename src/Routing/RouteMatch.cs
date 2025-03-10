// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RouteMatch.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections.Specialized;

namespace Sisk.Core.Routing;

/// <summary>
/// Represents the result of a route matching operation.
/// </summary>
public sealed class RouteMatch {

    /// <summary>
    /// Gets a value indicating whether the route matching operation was successful.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets a collection of parameters extracted from the route, or <see langword="null" /> if the route matching operation was not successful.
    /// </summary>
    public NameValueCollection? Parameters { get; }

    internal RouteMatch ( bool success, NameValueCollection? parameters ) {
        this.Success = success;
        this.Parameters = parameters;
    }
}