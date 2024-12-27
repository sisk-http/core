// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcDocumentation.cs
// Repository:  https://github.com/sisk-http/core

using LightJson;

namespace Sisk.JsonRPC.Documentation;

/// <summary>
/// Represents the documentation for JSON-RPC methods.
/// </summary>
public sealed class JsonRpcDocumentation {
    /// <summary>
    /// Gets the collection of JSON-RPC methods.
    /// </summary>
    public JsonRpcDocumentationMethod [] Methods { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonRpcDocumentation"/> class with the specified methods.
    /// </summary>
    /// <param name="methods">The array of JSON-RPC methods.</param>
    internal JsonRpcDocumentation ( JsonRpcDocumentationMethod [] methods ) {
        this.Methods = methods;
    }

    /// <summary>
    /// Exports this <see cref="JsonRpcDocumentation"/> with the specified <see cref="IJsonRpcDocumentationExporter"/>.
    /// </summary>
    /// <param name="exporter">The <see cref="IJsonRpcDocumentationExporter"/> instance.</param>
    public byte [] Export ( IJsonRpcDocumentationExporter exporter ) {
        return exporter.ExportDocumentBytes ( this );
    }

    /// <summary>
    /// Gets an JSON string representation of this <see cref="JsonRpcDocumentation"/>.
    /// </summary>
    /// <param name="options">The <see cref="JsonOptions"/> used to encode this documentation.</param>
    public string ExportToJson ( JsonOptions options ) {
        return new JsonRpcJsonExport ( options ).EncodeDocumentation ( this ).ToString ();
    }

    /// <summary>
    /// Gets an JSON string representation of this <see cref="JsonRpcDocumentation"/>.
    /// </summary>
    public string ExportToJson () => this.ExportToJson ( JsonOptions.Default );
}

/// <summary>
/// Represents the documentation for a single JSON-RPC method.
/// </summary>
public sealed class JsonRpcDocumentationMethod {
    /// <summary>
    /// Gets the name of the JSON-RPC method.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Gets the return type of the JSON-RPC method.
    /// </summary>
    public Type ReturnType { get; }

    /// <summary>
    /// Gets the category of the JSON-RPC method.
    /// </summary>
    public string? Category { get; }

    /// <summary>
    /// Gets the description of the JSON-RPC method.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the parameters of this JSON-RPC method.
    /// </summary>
    public JsonRpcDocumentationParameter [] Parameters { get; }

    internal JsonRpcDocumentationMethod ( string methodName, string? category, string? description, Type returnType, JsonRpcDocumentationParameter [] parameters ) {
        this.MethodName = methodName;
        this.Category = category;
        this.Description = description;
        this.ReturnType = returnType;
        this.Parameters = parameters;
    }
}

/// <summary>
/// Represents the documentation for a parameter of a JSON-RPC method.
/// </summary>
public sealed class JsonRpcDocumentationParameter {
    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// Gets the type of the parameter.
    /// </summary>
    public Type ParameterType { get; }

    /// <summary>
    /// Gets the description of the parameter.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets a value indicating whether the parameter is optional.
    /// </summary>
    public bool IsOptional { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonRpcDocumentationParameter"/> class with the specified details.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="parameterType">The type of the parameter.</param>
    /// <param name="description">The description of the parameter.</param>
    /// <param name="isOptional">Indicates whether the parameter is optional.</param>
    internal JsonRpcDocumentationParameter ( string parameterName, Type parameterType, string? description, bool isOptional ) {
        this.ParameterName = parameterName;
        this.ParameterType = parameterType;
        this.Description = description;
        this.IsOptional = isOptional;
    }
}
