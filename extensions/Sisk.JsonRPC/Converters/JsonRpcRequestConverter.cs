using LightJson;
using LightJson.Converters;

namespace Sisk.JsonRPC.Converters;

internal class JsonRpcRequestConverter : JsonConverter
{
    public override bool CanSerialize(Type type, JsonOptions options)
    {
        return type == typeof(JsonRpcRequest);
    }

    public override object Deserialize(JsonValue value, Type requestedType, JsonOptions options)
    {
        return new JsonRpcRequest(value.GetJsonObject());
    }

    public override JsonValue Serialize(object value, JsonOptions options)
    {
        throw new NotSupportedException();
    }
}
