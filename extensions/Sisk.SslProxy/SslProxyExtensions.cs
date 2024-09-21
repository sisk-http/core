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
using System.Text;

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
    /// <param name="certificate">Optional. The SSL/HTTPS certificate to use for encrypting communications.</param>
    /// <param name="allowedProtocols">Optional. The SSL/HTTPS protocols allowed for the connection. Defaults to <see cref="SslProtocols.Tls12"/> and <see cref="SslProtocols.Tls13"/>.</param>
    /// <param name="clientCertificateRequired">Optional. Specifies whether a client certificate is required for authentication. Defaults to <c>false</c>.</param>
    /// <param name="proxyAuthorization">Optional. Specifies the Proxy-Authorization header value for creating an trusted gateway between
    /// the application and the proxy.</param>
    /// <returns>The configured <see cref="HttpServerHostContextBuilder"/> instance.</returns>
    public static HttpServerHostContextBuilder UseSsl(
        this HttpServerHostContextBuilder builder,
        short sslListeningPort,
        X509Certificate? certificate = null,
        SslProtocols allowedProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
        bool clientCertificateRequired = false,
        object? proxyAuthorization = null)
    {
        var primaryHost = builder.ServerConfiguration.ListeningHosts[0];
        var primaryPort = primaryHost.Ports[0];
        var usableHosts = primaryHost.Ports.Select(p => p.Hostname);

        var endpoint = DnsUtil.ResolveEndpoint(primaryPort);
        if (certificate is null)
        {
            certificate = CertificateUtil.CreateTrustedDevelopmentCertificate(["localhost", .. usableHosts]);
        }

        var secureProxy = new SslProxy(sslListeningPort, certificate, endpoint);
        secureProxy.GatewayHostname = primaryPort.Hostname;
        secureProxy.ProxyAuthorization = proxyAuthorization?.ToString();

        var serverHandler = new SslProxyServerHandler(secureProxy);
        builder.UseHandler(serverHandler);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("The development SSL proxy is listening at:");
        foreach (var usableHost in usableHosts)
        {
            sb.AppendLine($"- https://{usableHost}:{sslListeningPort}/");
        }

        builder.UseStartupMessage(sb.ToString());

        return builder;
    }
}
