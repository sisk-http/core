// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpConnection.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using Sisk.Cadente.HttpSerializer;
using Sisk.Cadente.Streams;
using Sisk.Core.Http;

namespace Sisk.Cadente;

sealed class HttpConnection : IDisposable {
    private readonly HttpHost _host;
    private readonly IPEndPoint _endpoint;
    private readonly Stream _connectionStream;
    private bool disposedValue;

#if DEBUG
    public readonly int Id = Random.Shared.Next ( 0, ushort.MaxValue );
#else
    public readonly int Id = 0;
#endif

    // buffer dedicated to headers.
    public const int REQUEST_BUFFER_SIZE = 8192;
    public const int RESPONSE_BUFFER_SIZE = 4096;

    public HttpConnection ( Stream connectionStream, HttpHost host, IPEndPoint endpoint ) {
        this._connectionStream = connectionStream;
        this._host = host;
        this._endpoint = endpoint;
    }

    public async Task<HttpConnectionState> HandleConnectionEvents () {
        bool connectionCloseRequested = false;

        while (this._connectionStream.CanRead && !this.disposedValue) {

            HttpRequestReader requestReader = new HttpRequestReader ( this._connectionStream );
            Stream? responseStream = null;

            try {
                if (requestReader.TryReadHttpRequest ( out HttpRequestBase? nextRequest ) == false) {
                    return HttpConnectionState.ConnectionClosed;
                }

                HttpHostContext managedSession = new HttpHostContext ( nextRequest, this._endpoint, this._connectionStream );
                await this._host.InvokeContextCreated ( managedSession );

                if (!managedSession.KeepAlive || !nextRequest.CanKeepAlive) {
                    connectionCloseRequested = true;
                    managedSession.Response.Headers.Set ( new HttpHeader ( HttpHeaderName.Connection, "close" ) );
                }

                if (managedSession.Response.ResponseStream is Stream { } s) {
                    responseStream = s;
                }
                else {
                    managedSession.Response.Headers.Set ( new HttpHeader ( HttpHeaderName.ContentLength, "0" ) );
                }

                Stream outputStream = this._connectionStream;
                if (responseStream is not null) {

                    if (managedSession.Response.SendChunked || !responseStream.CanSeek) {

                        managedSession.Response.Headers.Set ( new HttpHeader ( HttpHeaderName.TransferEncoding, "chunked" ) );
                        responseStream = new HttpChunkedStream ( responseStream );
                    }
                    else {
                        managedSession.Response.Headers.Set ( new HttpHeader ( HttpHeaderName.ContentLength, responseStream.Length.ToString () ) );
                    }
                }

                if (managedSession.ResponseHeadersAlreadySent == false && !managedSession.WriteHttpResponseHeaders ()) {
                    return HttpConnectionState.ResponseWriteException;
                }

                if (responseStream is not null) {
                    await responseStream.CopyToAsync ( outputStream );
                }

                await this._connectionStream.FlushAsync ();

                if (connectionCloseRequested) {
                    break;
                }
            }
            finally {
                responseStream?.Dispose ();
            }
        }

        return HttpConnectionState.ConnectionClosed;
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
