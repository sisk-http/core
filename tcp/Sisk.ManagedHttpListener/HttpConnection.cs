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
using Sisk.ManagedHttpListener.Streams;

namespace Sisk.ManagedHttpListener;

sealed class HttpConnection : IDisposable {
    private readonly Stream _connectionStream;
    private bool disposedValue;

#if DEBUG
    public readonly int Id = Random.Shared.Next ( 0, ushort.MaxValue );
#else
    public readonly int Id = 0;
#endif

    public const int REQUEST_BUFFER_SIZE = 4096;
    public const int RESPONSE_BUFFER_SIZE = 8192;

    public HttpAction Action { get; set; }

    public HttpConnection ( Stream connectionStream, HttpAction action ) {
        this._connectionStream = connectionStream;
        this.Action = action;
    }

    public async Task<HttpConnectionState> HandleConnectionEvents () {
        ObjectDisposedException.ThrowIf ( this.disposedValue, this );

        bool connectionCloseRequested = false;
        byte [] buffer = ArrayPool<byte>.Shared.Rent ( REQUEST_BUFFER_SIZE );

        while (this._connectionStream.CanRead && !this.disposedValue) {

            HttpRequestReader requestReader = new HttpRequestReader ( this._connectionStream, ref buffer );
            Stream? responseStream = null;
            byte []? responseBytes = null;

            try {

                var readRequestState = await requestReader.ReadHttpRequest ();
                var nextRequest = readRequestState.Item2;

                if (nextRequest is null) {
                    return readRequestState.Item1 switch {
                        HttpRequestReadState.StreamZero => HttpConnectionState.ConnectionClosedByStreamRead,
                        _ => HttpConnectionState.BadRequest
                    };
                }

                //Logger.LogInformation ( $"[{this.Id}] Received \"{nextRequest.Method} {nextRequest.Path}\"" );
                HttpSession managedSession = new HttpSession ( nextRequest, this._connectionStream );

                this.Action ( managedSession );

                if (!managedSession.KeepAlive || !nextRequest.CanKeepAlive) {
                    connectionCloseRequested = true;
                    managedSession.Response.Headers.Set ( new HttpHeader ( "Connection", "close" ) );
                }

                if (managedSession.Response.ResponseStream is Stream { } s) {
                    responseStream = s;

                    if (managedSession.Response.TransferEncoding.HasFlag ( TransferEncoding.Chunked ) || !responseStream.CanSeek) {
                        managedSession.Response.Headers.Set ( new HttpHeader ( "Transfer-Encoding", "chunked" ) );
                        responseStream = new HttpChunkedStream ( responseStream );
                    }

                    else {
                        managedSession.Response.Headers.Set ( new HttpHeader ( "Content-Length", responseStream.Length.ToString () ) );
                    }
                }

                else if (managedSession.Response.ResponseBytes is byte [] b) {
                    responseBytes = b;
                    managedSession.Response.Headers.Set ( new HttpHeader ( "Content-Length", b.Length.ToString () ) );
                }

                else {
                    managedSession.Response.Headers.Set ( new HttpHeader ( "Content-Length", "0" ) );
                }

                if (!managedSession.ResponseHeadersAlreadySent && !await managedSession.WriteHttpResponseHeaders ()) {

                    return HttpConnectionState.ResponseWriteException;
                }

                if (responseStream is not null) {
                    await responseStream.CopyToAsync ( this._connectionStream );
                }
                else if (responseBytes is not null) {
                    await this._connectionStream.WriteAsync ( responseBytes );
                }

                this._connectionStream.Flush ();

                //Logger.LogInformation ( $"[{this.Id}] Response sent: {managedSession.Response.StatusCode} {managedSession.Response.StatusDescription}" );

                if (connectionCloseRequested) {
                    break;
                }
            }
            finally {
                responseStream?.Dispose ();
                ArrayPool<byte>.Shared.Return ( buffer );
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
