// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IJsonRpcDocumentationExporter.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.JsonRPC.Documentation;

/// <summary>
/// Defines a method to export JSON-RPC documentation to a byte array.
/// </summary>
public interface IJsonRpcDocumentationExporter {

    /// <summary>
    /// Exports the JSON-RPC documentation to a byte array.
    /// </summary>
    /// <param name="documentation">The JSON-RPC documentation to export.</param>
    /// <returns>A byte array containing the exported documentation.</returns>
    public byte [] ExportDocumentBytes ( JsonRpcDocumentation documentation );
}
