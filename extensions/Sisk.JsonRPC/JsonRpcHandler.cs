// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcHandler.cs
// Repository:  https://github.com/sisk-http/core

using LightJson;
using Sisk.Core.Http;
using Sisk.JsonRPC.Converters;
using Sisk.JsonRPC.Documentation;

namespace Sisk.JsonRPC;

/// <summary>
/// Represents a handler for JSON-RPC requests.
/// </summary>
public sealed class JsonRpcHandler {
    internal readonly JsonOptions _jsonOptions;
    internal readonly HttpServer _server;
    readonly JsonRpcMethodCollection _methodCollection;
    readonly JsonRpcTransportLayer _transport;

    /// <summary>
    /// Gets the transport layer used for communication.
    /// </summary>
    public JsonRpcTransportLayer Transport { get => _transport; }

    /// <summary>
    /// Gets the collection of JSON-RPC methods available in this handler.
    /// </summary>
    public JsonRpcMethodCollection Methods { get => _methodCollection; }

    /// <summary>
    /// Gets the JSON serializer options used for serialization and deserialization.
    /// </summary>
    public JsonOptions JsonSerializerOptions { get => _jsonOptions; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonRpcHandler"/> class.
    /// </summary>
    /// <param name="parentServer">Defines the <see cref="HttpServer"/> where the JSON-RPC instance will run on.</param>
    public JsonRpcHandler ( HttpServer parentServer ) {
        _transport = new JsonRpcTransportLayer ( this );
        _jsonOptions = new JsonOptions ();
        _methodCollection = new JsonRpcMethodCollection ();
        _server = parentServer;

        _jsonOptions.PropertyNameComparer = new JsonSanitizedComparer ();
        _jsonOptions.Converters.Add ( new JsonRpcErrorConverter () );
        _jsonOptions.Converters.Add ( new JsonRpcRequestConverter () );
        _jsonOptions.Converters.Add ( new JsonRpcResponseConverter () );
    }

    /// <summary>
    /// Gets the documentation for this JSON-RPC handler.
    /// </summary>
    public JsonRpcDocumentation GetDocumentation () => GetDocumentation ( null! );

    /// <summary>
    /// Gets the documentation for this JSON-RPC handler.
    /// </summary>
    /// <param name="metadata">The <see cref="JsonRpcDocumentationMetadata"/> to generate in the documentation.</param>
    public JsonRpcDocumentation GetDocumentation ( JsonRpcDocumentationMetadata metadata ) {
        return DocumentationDescriptor.GetDocumentationDescriptor ( this, metadata );
    }
}
