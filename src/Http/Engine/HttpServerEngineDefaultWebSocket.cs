// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerEngineDefaultWebSocket.cs
// Repository:  https://github.com/sisk-http/core

using System.Net.WebSockets;

namespace Sisk.Core.Http.Engine;

sealed class HttpServerEngineDefaultWebSocket ( WebSocket ws ) : HttpServerEngineWebSocket {
    readonly WebSocket _ws = ws;

    public override WebSocketState State => _ws.State;

    public override Task CloseAsync ( WebSocketCloseStatus closeStatus, string? reason, CancellationToken cancellation ) {
        return _ws.CloseAsync ( closeStatus, reason, cancellation );
    }

    public override Task CloseOutputAsync ( WebSocketCloseStatus closeStatus, string? reason, CancellationToken cancellation ) {
        return _ws.CloseOutputAsync ( closeStatus, reason, cancellation );
    }

    public override async ValueTask<ValueWebSocketReceiveResult> ReceiveAsync ( Memory<byte> buffer, CancellationToken cancellationToken ) {
        return await _ws.ReceiveAsync ( buffer, cancellationToken ).ConfigureAwait ( false );
    }

    public override async ValueTask SendAsync ( ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken ) {
        await _ws.SendAsync ( buffer, messageType, endOfMessage, cancellationToken ).ConfigureAwait ( false );
    }
}
