// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RegexRoute.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Routing;

/// <summary>
/// Represents an <see cref="Route"/> which it's implementation already enables
/// <see cref="Route.UseRegex"/>.
/// </summary>
/// <definition>
/// public class RegexRoute
/// </definition>
/// <type>
/// Class
/// </type>
public class RegexRoute : Route
{
    /// <inheritdoc/>
    /// <nodoc/>
#if NET6_0
    public RegexRoute(RouteMethod method, string path, RouteAction action) : base(method, path, action)
#elif NET7_0_OR_GREATER
    public RegexRoute(RouteMethod method, [StringSyntax(StringSyntaxAttribute.Regex)] string pattern, RouteAction action) : base(method, pattern, action)
#endif

    {
        UseRegex = true;
    }

    /// <inheritdoc/>
    /// <nodoc/>
#if NET6_0
    public RegexRoute(RouteMethod method, string path, string? name, RouteAction action, IRequestHandler[]? beforeCallback) : base(method, path, name, action, beforeCallback)
#elif NET7_0_OR_GREATER
    public RegexRoute(RouteMethod method, [StringSyntax(StringSyntaxAttribute.Regex)] string pattern, string? name, RouteAction action, IRequestHandler[]? beforeCallback) : base(method, pattern, name, action, beforeCallback)
#endif
    {
        UseRegex = true;
    }
}
