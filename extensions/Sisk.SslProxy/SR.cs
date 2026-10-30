// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SR.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.Ssl;

internal static class SR {
    public const string CertificateUtil_NoDnsNames = "At least one DNS name must be specified.";

    public const string DnsUtil_NoAddresses = "Couldn't resolve any IP addresses for {0}.";

    public const string SslProxyContextHandler_NoClientStream = "Unable to access the underlying client stream.";
    public const string SslProxyContextHandler_WebSocketHandshakeTimeout = "WebSocket handshake timed out.";
    public const string SslProxyContextHandler_WebSocketProxyError = "WebSocket proxy error: {0}";
    public const string SslProxyContextHandler_UnexpectedHandshakeEnd = "Unexpected end of stream while reading handshake response.";
    public const string SslProxyContextHandler_InvalidHandshakeResponse = "Invalid handshake response from upstream server.";
    public const string SslProxyContextHandler_InvalidHandshakeStatusCode = "Unable to parse status code from upstream handshake.";

    public const string SslProxyExtensions_ListeningHostNotConfigured = "Couldn't start the SslProxy: the server ListeningHost ins't configured yet. Run UseSsl() after defining the listening port.";

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static string Format ( string format, params object? [] items ) {
        return String.Format ( provider: null, format, items );
    }
}
