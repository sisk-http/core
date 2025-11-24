// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IExampleParameterTypeHandler.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting;

/// <summary>
/// Defines a contract for generating example parameters for a given <see cref="Type"/>.
/// </summary>
public interface IExampleParameterTypeHandler {
    /// <summary>
    /// Gets example parameters for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type for which to generate example parameters. Cannot be <see langword="null"/>.</param>
    /// <returns>
    /// An array of <see cref="ParameterExampleResult"/> containing example values for the type.
    /// </returns>
    ParameterExampleResult [] GetParameterExamplesForType ( Type type );
}