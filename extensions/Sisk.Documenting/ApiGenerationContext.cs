// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiGenerationContext.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting;

public sealed class ApiGenerationContext {

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

    public IExampleBodyTypeHandler? BodyExampleTypeHandler { get; set; }

    public IExampleParameterTypeHandler? ParameterExampleTypeHandler { get; set; }
}
