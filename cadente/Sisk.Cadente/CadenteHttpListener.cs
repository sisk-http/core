// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CadenteHttpListener.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Channels;

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP host that listens for incoming TCP connections and handles HTTP requests.
/// </summary>
public sealed class CadenteHttpListener : IDisposable {

    // defines the connection queue size (worker connections)
    const int QUEUE_SIZE = 512;

    private readonly TcpListener _listener;
    private readonly Channel<TcpClient> clientQueue;
    private readonly ChannelWriter<TcpClient> writerQueue;
    private readonly ChannelReader<TcpClient> readerQueue;
    private readonly Thread channelConsumerThread;

    private bool disposedValue;

    /// <summary>
    /// Gets the action handler for processing HTTP actions.
    /// </summary>
    public HttpAction ActionHandler { get; }

    /// <summary>
    /// Gets a value indicating whether the host has been disposed.
    /// </summary>
    public bool IsDisposed { get => this.disposedValue; }

    /// <summary>
    /// Gets or sets the port on which the HTTP host listens for incoming connections.
    /// Default is 8080.
    /// </summary>
    public int Port { get; set; } = 8080;

    /// <summary>
    /// Gets or sets the options for HTTPS configuration.
    /// </summary>
    public HttpsOptions? HttpsOptions { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CadenteHttpListener"/> class with the specified port and action handler.
    /// </summary>
    /// <param name="port">The port on which the host will listen for incoming connections.</param>
    /// <param name="actionHandler">The action handler for processing HTTP actions.</param>
    public CadenteHttpListener ( int port, HttpAction actionHandler ) {
        this._listener = new TcpListener ( new IPEndPoint ( IPAddress.Any, port ) );
        this.channelConsumerThread = new Thread ( this.ConsumerJobThread );
        this.clientQueue = Channel.CreateBounded<TcpClient> (
            new BoundedChannelOptions ( QUEUE_SIZE ) { SingleReader = true, SingleWriter = true, AllowSynchronousContinuations = true } );
        this.readerQueue = this.clientQueue.Reader;
        this.writerQueue = this.clientQueue.Writer;
        this.ActionHandler = actionHandler;
    }

    /// <summary>
    /// Starts the Cadente HTTP host, beginning to listen for incoming connections.
    /// </summary>
    public void Start () {
        ObjectDisposedException.ThrowIf ( this.disposedValue, this );

        this._listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 3 );
        this._listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 120 );
        this._listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3 );
        this._listener.Server.SetSocketOption ( SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true );

        this._listener.Start ();
        this._listener.BeginAcceptTcpClient ( this.ReceiveClient, null );

        this.channelConsumerThread.Start ();
    }

    private async void ReceiveClient ( IAsyncResult result ) {

        var client = this._listener.EndAcceptTcpClient ( result );
        this._listener.BeginAcceptTcpClient ( this.ReceiveClient, null );

        await this.writerQueue.WriteAsync ( client );
    }

    private async Task HandleTcpClient ( TcpClient client ) {
        try {
            { // setup the tcpclient
                client.NoDelay = true;

                client.ReceiveTimeout = 5_000;
                client.SendTimeout = 5_000;

                client.ReceiveBufferSize = HttpConnection.REQUEST_BUFFER_SIZE;
                client.SendBufferSize = HttpConnection.RESPONSE_BUFFER_SIZE;

                client.LingerState = new LingerOption ( true, 0 ); // immediately close client after connection handling
            }

            Stream connectionStream;
            Stream clientStream = client.GetStream ();

            if (this.HttpsOptions is not null) {
                connectionStream = new SslStream ( clientStream, leaveInnerStreamOpen: false );
            }
            else {
                connectionStream = clientStream;
            }

            using (HttpConnection connection = new HttpConnection ( connectionStream, this.ActionHandler )) {

                if (connectionStream is SslStream sslStream && this.HttpsOptions is not null) {
                    //Logger.LogInformation ( $"[{connection.Id}] Begin SSL authenticate" );
                    try {
                        await sslStream.AuthenticateAsServerAsync (
                            serverCertificate: this.HttpsOptions.ServerCertificate,
                            clientCertificateRequired: this.HttpsOptions.ClientCertificateRequired,
                            checkCertificateRevocation: this.HttpsOptions.CheckCertificateRevocation,
                            enabledSslProtocols: this.HttpsOptions.AllowedProtocols );
                    }
                    catch (Exception) {
                        //Logger.LogInformation ( $"[{connection.Id}] Failed SSL authenticate: {ex.Message}" );
                        // TODO drop connection with 401
                    }
                }

                var state = await connection.HandleConnectionEvents ();

            }
        }
        finally {
            client.Dispose ();
        }
    }

    async void ConsumerJobThread () {
        while (!this.disposedValue && await this.readerQueue.WaitToReadAsync ()) {
            while (!this.disposedValue && this.readerQueue.TryRead ( out var client )) {
                _ = this.HandleTcpClient ( client );
            }
        }
    }

    private void Dispose ( bool disposing ) {
        if (!this.disposedValue) {
            if (disposing) {
                this._listener.Dispose ();
                this.channelConsumerThread.Join ();
            }

            this.disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose () {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose ( disposing: true );
        GC.SuppressFinalize ( this );
    }
}
