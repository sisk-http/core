// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpListenerAbstractEngine.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Http.Engine;

/// <summary>
/// Provides an implementation of <see cref="HttpServerEngine"/> using <see cref="HttpListener"/>.
/// </summary>
public sealed class HttpListenerAbstractEngine : HttpServerEngine {
    private HttpListener _listener;
    private static Lazy<HttpListenerAbstractEngine> shared = new Lazy<HttpListenerAbstractEngine> ( () => new HttpListenerAbstractEngine () );

    /// <summary>
    /// Gets the shared instance of the <see cref="HttpListenerAbstractEngine"/> class.
    /// </summary>
    public static HttpListenerAbstractEngine Shared => shared.Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpListenerAbstractEngine"/> class.
    /// </summary>
    public HttpListenerAbstractEngine () {
        _listener = new HttpListener {
            IgnoreWriteExceptions = true
        };
    }

    /// <inheritdoc/>
    public override TimeSpan IdleConnectionTimeout {
        get => _listener.TimeoutManager.IdleConnection;
        set => _listener.TimeoutManager.IdleConnection = value;
    }

    /// <inheritdoc/>
    public override void AddListeningPrefix ( string prefix ) {
        _listener.Prefixes.Add ( prefix );
    }

    /// <inheritdoc/>
    public override IAsyncResult BeginGetContext ( AsyncCallback? callback, object? state ) {
        return _listener.BeginGetContext ( callback, state );
    }

    /// <inheritdoc/>
    public override HttpServerEngineContext EndGetContext ( IAsyncResult asyncResult ) {
        var context = _listener.EndGetContext ( asyncResult );
        return new HttpListenerContextAbstraction ( context );
    }

    /// <inheritdoc/>
    public override void ClearPrefixes () {
        _listener.Prefixes.Clear ();
    }

    /// <inheritdoc/>
    public override void Dispose () {
        ((IDisposable) _listener).Dispose ();
    }

    /// <inheritdoc/>
    public override void StartServer () {
        _listener.Start ();
    }

    /// <inheritdoc/>
    public override void StopServer () {
        _listener.Stop ();
    }

    sealed class HttpListenerContextAbstraction ( HttpListenerContext context ) : HttpServerEngineContext {

        HttpListenerContext _context = context;

        public override HttpServerEngineContextRequest Request => new HttpListenerContextRequestAbstraction ( _context );

        public override HttpServerEngineContextResponse Response => new HttpListenerContextResponseAbstraction ( _context );

        public override CancellationToken ContextAbortedToken => throw new HttpEngineException ( new NotSupportedException () );

        public override async Task<HttpServerEngineWebSocket> AcceptWebSocketAsync ( string? subProtocol ) {
            var ws = await _context.AcceptWebSocketAsync ( subProtocol ).ConfigureAwait ( false );
            return new HttpListenerContextWebSocketAbstraction ( ws.WebSocket );
        }
    }

    sealed class HttpListenerContextRequestAbstraction ( HttpListenerContext context ) : HttpServerEngineContextRequest {

        readonly HttpListenerRequest _request = context.Request;

        public override bool IsLocal => _request.IsLocal;
        public override string? RawUrl => _request.RawUrl ?? "/";
        public override NameValueCollection QueryString => _request.QueryString;
        public override Version ProtocolVersion => _request.ProtocolVersion;
        public override string UserHostName => _request.UserHostName;
        public override Uri? Url => _request.Url;
        public override string HttpMethod => _request.HttpMethod;
        public override IPEndPoint LocalEndPoint => _request.LocalEndPoint;
        public override IPEndPoint RemoteEndPoint => _request.RemoteEndPoint;
        public override Guid RequestTraceIdentifier => _request.RequestTraceIdentifier;
        public override NameValueCollection Headers => _request.Headers;
        public override Stream InputStream => _request.InputStream;
        public override long ContentLength64 => _request.ContentLength64;
        public override bool IsSecureConnection => _request.IsSecureConnection;
        public override Encoding ContentEncoding => _request.ContentEncoding;
    }

    sealed class HttpListenerContextResponseAbstraction ( HttpListenerContext context ) : HttpServerEngineContextResponse {

        readonly HttpListenerResponse derived = context.Response;

        public override int StatusCode { get => derived.StatusCode; set => derived.StatusCode = value; }
        public override string StatusDescription { get => derived.StatusDescription; set => derived.StatusDescription = value; }
        public override bool KeepAlive { get => derived.KeepAlive; set => derived.KeepAlive = value; }
        public override bool SendChunked { get => derived.SendChunked; set => derived.SendChunked = value; }
        public override long ContentLength64 { get => derived.ContentLength64; set => derived.ContentLength64 = value; }
        public override string? ContentType { get => derived.ContentType; set => derived.ContentType = value; }
        public override WebHeaderCollection Headers { get => derived.Headers; set => derived.Headers = value; }
        public override Stream OutputStream { get => derived.OutputStream; }

        public override void Abort () {
            derived.Abort ();
        }

        public override void AppendHeader ( string name, string value ) {
            derived.AppendHeader ( name, value );
        }

        public override void Close () {
            derived.Close ();
        }

        public override void Dispose () {
            (derived as IDisposable)?.Dispose ();
        }
    }

    sealed class HttpListenerContextWebSocketAbstraction ( WebSocket ws ) : HttpServerEngineWebSocket {
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
}
