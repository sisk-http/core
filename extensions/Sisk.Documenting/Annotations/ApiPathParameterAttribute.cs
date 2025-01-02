namespace Sisk.Documenting.Annotations;

/// <summary>
/// Specifies an attribute for an API path parameter, allowing metadata such as name, type, and description to be associated with methods.
/// </summary>
[AttributeUsage ( AttributeTargets.Method, AllowMultiple = true )]
public sealed class ApiPathParameterAttribute : Attribute {
    /// <summary>
    /// Gets the name of the path parameter.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the type of the path parameter.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the description of the path parameter.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiPathParameterAttribute"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the path parameter.</param>
    public ApiPathParameterAttribute ( string name ) {
        this.Name = name;
    }

    internal ApiEndpointPathParameter GetApiEndpointObject () {
        return new ApiEndpointPathParameter {
            Name = this.Name,
            Description = this.Description,
            Type = this.Type
        };
    }
}
