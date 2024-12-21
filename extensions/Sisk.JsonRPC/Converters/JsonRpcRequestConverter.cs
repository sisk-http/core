// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcRequestConverter.cs
// Repository:  https://github.com/sisk-http/core

using LightJson;
using LightJson.Converters;

namespace Sisk.JsonRPC.Converters;

internal class JsonRpcRequestConverter : JsonConverter {
    public override bool CanSerialize ( Type type, JsonOptions options ) {
        return type == typeof ( JsonRpcRequest );
    }

    public override object Deserialize ( JsonValue value, Type requestedType, JsonOptions options ) {
        return new JsonRpcRequest ( value.GetJsonObject () );
    }

    public override JsonValue Serialize ( object value, JsonOptions options ) {
        throw new NotSupportedException ();
    }
}
