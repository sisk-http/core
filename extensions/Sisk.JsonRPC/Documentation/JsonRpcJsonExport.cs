// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcJsonExport.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;
using LightJson;

namespace Sisk.JsonRPC.Documentation;

/// <summary>
/// Provides an JSON-based <see cref="IJsonRpcDocumentationExporter"/>.
/// </summary>
public sealed class JsonRpcJsonExport : IJsonRpcDocumentationExporter {

    /// <summary>
    /// The <see cref="JsonOptions"/> instance used to encode the documentation.
    /// </summary>
    public JsonOptions JsonOptions { get; set; }

    /// <summary>
    /// Creates an new <see cref="JsonRpcJsonExport"/> instance with default parameters.
    /// </summary>
    public JsonRpcJsonExport () {
        this.JsonOptions = JsonOptions.Default;
    }

    /// <summary>
    /// Creates an new <see cref="JsonRpcJsonExport"/> instance with the provided <see cref="LightJson.JsonOptions"/>
    /// instance.
    /// </summary>
    public JsonRpcJsonExport ( JsonOptions options ) {
        this.JsonOptions = options;
    }

    /// <summary>
    /// Encodes the specified documentation into an <see cref="JsonValue"/>.
    /// </summary>
    /// <param name="documentation">The JSON-RPC documentation to encode.</param>
    /// <returns></returns>
    public JsonValue EncodeDocumentation ( JsonRpcDocumentation documentation ) {
        JsonArray arr = this.JsonOptions.CreateJsonArray ();

        foreach (var method in documentation.Methods) {

            var item = new {
                name = method.MethodName,
                description = method.Description,
                returns = method.ReturnType,
                parameters = method.Parameters
                    .Select ( p => new {
                        name = p.ParameterName,
                        typeName = p.ParameterType.Name,
                        description = p.Description,
                        isOptional = p.IsOptional
                    } )
                    .ToArray ()
            };

            arr.Add ( JsonValue.Serialize ( item, this.JsonOptions ) );
        }

        return arr.AsJsonValue ();
    }

    /// <inheritdoc/>
    public byte [] ExportDocumentBytes ( JsonRpcDocumentation documentation ) {
        string json = this.EncodeDocumentation ( documentation ).ToString ();
        return Encoding.UTF8.GetBytes ( json );
    }
}
