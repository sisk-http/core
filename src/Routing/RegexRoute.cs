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
    /// <summary>
    /// Initializes a new instance of the <see cref="RegexRoute"/> class.
    /// </summary>
    /// <param name="method">The HTTP method for this route.</param>
    /// <param name="pattern">The regular expression pattern for this route.</param>
    /// <param name="action">The action to be executed when this route is matched.</param>
    public RegexRoute ( RouteMethod method, [StringSyntax ( StringSyntaxAttribute.Regex )] string pattern, RouteAction action ) : base ( method, pattern, action ) {
        UseRegex = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegexRoute"/> class.
    /// </summary>
    /// <param name="method">The HTTP method for this route.</param>
    /// <param name="pattern">The regular expression pattern for this route.</param>
    /// <param name="name">The name of this route.</param>
    /// <param name="action">The action to be executed when this route is matched.</param>
    /// <param name="beforeCallback">The callback to be executed before the action.</param>
    public RegexRoute ( RouteMethod method, [StringSyntax ( StringSyntaxAttribute.Regex )] string pattern, string? name, RouteAction action, IRequestHandler []? beforeCallback ) : base ( method, pattern, name, action, beforeCallback ) {
        UseRegex = true;
    }
}
