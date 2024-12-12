using LightJson;
using LightJson.Converters;

namespace Sisk.JsonRPC.Converters;

internal class JsonRpcErrorConverter : JsonConverter
{
    public override bool CanSerialize(Type type, JsonOptions options)
    {
        return type == typeof(JsonRpcError);
    }

    public override object Deserialize(JsonValue value, Type requestedType, JsonOptions options)
    {
        throw new NotSupportedException();
    }

    public override JsonValue Serialize(object value, JsonOptions options)
    {
        JsonRpcError error = (JsonRpcError)value;
        return new JsonObject()
        {
            ["code"] = error.Code,
            ["message"] = error.Message,
            ["data"] = error.Data
        };
    }
}
