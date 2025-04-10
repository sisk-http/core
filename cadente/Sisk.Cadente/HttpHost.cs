﻿// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHost.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP host that listens for incoming TCP connections and handles HTTP requests.
/// </summary>
public sealed class HttpHost : IDisposable {

    private readonly TcpListener _listener;
    private bool disposedValue;

    /// <summary>
    /// Gets or sets the client queue size of all <see cref="HttpHost"/> instances. This value indicates how many
    /// connections the server can maintain simultaneously before queueing other connections attempts.
    /// </summary>
    public static int QueueSize { get; set; } = 1024;

    /// <summary>
    /// Gets or sets the name of the server in the header name.
    /// </summary>
    public static string ServerNameHeader { get; set; } = "Sisk";

    /// <summary>
    /// Gets or sets an <see cref="HttpHostHandler"/> instance for this <see cref="HttpHost"/>.
    /// </summary>
    public HttpHostHandler? Handler { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="HttpHost"/> has been disposed.
    /// </summary>
    public bool IsDisposed { get => disposedValue; }

    /// <summary>
    /// Gets or sets the HTTPS options for secure connections. Setting an <see cref="Sisk.Cadente.HttpsOptions"/> object in this
    /// property, the <see cref="Sisk.Cadente.HttpHost"/> will use HTTPS instead of HTTP.
    /// </summary>
    public HttpsOptions? HttpsOptions { get; set; }

    /// <summary>
    /// Gets the <see cref="HttpHostTimeoutManager"/> for this <see cref="HttpHost"/>.
    /// </summary>
    public HttpHostTimeoutManager TimeoutManager { get; } = new HttpHostTimeoutManager ();

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHost"/> class using the specified <see cref="IPEndPoint"/>.
    /// </summary>
    /// <param name="endpoint">The <see cref="IPEndPoint"/> to listen on.</param>
    public HttpHost ( IPEndPoint endpoint ) {
        _listener = new TcpListener ( endpoint );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHost"/> class using the specified port on the loopback address.
    /// </summary>
    /// <param name="port">The port number to listen on.</param>
    public HttpHost ( int port ) : this ( new IPEndPoint ( IPAddress.Loopback, port ) ) {
    }

    /// <summary>
    /// Starts the HTTP host and begins listening for incoming connections.
    /// </summary>
    public void Start () {
        ObjectDisposedException.ThrowIf ( disposedValue, this );

        _listener.Server.NoDelay = true;
        _listener.Server.LingerState = new LingerOption ( true, 0 );
        _listener.Server.ReceiveBufferSize = HttpConnection.REQUEST_BUFFER_SIZE;
        _listener.Server.SendBufferSize = HttpConnection.RESPONSE_BUFFER_SIZE;

        _listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1 );
        _listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 120 );
        _listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3 );
        _listener.Server.SetSocketOption ( SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true );
        _listener.Server.SetSocketOption ( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );

        _listener.Start ( QueueSize );
        _listener.BeginAcceptTcpClient ( ReceiveClient, null );
    }

    private async void ReceiveClient ( IAsyncResult result ) {

        _listener.BeginAcceptTcpClient ( ReceiveClient, null );
        var client = _listener.EndAcceptTcpClient ( result );

        await HandleTcpClient ( client );
    }

    private async Task HandleTcpClient ( TcpClient client ) {
        try {
            client.ReceiveTimeout = TimeoutManager._ClientReadTimeoutSeconds;
            client.SendTimeout = TimeoutManager._ClientWriteTimeoutSeconds;

            if (Handler is null)
                return;

            Stream connectionStream;
            using Stream clientStream = client.GetStream ();

            if (HttpsOptions is not null) {
                connectionStream = new SslStream ( clientStream, leaveInnerStreamOpen: false );
            }
            else {
                connectionStream = clientStream;
            }

            IPEndPoint clientEndpoint = (IPEndPoint) client.Client.RemoteEndPoint!;
            HttpHostClient hostClient = new HttpHostClient ( clientEndpoint );

            using (HttpConnection connection = new HttpConnection ( hostClient, connectionStream, this, clientEndpoint )) {

                if (connectionStream is SslStream sslStream) {
                    try {
                        await sslStream.AuthenticateAsServerAsync (
                            serverCertificate: HttpsOptions!.ServerCertificate,
                            clientCertificateRequired: HttpsOptions.ClientCertificateRequired,
                            checkCertificateRevocation: HttpsOptions.CheckCertificateRevocation,
                            enabledSslProtocols: HttpsOptions.AllowedProtocols );

                        hostClient.ClientCertificate = sslStream.RemoteCertificate;
                    }
                    catch (Exception) {
                        return;
                    }
                }

                await Handler.OnClientConnectedAsync ( this, hostClient );
                await connection.HandleConnectionEvents ();
                await Handler.OnClientDisconnectedAsync ( this, hostClient );
            }
        }
        finally {
            client.Dispose ();
        }
    }

    private void Dispose ( bool disposing ) {
        if (!disposedValue) {
            if (disposing) {
                _listener.Dispose ();
            }

            disposedValue = true;
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    internal async ValueTask InvokeContextCreated ( HttpHostContext context ) {
        if (disposedValue)
            return;
        if (Handler is null)
            return;

        await Handler.OnContextCreatedAsync ( this, context );
    }

    /// <inheritdoc/>
    public void Dispose () {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose ( disposing: true );
        GC.SuppressFinalize ( this );
    }
}
