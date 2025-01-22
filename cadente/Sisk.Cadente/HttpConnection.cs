// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpConnection.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using Sisk.Cadente.HttpSerializer;
using Sisk.Cadente.Streams;
using Sisk.Core.Http;

namespace Sisk.Cadente;

sealed class HttpConnection : IDisposable {
    private readonly Stream _connectionStream;
    private bool disposedValue;

#if DEBUG
    public readonly int Id = Random.Shared.Next ( 0, ushort.MaxValue );
#else
    public readonly int Id = 0;
#endif

    public const int REQUEST_BUFFER_SIZE = 8192; // buffer dedicated to headers. more than it? return 400.
    public const int RESPONSE_BUFFER_SIZE = 8192;

    public HttpAction Action { get; set; }

    public HttpConnection ( Stream connectionStream, HttpAction action ) {
        this._connectionStream = connectionStream;
        this.Action = action;
    }

    public async Task<HttpConnectionState> HandleConnectionEvents () {
        ObjectDisposedException.ThrowIf ( this.disposedValue, this );

        bool connectionCloseRequested = false;
        var bufferOwnership = MemoryPool<byte>.Shared.Rent ( REQUEST_BUFFER_SIZE );
        try {

            while (this._connectionStream.CanRead && !this.disposedValue) {

                HttpRequestReader requestReader = new HttpRequestReader ( this._connectionStream, ref bufferOwnership );
                Stream? responseStream = null;

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

                    if (managedSession.ResponseHeadersAlreadySent == false && !await managedSession.WriteHttpResponseHeaders ()) {
                        return HttpConnectionState.ResponseWriteException;
                    }

                    if (responseStream is not null) {
                        await responseStream.CopyToAsync ( outputStream );
                    }

                    this._connectionStream.Flush ();

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
        finally {
            bufferOwnership.Dispose ();
        }
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
