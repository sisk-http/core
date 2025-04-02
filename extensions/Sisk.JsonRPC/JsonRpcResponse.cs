// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcResponse.cs
// Repository:  https://github.com/sisk-http/core

using LightJson;

namespace Sisk.JsonRPC;

/// <summary>
/// Represents an JSON-RPC response message.
/// </summary>
public sealed class JsonRpcResponse {
    /// <summary>
    /// Gets the JSON-RPC response version. This property will always return "2.0".
    /// </summary>
    public string Version { get; } = "2.0";

    /// <summary>
    /// Gets the JSON-RPC response result.
    /// </summary>
    public JsonValue? Result { get; }

    /// <summary>
    /// Gets the JSON-RPC response error.
    /// </summary>
    public JsonRpcError? Error { get; }

    /// <summary>
    /// Gets the JSON-RPC response id.
    /// </summary>
    public JsonValue Id { get; }

    internal JsonRpcResponse ( JsonValue? result, JsonRpcError? error, JsonValue id ) {
        Result = result;
        Error = error;
        if (id.Type == JsonValueType.Undefined) {
            Id = JsonValue.Null;
        }
        else {
            Id = id;
        }
    }

    /// <summary>
    /// Creates an new success <see cref="JsonRpcResponse"/> with given parameters.
    /// </summary>
    /// <param name="id">The JSON-RPC response id.</param>
    /// <param name="result">The JSON-RPC response object.</param>
    public static JsonRpcResponse CreateSuccessResponse ( JsonValue id, JsonValue result ) {
        return new JsonRpcResponse ( result, null, id );
    }

    /// <summary>
    /// Creates an new error <see cref="JsonRpcResponse"/> with given parameters.
    /// </summary>
    /// <param name="id">The JSON-RPC response id.</param>
    /// <param name="error">The JSON-RPC response error.</param>
    public static JsonRpcResponse CreateErrorResponse ( JsonValue id, JsonRpcError error ) {
        return new JsonRpcResponse ( null, error, id );
    }
}
