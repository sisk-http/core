// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RegexRoute.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;

namespace Sisk.Core.Routing;

/// <summary>
/// Represents an <see cref="Route"/> which it's implementation already enables
/// <see cref="Route.UseRegex"/>.
/// </summary>
public class RegexRoute : Route
{
    /// <inheritdoc/>
    /// <exclude/>
#if NET6_0
    public RegexRoute(RouteMethod method, string path, RouteAction action) : base(method, path, action)
#elif NET7_0_OR_GREATER
    public RegexRoute(RouteMethod method, [StringSyntax(StringSyntaxAttribute.Regex)] string pattern, RouteAction action) : base(method, pattern, action)
#endif

    {
        this.UseRegex = true;
    }

    /// <inheritdoc/>
    /// <exclude/>
#if NET6_0
    public RegexRoute(RouteMethod method, string path, string? name, RouteAction action, IRequestHandler[]? beforeCallback) : base(method, path, name, action, beforeCallback)
#elif NET7_0_OR_GREATER
    public RegexRoute(RouteMethod method, [StringSyntax(StringSyntaxAttribute.Regex)] string pattern, string? name, RouteAction action, IRequestHandler[]? beforeCallback) : base(method, pattern, name, action, beforeCallback)
#endif
    {
        this.UseRegex = true;
    }
}
