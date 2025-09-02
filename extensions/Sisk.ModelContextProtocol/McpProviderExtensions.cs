// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   McpProviderExtensions.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sisk.Core.Http.Hosting;

namespace Sisk.ModelContextProtocol;

/// <summary>
/// Provides extension methods for configuring and handling requests with the Model Context Protocol (MCP).
/// </summary>
public static class McpProviderExtensions {

    internal static McpProvider? singleton = null;

    /// <summary>
    /// Configures the HTTP server host builder to use a specific MCP provider.
    /// </summary>
    /// <param name="builder">The HTTP server host builder to configure.</param>
    /// <param name="provider">The MCP provider to use.</param>
    /// <returns>The configured HTTP server host builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provider is null.</exception>
    public static HttpServerHostContextBuilder UseMcp ( this HttpServerHostContextBuilder builder, McpProvider provider ) {
        ArgumentNullException.ThrowIfNull ( provider );
        singleton = provider;
        return builder;
    }

    /// <summary>
    /// Configures the HTTP server host builder to use an MCP provider built with the provided action.
    /// </summary>
    /// <param name="builder">The HTTP server host builder to configure.</param>
    /// <param name="providerBuilder">An action to configure the MCP provider.</param>
    /// <returns>The configured HTTP server host builder.</returns>
    public static HttpServerHostContextBuilder UseMcp ( this HttpServerHostContextBuilder builder, Action<McpProvider> providerBuilder ) {
        singleton = new McpProvider ();
        providerBuilder ( singleton );
        return builder;
    }

    /// <summary>
    /// Handles an incoming HTTP request using the configured MCP provider.
    /// </summary>
    /// <param name="request">The HTTP request to handle.</param>
    /// <param name="cancellation">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the HTTP response.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the MCP provider has not been configured.</exception>
    public static Task<HttpResponse> HandleMcpRequestAsync ( this HttpRequest request, CancellationToken cancellation = default ) {
        if (singleton == null)
            throw new InvalidOperationException ( "MCP provider is not configured. Please call UseMcp() on the HttpServerHost builder first." );
        return singleton.HandleRequestAsync ( request, cancellation );
    }
}