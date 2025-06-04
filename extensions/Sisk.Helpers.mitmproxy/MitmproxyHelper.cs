// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MitmproxyHelper.cs
// Repository:  https://github.com/sisk-http/core

using Asmichi.ProcessManagement;
using Sisk.Core.Http.Hosting;
using Sisk.Helpers.Mitmproxy;

namespace Sisk.Helpers.mitmproxy;

/// <summary>
/// Provides extension methods for configuring an HTTP server to use mitmproxy.
/// </summary>
public static class MitmproxyHelper {
    /// <summary>
    /// Configures the specified <see cref="HttpServerHostContextBuilder"/> to use mitmproxy with a random proxy port.
    /// </summary>
    /// <param name="builder">The <see cref="HttpServerHostContextBuilder"/> instance to configure.</param>
    /// <returns>The updated <see cref="HttpServerHostContextBuilder"/> instance.</returns>
    public static HttpServerHostContextBuilder UseMitmproxy ( this HttpServerHostContextBuilder builder ) => UseMitmproxy ( builder, proxyPort: 0 );

    /// <summary>
    /// Configures the specified <see cref="HttpServerHostContextBuilder"/> to use mitmproxy with the specified options.
    /// </summary>
    /// <param name="builder">The <see cref="HttpServerHostContextBuilder"/> instance to configure.</param>
    /// <param name="proxyPort">The port on which the mitmproxy will listen.</param>
    /// <param name="silent">Indicates whether the mitmproxy should run in silent mode. Default is false.</param>
    /// <param name="setupAction">An optional action to configure the child process start information.</param>
    /// <returns>The updated <see cref="HttpServerHostContextBuilder"/> instance.</returns>
    public static HttpServerHostContextBuilder UseMitmproxy ( this HttpServerHostContextBuilder builder,
        ushort proxyPort = 0,
        bool silent = false,
        Action<ChildProcessStartInfo>? setupAction = null ) {
        var proxy = new MitmproxyProvider ( proxyPort, setupAction ) {
            Silent = silent
        };

        builder.UseHandler ( proxy );
        builder.UseStartupMessage ( () => $"""
            The mitmproxy is listening at:
            - https://localhost:{proxy.ProxyPort}/
            """ );

        return builder;
    }
}
