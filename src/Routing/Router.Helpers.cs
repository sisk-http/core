// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Router.Helpers.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Helpers;
using Sisk.Core.Internal;

namespace Sisk.Core.Routing;

partial class Router {

    /// <summary>
    /// Combines an array of string parts into a single path.
    /// </summary>
    /// <param name="parts">An array of string parts to combine.</param>
    /// <returns>A string representing the combined path.</returns>
    public static string Combine ( params string [] parts ) {
        return PathHelper.CombinePaths ( parts );
    }

    /// <summary>
    /// Attempts to match the specified route expression against the given path.
    /// </summary>
    /// <param name="routeExpression">The route expression to match.</param>
    /// <param name="path">The path to match against the route expression.</param>
    /// <param name="stringComparer">The string comparison to use when matching the route expression. Defaults to <see cref="StringComparison.Ordinal"/>.</param>
    /// <returns>A <see cref="RouteMatch"/> object indicating the result of the match.</returns>
    public static RouteMatch MatchRouteExpression ( in ReadOnlySpan<char> routeExpression, in ReadOnlySpan<char> path, StringComparison stringComparer = StringComparison.Ordinal ) {
        return HttpStringInternals.IsReqPathMatch ( routeExpression, path, stringComparer );
    }

    /// <summary>
    /// Attempts to match the specified route expression against the given path.
    /// </summary>
    /// <param name="routeExpression">The route expression to match.</param>
    /// <param name="path">The path to match against the route expression.</param>
    /// <param name="stringComparer">The string comparison to use when matching the route expression. Defaults to <see cref="StringComparison.Ordinal"/>.</param>
    /// <returns>A <see cref="RouteMatch"/> object indicating the result of the match.</returns>
    public static RouteMatch MatchRouteExpression ( string routeExpression, string path, StringComparison stringComparer = StringComparison.Ordinal ) {
        ArgumentNullException.ThrowIfNullOrEmpty ( routeExpression, nameof ( routeExpression ) );
        ArgumentNullException.ThrowIfNullOrEmpty ( path, nameof ( path ) );

        return HttpStringInternals.IsReqPathMatch ( routeExpression, path, stringComparer );
    }

    /// <summary>
    /// Determines whether two route expressions overlap.
    /// </summary>
    /// <param name="routeExpression1">The first route expression to compare.</param>
    /// <param name="routeExpression2">The second route expression to compare.</param>
    /// <param name="stringComparer">The string comparison to use when comparing the route expressions. Defaults to <see cref="StringComparison.Ordinal"/>.</param>
    /// <returns><c>true</c> if the route expressions overlap; otherwise, <c>false</c>.</returns>
    public static bool IsRouteExpressionsOverlap ( in ReadOnlySpan<char> routeExpression1, in ReadOnlySpan<char> routeExpression2, StringComparison stringComparer = StringComparison.Ordinal ) {
        return HttpStringInternals.IsRoutePatternMatch ( routeExpression1, routeExpression2, stringComparer );
    }

    /// <summary>
    /// Determines whether two route expressions overlap.
    /// </summary>
    /// <param name="routeExpression1">The first route expression to compare.</param>
    /// <param name="routeExpression2">The second route expression to compare.</param>
    /// <param name="stringComparer">The string comparison to use when comparing the route expressions. Defaults to <see cref="StringComparison.Ordinal"/>.</param>
    /// <returns><c>true</c> if the route expressions overlap; otherwise, <c>false</c>.</returns>
    public static bool IsRouteExpressionsOverlap ( string routeExpression1, string routeExpression2, StringComparison stringComparer = StringComparison.Ordinal ) {
        ArgumentNullException.ThrowIfNullOrEmpty ( routeExpression1, nameof ( routeExpression1 ) );
        ArgumentNullException.ThrowIfNullOrEmpty ( routeExpression2, nameof ( routeExpression2 ) );

        return HttpStringInternals.IsRoutePatternMatch ( routeExpression1, routeExpression2, stringComparer );
    }
}
