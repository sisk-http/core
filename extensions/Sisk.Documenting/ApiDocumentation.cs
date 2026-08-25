// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiDocumentation.cs
// Repository:  https://github.com/sisk-http/core

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
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Gets or sets the description of the application.
    /// </summary>
    public string? ApplicationDescription { get; set; }

    /// <summary>
    /// Gets or sets the version of the API.
    /// </summary>
    public string? ApiVersion { get; set; }

    /// <summary>
    /// Gets or sets the array of API endpoints.
    /// </summary>
    public ApiEndpoint [] Endpoints { get; set; } = null!;

    /// <summary>
    /// Generates an <see cref="ApiDocumentation"/> instance based on the provided
    /// <see cref="Router"/> and <see cref="ApiGenerationContext"/>.
    /// </summary>
    /// <param name="router">The router containing the API routes.</param>
    /// <param name="context">The context used during API generation.</param>
    /// <returns>An <see cref="ApiDocumentation"/> instance populated with the
    /// application details and endpoints.</returns>
    public static ApiDocumentation Generate ( Router router, ApiGenerationContext context ) {
        return ApiDocumentationReader.ReadDocumentation ( context, router );
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
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets the canonical name of the API endpoint (the route name).
    /// </summary>
    public string CanonicalName { get; set; } = null!;

    /// <summary>
    /// Gets the description of the API endpoint.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets the group to which the API endpoint belongs.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets the route method used for the API endpoint.
    /// </summary>
    public RouteMethod RouteMethod { get; set; }

    /// <summary>
    /// Gets the headers associated with the API endpoint.
    /// </summary>
    public ApiEndpointHeader [] Headers { get; set; } = null!;

    /// <summary>
    /// Gets the parameters accepted by the API endpoint.
    /// </summary>
    public ApiEndpointParameter [] Parameters { get; set; } = null!;

    /// <summary>
    /// Gets the situational examples associated with request parameters.
    /// </summary>
    public ApiEndpointParameterExample [] ParameterExamples { get; set; } = null!;

    /// <summary>
    /// Gets the example requests accepted by the API endpoint.
    /// </summary>
    public ApiEndpointRequestExample [] RequestExamples { get; set; } = null!;

    /// <summary>
    /// Gets the possible responses from the API endpoint.
    /// </summary>
    public ApiEndpointResponse [] Responses { get; set; } = null!;

    /// <summary>
    /// Gets the path parameters for the API endpoint.
    /// </summary>
    public ApiEndpointPathParameter [] PathParameters { get; set; } = null!;

    /// <summary>
    /// Gets the collection of query parameters supported by the API endpoint.
    /// </summary>
    public ApiEndpointQueryParameter [] QueryParameters { get; set; } = null!;

    /// <summary>
    /// Gets the relative ordering index for this instance within its containing collection or context.
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// Gets the path of the API endpoint.
    /// </summary>
    public string Path { get; set; } = null!;

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
    public string Description { get; set; } = null!;

    /// <summary>
    /// Gets the programming language used in the example, if applicable.
    /// </summary>
    public string? ExampleLanguage { get; set; }

    /// <summary>
    /// Gets the actual example request content.
    /// </summary>
    public string? Example { get; set; }

    /// <summary>
    /// Gets the JSON schema definition associated with this instance.
    /// </summary>
    public string? JsonSchema { get; set; }

    internal ApiEndpointRequestExample () {
    }
}

/// <summary>
/// Represents a query parameter for an API endpoint, including its name, type, and description.
/// </summary>
public sealed class ApiEndpointQueryParameter {

    /// <summary>
    /// Gets the name of the query parameter.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets the type of the query parameter.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets the description of the query parameter.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets a value indicating whether the item is required.
    /// </summary>
    public bool IsRequired { get; set; }

    internal ApiEndpointQueryParameter () {
    }
}

/// <summary>
/// Represents a path parameter for an API endpoint, including its name, type, and description.
/// </summary>
public sealed class ApiEndpointPathParameter {

    /// <summary>
    /// Gets the name of the path parameter.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets the type of the path parameter.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets the description of the path parameter.
    /// </summary>
    public string? Description { get; set; }

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
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets the type name of the parameter.
    /// </summary>
    public string TypeName { get; set; } = null!;

    /// <summary>
    /// Gets the description of the parameter.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets a value indicating whether the parameter is required.
    /// </summary>
    public bool IsRequired { get; set; }

    internal ApiEndpointParameter () {
    }
}

/// <summary>
/// Represents an example value for a request parameter in a specific situation.
/// </summary>
public sealed class ApiEndpointParameterExample {

    /// <summary>
    /// Gets the name of the parameter to which the example applies.
    /// </summary>
    public string ParameterName { get; set; } = null!;

    /// <summary>
    /// Gets the title that identifies the situation illustrated by the example.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets the optional description of when or why the example applies.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets the language used to format the example value, if applicable.
    /// </summary>
    public string? ExampleLanguage { get; set; }

    /// <summary>
    /// Gets the example parameter value.
    /// </summary>
    public string? Example { get; set; }

    internal ApiEndpointParameterExample () {
    }
}

/// <summary>
/// Represents a response for an API endpoint, including the status code and example content.
/// </summary>
public sealed class ApiEndpointResponse {

    /// <summary>
    /// Gets the HTTP status code for the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets the description of the response.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets the example response content.
    /// </summary>
    public string? Example { get; set; }

    /// <summary>
    /// Gets the programming language used in the example, if applicable.
    /// </summary>
    public string? ExampleLanguage { get; set; }

    /// <summary>
    /// Gets the JSON schema definition associated with this instance.
    /// </summary>
    public string? JsonSchema { get; set; }

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
    public string HeaderName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description of the header.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the header is required.
    /// </summary>
    public bool IsRequired { get; set; }

    internal ApiEndpointHeader () {
    }
}