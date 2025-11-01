// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ListeningHostSslOptions.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Http;

/// <summary>
/// Represents the options for configuring HTTPS on a <see cref="ListeningHost"/>.
/// </summary>
public sealed class ListeningHostSslOptions {

    /// <summary>
    /// Gets the SSL certificate used by the proxy server.
    /// </summary>
    public X509Certificate ServerCertificate { get; }

    /// <summary>
    /// Gets or sets a value indicating whether client certificates are required for authentication.
    /// </summary>
    public bool ClientCertificateRequired { get; set; }

    /// <summary>
    /// Gets or sets the SSL/HTTPS protocols allowed for connections.
    /// </summary>
    public SslProtocols AllowedProtocols { get; set; } = SslProtocols.Tls12 | SslProtocols.Tls13;

    /// <summary>
    /// Gets or sets a value indicating whether to check for certificate revocation.
    /// </summary>
    public bool CheckCertificateRevocation { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListeningHostSslOptions"/> class.
    /// </summary>
    /// <param name="certificate">The <see cref="X509Certificate"/> used to encrypt data between the client and the server.</param>
    public ListeningHostSslOptions ( X509Certificate certificate ) {
        ServerCertificate = certificate;
    }
}
