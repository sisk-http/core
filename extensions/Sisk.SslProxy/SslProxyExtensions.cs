// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SslProxyExtensions.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http.Hosting;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Sisk.Ssl;

/// <summary>
/// Provides extension methods for <see cref="SslProxy"/>.
/// </summary>
public static class SslProxyExtensions
{
    /// <summary>
    /// Configures the <see cref="HttpServerHostContext"/> to use <see cref="SslProxy"/> with the specified parameters.
    /// </summary>
    /// <param name="builder">The <see cref="HttpServerHostContextBuilder"/> instance to configure.</param>
    /// <param name="sslListeningPort">The port number on which the server will listen for SSL/HTTPS connections.</param>
    /// <param name="certificate">The SSL/HTTPS certificate to use for encrypting communications.</param>
    /// <param name="allowedProtocols">The SSL/HTTPS protocols allowed for the connection. Defaults to <see cref="SslProtocols.Tls12"/> and <see cref="SslProtocols.Tls13"/>.</param>
    /// <param name="clientCertificateRequired">Specifies whether a client certificate is required for authentication. Defaults to <c>false</c>.</param>
    /// <returns>The configured <see cref="HttpServerHostContextBuilder"/> instance.</returns>
    public static HttpServerHostContextBuilder UseSsl(
        this HttpServerHostContextBuilder builder,
        short sslListeningPort,
        X509Certificate certificate,
        SslProtocols allowedProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
        bool clientCertificateRequired = false)
    {
        var endpoint = DnsUtil.ResolveEndpoint(builder.ServerConfiguration.ListeningHosts[0].Ports[0]);
        var secureProxy = new SslProxy(sslListeningPort, certificate, endpoint);
        var serverHandler = new SslProxyServerHandler(secureProxy);

        builder.UseHandler(serverHandler);
        builder.UseStartupMessage($"The SSL proxy is listening at:\n- https://localhost:{sslListeningPort}/");

        return builder;
    }
}
