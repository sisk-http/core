// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ParameterExampleResult.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting;

public sealed class ParameterExampleResult {
    public string ParameterName { get; init; }
    public string TypeName { get; init; }
    public string? Description { get; init; }
    public bool IsRequired { get; init; }

    public ParameterExampleResult ( string parameterName, string typeName, bool isRequired, string? description ) {
        ParameterName = parameterName;
        TypeName = typeName;
        IsRequired = isRequired;
        Description = description;
    }

    public ParameterExampleResult ( string parameterName, string typeName, bool isRequired ) {
        ParameterName = parameterName;
        TypeName = typeName;
        IsRequired = isRequired;
    }
}
