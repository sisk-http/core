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
        Version = "2.0";
        Method = method;
        Parameters = parameters;
        Id = id;

        if (string.IsNullOrEmpty ( Method ))
            throw new JsonRpcException ( SR.JsonRpcRequest_MethodEmpty );
        if (Parameters.Type is not JsonValueType.Array &&
            Parameters.Type is not JsonValueType.Object) {
            throw new JsonRpcException ( SR.JsonRpcRequest_ParametersInvalid );
        }
    }

    internal JsonRpcRequest ( JsonObject rpcMessage ) {
        Version = rpcMessage [ "jsonrpc" ].GetString ();
        Method = rpcMessage [ "Method" ].GetString ();
        Parameters = rpcMessage [ "params" ];
        Id = rpcMessage [ "id" ];

        if (Id.Type != JsonValueType.String && Id.Type != JsonValueType.Number && Id.Type != JsonValueType.Null && Id.Type != JsonValueType.Undefined) {
            throw new JsonRpcException ( SR.JsonRpcRequest_IdInvalid );
        }

        if (Version != "2.0")
            throw new JsonRpcException ( SR.JsonRpcRequest_VersionInvalid );
        if (string.IsNullOrEmpty ( Method ))
            throw new JsonRpcException ( SR.JsonRpcRequest_MethodEmpty );
        if (Parameters.Type is not JsonValueType.Array &&
            Parameters.Type is not JsonValueType.Object) {
            throw new JsonRpcException ( SR.JsonRpcRequest_ParametersInvalid );
        }
    }
}
