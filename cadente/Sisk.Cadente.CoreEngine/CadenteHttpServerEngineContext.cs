
// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CadenteHttpServerEngineContext.cs
// Repository:  https://github.com/sisk-http/core

using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using Sisk.Core.Http.Engine;

namespace Sisk.Cadente.CoreEngine {

    /// <summary>
    /// Represents the context for an HTTP request and response within the Cadente engine.
    /// </summary>
    public sealed class CadenteHttpServerEngineContext : HttpServerEngineContext {
        private readonly CadenteHttpServerEngineRequest _request;
        private readonly CadenteHttpServerEngineResponse _response;
        private readonly TaskCompletionSource<object?> _processingTcs = new ();

        /// <summary>
        /// Gets a task that represents the completion of the processing for this context.
        /// </summary>
        public Task ProcessingTask => _processingTcs.Task;

        internal void CompleteProcessing () => _processingTcs.TrySetResult ( null );

        /// <inheritdoc/>
        public CadenteHttpServerEngineContext ( CadenteHttpServerEngineRequest request, CadenteHttpServerEngineResponse response ) {
            _request = request;
            _response = response;
            _response.SetContext ( this );
        }

        /// <inheritdoc/>
        public override HttpServerEngineContextRequest Request => _request;

        /// <inheritdoc/>
        public override HttpServerEngineContextResponse Response => _response;

        /// <inheritdoc/>
        public override CancellationToken ContextAbortedToken => _request._context.Client.DisconnectToken;

        /// <inheritdoc/>
        public override Task<HttpServerEngineWebSocket> AcceptWebSocketAsync ( string? subProtocol ) {

            try {
                string? wsKey = _request._context.Request.Headers.Get ( "Sec-WebSocket-Key" ).FirstOrDefault ()
                ?? throw new InvalidOperationException ( "Missing 'Sec-WebSocket-Key' header in WebSocket upgrade request." );

                byte [] wsAcceptToken = SHA1.HashData ( Encoding.UTF8.GetBytes ( $"{wsKey}258EAFA5-E914-47DA-95CA-C5AB0DC85B11" ) );

                var underlyingResponse = _request._context.Response;

                underlyingResponse.StatusCode = 101;
                underlyingResponse.StatusDescription = "Switching Protocols";

                underlyingResponse.Headers.Set ( new HttpHeader ( "Connection", "Upgrade" ) );
                underlyingResponse.Headers.Set ( new HttpHeader ( "Upgrade", "websocket" ) );
                underlyingResponse.Headers.Set ( new HttpHeader ( "Sec-WebSocket-Accept", Convert.ToBase64String ( wsAcceptToken ) ) );
                underlyingResponse.Headers.Set ( new HttpHeader ( "Sec-WebSocket-Version", "13" ) );

                if (subProtocol is { Length: > 0 }) {

                    string [] clientSubProtocols = _request._context.Request.Headers.Get ( "Sec-WebSocket-Protocol" )
                        .SelectMany ( s => s.Split ( ",", StringSplitOptions.RemoveEmptyEntries ) )
                        .Select ( s => s.Trim () )
                        .ToArray ();

                    if (!clientSubProtocols.Contains ( subProtocol, StringComparer.Ordinal )) {

                        underlyingResponse.StatusCode = 426;
                        underlyingResponse.StatusDescription = "Upgrade Required";

                        throw new InvalidOperationException ( $"The requested sub-protocol '{subProtocol}' is not supported by the client." );
                    }

                    underlyingResponse.Headers.Set ( new HttpHeader ( "Sec-WebSocket-Protocol", subProtocol ) );
                }

                Stream underlyingStream = underlyingResponse.GetResponseStream ( chunked: false );

                var ws = WebSocket.CreateFromStream ( underlyingStream, new WebSocketCreationOptions () {
                    IsServer = true,
                    SubProtocol = subProtocol,
                    KeepAliveInterval = TimeSpan.FromSeconds ( 20 )
                } );

                return Task.FromResult ( HttpServerEngineWebSocket.CreateFromWebSocket ( ws ) );
            }
            catch (Exception ex) {
                throw new Sisk.Core.Http.HttpRequestException("Failed to enter WebSocket context. See inner exception.", ex);
            }
        }
    }
}
