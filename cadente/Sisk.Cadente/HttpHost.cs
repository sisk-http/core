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
    private Thread? _eventLoopThread;
    private bool disposedValue;
    private bool isListening;

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
        _listener.Server.ReceiveBufferSize = HttpConnection.RESERVED_BUFFER_SIZE;
        _listener.Server.SendBufferSize = HttpConnection.RESERVED_BUFFER_SIZE;

        _listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1 );
        _listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 120 );
        _listener.Server.SetSocketOption ( SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3 );
        _listener.Server.SetSocketOption ( SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true );
        _listener.Server.SetSocketOption ( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );

        _listener.Start ( backlog: 8192 );
        isListening = true;

        _eventLoopThread = new Thread ( EventLoopThreadRunner );
        _eventLoopThread.Priority = ThreadPriority.Highest;
        _eventLoopThread.Start ();
    }

    async void EventLoopThreadRunner () {

        while (isListening) {
            var client = await _listener.AcceptTcpClientAsync ().ConfigureAwait ( false );
            HttpHostThreadPoolWorkItem workItem = new HttpHostThreadPoolWorkItem ( this, client );
            ThreadPool.UnsafeQueueUserWorkItem ( workItem, preferLocal: true );
        }
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
    internal async Task InvokeContextCreated ( HttpHostContext context ) {
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
