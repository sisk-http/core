// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHostClient.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP host client with its endpoint and certificate information.
/// </summary>
public sealed class HttpHostClient {

    /// <summary>
    /// Gets the endpoint of the client.
    /// </summary>
    public IPEndPoint ClientEndpoint { get; }

    /// <summary>
    /// Gets the client certificate, if any.
    /// </summary>
    public X509Certificate? ClientCertificate { get; internal set; }

    /// <summary>
    /// Gets or sets an optional state object associated with the client.
    /// </summary>
    public object? State { get; set; }

    internal HttpHostClient ( IPEndPoint clientEndpoint ) {
        this.ClientEndpoint = clientEndpoint;
    }
}
