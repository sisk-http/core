// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHostTimeoutManager.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Cadente;

/// <summary>
/// Manages timeouts for HTTP hosts.
/// </summary>
public sealed class HttpHostTimeoutManager {

    /// <summary>
    /// Gets or sets the timeout for client read operations.
    /// </summary>
    public TimeSpan ClientReadTimeout { get; set; } = TimeSpan.FromSeconds ( 30 );

    /// <summary>
    /// Gets or sets the timeout for client write operations.
    /// </summary>
    public TimeSpan ClientWriteTimeout { get; set; } = TimeSpan.FromSeconds ( 30 );

    /// <summary>
    /// Gets or sets the timeout for SSL/TLS handshake operations.
    /// </summary>
    /// <value>The default value is 5 seconds.</value>
    public TimeSpan SslHandshakeTimeout { get; set; } = TimeSpan.FromSeconds ( 5 );

    /// <summary>
    /// Gets or sets the timeout for HTTP header parsing operations.
    /// </summary>
    /// <value>The default value is 30 seconds.</value>
    public TimeSpan HeaderParsingTimeout { get; set; } = TimeSpan.FromSeconds ( 30 );

    internal HttpHostTimeoutManager () { }
}