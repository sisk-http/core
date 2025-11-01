// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHost.cs
// Repository:  https://github.com/sisk-http/core

using System.ComponentModel;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP host that listens for incoming TCP connections and handles HTTP requests.
/// </summary>
public sealed class HttpHost : IDisposable {

    private readonly IPEndPoint _endpoint;
    private readonly TcpListener _listener;
    private bool disposedValue;
    private bool isListening;

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
    /// Gets the endpoint of the <see cref="HttpHost"/>.
    /// </summary>
    public IPEndPoint Endpoint => _endpoint;

    /// <summary>
    /// Gets or sets an <see cref="HttpHostHandler"/> instance for this <see cref="HttpHost"/>.
    /// </summary>
    public HttpHostHandler? Handler { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="HttpHost"/> has been disposed.
    /// </summary>
    public bool IsDisposed => disposedValue;

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
        _endpoint = endpoint;
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
        if (isListening)
            return;

        ObjectDisposedException.ThrowIf ( disposedValue, this );

        _listener.Server.NoDelay = true;
        _listener.Server.LingerState = new LingerOption ( true, 3 );
        _listener.Server.ReceiveBufferSize = HttpConnection.REQUEST_BUFFER_SIZE;
        _listener.Server.SendBufferSize = HttpConnection.RESPONSE_BUFFER_SIZE;

        _listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1 );
        _listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 120 );
        _listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3 );
        _listener.Server.SetSocketOption ( SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true );
        _listener.Server.SetSocketOption ( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );

        _listener.Start ( QueueSize );
        isListening = true;

        _listener.BeginAcceptTcpClient ( ReceiveClient, null );
    }

    /// <summary>
    /// Stops the HTTP host from listening for incoming HTTP requests.
    /// </summary>
    public void Stop () {
        if (!isListening)
            return;

        isListening = false;
        _listener.Stop ();
    }

    private async void ReceiveClient ( IAsyncResult result ) {

        if (!isListening)
            return;

        _listener.BeginAcceptTcpClient ( ReceiveClient, null );
        var client = _listener.EndAcceptTcpClient ( result );

        await HandleTcpClient ( client );
    }

    private byte[] GetBadRequestMessage ( string message ) {
        string content = $"""
            <HTML>
                <HEAD>
                    <TITLE>400 - Bad Request</TITLE>
                </HEAD>
                <BODY>
                    <H1>400 - Bad Request</H1>
                    <P>{message}</P>
                    <HR>
                    <P><EM>Cadente</EM></P>
                </BODY>
            </HTML>
            """;

        string html = 
            $"HTTP/1.1 400 Bad Request\r\n" +
            $"Content-Type: text/html\r\n" +
            $"Content-Length: {content.Length}\r\n" +
            $"Connection: close\r\n" +
            $"\r\n" +
            content;

        return Encoding.ASCII.GetBytes ( html );
    }

    private async Task HandleTcpClient ( TcpClient client ) {

        try {
            int clientReadTimeoutMs = (int) TimeoutManager.ClientReadTimeout.TotalMilliseconds;
            int clientWriteTimeoutMs = (int) TimeoutManager.ClientWriteTimeout.TotalMilliseconds;

            client.ReceiveTimeout = clientReadTimeoutMs;
            client.SendTimeout = clientWriteTimeoutMs;

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
            HttpHostClient hostClient = new HttpHostClient ( clientEndpoint, CancellationToken.None );

            connectionStream.ReadTimeout = clientReadTimeoutMs;
            connectionStream.WriteTimeout = clientWriteTimeoutMs;

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

                        var message = GetBadRequestMessage ( "SSL/TLS Handshake failed." );
                        await clientStream.WriteAsync ( message, 0, message.Length );
                        return;
                    }
                }

                await Handler.OnClientConnectedAsync ( this, hostClient );
                await connection.HandleConnectionEventsAsync ();
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
                isListening = false;
                _listener.Stop ();
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

    /// <inheritdoc/>
    ~HttpHost () {
        Dispose ( disposing: false );
    }
}
