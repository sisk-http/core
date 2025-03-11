// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpsOptions.cs
// Repository:  https://github.com/sisk-http/core

using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Sisk.Cadente;

/// <summary>
/// Represents the options for configuring an HTTPS server.
/// </summary>
public sealed class HttpsOptions {

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
    /// Initializes a new instance of the <see cref="HttpsOptions"/> class.
    /// </summary>
    /// <param name="certificate">The <see cref="X509Certificate"/> used to encrypt data between the client and the server.</param>
    public HttpsOptions ( X509Certificate certificate ) {
        ServerCertificate = certificate;
    }
}
