// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcRequest.cs
// Repository:  https://github.com/sisk-http/core

using LightJson;

namespace Sisk.JsonRPC;

/// <summary>
/// Represents an JSON-RPC request message.
/// </summary>
public sealed class JsonRpcRequest {
    /// <summary>
    /// Gets the version used in the JSON-RPC message.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the method name of the JSON-RPC message.
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// Gets the <see cref="JsonValue"/> containing the message parameter values.
    /// </summary>
    public JsonValue Parameters { get; }

    /// <summary>
    /// Gets the ID of the JSON-RPC message.
    /// </summary>
    public JsonValue Id { get; }

    internal JsonRpcRequest ( string method, JsonValue parameters, string id ) {
        this.Version = "2.0";
        this.Method = method;
        this.Parameters = parameters;
        this.Id = id;

        if (string.IsNullOrEmpty ( this.Method ))
            throw new JsonRpcException ( "The JSON-RPC request Method cannot be null or an empty string." );
        if (this.Parameters.Type is not JsonValueType.Array &&
            this.Parameters.Type is not JsonValueType.Object) {
            throw new JsonRpcException ( "The JSON-RPC request parameters must be an array or an object." );
        }
    }

    internal JsonRpcRequest ( JsonObject rpcMessage ) {
        this.Version = rpcMessage [ "jsonrpc" ].GetString ();
        this.Method = rpcMessage [ "Method" ].GetString ();
        this.Parameters = rpcMessage [ "params" ];
        this.Id = rpcMessage [ "id" ];

        if (this.Id.Type != JsonValueType.String && this.Id.Type != JsonValueType.Number && this.Id.Type != JsonValueType.Null && this.Id.Type != JsonValueType.Undefined) {
            throw new JsonRpcException ( "The JSON-RPC request id must be an string, number or null." );
        }

        if (this.Version != "2.0")
            throw new JsonRpcException ( "The JSON-RPC request version must be \"2.0\"." );
        if (string.IsNullOrEmpty ( this.Method ))
            throw new JsonRpcException ( "The JSON-RPC request Method cannot be null or an empty string." );
        if (this.Parameters.Type is not JsonValueType.Array &&
            this.Parameters.Type is not JsonValueType.Object) {
            throw new JsonRpcException ( "The JSON-RPC request parameters must be an array or an object." );
        }
    }
}
