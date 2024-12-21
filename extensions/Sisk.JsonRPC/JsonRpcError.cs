// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcError.cs
// Repository:  https://github.com/sisk-http/core

using LightJson;

namespace Sisk.JsonRPC;

/// <summary>
/// Represents an JSON-RPC error.
/// </summary>
public readonly struct JsonRpcError {
    /// <summary>
    /// Gets the JSON-RPC error code.
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// Gets the JSON-RPC error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the JSON-RPC error additional data.
    /// </summary>
    public JsonValue Data { get; }

    /// <summary>
    /// Creates an new instance of the <see cref="JsonRpcError"/> structure.
    /// </summary>
    public JsonRpcError () {
        this.Code = -32603;
        this.Message = "An exception was thrown.";
        this.Data = JsonValue.Null;
    }

    /// <summary>
    /// Creates an new instance of the <see cref="JsonRpcError"/> structure with given
    /// parameters.
    /// </summary>
    /// <param name="code">The JSON-RPC error code.</param>
    /// <param name="message">The JSON-RPC error message.</param>
    public JsonRpcError ( int code, string message ) {
        this.Code = code;
        this.Message = message;
        this.Data = JsonValue.Null;
    }

    /// <summary>
    /// Creates an new instance of the <see cref="JsonRpcError"/> structure with given
    /// parameters.
    /// </summary>
    /// <param name="code">The JSON-RPC error code.</param>
    /// <param name="message">The JSON-RPC error message.</param>
    /// <param name="data">The JSON-RPC error additional data.</param>
    public JsonRpcError ( int code, string message, JsonValue data ) {
        this.Code = code;
        this.Message = message;
        this.Data = data;
    }
}
