using LightJson;
using Sisk.JsonRPC.Converters;

namespace Sisk.JsonRPC;

/// <summary>
/// Represents a handler for JSON-RPC requests.
/// </summary>
public sealed class JsonRpcHandler
{
    internal readonly JsonOptions _jsonOptions;
    readonly JsonRpcMethodCollection _methodCollection;
    readonly RpcTransportLayer _transport;

    /// <summary>
    /// Gets the transport layer used for communication.
    /// </summary>
    public RpcTransportLayer Transport { get => _transport; }

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
    public JsonRpcHandler()
    {
        _transport = new RpcTransportLayer(this);
        _jsonOptions = new JsonOptions();
        _methodCollection = new JsonRpcMethodCollection();

        _jsonOptions.PropertyNameComparer = new JsonSanitizedComparer();
        _jsonOptions.Converters.Add(new JsonRpcErrorConverter());
        _jsonOptions.Converters.Add(new JsonRpcRequestConverter());
        _jsonOptions.Converters.Add(new JsonRpcResponseConverter());
    }
}
