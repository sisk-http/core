using System.Net;
using Sisk.Core.Routing;

namespace Sisk.Documenting;

/// <summary>
/// Represents the API documentation, including application details and endpoints.
/// </summary>
public sealed class ApiDocumentation {

    /// <summary>
    /// Gets or sets the name of the application.
    /// </summary>
    public string? ApplicationName { get; internal set; }
    
    /// <summary>
    /// Gets or sets the description of the application.
    /// </summary>
    public string? ApplicationDescription { get; internal set; }
    
    /// <summary>
    /// Gets or sets the version of the API.
    /// </summary>
    public string? ApiVersion { get; internal set; }
    
    /// <summary>
    /// Gets or sets the array of API endpoints.
    /// </summary>
    public ApiEndpoint [] Endpoints { get; internal set; } = null!;
    
    /// <summary>
    /// Generates an instance of <see cref="ApiDocumentation"/> by reading documentation from the specified router and identifier.
    /// </summary>
    /// <param name="router">The router used to generate the documentation.</param>
    /// <param name="identifier">The identifier for the API documentation.</param>
    /// <returns>An instance of <see cref="ApiDocumentation"/>.</returns>
    public static ApiDocumentation Generate ( Router router, ApiIdentifier identifier ) {
        return ApiDocumentationReader.ReadDocumentation ( identifier, router );
    }

    internal ApiDocumentation () {
    }
}

/// <summary>
/// Represents an API endpoint, including its metadata, request and response details.
/// </summary>
public sealed class ApiEndpoint {

    /// <summary>
    /// Gets the name of the API endpoint.
    /// </summary>
    public string Name { get; internal set; } = null!;

    /// <summary>
    /// Gets the description of the API endpoint.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    /// Gets the group to which the API endpoint belongs.
    /// </summary>
    public string? Group { get; internal set; }

    /// <summary>
    /// Gets the route method used for the API endpoint.
    /// </summary>
    public RouteMethod RouteMethod { get; internal set; }

    /// <summary>
    /// Gets the headers associated with the API endpoint.
    /// </summary>
    public ApiEndpointHeader [] Headers { get; internal set; } = null!;

    /// <summary>
    /// Gets the parameters accepted by the API endpoint.
    /// </summary>
    public ApiEndpointParameter [] Parameters { get; internal set; } = null!;

    /// <summary>
    /// Gets the parameters accepted by the API endpoint.
    /// </summary>
    public ApiEndpointRequestExample [] RequestExamples { get; internal set; } = null!;

    /// <summary>
    /// Gets the possible responses from the API endpoint.
    /// </summary>
    public ApiEndpointResponse [] Responses { get; internal set; } = null!;

    /// <summary>
    /// Gets the path parameters for the API endpoint.
    /// </summary>
    public ApiEndpointPathParameter [] PathParameters { get; internal set; } = null!;

    /// <summary>
    /// Gets the path of the API endpoint.
    /// </summary>
    public string Path { get; internal set; } = null!;

    internal ApiEndpoint () {
    }
}

/// <summary>
/// Represents an example request for an API endpoint, including its description and example content.
/// </summary>
public sealed class ApiEndpointRequestExample {

    /// <summary>
    /// Gets the description of the request example.
    /// </summary>
    public string Description { get; internal set; } = null!;

    /// <summary>
    /// Gets the programming language used in the example, if applicable.
    /// </summary>
    public string? ExampleLanguage { get; internal set; }

    /// <summary>
    /// Gets the actual example request content.
    /// </summary>
    public string? Example { get; internal set; }

    internal ApiEndpointRequestExample () {
    }
}

/// <summary>
/// Represents a path parameter for an API endpoint, including its name, type, and description.
/// </summary>
public sealed class ApiEndpointPathParameter {

    /// <summary>
    /// Gets the name of the path parameter.
    /// </summary>
    public string Name { get; internal set; } = null!;

    /// <summary>
    /// Gets the type of the path parameter.
    /// </summary>
    public string? Type { get; internal set; }

    /// <summary>
    /// Gets the description of the path parameter.
    /// </summary>
    public string? Description { get; internal set; }

    internal ApiEndpointPathParameter () {
    }
}

/// <summary>
/// Represents a parameter for an API endpoint, including its name, type, and requirements.
/// </summary>
public sealed class ApiEndpointParameter {

    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    public string Name { get; internal set; } = null!;

    /// <summary>
    /// Gets the type name of the parameter.
    /// </summary>
    public string TypeName { get; internal set; } = null!;

    /// <summary>
    /// Gets the description of the parameter.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the parameter is required.
    /// </summary>
    public bool IsRequired { get; internal set; }

    internal ApiEndpointParameter () {
    }
}

/// <summary>
/// Represents a response for an API endpoint, including the status code and example content.
/// </summary>
public sealed class ApiEndpointResponse {

    /// <summary>
    /// Gets the HTTP status code for the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; internal set; }

    /// <summary>
    /// Gets the description of the response.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    /// Gets the example response content.
    /// </summary>
    public string? Example { get; internal set; }

    /// <summary>
    /// Gets the programming language used in the example, if applicable.
    /// </summary>
    public string? ExampleLanguage { get; internal set; }

    internal ApiEndpointResponse () {
    }
}

/// <summary>
/// Represents a header for an API endpoint, including its name and requirements.
/// </summary>
public sealed class ApiEndpointHeader {

    /// <summary>
    /// Gets or sets the name of the header.
    /// </summary>
    public string HeaderName { get; internal set; } = null!;

    /// <summary>
    /// Gets or sets the description of the header.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    /// Gets or sets a value indicating whether the header is required.
    /// </summary>
    public bool IsRequired { get; internal set; }

    internal ApiEndpointHeader () {
    }
}