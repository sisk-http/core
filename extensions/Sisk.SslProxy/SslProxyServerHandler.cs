// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SslProxyServerHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Http.Handlers;

namespace Sisk.Ssl;

/// <summary>
/// Provides event handlers and hooks for <see cref="SslProxy"/>.
/// </summary>
public sealed class SslProxyServerHandler : HttpServerHandler
{
    /// <summary>
    /// Gets the <see cref="SslProxy"/> instance used in this server handler.
    /// </summary>
    public SslProxy SecureProxy { get; }

    /// <summary>
    /// Creates an new <see cref="SslProxyServerHandler"/> instance with the
    /// specified <see cref="SslProxy"/> instance.
    /// </summary>
    /// <param name="secureProxy">The <see cref="SslProxy"/> instance.</param>
    public SslProxyServerHandler(SslProxy secureProxy)
    {
        this.SecureProxy = secureProxy;
    }

    /// <exclude/>
    /// <inheritdoc/>
    protected override void OnServerStarting(HttpServer server)
    {
        this.SecureProxy.Start();
    }

    /// <exclude/>
    /// <inheritdoc/>
    protected override void OnServerStopping(HttpServer server)
    {
        this.SecureProxy.Dispose();
    }
}
