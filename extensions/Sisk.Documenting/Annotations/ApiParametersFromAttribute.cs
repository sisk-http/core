// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiParametersFromAttribute.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting.Annotations;

/// <summary>
/// Specifies that the parameters for an API endpoint should be derived from the properties of the specified type.
/// </summary>
[AttributeUsage ( AttributeTargets.Method, AllowMultiple = true )]
public sealed class ApiParametersFromAttribute : Attribute {

    /// <summary>
    /// Gets the type whose properties will be used to generate parameter examples.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiParametersFromAttribute"/> class with the specified type.
    /// </summary>
    /// <param name="type">The type whose properties will be used to generate parameter examples.</param>
    public ApiParametersFromAttribute ( Type type ) {
        Type = type;
    }

    /// <summary>
    /// Generates an array of <see cref="ApiEndpointParameter"/> instances based on the properties of <see cref="Type"/>.
    /// </summary>
    /// <param name="context">The API generation context.</param>
    /// <returns>An array of <see cref="ApiEndpointParameter"/> instances, or an empty array if no handler is available.</returns>
    internal ApiEndpointParameter [] GetParameters ( ApiGenerationContext context ) {
        return context.ParameterExampleTypeHandler?.GetParameterExamplesForType ( Type )
                .Select ( x => new ApiEndpointParameter () {
                    Name = x.ParameterName,
                    Description = x.Description,
                    IsRequired = x.IsRequired,
                    TypeName = x.TypeName
                } ).ToArray ()
            ?? Array.Empty<ApiEndpointParameter> ();
    }
}