// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ParamDescriptionAttribute.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.JsonRPC.Annotations;

/// <summary>
/// Specifies a description for a method parameter.
/// </summary>
[AttributeUsage ( AttributeTargets.Method, AllowMultiple = true )]
public sealed class ParamDescriptionAttribute : Attribute {
    /// <summary>
    /// Gets the description of the method parameter.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the target parameter name.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParamDescriptionAttribute"/> class with the specified description.
    /// </summary>
    /// <param name="paramName">The parameter name.</param>
    /// <param name="description">The description of the method parameter.</param>
    public ParamDescriptionAttribute ( string paramName, string description ) {
        ParameterName = paramName;
        Description = description;
    }
}
