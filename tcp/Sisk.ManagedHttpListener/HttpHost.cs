// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHost.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Net.Sockets;

namespace Sisk.ManagedHttpListener;

public sealed class HttpHost : IDisposable {
    private readonly TcpListener _listener;
    private bool disposedValue;

    public HttpAction ActionHandler { get; }
    public bool IsDisposed { get => this.disposedValue; }
    public int Port { get; set; } = 8080;

    public HttpHost ( int port, HttpAction actionHandler ) {
        this._listener = new TcpListener ( new IPEndPoint ( IPAddress.Any, port ) );
        this.ActionHandler = actionHandler;
    }

    public void Start () {
        ObjectDisposedException.ThrowIf ( this.disposedValue, this );

        this._listener.Start ();
        this._listener.BeginAcceptTcpClient ( this.ReceiveClient, null );
    }

    private void ReceiveClient ( IAsyncResult result ) {
        using (TcpClient client = this._listener.EndAcceptTcpClient ( result )) {
            this._listener.BeginAcceptTcpClient ( this.ReceiveClient, null );

            Stream clientStream = client.GetStream ();
            using (HttpConnection connection = new HttpConnection ( clientStream, this.ActionHandler )) {
                connection.HandleConnectionEvents ();
            }
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

    public void Dispose () {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose ( disposing: true );
        GC.SuppressFinalize ( this );
    }
}
