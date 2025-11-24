// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ParameterExampleResult.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting;

/// <summary>
/// Represents a parameter example used in documentation.
/// </summary>
public sealed class ParameterExampleResult {
    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    public string ParameterName { get; init; }

    /// <summary>
    /// Gets the name of the parameter's type.
    /// </summary>
    public string TypeName { get; init; }

    /// <summary>
    /// Gets the description of the parameter, or <see langword="null"/> if no description is provided.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets a value indicating whether the parameter is required.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterExampleResult"/> class with the specified parameter name, type name, requirement, and description.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="typeName">The name of the parameter's type.</param>
    /// <param name="isRequired"><see langword="true"/> if the parameter is required; otherwise, <see langword="false"/>.</param>
    /// <param name="description">The description of the parameter, or <see langword="null"/> if no description is provided.</param>
    public ParameterExampleResult ( string parameterName, string typeName, bool isRequired, string? description ) {
        ParameterName = parameterName;
        TypeName = typeName;
        IsRequired = isRequired;
        Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterExampleResult"/> class with the specified parameter name, type name, and requirement.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="typeName">The name of the parameter's type.</param>
    /// <param name="isRequired"><see langword="true"/> if the parameter is required; otherwise, <see langword="false"/>.</param>
    public ParameterExampleResult ( string parameterName, string typeName, bool isRequired ) {
        ParameterName = parameterName;
        TypeName = typeName;
        IsRequired = isRequired;
    }
}