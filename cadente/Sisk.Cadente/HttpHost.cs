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
    private Socket _listener;

    // cache line padding to reduce false sharing
    private volatile bool _disposedValue;
    private volatile bool _isListening;

    private readonly SocketAsyncEventArgs [] _acceptArgsPool;
    private readonly int [] _acceptArgsAvailable;
    private int _listenerRestarting = 0;
    private const int AcceptPoolSize = 8;
    private const int ListenerAcceptRetryDelayMilliseconds = 250;

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
        _listener = CreateListenerSocket ();

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

        try {
            _listener.Dispose ();
        }
        catch { }

        _listener = CreateListenerSocket ();
        _listener.Bind ( _endpoint );
        _listener.Listen ( backlog: 4096 ); // Alto para burst de conexões
        _isListening = true;

        // Iniciar múltiplos accepts
        for (int i = 0; i < AcceptPoolSize; i++) {
            StartAccept ( i );
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private Socket CreateListenerSocket () {
        var listener = new Socket ( _endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp );

        listener.NoDelay = true;
        listener.LingerState = new LingerOption ( false, 0 );

        // Buffers grandes para o listener reduzem syscalls
        listener.ReceiveBufferSize = 128 * 1024;
        listener.SendBufferSize = 128 * 1024;

        if (listener.AddressFamily == AddressFamily.InterNetworkV6 && _endpoint.Address.Equals ( IPAddress.IPv6Any )) {
            listener.DualMode = true;
        }

        listener.SetSocketOption ( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );
        listener.SetSocketOption ( SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true );
        listener.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 3 );
        listener.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 300 );
        listener.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3 );

        return listener;
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private void StartAccept ( int poolIndex ) {
        if (!_isListening)
            return;

        if (Volatile.Read ( ref _listenerRestarting ) == 1)
            return;

        while (_isListening) {
            var args = _acceptArgsPool [ poolIndex ];
            args.AcceptSocket = null;

            try {
                if (_listener.AcceptAsync ( args ))
                    return;
            }
            catch (ObjectDisposedException) {
                return;
            }
            catch (SocketException) {
                if (Volatile.Read ( ref _listenerRestarting ) == 1)
                    return;

                QueueStartAccept ( poolIndex, ListenerAcceptRetryDelayMilliseconds );
                return;
            }

            int rearmDelayMs = ProcessAcceptInline ( args, poolIndex );
            if (rearmDelayMs < 0)
                return;

            if (rearmDelayMs > 0) {
                QueueStartAccept ( poolIndex, rearmDelayMs );
                return;
            }
        }
    }

    private void OnAcceptCompleted ( object? sender, SocketAsyncEventArgs e ) {
        int poolIndex = (int) e.UserToken!;
        int rearmDelayMs = ProcessAcceptInline ( e, poolIndex );
        if (rearmDelayMs < 0)
            return;

        if (rearmDelayMs > 0)
            QueueStartAccept ( poolIndex, rearmDelayMs );
        else
            StartAccept ( poolIndex );
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    private int ProcessAcceptInline ( SocketAsyncEventArgs e, int poolIndex ) {
        if (e.SocketError != SocketError.Success || e.AcceptSocket is null) {
            var socketError = e.SocketError;
            e.AcceptSocket?.Dispose ();
            e.AcceptSocket = null;

            if (IsListenerFatalError ( socketError )) {
                TriggerListenerRebuild ();
                return -1;
            }

            return IsConnectionAcceptNoise ( socketError )
                ? 0
                : ListenerAcceptRetryDelayMilliseconds;
        }

        Socket client = e.AcceptSocket;
        e.AcceptSocket = null;

        var workItem = new ConnectionWorkItem { Host = this, Socket = client };
        ThreadPool.UnsafeQueueUserWorkItem ( workItem, preferLocal: false );

        return 0;
    }

    private void QueueStartAccept ( int poolIndex, int delayMs = 0 ) {
        if (!_isListening)
            return;

        if (delayMs <= 0) {
            ThreadPool.UnsafeQueueUserWorkItem (
                static state => state.Host.StartAccept ( state.PoolIndex ),
                (Host: this, PoolIndex: poolIndex),
                preferLocal: false );
            return;
        }

        _ = QueueStartAcceptAsync ( poolIndex, delayMs );
    }

    private async Task QueueStartAcceptAsync ( int poolIndex, int delayMs ) {
        await Task.Delay ( delayMs ).ConfigureAwait ( false );

        if (_isListening)
            StartAccept ( poolIndex );
    }

    private static bool IsConnectionAcceptNoise ( SocketError socketError ) =>
        socketError is SocketError.Success
            or SocketError.ConnectionReset
            or SocketError.ConnectionAborted
            or SocketError.NetworkReset;

    private static bool IsListenerFatalError ( SocketError socketError ) =>
        socketError is SocketError.InvalidArgument
            or SocketError.NotSocket
            or SocketError.Shutdown
            or SocketError.OperationAborted
            or SocketError.Interrupted;

    private void TriggerListenerRebuild () {
        if (Interlocked.CompareExchange ( ref _listenerRestarting, 1, 0 ) != 0)
            return;

        _ = RebuildListenerAsync ();
    }

    private async Task RebuildListenerAsync () {
        try { _listener.Close (); } catch { }
        try { _listener.Dispose (); } catch { }

        while (_isListening && !_disposedValue) {
            await Task.Delay ( ListenerAcceptRetryDelayMilliseconds ).ConfigureAwait ( false );

            try {
                Socket newListener = CreateListenerSocket ();
                newListener.Bind ( _endpoint );
                newListener.Listen ( backlog: 4096 );
                _listener = newListener;
                break;
            }
            catch (SocketException) {
            }
        }

        Interlocked.Exchange ( ref _listenerRestarting, 0 );

        if (_isListening && !_disposedValue) {
            for (int i = 0; i < AcceptPoolSize; i++)
                StartAccept ( i );
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    internal async Task ProcessConnectionCoreAsync ( Socket client ) {
        // Early exit se não há handler
        if (Handler is null) {
            client.Dispose ();
            return;
        }

        Logger.LogInformation ( $"Connection started from {client.RemoteEndPoint} on {client.LocalEndPoint}" );

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
                Logger.LogInformation ( $"Starting SSL handshake" );
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

                    Logger.LogInformation ( $"SSL handshake successfull" );
                }
                catch (Exception ex) {
                    // Responder erro no stream não-SSL
                    Logger.LogInformation ( $"Failed SSL handshake: {ex.Message}" );
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
                hostClient.IsSecureConnection = true;
                hostClient.ClientCertificate = sslStream.RemoteCertificate;
            }

            // await using para dispose correto
            await using HttpConnection connection = new ( hostClient, connectionStream, this, clientEndpoint );

            Logger.LogInformation ( $"call OnClientConnectedAsync" );
            await Handler.OnClientConnectedAsync ( this, hostClient ).ConfigureAwait ( false );

            try {
                Logger.LogInformation ( $"call HandleConnectionEventsAsync" );
                await connection.HandleConnectionEventsAsync ( default ).ConfigureAwait ( false );
            }
            catch (Exception ex) {
                Logger.LogInformation ( $"HandleConnectionEventsAsync/exception: {ex}" );
            }
            finally {
                Logger.LogInformation ( $"call OnClientDisconnectedAsync" );
                await Handler.OnClientDisconnectedAsync ( this, hostClient ).ConfigureAwait ( false );
            }
        }
        catch (SocketException sex) {
            Logger.LogInformation ( $"SocketException: {sex.Message}" );
        }
        catch (IOException iox) {
            Logger.LogInformation ( $"IOException: {iox.Message}" );
        }
        catch (Exception ex) {
            Logger.LogInformation ( $"Exception: {ex.Message}" );
        }
        finally {
            // Cleanup garantido
            Logger.LogInformation ( $"cleanup" );
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
