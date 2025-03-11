// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MethodDescriptionAttribute.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.JsonRPC.Annotations;

/// <summary>
/// Specifies a description for a method.
/// </summary>
[AttributeUsage ( AttributeTargets.Method, AllowMultiple = false )]
public sealed class MethodDescriptionAttribute : Attribute {
    /// <summary>
    /// Gets the description of the method.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Optional. Gets or sets the web method category.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodDescriptionAttribute"/> class with the specified description.
    /// </summary>
    /// <param name="description">The description of the method.</param>
    public MethodDescriptionAttribute ( string description ) {
        Description = description;
    }
}
