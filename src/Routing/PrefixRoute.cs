// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   PrefixRoute.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Routing;

/// <summary>
/// Represents an <see cref="Route"/> which matches the request paths equal to its path or nested under it,
/// comparing whole path segments.
/// </summary>
public sealed class PrefixRoute : Route {
    static readonly RouteMatch Matched = new ( true, null );

    /// <inheritdoc/>
    public override bool AllowRewrites => false;

    /// <inheritdoc/>
    public override bool IsStatic => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrefixRoute"/> class.
    /// </summary>
    /// <param name="method">The HTTP method for this route.</param>
    /// <param name="prefix">The path prefix for this route.</param>
    /// <param name="name">The name of this route.</param>
    /// <param name="action">The action to be executed when this route is matched.</param>
    /// <param name="beforeCallback">The callback to be executed before the action.</param>
    public PrefixRoute ( RouteMethod method, string prefix, string? name, RouteAction action, IRequestHandler []? beforeCallback )
        : base ( method, "/" + prefix.Trim ( '/' ), name, action, beforeCallback ) {
    }

    /// <inheritdoc/>
    public override RouteMatch Match ( string requestPath, Router router ) {
        ReadOnlySpan<char> prefix = Path.AsSpan ().TrimEnd ( '/' );
        StringComparison comparison = router.MatchRoutesIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        bool matches =
            requestPath.AsSpan ().StartsWith ( prefix, comparison ) &&
            (requestPath.Length == prefix.Length || requestPath [ prefix.Length ] == '/');

        return matches ? Matched : RouteMatch.NotMatched;
    }

}
