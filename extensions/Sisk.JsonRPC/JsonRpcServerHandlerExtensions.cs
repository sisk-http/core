// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcServerHandlerExtensions.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http.Hosting;

namespace Sisk.JsonRPC;

/// <summary>
/// Provides extensions methods for the HTTP server and JSON-RPC handler.
/// </summary>
public static class JsonRpcServerHandlerExtensions {
    /// <summary>
    /// Enables JSON-RPC in this HTTP server.
    /// </summary>
    /// <param name="builder">The self <see cref="HttpServerHostContextBuilder"/> for fluent chaining.</param>
    /// <param name="configure">The event handler callback that is called to configure routes and web methods for the JSON-RPC.</param>
    public static HttpServerHostContextBuilder UseJsonRPC ( this HttpServerHostContextBuilder builder, EventHandler<JsonRpcServerConfigurationEventArgs> configure ) {
        JsonRpcServerHandler serverHandler = new JsonRpcServerHandler ();
        serverHandler.ConfigureAction += configure;

        builder.UseHandler ( serverHandler );
        return builder;
    }
}
