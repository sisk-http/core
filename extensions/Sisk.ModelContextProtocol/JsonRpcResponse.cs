// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcResponse.cs
// Repository:  https://github.com/sisk-http/core

using System.Text.Json.Serialization;

namespace Sisk.ModelContextProtocol;

sealed class JsonRpcResponse {

    [JsonPropertyName ( "jsonrpc" )]
    public string Version => "2.0";

    [JsonPropertyName ( "result" )]
    public JsonValue Result { get; }

    [JsonPropertyName ( "id" )]
    public JsonValue Id { get; }

    public JsonRpcResponse ( JsonValue result, JsonValue id ) {
        Result = result;
        Id = id;
    }
}

sealed class JsonRpcErrorResponse {

    [JsonPropertyName ( "jsonrpc" )]
    public string Version => "2.0";

    [JsonPropertyName ( "error" )]
    public JsonObject Error { get; }

    [JsonPropertyName ( "id" )]
    [JsonIgnore ( Condition = JsonIgnoreCondition.WhenWritingNull )]
    public JsonValue? Id { get; }

    public JsonRpcErrorResponse ( int code, string message, JsonValue? id, JsonValue? data = null ) {
        Error = new JsonObject () {
            [ "code" ] = code,
            [ "message" ] = message
        };
        if (data is { } errorData)
            Error [ "data" ] = errorData;
        Id = id;
    }
}

class McpJsonResponse : HttpResponse {
    public McpJsonResponse ( object obj, string? sessionId ) : base ( System.Net.HttpStatusCode.OK ) {
        Headers [ HttpKnownHeaderNames.ContentType ] = "application/json; charset=utf-8";
        if (sessionId is { })
            Headers [ "Mcp-Session-Id" ] = sessionId;
        Content = new ByteArrayContent ( McpProvider.Json.SerializeUtf8Bytes ( obj ) );
    }
}
