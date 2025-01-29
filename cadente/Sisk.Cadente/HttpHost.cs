// The Sisk Framework source code
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

    // internal readonly SemaphoreSlim HostLimiter = new SemaphoreSlim ( 64 );
    private readonly LingerOption tcpLingerOption = new LingerOption ( true, 0 );

    private bool disposedValue;

    /// <summary>
    /// Gets or sets the client queue size of all <see cref="HttpHost"/> instances. This value indicates how many
    /// connections the server can maintain simultaneously before queueing other connections attempts.
    /// </summary>
    public static int QueueSize { get; set; } = 1024;

    /// <summary>
    /// Gets or sets the action handler for HTTP requests.
    /// </summary>
    public event HttpContextHandler? ContextCreated;

    /// <summary>
    /// Gets a value indicating whether this <see cref="HttpHost"/> has been disposed.
    /// </summary>
    public bool IsDisposed { get => this.disposedValue; }

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
        this._listener = new TcpListener ( endpoint );
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
        ObjectDisposedException.ThrowIf ( this.disposedValue, this );

        this._listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1 );
        this._listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 120 );
        this._listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3 );
        this._listener.Server.SetSocketOption ( SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true );

        this._listener.Start ( QueueSize );
        this._listener.BeginAcceptTcpClient ( this.ReceiveClient, null );
    }

    private async void ReceiveClient ( IAsyncResult result ) {

        this._listener.BeginAcceptTcpClient ( this.ReceiveClient, null );
        var client = this._listener.EndAcceptTcpClient ( result );

        await this.HandleTcpClient ( client );
    }

    private async Task HandleTcpClient ( TcpClient client ) {
        try {
            { // setup the tcpclient
                client.NoDelay = true;

                client.ReceiveTimeout = this.TimeoutManager._ClientReadTimeoutSeconds;
                client.SendTimeout = this.TimeoutManager._ClientWriteTimeoutSeconds;

                client.ReceiveBufferSize = HttpConnection.REQUEST_BUFFER_SIZE;
                client.SendBufferSize = HttpConnection.RESPONSE_BUFFER_SIZE;

                client.LingerState = this.tcpLingerOption;
            }

            Stream connectionStream;
            Stream clientStream = client.GetStream ();

            if (this.HttpsOptions is not null) {
                connectionStream = new SslStream ( clientStream, leaveInnerStreamOpen: false );
            }
            else {
                connectionStream = clientStream;
            }

            using (HttpConnection connection = new HttpConnection ( connectionStream, this, (IPEndPoint) client.Client.RemoteEndPoint! )) {

                if (connectionStream is SslStream sslStream) {
                    try {
                        await sslStream.AuthenticateAsServerAsync (
                            serverCertificate: this.HttpsOptions!.ServerCertificate,
                            clientCertificateRequired: this.HttpsOptions.ClientCertificateRequired,
                            checkCertificateRevocation: this.HttpsOptions.CheckCertificateRevocation,
                            enabledSslProtocols: this.HttpsOptions.AllowedProtocols );
                    }
                    catch (Exception) {
                        return;
                    }
                }

                var state = await connection.HandleConnectionEvents ();
                ;
            }
        }
        finally {
            client.Dispose ();
        }
    }

    private void Dispose ( bool disposing ) {
        if (!this.disposedValue) {
            if (disposing) {
                this._listener.Dispose ();
            }

            this.disposedValue = true;
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    internal void InvokeContextCreated ( HttpHostContext context ) {
        this.ContextCreated!.Invoke ( this, context );
    }

    /// <inheritdoc/>
    public void Dispose () {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose ( disposing: true );
        GC.SuppressFinalize ( this );
    }
}
