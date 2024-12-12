using LightJson;

namespace Sisk.JsonRPC;

/// <summary>
/// Represents an JSON-RPC error.
/// </summary>
public readonly struct JsonRpcError
{
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
    public JsonRpcError()
    {
        Code = -32603;
        Message = "An exception was thrown.";
        Data = JsonValue.Null;
    }

    /// <summary>
    /// Creates an new instance of the <see cref="JsonRpcError"/> structure with given
    /// parameters.
    /// </summary>
    /// <param name="code">The JSON-RPC error code.</param>
    /// <param name="message">The JSON-RPC error message.</param>
    public JsonRpcError(int code, string message)
    {
        Code = code;
        Message = message;
        Data = JsonValue.Null;
    }

    /// <summary>
    /// Creates an new instance of the <see cref="JsonRpcError"/> structure with given
    /// parameters.
    /// </summary>
    /// <param name="code">The JSON-RPC error code.</param>
    /// <param name="message">The JSON-RPC error message.</param>
    /// <param name="data">The JSON-RPC error additional data.</param>
    public JsonRpcError(int code, string message, JsonValue data)
    {
        Code = code;
        Message = message;
        Data = data;
    }
}
