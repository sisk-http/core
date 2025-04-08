// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IExampleParameterTypeHandler.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting;

public interface IExampleParameterTypeHandler {
    public ParameterExampleResult [] GetParameterExamplesForType ( Type type );
}
