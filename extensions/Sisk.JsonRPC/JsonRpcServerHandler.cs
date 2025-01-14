// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcServerHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Http.Handlers;
using Sisk.Core.Routing;

namespace Sisk.JsonRPC;

/// <summary>
/// Provides an <see cref="HttpServerHandler"/> for configuring the JSON-RPC handler
/// for the HTTP server.
/// </summary>
public sealed class JsonRpcServerHandler : HttpServerHandler {
    private JsonRpcHandler? _handler;

    /// <summary>
    /// Gets or sets the action which will be called in the configuring <see cref="Router"/>
    /// with this handler.
    /// </summary>
    public event EventHandler<JsonRpcServerConfigurationEventArgs>? ConfigureAction;

    /// <summary>
    /// Creates an new instance of the <see cref="JsonRpcServerHandler"/> class.
    /// </summary>
    public JsonRpcServerHandler () {
    }

    /// <inheritdoc/>
    protected override void OnServerStarting ( HttpServer server ) {
        this._handler = new JsonRpcHandler ( server );
    }

    /// <inheritdoc/>
    protected override void OnSetupRouter ( Router router ) {
        base.OnSetupRouter ( router );
        if (ConfigureAction != null && this._handler is not null)
            ConfigureAction ( this, new JsonRpcServerConfigurationEventArgs ( router, this._handler ) );
    }
}

/// <summary>
/// Represents the class which contains event data for the JSON-RPC configuration event.
/// </summary>
public sealed class JsonRpcServerConfigurationEventArgs : EventArgs {
    /// <summary>
    /// Gets the target <see cref="Sisk.Core.Routing.Router"/> which are being configured.
    /// </summary>
    public Router Router { get; }

    /// <summary>
    /// Gets the configuring <see cref="JsonRpcHandler"/>.
    /// </summary>
    public JsonRpcHandler Handler { get; }

    internal JsonRpcServerConfigurationEventArgs ( Router router, JsonRpcHandler handler ) {
        this.Router = router;
        this.Handler = handler;
    }
}