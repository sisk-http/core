// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiEndpointFlags.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting;

/// <summary>
/// Specifies classification flags for an API endpoint, such as its stability level or availability.
/// </summary>
/// <remarks>
/// Flags are descriptive metadata: the <see cref="ApiEndpoint"/> keeps them and each
/// <see cref="Exporters.IApiDocumentationExporter"/> decides how to represent them.
/// </remarks>
[Flags]
public enum ApiEndpointFlags {

    /// <summary>
    /// Specifies that the endpoint has no classification flags.
    /// </summary>
    None = 0,

    /// <summary>
    /// Specifies that the endpoint is published as a beta and may still receive breaking changes.
    /// </summary>
    Beta = 1 << 0,

    /// <summary>
    /// Specifies that the endpoint is experimental and its behavior is not guaranteed.
    /// </summary>
    Experimental = 1 << 1,

    /// <summary>
    /// Specifies that the endpoint is a preview of an unreleased feature.
    /// </summary>
    Preview = 1 << 2,

    /// <summary>
    /// Specifies that the endpoint is deprecated and may be removed in a future version.
    /// </summary>
    Deprecated = 1 << 3,

    /// <summary>
    /// Specifies that the endpoint is internal and not intended for public use.
    /// </summary>
    Internal = 1 << 4
}