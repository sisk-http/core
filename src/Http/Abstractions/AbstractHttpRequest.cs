// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   AbstractHttpRequest.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Sisk.Core.Http.Streams;

namespace Sisk.Core.Http.Abstractions;

public abstract class AbstractHttpServer {

}

public abstract class AbstractHttpContext {

    public abstract AbstractHttpRequest Request { get; }
    public abstract AbstractHttpResponse Response { get; }
    public abstract Task<AbstractWebSocketContext> AcceptWebSocketAsync ( string? subProtocol );
}

public abstract class AbstractWebSocketContext {
    public abstract WebSocketState State { get; }
    public abstract Task CloseOutputAsync ( WebSocketCloseStatus closeStatus, string? reason, CancellationToken cancellation );
    public abstract Task CloseAsync ( WebSocketCloseStatus closeStatus, string? reason, CancellationToken cancellation );
    public abstract ValueTask<WebSocketReceiveResult> ReceiveAsync ( Memory<byte> buffer, CancellationToken cancellationToken );
    public abstract ValueTask SendAsync ( ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken );
}

public abstract class AbstractHttpRequest {

    public abstract bool IsLocal { get; }
    public abstract string RawUrl { get; }
    public abstract NameValueCollection QueryString { get; }
    public abstract Version ProtocolVersion { get; }
    public abstract string UserHostName { get; }
    public abstract Uri Url { get; }
    public abstract string HttpMethod { get; }
    public abstract IPEndPoint LocalEndPoint { get; }
    public abstract IPEndPoint RemoteEndPoint { get; }
    public abstract Guid RequestTraceIdentifier { get; }
    public abstract WebHeaderCollection Headers { get; }
    public abstract Stream InputStream { get; }
    public abstract long ContentLength64 { get; }
    public abstract bool IsSecureConnection { get; }
    public abstract Encoding ContentEncoding { get; }
}

public abstract class AbstractHttpResponse : IDisposable {

    public abstract int StatusCode { get; set; }
    public abstract string StatusDescription { get; set; }
    public abstract bool KeepAlive { get; set; }
    public abstract bool SendChunked { get; set; }
    public abstract long ContentLength64 { get; set; }
    public abstract string? ContentType { get; set; }
    public abstract WebHeaderCollection Headers { get; set; }
    public abstract void AppendHeader ( string name, string value );
    public abstract void Abort ();
    public abstract void Close ();
    public abstract void Dispose ();
    public abstract Stream OutputStream { get; set; }
}
