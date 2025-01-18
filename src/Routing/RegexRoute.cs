// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
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
public sealed class RegexRoute : Route {
    /// <inheritdoc/>
    /// <exclude/>
    public RegexRoute ( RouteMethod method, [StringSyntax ( StringSyntaxAttribute.Regex )] string pattern, RouteAction action ) : base ( method, pattern, action ) {
        this.UseRegex = true;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public RegexRoute ( RouteMethod method, [StringSyntax ( StringSyntaxAttribute.Regex )] string pattern, string? name, RouteAction action, IRequestHandler []? beforeCallback ) : base ( method, pattern, name, action, beforeCallback ) {
        this.UseRegex = true;
    }
}
