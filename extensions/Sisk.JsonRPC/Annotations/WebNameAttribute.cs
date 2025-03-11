// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   WebNameAttribute.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.JsonRPC.Annotations;

/// <summary>
/// Represents an attribute which holds the class name for a group of
/// JSON-RPC methods.
/// </summary>
[AttributeUsage ( AttributeTargets.Class )]
public sealed class WebNameAttribute : Attribute {
    /// <summary>
    /// Gets or sets the name associated with the method group.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Creates an new instance of the <see cref="WebMethodAttribute"/> attribute.
    /// </summary>
    /// <param name="name">The method-group name.</param>
    public WebNameAttribute ( string name ) {
        Name = name;
    }
}
