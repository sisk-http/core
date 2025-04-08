// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiParametersFrom.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting.Annotations;

[AttributeUsage ( AttributeTargets.Method, AllowMultiple = true )]
public sealed class ApiParametersFromAttribute : Attribute {

    public Type Type { get; }

    public ApiParametersFromAttribute ( Type type ) {
        Type = type;
    }

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
