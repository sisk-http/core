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
        JsonOptions = new JsonOptions () { NamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
    }

    /// <summary>
    /// Creates an new <see cref="JsonRpcJsonExport"/> instance with the provided <see cref="LightJson.JsonOptions"/>
    /// instance.
    /// </summary>
    public JsonRpcJsonExport ( JsonOptions options ) {
        JsonOptions = options;
    }

    /// <summary>
    /// Encodes the specified documentation into an <see cref="JsonValue"/>.
    /// </summary>
    /// <param name="documentation">The JSON-RPC documentation to encode.</param>
    /// <returns></returns>
    public JsonValue EncodeDocumentation ( JsonRpcDocumentation documentation ) {
        JsonArray arr = JsonOptions.CreateJsonArray ();

        foreach (var method in documentation.Methods) {

            var item = new {
                Name = method.MethodName,
                Description = method.Description,
                Returns = method.ReturnType.Name,
                Parameters = method.Parameters
                    .Select ( p => new {
                        Name = p.ParameterName,
                        TypeName = p.ParameterType.Name,
                        Description = p.Description,
                        IsOptional = p.IsOptional
                    } )
                    .ToArray ()
            };

            arr.Add ( JsonValue.Serialize ( item, JsonOptions ) );
        }

        return JsonOptions.Serialize ( new {
            Metadata = Pipe ( documentation.Metadata, m => new {
                m!.ApplicationName,
                m.ApplicationDescription,
                m.ServicePath,
                m.AllowedMethods
            } ),
            Methods = arr
        } );
    }

    /// <inheritdoc/>
    public byte [] ExportDocumentBytes ( JsonRpcDocumentation documentation ) {
        string json = EncodeDocumentation ( documentation ).ToString ();
        return Encoding.UTF8.GetBytes ( json );
    }

    U? Pipe<T, U> ( T value, Func<T, U> transform ) {
        if (value == null)
            return default;
        return transform ( value );
    }
}
