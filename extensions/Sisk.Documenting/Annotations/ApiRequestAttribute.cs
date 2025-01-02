namespace Sisk.Documenting.Annotations;

/// <summary>
/// Specifies an attribute for an API request, allowing metadata such as description, example language, and example content to be associated with methods.
/// </summary>
[AttributeUsage ( AttributeTargets.Method, AllowMultiple = true )]
public sealed class ApiRequestAttribute : Attribute {
    /// <summary>
    /// Gets or sets the description of the API request.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the programming language used in the example, if applicable.
    /// </summary>
    public string? ExampleLanguage { get; set; }

    /// <summary>
    /// Gets or sets the actual example request content.
    /// </summary>
    public string? Example { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiRequestAttribute"/> class with the specified description.
    /// </summary>
    /// <param name="description">The description of the API request.</param>
    public ApiRequestAttribute ( string description ) {
        this.Description = description;
    }

    internal ApiEndpointRequestExample GetApiEndpointObject () {
        return new ApiEndpointRequestExample {
            Description = this.Description,
            ExampleLanguage = this.ExampleLanguage,
            Example = this.Example,
        };
    }
}
