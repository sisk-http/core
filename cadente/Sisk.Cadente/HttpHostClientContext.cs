// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHostClientContext.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP connection state that manages the entire client connection.
/// </summary>
public sealed class HttpHostClientContext : IDisposable {

    private TcpClient client;
    private AutoResetEvent disposedEvent = new AutoResetEvent ( false );
    private HttpHost baseHost;
    bool ended = false;

    /// <summary>
    /// Gets the remote address of the connected client.
    /// </summary>
    public IPEndPoint RemoteAddress { get; }

    /// <summary>
    /// Gets the remote certificate of the connected client if authenticated with an SSL connection.
    /// </summary>
    public X509Certificate? RemoteCertificate { get; internal set; }

    /// <summary>
    /// Gets or sets the action handler for incoming HTTP requests within this client context.
    /// </summary>
    public event HttpContextHandler? ContextCreated;

    /// <summary>
    /// Closes the client connection.
    /// </summary>

    // Note: This method shouldn't be called on the dispose. The HttpHost.HandleTcpClient method should
    // dispose the TcpClient instead.
    public void Close () {
        this.client.Close ();
        this.ended = true;
    }

    /// <summary>
    /// Blocks the current thread until this <see cref="HttpHostClientContext"/> connection
    /// is terminated.
    /// </summary>
    public bool WaitUntilClose () {
        return this.disposedEvent.WaitOne ();
    }

    /// <summary>
    /// Blocks the current thread until this <see cref="HttpHostClientContext"/> connection
    /// is terminated, using a maximum timeout interval.
    /// </summary>
    /// <param name="timeout">Defines the maximum time waiting for the connection termination.</param>
    /// <param name="closeOnTimeout">Defines if this client context should terminate the client connection if the timeout is reached.</param>
    public bool WaitUntilClose ( TimeSpan timeout, bool closeOnTimeout = false ) {
        var b = this.disposedEvent.WaitOne ( timeout );
        if (!b && closeOnTimeout)
            this.Close ();

        return b;
    }

    internal HttpHostClientContext ( TcpClient client, HttpHost baseHost, IPEndPoint remoteAddress ) {
        this.baseHost = baseHost;
        this.RemoteAddress = remoteAddress;
        this.client = client;
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    internal async ValueTask InvokeContextCreated ( HttpHostContext context ) {
        if (this.ended)
            return;
        if (ContextCreated != null)
            await ContextCreated.Invoke ( this.baseHost, context );
    }

    /// <inheritdoc/>
    public void Dispose () {
        this.ended = true;
        this.disposedEvent.Set ();
        this.disposedEvent.Dispose ();
    }
}
