// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IExampleBodyTypeHandler.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting;

/// <summary>
/// Defines a contract for generating example bodies for a given <see cref="Type"/>.
/// </summary>
public interface IExampleBodyTypeHandler {
    /// <summary>
    /// Gets an example body for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type for which to generate an example body. Cannot be <see langword="null"/>.</param>
    /// <returns>
    /// A <see cref="BodyExampleResult"/> containing the example contents and language, or
    /// <see langword="null"/> if no example can be generated for the specified type.
    /// </returns>
    BodyExampleResult? GetBodyExampleForType ( Type type );
}