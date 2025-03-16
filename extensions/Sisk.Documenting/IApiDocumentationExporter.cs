// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IApiDocumentationExporter.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting;

/// <summary>
/// Defines a contract for exporting API documentation content.
/// </summary>
public interface IApiDocumentationExporter {

    /// <summary>
    /// Exports the specified API documentation content.
    /// </summary>
    /// <param name="documentation">The API documentation to export.</param>
    /// <returns>An <see cref="HttpContent"/> representing the exported documentation.</returns>
    public HttpContent ExportDocumentationContent ( ApiDocumentation documentation );
}