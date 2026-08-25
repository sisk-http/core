// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiParameterExampleAttribute.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting.Annotations;

/// <summary>
/// Specifies an example value for an API request parameter in a particular situation.
/// </summary>
[AttributeUsage ( AttributeTargets.Method, AllowMultiple = true )]
public sealed class ApiParameterExampleAttribute : Attribute {

    /// <summary>
    /// Gets the name of the parameter to which the example applies.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// Gets the title that identifies the situation illustrated by the example.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets or sets the optional description of when or why the example applies.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the language used to format the example value, if applicable.
    /// </summary>
    public string? ExampleLanguage { get; set; }

    /// <summary>
    /// Gets or sets the example parameter value.
    /// </summary>
    public string? Example { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiParameterExampleAttribute"/> class.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to which the example applies.</param>
    /// <param name="title">The title that identifies the situation illustrated by the example.</param>
    public ApiParameterExampleAttribute ( string parameterName, string title ) {
        ParameterName = parameterName;
        Title = title;
    }

    internal ApiEndpointParameterExample GetApiEndpointObject () {
        return new ApiEndpointParameterExample {
            ParameterName = ParameterName,
            Title = Title,
            Description = Description,
            ExampleLanguage = ExampleLanguage,
            Example = Example
        };
    }
}