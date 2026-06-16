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
/// Provides hooks for modifying or removing endpoint documentation items during generation.
/// </summary>
public abstract class ApiDocumentationHandler {

    /// <summary>
    /// Handles a response documentation item.
    /// </summary>
    public virtual ApiEndpointResponse? HandleApiEndpointResponse ( ApiEndpointResponse value, Route route ) {

        return value;
    }

    /// <summary>
    /// Handles a parameter documentation item.
    /// </summary>
    public virtual ApiEndpointParameter? HandleApiEndpointParameter ( ApiEndpointParameter value, Route route ) {

        return value;
    }

    /// <summary>
    /// Handles a header documentation item.
    /// </summary>
    public virtual ApiEndpointHeader? HandleApiEndpointHeader ( ApiEndpointHeader value, Route route ) {

        return value;
    }

    /// <summary>
    /// Handles a path parameter documentation item.
    /// </summary>
    public virtual ApiEndpointPathParameter? HandleApiEndpointPathParameter ( ApiEndpointPathParameter value, Route route ) {

        return value;
    }

    /// <summary>
    /// Handles a request example documentation item.
    /// </summary>
    public virtual ApiEndpointRequestExample? HandleApiEndpointRequestExample ( ApiEndpointRequestExample value, Route route ) {

        return value;
    }

    /// <summary>
    /// Handles a query parameter documentation item.
    /// </summary>
    public virtual ApiEndpointQueryParameter? HandleApiEndpointQueryParameter ( ApiEndpointQueryParameter value, Route route ) {

        return value;
    }

    /// <summary>
    /// Determines whether an endpoint documentation item should be included.
    /// </summary>
    public virtual bool ShouldCreateApiEndpoint ( ApiEndpoint endpoint, Route route ) {

        return true;
    }
}
