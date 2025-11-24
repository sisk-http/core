// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiQueryParameterAttribute.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting.Annotations;

/// <summary>
/// Specifies an attribute for an API path parameter, allowing metadata such as name, type, and description to be associated with methods.
/// </summary>
[AttributeUsage ( AttributeTargets.Method, AllowMultiple = true )]
public sealed class ApiQueryParameterAttribute : Attribute {
    /// <summary>
    /// Gets the name of the query parameter.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the type of the query parameter.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the description of the query parameter.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property is required.
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiQueryParameterAttribute"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the query parameter.</param>
    public ApiQueryParameterAttribute ( string name ) {
        Name = name;
    }

    internal ApiEndpointQueryParameter GetApiEndpointObject () {
        return new ApiEndpointQueryParameter {
            Name = Name,
            Description = Description,
            Type = Type,
            IsRequired = IsRequired
        };
    }
}
