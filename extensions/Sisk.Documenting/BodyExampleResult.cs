// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   BodyExampleResult.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting;

/// <summary>
/// Represents the result of generating a body example, containing the example contents and an optional language.
/// </summary>
public sealed class BodyExampleResult {
    /// <summary>
    /// Gets the example contents.
    /// </summary>
    public string ExampleContents { get; }

    /// <summary>
    /// Gets the language of the example, if specified; otherwise <see langword="null"/>.
    /// </summary>
    public string? ExampleLanguage { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BodyExampleResult"/> class.
    /// </summary>
    /// <param name="exampleContents">The example contents. Cannot be <see langword="null"/>.</param>
    /// <param name="exampleLanguage">The language of the example, or <see langword="null"/> if not applicable.</param>
    public BodyExampleResult ( string exampleContents, string? exampleLanguage ) {
        ExampleContents = exampleContents;
        ExampleLanguage = exampleLanguage;
    }
}