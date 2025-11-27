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
using Sisk.Cadente.HttpSerializer;

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP host that listens for incoming TCP connections and handles HTTP requests.
/// </summary>
public sealed class HttpHost : IDisposable {

    private readonly IPEndPoint _endpoint;
    private readonly Socket _listener;

    // cache line padding to reduce false sharing
    private volatile bool _disposedValue;
    private volatile bool _isListening;

    private readonly SocketAsyncEventArgs [] _acceptArgsPool;
    private readonly int [] _acceptArgsAvailable;
    private const int AcceptPoolSize = 8;

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
    public bool IsDisposed => _disposedValue;

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
        _listener = new Socket ( SocketType.Stream, ProtocolType.Tcp );

        _acceptArgsPool = new SocketAsyncEventArgs [ AcceptPoolSize ];
        _acceptArgsAvailable = new int [ AcceptPoolSize ];

        for (int i = 0; i < AcceptPoolSize; i++) {
            var args = new SocketAsyncEventArgs ();
            args.Completed += OnAcceptCompleted;
            args.UserToken = i;
            _acceptArgsPool [ i ] = args;
            _acceptArgsAvailable [ i ] = 1;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHost"/> class using the specified port on the loopback address.
    /// </summary>
    /// <param name="port">The port number to listen on.</param>
    public HttpHost ( int port ) : this ( new IPEndPoint ( IPAddress.Loopback, port ) ) { }

    /// <summary>
    /// Starts the HTTP host and begins listening for incoming connections.
    /// </summary>
    public void Start () {
        if (_isListening)
            return;
        ObjectDisposedException.ThrowIf ( _disposedValue, this );

        ConfigureListenerSocket ();
        _listener.Bind ( _endpoint );
        _listener.Listen ( backlog: 4096 ); // Alto para burst de conexões
        _isListening = true;

        // Iniciar múltiplos accepts
        for (int i = 0; i < AcceptPoolSize; i++) {
            StartAccept ( i );
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private void ConfigureListenerSocket () {
        _listener.NoDelay = true;
        _listener.LingerState = new LingerOption ( false, 0 );

        // Buffers grandes para o listener reduzem syscalls
        _listener.ReceiveBufferSize = 128 * 1024;
        _listener.SendBufferSize = 128 * 1024;

        if (_listener.AddressFamily == AddressFamily.InterNetworkV6) {
            _listener.DualMode = true;
        }

        _listener.SetSocketOption ( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );
        _listener.SetSocketOption ( SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true );
        _listener.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 3 );
        _listener.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 300 );
        _listener.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3 );
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private void StartAccept ( int poolIndex ) {
        if (!_isListening)
            return;

        var args = _acceptArgsPool [ poolIndex ];
        args.AcceptSocket = null;

        try {
            // Se completar sincronamente, processar inline (mais rápido)
            if (!_listener.AcceptAsync ( args )) {
                // IMPORTANTE: Não usar ThreadPool aqui para sync completion
                // ProcessAccept já vai fazer o queue se necessário
                ProcessAcceptInline ( args, poolIndex );
            }
            // Se async, o callback OnAcceptCompleted será chamado
        }
        catch (ObjectDisposedException) { }
        catch (SocketException) {
            // Retry
            if (_isListening) {
                StartAccept ( poolIndex );
            }
        }
    }

    private void OnAcceptCompleted ( object? sender, SocketAsyncEventArgs e ) {
        int poolIndex = (int) e.UserToken!;
        ProcessAcceptInline ( e, poolIndex );
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    private void ProcessAcceptInline ( SocketAsyncEventArgs e, int poolIndex ) {
        if (e.SocketError != SocketError.Success || e.AcceptSocket is null) {
            if (_isListening)
                StartAccept ( poolIndex );
            return;
        }

        Socket client = e.AcceptSocket;
        StartAccept ( poolIndex );

        var workItem = new ConnectionWorkItem { Host = this, Socket = client };
        ThreadPool.UnsafeQueueUserWorkItem ( workItem, preferLocal: false );
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    internal async Task ProcessConnectionCoreAsync ( Socket client ) {
        // Early exit se não há handler
        if (Handler is null) {
            client.Dispose ();
            return;
        }

        int readTimeoutMs = (int) TimeoutManager.ClientReadTimeout.TotalMilliseconds;
        int writeTimeoutMs = (int) TimeoutManager.ClientWriteTimeout.TotalMilliseconds;

        client.ReceiveTimeout = readTimeoutMs;
        client.SendTimeout = writeTimeoutMs;
        client.NoDelay = true; // Importante para cada socket também

        // NetworkStream com ownsSocket: true - elimina dispose manual
        NetworkStream clientStream = new ( client, ownsSocket: true );
        clientStream.ReadTimeout = readTimeoutMs;
        clientStream.WriteTimeout = writeTimeoutMs;

        Stream connectionStream;
        SslStream? sslStream = null;

        try {
            if (HttpsOptions is not null) {
                sslStream = new SslStream ( clientStream, leaveInnerStreamOpen: false );
                connectionStream = sslStream;

                // SSL Handshake com timeout
                using var handshakeCts = new CancellationTokenSource ( TimeoutManager.SslHandshakeTimeout );

                try {
                    await sslStream.AuthenticateAsServerAsync ( new SslServerAuthenticationOptions {
                        ServerCertificate = HttpsOptions.ServerCertificate,
                        ClientCertificateRequired = HttpsOptions.ClientCertificateRequired,
                        EnabledSslProtocols = HttpsOptions.AllowedProtocols,
                        CertificateRevocationCheckMode = HttpsOptions.CheckCertificateRevocation
                            ? System.Security.Cryptography.X509Certificates.X509RevocationMode.Online
                            : System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck
                    }, handshakeCts.Token ).ConfigureAwait ( false );
                }
                catch {
                    // Responder erro no stream não-SSL
                    await WriteHandshakeErrorAsync ( clientStream ).ConfigureAwait ( false );
                    return;
                }
            }
            else {
                connectionStream = clientStream;
            }

            IPEndPoint clientEndpoint = (IPEndPoint) client.RemoteEndPoint!;

            // TODO: Pool de HttpHostClient se profiling mostrar que é hot spot
            HttpHostClient hostClient = new ( clientEndpoint, CancellationToken.None );

            if (sslStream is not null) {
                hostClient.ClientCertificate = sslStream.RemoteCertificate;
            }

            // await using para dispose correto
            await using HttpConnection connection = new ( hostClient, connectionStream, this, clientEndpoint );

            await Handler.OnClientConnectedAsync ( this, hostClient ).ConfigureAwait ( false );

            try {
                await connection.HandleConnectionEventsAsync ( default ).ConfigureAwait ( false );
            }
            finally {
                await Handler.OnClientDisconnectedAsync ( this, hostClient ).ConfigureAwait ( false );
            }
        }
        catch (SocketException) { }
        catch (IOException) { }
        catch (ObjectDisposedException) { }
        finally {
            // Cleanup garantido
            if (sslStream is not null) {
                await sslStream.DisposeAsync ().ConfigureAwait ( false );
            }
            else {
                await clientStream.DisposeAsync ().ConfigureAwait ( false );
            }
        }
    }

    // Método separado para não poluir o hot path com byte array
    [MethodImpl ( MethodImplOptions.NoInlining )]
    private static async Task WriteHandshakeErrorAsync ( Stream stream ) {
        byte [] message = HttpResponseSerializer.GetRawMessage ( "SSL/TLS Handshake failed.", 400, "Bad Request" );
        try {
            await stream.WriteAsync ( message ).ConfigureAwait ( false );
        }
        catch { }
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    internal ValueTask InvokeContextCreated ( HttpHostContext context ) {
        if (_disposedValue || Handler is null)
            return ValueTask.CompletedTask;

        return new ValueTask ( Handler.OnContextCreatedAsync ( this, context ) );
    }

    /// <summary>
    /// Stops the HTTP host from listening for incoming HTTP requests.
    /// </summary>
    public void Stop () {
        if (!_isListening)
            return;
        _isListening = false;

        try {
            _listener.Close ();
        }
        catch { }
    }

    private void Dispose ( bool disposing ) {
        if (_disposedValue)
            return;
        _disposedValue = true;

        if (disposing) {
            _isListening = false;

            try { _listener.Close (); }
            catch { }
            try { _listener.Dispose (); }
            catch { }

            // Dispose pool de accept args
            for (int i = 0; i < AcceptPoolSize; i++) {
                try { _acceptArgsPool [ i ].Dispose (); }
                catch { }
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose () {
        Dispose ( true );
        GC.SuppressFinalize ( this );
    }

    /// <inheritdoc/>
    ~HttpHost () {
        Dispose ( false );
    }
}