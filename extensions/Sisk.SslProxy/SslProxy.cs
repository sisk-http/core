// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SslProxy.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Sisk.Cadente;

namespace Sisk.Ssl;

/// <summary>
/// Represents a HTTP/1.1 proxy server that forwards traffic over SSL/HTTPS into an insecure HTTP
/// gateway.
/// </summary>
public sealed class SslProxy : IDisposable {
    private readonly HttpHost host;
    private readonly IPEndPoint remoteEndpoint;

    /// <summary>
    /// Gets or sets the Proxy-Authorization header value for creating an trusted gateway between
    /// the application and the proxy.
    /// </summary>
    public string? ProxyAuthorization { get; set; }

    /// <summary>
    /// Gets or sets whether keep-alive connections should be used.
    /// </summary>
    public bool KeepAliveEnabled { get; set; } = true;

    /// <summary>
    /// Gets the SSL certificate used by the proxy server.
    /// </summary>
    public X509Certificate ServerCertificate { get; }

    /// <summary>
    /// Gets or sets a value indicating whether client certificates are required for authentication.
    /// </summary>
    public bool ClientCertificateRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets the SSL/HTTPS protocols allowed for connections.
    /// </summary>
    public SslProtocols AllowedProtocols { get; set; } = SslProtocols.Tls12 | SslProtocols.Tls13;

    /// <summary>
    /// Gets or sets a value indicating whether to check for certificate revocation.
    /// </summary>
    public bool CheckCertificateRevocation { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum time that the gateway should take to
    /// respond to a connection or message from the proxy.
    /// </summary>
    public TimeSpan GatewayTimeout { get; set; } = TimeSpan.FromSeconds ( 120 );

    /// <summary>
    /// Gets or sets an fixed proxy host header value for incoming requests.
    /// </summary>
    public string? GatewayHostname { get; set; }

    /// <summary>
    /// Gets the proxy endpoint.
    /// </summary>
    public IPEndPoint GatewayEndpoint { get => this.remoteEndpoint; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SslProxy"/> class.
    /// </summary>
    /// <param name="sslListeningPort">The port number on which the proxy server listens for incoming connections.</param>
    /// <param name="certificate">The SSL/TLS certificate used by the proxy server.</param>
    /// <param name="remoteEndpoint">The remote endpoint to which the proxy server forwards traffic.</param>
    public SslProxy ( int sslListeningPort, X509Certificate certificate, IPEndPoint remoteEndpoint ) {
        this.host = new HttpHost ( new IPEndPoint ( IPAddress.Any, sslListeningPort ) );
        this.remoteEndpoint = remoteEndpoint;
        this.ServerCertificate = certificate;
    }

    /// <summary>
    /// Starts the <see cref="SslProxy"/> and start routing traffic to the set remote endpoint.
    /// </summary>
    public void Start () {
        this.host.Handler = new SslProxyContextHandler ( this );
        this.host.HttpsOptions = new HttpsOptions ( this.ServerCertificate ) {
            AllowedProtocols = this.AllowedProtocols,
            ClientCertificateRequired = this.ClientCertificateRequired
        };

        this.host.Start ();
    }

    /// <inheritdoc/>
    public void Dispose () {
        this.host.Dispose ();
    }
}
