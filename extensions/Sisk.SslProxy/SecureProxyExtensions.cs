// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SecureProxyExtensions.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Ssl;

public static class SecureProxyExtensions
{
    public static HttpServerHostContextBuilder UseSsl(this HttpServerHostContextBuilder builder,
        short sslListeningPort,
        X509Certificate2 certificate,
        SslProtocols allowedProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
        bool clientCertificateRequired = false)
    {
        var endpoint = DnsUtil.ResolveEndpoint(builder.ServerConfiguration.ListeningHosts[0].Ports[0]);
        var secureProxy = new SecureProxy(sslListeningPort, certificate, endpoint);
        var serverHandler = new SecureProxyServerHandler(secureProxy);

        builder.UseHandler(serverHandler);
        builder.UseStartupMessage($"The SSL proxy is listening at:\n- https://localhost:{sslListeningPort}/");

        return builder;
    }
}
