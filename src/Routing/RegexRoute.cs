// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RegexRoute.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Sisk.Core.Routing;

/// <summary>
/// Represents an <see cref="Route"/> which it's path is interpreted as an regular expression.
/// </summary>
public sealed class RegexRoute : Route {
    private Regex? routeRegex;

    /// <inheritdoc/>
    public override bool AllowRewrites => false;

    /// <inheritdoc/>
    public override bool IsStatic => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegexRoute"/> class with no parameters.
    /// </summary>
    public RegexRoute () : base () {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegexRoute"/> class.
    /// </summary>
    /// <param name="method">The HTTP method for this route.</param>
    /// <param name="pattern">The regular expression pattern for this route.</param>
    /// <param name="action">The action to be executed when this route is matched.</param>
    public RegexRoute ( RouteMethod method, [StringSyntax ( StringSyntaxAttribute.Regex )] string pattern, RouteAction action ) : base ( method, pattern, action ) {
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
    }

    /// <inheritdoc/>
    public override RouteMatch Match ( string requestPath, Router router ) {
        RegexOptions options = router.MatchRoutesIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

        Regex? regex = routeRegex;
        if (regex is null || regex.Options != options) {
            routeRegex = regex = new Regex ( Path, options );
        }

        // The pattern is matched as written, without implicit anchors: an unanchored pattern such as "/admin"
        // also matches "/x/admin/y". Anchoring here would break existing patterns that rely on partial matches,
        // so routes that must match the whole path should use explicit "^...$" anchors.
        var test = regex.Match ( requestPath );
        if (test.Success) {
            NameValueCollection query = new NameValueCollection ();
            for (int i = 0; i < test.Groups.Count; i++) {
                Group group = test.Groups [ i ];
                if (group.Index.ToString ( provider: null ) == group.Name)
                    continue;
                query.Add ( group.Name, group.Value );
            }
            return new RouteMatch ( true, query );
        }
        else {
            return RouteMatch.NotMatched;
        }
    }

    /// <inheritdoc/>
    protected override void OnPathModified ( string oldPath, string newPath ) {
        routeRegex = null;
    }
}
