using LightJson;

namespace Sisk.JsonRPC;

/// <summary>
/// Represents an error that occur during the JSON-RPC application
/// execution.
/// </summary>
public class JsonRpcException : Exception
{
    /// <summary>
    /// Gets the error code associated with the JSON-RPC error.
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// Gets additional data associated with the error, if any.
    /// </summary>
    public new object? Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonRpcException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public JsonRpcException(string message) : base(message)
    {
        Code = 1;
        Data = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonRpcException"/> class
    /// with a specified error message, error code, and additional data.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="code">The error code associated with the JSON-RPC error.</param>
    /// <param name="data">Additional data associated with the error.</param>
    public JsonRpcException(string message, int code, object? data) : base(message)
    {
        Code = code;
        Data = data;
    }

    /// <summary>
    /// Converts the current <see cref="JsonRpcException"/> into a <see cref="JsonRpcError"/>.
    /// </summary>
    /// <returns>A <see cref="JsonRpcError"/> representing the error details.</returns>
    public JsonRpcError AsRpcError()
    {
        return new JsonRpcError(Code, Message, JsonValue.Serialize(Data));
    }
}
