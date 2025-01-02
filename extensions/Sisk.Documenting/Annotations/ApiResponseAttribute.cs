using System.Net;

namespace Sisk.Documenting.Annotations;

/// <summary>
/// Specifies an attribute for an API response, allowing metadata such as status code, description, example content, and example language to be associated with methods.
/// </summary>
[AttributeUsage ( AttributeTargets.Method, AllowMultiple = true )]
public sealed class ApiResponseAttribute : Attribute {
    /// <summary>
    /// Gets the HTTP status code for the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets or sets the description of the response.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the example response content.
    /// </summary>
    public string? Example { get; set; }

    /// <summary>
    /// Gets or sets the programming language used in the example, if applicable.
    /// </summary>
    public string? ExampleLanguage { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResponseAttribute"/> class with the specified status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code for the response.</param>
    public ApiResponseAttribute ( HttpStatusCode statusCode ) {
        this.StatusCode = statusCode;
    }

    internal ApiEndpointResponse GetApiEndpointObject () {
        return new ApiEndpointResponse {
            StatusCode = this.StatusCode,
            Description = this.Description,
            Example = this.Example,
            ExampleLanguage = this.ExampleLanguage,
        };
    }
}
