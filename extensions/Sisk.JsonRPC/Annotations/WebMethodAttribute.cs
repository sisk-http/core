// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   WebMethodAttribute.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.JsonRPC.Annotations;

/// <summary>
/// Represents an JSON-RPC method.
/// </summary>
[AttributeUsage ( AttributeTargets.Method, AllowMultiple = false, Inherited = false )]
public sealed class WebMethodAttribute : Attribute {
    /// <summary>
    /// Gets or sets the method name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Creates an new <see cref="WebMethodAttribute"/> with no parameters.
    /// </summary>
    public WebMethodAttribute () {
    }

    /// <summary>
    /// Creates an new <see cref="WebMethodAttribute"/> with given
    /// parameters.
    /// </summary>
    /// <param name="methodName">The method name.</param>
    public WebMethodAttribute ( string methodName ) {
        this.Name = methodName;
    }
}
