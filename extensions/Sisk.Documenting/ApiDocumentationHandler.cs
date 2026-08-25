// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiDocumentationHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Routing;

namespace Sisk.Documenting;

/// <summary>
/// Provides hooks for filtering and customizing generated API documentation.
/// </summary>
/// <remarks>
/// Item hooks run after documentation from the route and its request handlers is collected and before
/// <see cref="ShouldCreateApiEndpoint"/> is called. Returning <see langword="false"/> excludes the item;
/// returning <see langword="true"/> includes it. Override a hook to modify the supplied item before it is exported.
/// </remarks>
public abstract class ApiDocumentationHandler {

    /// <summary>
    /// Filters or customizes a documented response for a route.
    /// </summary>
    /// <param name="value">The response documentation collected from the route or one of its request handlers.</param>
    /// <param name="route">The route that owns the documented response.</param>
    /// <returns><see langword="true"/> to include the response; otherwise, <see langword="false"/>.</returns>
    public virtual bool HandleApiEndpointResponse ( ApiEndpointResponse value, Route route ) {
        return true;
    }

    /// <summary>
    /// Filters or customizes a documented request parameter for a route.
    /// </summary>
    /// <param name="value">The parameter documentation collected from the route or one of its request handlers.</param>
    /// <param name="route">The route that owns the documented parameter.</param>
    /// <returns><see langword="true"/> to include the parameter; otherwise, <see langword="false"/>.</returns>
    public virtual bool HandleApiEndpointParameter ( ApiEndpointParameter value, Route route ) {
        return true;
    }

    /// <summary>
    /// Filters or customizes a documented request parameter example for a route.
    /// </summary>
    /// <param name="value">The parameter example collected from the route or one of its request handlers.</param>
    /// <param name="route">The route that owns the documented parameter example.</param>
    /// <returns><see langword="true"/> to include the parameter example; otherwise, <see langword="false"/>.</returns>
    public virtual bool HandleApiEndpointParameterExample ( ApiEndpointParameterExample value, Route route ) {
        return true;
    }

    /// <summary>
    /// Filters or customizes a documented request header for a route.
    /// </summary>
    /// <param name="value">The header documentation collected from the route or one of its request handlers.</param>
    /// <param name="route">The route that owns the documented header.</param>
    /// <returns><see langword="true"/> to include the header; otherwise, <see langword="false"/>.</returns>
    public virtual bool HandleApiEndpointHeader ( ApiEndpointHeader value, Route route ) {
        return true;
    }

    /// <summary>
    /// Filters or customizes a documented path parameter for a route.
    /// </summary>
    /// <param name="value">The path parameter documentation collected from the route or one of its request handlers.</param>
    /// <param name="route">The route that owns the documented path parameter.</param>
    /// <returns><see langword="true"/> to include the path parameter; otherwise, <see langword="false"/>.</returns>
    public virtual bool HandleApiEndpointPathParameter ( ApiEndpointPathParameter value, Route route ) {
        return true;
    }

    /// <summary>
    /// Filters or customizes a documented request example for a route.
    /// </summary>
    /// <param name="value">The request example documentation collected from the route or one of its request handlers.</param>
    /// <param name="route">The route that owns the documented request example.</param>
    /// <returns><see langword="true"/> to include the request example; otherwise, <see langword="false"/>.</returns>
    public virtual bool HandleApiEndpointRequestExample ( ApiEndpointRequestExample value, Route route ) {
        return true;
    }

    /// <summary>
    /// Filters or customizes a documented query parameter for a route.
    /// </summary>
    /// <param name="value">The query parameter documentation collected from the route or one of its request handlers.</param>
    /// <param name="route">The route that owns the documented query parameter.</param>
    /// <returns><see langword="true"/> to include the query parameter; otherwise, <see langword="false"/>.</returns>
    public virtual bool HandleApiEndpointQueryParameter ( ApiEndpointQueryParameter value, Route route ) {
        return true;
    }

    /// <summary>
    /// Determines whether the generated documentation endpoint should be included.
    /// </summary>
    /// <param name="endpoint">The fully populated endpoint documentation item.</param>
    /// <param name="route">The route that produced the endpoint documentation item.</param>
    /// <returns><see langword="true"/> to include the endpoint; otherwise, <see langword="false"/>.</returns>
    public virtual bool ShouldCreateApiEndpoint ( ApiEndpoint endpoint, Route route ) {
        return true;
    }
}
