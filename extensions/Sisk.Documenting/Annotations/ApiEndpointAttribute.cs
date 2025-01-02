﻿namespace Sisk.Documenting.Annotations;

/// <summary>
/// Specifies an attribute for an API endpoint, allowing metadata such as name, description, and group to be associated with methods.
/// </summary>
[AttributeUsage ( AttributeTargets.Method, AllowMultiple = false )]
public sealed class ApiEndpointAttribute : Attribute {
    /// <summary>
    /// Gets or sets the name of the API endpoint.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the API endpoint.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the group to which the API endpoint belongs.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiEndpointAttribute"/> class with the specified endpoint name.
    /// </summary>
    /// <param name="name">The name of the API endpoint.</param>
    public ApiEndpointAttribute ( string name ) {
        this.Name = name;
    }
}