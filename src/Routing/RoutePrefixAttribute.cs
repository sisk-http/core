// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RoutePrefixAttribute.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Routing;

/// <summary>
/// Represents an attribute that, when applied to an class containing routes, all child routes will start with
/// the specified prefix.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RoutePrefixAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the route prefix.
    /// </summary>
    public string Prefix { get; set; }

    /// <summary>
    /// Initializes an new <see cref="RoutePrefixAttribute"/> with given prefix.
    /// </summary>
    public RoutePrefixAttribute(string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
        {
            throw new ArgumentNullException(nameof(prefix));
        }
        this.Prefix = prefix;
    }
}
