// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiGenerationContext.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting;

/// <summary>
/// Provides context information used during API generation, such as application metadata
/// and handlers for generating example bodies and parameters.
/// </summary>
public sealed class ApiGenerationContext {

    /// <summary>
    /// Initializes a new <see cref="ApiGenerationContext"/> class instance.
    /// </summary>
    public ApiGenerationContext () {
    }

    /// <summary>
    /// Gets or sets the name of the application.
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Gets or sets the version of the application.
    /// </summary>
    public string? ApplicationVersion { get; set; }

    /// <summary>
    /// Gets or sets the description of the application.
    /// </summary>
    public string? ApplicationDescription { get; set; }

    /// <summary>
    /// Gets or sets the handler used to generate example bodies for request and response types.
    /// </summary>
    public IExampleBodyTypeHandler? BodyExampleTypeHandler { get; set; }

    /// <summary>
    /// Gets or sets the handler used to generate example parameters for query, route, and header parameters.
    /// </summary>
    public IExampleParameterTypeHandler? ParameterExampleTypeHandler { get; set; }

    /// <summary>
    /// Gets or sets the handler used to process content schema types.
    /// </summary>
    /// <remarks>Assign an implementation of <see cref="IContentSchemaTypeHandler"/> to customize how content
    /// schema types are interpreted or validated. If <see langword="null"/>, default handling will be used.</remarks>
    public IContentSchemaTypeHandler? ContentSchemaTypeHandler { get; set; }
}
