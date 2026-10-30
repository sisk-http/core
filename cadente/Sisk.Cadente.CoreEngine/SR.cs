// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SR.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.Cadente.CoreEngine;

internal static class SR {
    public const string CadenteHttpServerEngine_DnsResolveFailed = "Failed to resolve DNS for host {0}.";
    public const string CadenteHttpServerEngine_FailedToEnqueueContext = "Failed to enqueue HTTP context.";
    public const string CadenteHttpServerEngine_NotConfigured = "The Cadente engine has not been configured.";

    public const string CadenteHttpServerEngineContext_MissingWebSocketKey = "Missing 'Sec-WebSocket-Key' header in WebSocket upgrade request.";
    public const string CadenteHttpServerEngineContext_SubProtocolNotSupported = "The requested sub-protocol '{0}' is not supported by the client.";
    public const string CadenteHttpServerEngineContext_WebSocketEnterFailed = "Failed to enter WebSocket context. See inner exception.";

    public const string CadenteHttpServerEngineRequest_InvalidHostHeader = "Invalid Host header.";
    public const string CadenteHttpServerEngineRequest_InvalidRequestTarget = "Invalid request target.";

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static string Format ( string format, params object? [] items ) {
        return String.Format ( provider: null, format, items );
    }
}
