// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiParameterAttribute.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting.Annotations;

/// <summary>
/// Specifies an attribute for an API parameter, allowing metadata such as name, type, description, and requirement status to be associated with methods.
/// </summary>
[AttributeUsage ( AttributeTargets.Method, AllowMultiple = true )]
public sealed class ApiParameterAttribute : Attribute {
    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type name of the parameter.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Gets or sets the description of the parameter.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the parameter is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiParameterAttribute"/> class with the specified name and type name.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="typeName">The type name of the parameter.</param>
    public ApiParameterAttribute ( string name, string typeName ) {
        Name = name;
        TypeName = typeName;
    }

    internal ApiEndpointParameter GetApiEndpointObject () {
        return new ApiEndpointParameter {
            Name = Name,
            TypeName = TypeName,
            Description = Description,
            IsRequired = IsRequired
        };
    }
}
