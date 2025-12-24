// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiRequestAttribute.cs
// Repository:  https://github.com/sisk-http/core

using LightJson.Serialization;

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
    /// Gets or sets the type used to generate the request example and schema.
    /// </summary>
    public Type? PayloadType { get; set; }

    /// <summary>
    /// Gets or sets the JSON schema that describes the expected request payload.
    /// </summary>
    public string? JsonSchema { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiRequestAttribute"/> class with the specified description.
    /// </summary>
    /// <param name="description">The description of the API request.</param>
    public ApiRequestAttribute ( string description ) {
        Description = description;
    }

    internal ApiEndpointRequestExample GetApiEndpointObject ( ApiGenerationContext context ) {

        string? example = Example;
        string? exampleLanguage = ExampleLanguage;

        if (PayloadType != null && context.BodyExampleTypeHandler?.GetBodyExampleForType ( PayloadType ) is { } exampleResult) {
            example = exampleResult.ExampleContents;
            exampleLanguage = exampleResult.ExampleLanguage;
        }
        if (PayloadType != null && JsonSchema == null && context.ContentSchemaTypeHandler?.GetJsonSchemaForType ( PayloadType ) is { } schemaResult) {
            JsonSchema = JsonWriter.Serialize ( schemaResult, prettyOutput: true );
        }

        return new ApiEndpointRequestExample {
            Description = Description,
            ExampleLanguage = exampleLanguage,
            Example = example,
            JsonSchema = JsonSchema
        };
    }
}
