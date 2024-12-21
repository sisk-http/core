// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcErrorConverter.cs
// Repository:  https://github.com/sisk-http/core

using LightJson;
using LightJson.Converters;

namespace Sisk.JsonRPC.Converters;

internal class JsonRpcErrorConverter : JsonConverter {
    public override bool CanSerialize ( Type type, JsonOptions options ) {
        return type == typeof ( JsonRpcError );
    }

    public override object Deserialize ( JsonValue value, Type requestedType, JsonOptions options ) {
        throw new NotSupportedException ();
    }

    public override JsonValue Serialize ( object value, JsonOptions options ) {
        JsonRpcError error = (JsonRpcError) value;
        return new JsonObject () {
            [ "code" ] = error.Code,
            [ "message" ] = error.Message,
            [ "data" ] = error.Data
        };
    }
}
