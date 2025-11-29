// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpConnection.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Net;
using System.Runtime.CompilerServices;
using Sisk.Cadente.HttpSerializer;
using Sisk.Cadente.Streams;
using Sisk.Core.Http;

namespace Sisk.Cadente;

sealed class HttpConnection : IDisposable, IAsyncDisposable {
    private readonly HttpHost _host;
    private readonly IPEndPoint _endpoint;
    private readonly HttpHostClient _client;
    private bool disposedValue;
    private int headerParsingTimeout;

#if DEBUG
    public static readonly AsyncLocal<int> Id = new AsyncLocal<int> ();
#endif

    // buffer dedicated to headers
    public const int RESERVED_BUFFER_SIZE = 8 * 1024;

    internal readonly Stream networkStream;
    internal IMemoryOwner<byte> requestPool, responsePool;

    static readonly HttpHeader ConnCloseHeader = new HttpHeader ( HttpHeaderName.Connection, "close" );
    static readonly HttpHeader ContLengzHeader = new HttpHeader ( HttpHeaderName.ContentLength, "0" );

    public HttpConnection ( HttpHostClient client, Stream connectionStream, HttpHost host, IPEndPoint endpoint ) {
        _client = client;
        _host = host;
        _endpoint = endpoint;
        headerParsingTimeout = (int) host.TimeoutManager.HeaderParsingTimeout.TotalMilliseconds;

        networkStream = connectionStream;

        requestPool = MemoryPool<byte>.Shared.Rent ( RESERVED_BUFFER_SIZE );
        responsePool = MemoryPool<byte>.Shared.Rent ( RESERVED_BUFFER_SIZE );
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    public async Task<HttpConnectionState> HandleConnectionEventsAsync ( CancellationToken shutdownToken ) {
        bool connectionCloseRequested = false;

#if DEBUG
        Id.Value = Random.Shared.Next ( 100_000, 999_999 );
#endif

        while (!disposedValue) {

            HttpRequestBase? nextRequest = await HttpRequestReader.TryReadHttpRequestAsync ( requestPool.Memory, networkStream, shutdownToken, headerParsingTimeout ).ConfigureAwait ( false );

            if (nextRequest is null) {
                return HttpConnectionState.ConnectionClosed;
            }

            HttpHostContext managedSession = new HttpHostContext ( _host, this, nextRequest, _client );
            await _host.InvokeContextCreated ( managedSession ).ConfigureAwait ( false );

            Logger.LogInformation ( $"HTTP {managedSession.Request.Method} {managedSession.Request.Path} Headers={managedSession.Request.Headers.Count} ConLength={managedSession.Request.ContentLength}" );

            if (!managedSession.KeepAlive || !nextRequest.CanKeepAlive) {
                connectionCloseRequested = true;
                managedSession.Response.Headers.Set ( ConnCloseHeader );
            }

            if (!managedSession.ResponseHeadersAlreadySent) {
                managedSession.Response.Headers.Set ( ContLengzHeader );

                await managedSession.WriteHttpResponseHeadersAsync ();
            }

            await networkStream.FlushAsync ().ConfigureAwait ( false );

            if (nextRequest.IsChunked || nextRequest.ContentLength > 0) {
                EndableStream? requestStream = managedSession.Request._readingStream;

                if (requestStream is null) {
                    // content was not read, we need to drain it
                    requestStream = managedSession.Request.GetRequestStreamCore ( sendExpectation: false ) as EndableStream;
                }
                if (requestStream is null) {
                    // couldn't get request stream, already drained. This should not occur; treat as error.
                    Logger.LogInformation ( $"Unexpected: request body already drained and stream is null." );
                    return HttpConnectionState.ConnectionClosed;
                }
                else if (!requestStream.IsEnded) {
                    using var ct = new CancellationTokenSource ( _host.TimeoutManager.BodyDrainTimeout );

                    if (!await requestStream.DrainAsync ( ct.Token )) {
                        // drain timed out, close connection
                        Logger.LogInformation ( $"body drained: closed on ct" );
                        return HttpConnectionState.ConnectionClosed;
                    }

                    Logger.LogInformation ( $"body drained: ended" );
                }
            }

            if (connectionCloseRequested) {
                break;
            }
        }

        return HttpConnectionState.ConnectionClosed;
    }

    private void Dispose ( bool disposing ) {
        if (!disposedValue) {
            if (disposing) {
                networkStream.Dispose ();
                requestPool.Dispose ();
                responsePool.Dispose ();
            }

            disposedValue = true;
        }
    }

    public void Dispose () {
        Dispose ( disposing: true );
        GC.SuppressFinalize ( this );
    }

    public async ValueTask DisposeAsync () {
        await networkStream.DisposeAsync ().ConfigureAwait ( false );
        requestPool.Dispose ();
        responsePool.Dispose ();
        disposedValue = true;
    }
}
