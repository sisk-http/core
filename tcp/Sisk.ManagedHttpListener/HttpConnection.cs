// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpConnection.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using Sisk.ManagedHttpListener.HttpSerializer;

namespace Sisk.ManagedHttpListener;

public sealed class HttpConnection : IDisposable {
    private readonly Stream _connectionStream;
    private bool disposedValue;

    public HttpAction Action { get; set; }

    public HttpConnection ( Stream connectionStream, HttpAction action ) {
        this._connectionStream = connectionStream;
        this.Action = action;
    }

    public int HandleConnectionEvents () {
        ObjectDisposedException.ThrowIf ( this.disposedValue, this );

        while (this._connectionStream.CanRead && !this.disposedValue) {
            HttpRequestReader requestReader = new HttpRequestReader ( this._connectionStream );
            HttpRequestBase? nextRequest = requestReader.ReadHttpRequest ();
            Stream? responseStream = null;

            try {
                if (nextRequest is null) {
                    Logger.LogInformation ( $"couldn't read request" );
                    return 1;
                }

                HttpSession managedSession = new HttpSession ( nextRequest, this._connectionStream );

                this.Action ( managedSession );

                if (!managedSession.KeepAlive)
                    managedSession.Response.Headers.Set ( ("Connection", "Close") );

                responseStream = managedSession.Response.ResponseStream;
                if (responseStream is not null) {
                    if (responseStream.CanSeek) {
                        managedSession.Response.Headers.Set ( ("Content-Length", responseStream.Length.ToString ()) );
                    }
                    else {
                        // implement chunked-encodind
                    }
                }
                else {
                    managedSession.Response.Headers.Set ( ("Content-Length", "0") );
                }

                if (!HttpResponseSerializer.WriteHttpResponseHeaders (
                    this._connectionStream,
                    managedSession.Response.StatusCode,
                    managedSession.Response.StatusDescription,
                    managedSession.Response.Headers )) {
                    Logger.LogInformation ( $"couldn't write response" );
                    return 2;
                }

                if (responseStream is not null) {
                    responseStream.CopyTo ( this._connectionStream );
                    responseStream.Dispose ();
                }

                this._connectionStream.Flush ();

                if (!managedSession.KeepAlive) {
                    break;
                }
            }
            finally {
                responseStream?.Dispose ();
                if (nextRequest is not null)
                    ArrayPool<byte>.Shared.Return ( nextRequest.BufferedContent );
            }
        }

        return 0;
    }

    private void Dispose ( bool disposing ) {
        if (!this.disposedValue) {
            if (disposing) {
                this._connectionStream.Dispose ();
            }

            this.disposedValue = true;
        }
    }

    public void Dispose () {
        this.Dispose ( disposing: true );
        GC.SuppressFinalize ( this );
    }
}
