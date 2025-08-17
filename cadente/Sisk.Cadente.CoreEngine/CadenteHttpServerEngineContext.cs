
// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CadenteHttpServerEngineContext.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        internal void CompleteProcessing () => _processingTcs.SetResult ( null );

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
            // Sisk.Cadente does not support WebSockets yet.
            throw new NotSupportedException ( "Sisk.Cadente does not support WebSockets yet." );
        }
    }
}
