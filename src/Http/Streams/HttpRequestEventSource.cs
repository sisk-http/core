// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestEventSource.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using Sisk.Core.Internal;

namespace Sisk.Core.Http.Streams;

/// <summary>
/// An <see cref="HttpRequestEventSource"/> instance opens a persistent connection to the request, which sends events in text/event-stream format.
/// </summary>
public sealed class HttpRequestEventSource : IDisposable {
    readonly ManualResetEvent terminatingMutex = new ManualResetEvent ( false );
    readonly HttpStreamPingPolicy pingPolicy;
    readonly HttpListenerResponse res;
    readonly HttpListenerRequest req;
    readonly HttpRequest reqObj;
    readonly HttpServer hostServer;
    readonly HttpRequestEventSourceWriter writer;
    TimeSpan keepAlive = TimeSpan.Zero;

    private bool isClosed;
    private bool isDisposed;

    /// <summary>
    /// Gets the <see cref="HttpStreamPingPolicy"/> for this HTTP event source connection.
    /// </summary>
    public HttpStreamPingPolicy PingPolicy { get => pingPolicy; }

    /// <summary>
    /// Gets the <see cref="Http.HttpRequest"/> object which created this Event Source instance.
    /// </summary>
    public HttpRequest HttpRequest => reqObj;

    /// <summary>
    /// Gets an unique identifier label to this EventStream connection, useful for finding this connection's reference later.
    /// </summary>
    public string? Identifier { get; private set; }

    /// <summary>
    /// Gets an boolean indicating if this connection is open and this instance can send messages.
    /// </summary>
    public bool IsActive { get => (isDisposed && isClosed) == false; }

    internal HttpRequestEventSource ( string? identifier, HttpListenerResponse res, HttpListenerRequest req, HttpRequest host ) {
        this.req = req;
        this.res = res;

        Identifier = identifier;

        hostServer = host.baseServer;
        reqObj = host;
        pingPolicy = new HttpStreamPingPolicy ( this );

        hostServer._eventCollection.RegisterEventSource ( this );

        res.AddHeader ( HttpKnownHeaderNames.CacheControl, "no-store, no-cache" );
        res.AddHeader ( HttpKnownHeaderNames.ContentType, "text/event-stream" );

        if (host.baseServer.ServerConfiguration.SendSiskHeader)
            res.AddHeader ( HttpKnownHeaderNames.XPoweredBy, HttpServer.PoweredBy );

        if (host.Context.MatchedRoute?.UseCors == true)
            HttpServer.SetCorsHeaders ( req, host.Context.ListeningHost?.CrossOriginResourceSharingPolicy, res );

        writer = new HttpRequestEventSourceWriter ( new StreamWriter ( res.OutputStream, req.ContentEncoding ) );
    }

    private void KeepAliveTask () {
        while (IsActive) {
            if (writer.lastSuccessfullMessage < DateTime.Now - keepAlive) {
                Dispose ();
                break;
            }
            else {
                Thread.Sleep ( 500 );
            }
        }
    }

    /// <summary>
    /// Configures the ping policy for this instance of HTTP Event Source.
    /// </summary>
    /// <param name="act">The method that runs on the ping policy for this HTTP Event Source.</param>
    public void WithPing ( Action<HttpStreamPingPolicy> act ) {
        act ( pingPolicy );
    }

    /// <summary>
    /// Sends an header to the streaming context.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    public void AppendHeader ( string name, string value ) {
        if (writer.hasSentData) {
            throw new InvalidOperationException ( SR.Httpserver_Commons_HeaderAfterContents );
        }
        res.AddHeader ( name, value );
    }

    /// <summary>
    /// Sends data to the output stream.
    /// </summary>
    /// <param name="data">The data to send, or <c>null</c> to send no data.</param>
    /// <param name="splitLines">Whether to split the data into separate lines if it contains newline characters.</param>
    /// <returns>Whether the operation was successful.</returns>
    public bool Send ( object? data, bool splitLines = false ) => SendAsync ( data?.ToString (), splitLines ).GetSyncronizedResult ();

    /// <summary>
    /// Sends an event to the output stream.
    /// </summary>
    /// <param name="eventName">The name of the event to send.</param>
    /// <returns>Whether the operation was successful.</returns>
    public bool SendEvent ( string eventName ) => SendEventAsync ( eventName ).GetSyncronizedResult ();

    /// <summary>
    /// Sends an ID to the output stream.
    /// </summary>
    /// <param name="id">The ID to send.</param>
    /// <returns>Whether the operation was successful.</returns>
    public bool SendId ( string id ) => SendIdAsync ( id ).GetSyncronizedResult ();

    /// <summary>
    /// Sends a retry-after directive to the output stream.
    /// </summary>
    /// <param name="retryAfter">The time after which the client should retry the request.</param>
    /// <returns>Whether the operation was successful.</returns>
    public bool SendRetryAfter ( TimeSpan retryAfter ) => SendRetryAfterAsync ( retryAfter ).GetSyncronizedResult ();

    /// <summary>
    /// Sends a retry-after directive to the output stream.
    /// </summary>
    /// <param name="ms">The time in milliseconds after which the client should retry the request.</param>
    /// <returns>Whether the operation was successful.</returns>
    public bool SendRetryAfter ( int ms ) => SendRetryAfterAsync ( ms ).GetSyncronizedResult ();

    /// <summary>
    /// Asynchronously sends data to the output stream.
    /// </summary>
    /// <param name="data">The data to send, or <c>null</c> to send no data.</param>
    /// <param name="splitLines">Whether to split the data into separate lines if it contains newline characters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
    public async ValueTask<bool> SendAsync ( string? data, bool splitLines = false ) {
        if (!IsActive)
            return false;

        if (splitLines && data?.Contains ( '\n' ) == true) {
            foreach (var chunk in data.Split ( '\n' )) {
                if (await writer.SendMessageAsync ( "data", chunk, breakLineAfter: false ) == false)
                    return false;
            }
            return await writer.WriteLineAsync ();
        }
        else {
            return await writer.SendMessageAsync ( "data", data, breakLineAfter: true );
        }
    }

    /// <summary>
    /// Asynchronously sends an event to the output stream.
    /// </summary>
    /// <param name="eventName">The name of the event to send.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
    public async ValueTask<bool> SendEventAsync ( string eventName ) {
        if (!IsActive)
            return false;

        return await writer.SendMessageAsync ( "event", eventName, breakLineAfter: true );
    }

    /// <summary>
    /// Asynchronously sends an ID to the output stream.
    /// </summary>
    /// <param name="id">The ID to send.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
    public async ValueTask<bool> SendIdAsync ( string id ) {
        if (!IsActive)
            return false;

        return await writer.SendMessageAsync ( "id", id, breakLineAfter: true );
    }

    /// <summary>
    /// Asynchronously sends a retry-after directive to the output stream.
    /// </summary>
    /// <param name="retryAfter">The time after which the client should retry the request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
    public ValueTask<bool> SendRetryAfterAsync ( TimeSpan retryAfter ) => SendRetryAfterAsync ( (int) retryAfter.TotalMilliseconds );

    /// <summary>
    /// Asynchronously sends a retry-after directive to the output stream.
    /// </summary>
    /// <param name="ms">The time in milliseconds after which the client should retry the request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
    public async ValueTask<bool> SendRetryAfterAsync ( int ms ) {
        if (!IsActive)
            return false;

        return await writer.SendMessageAsync ( "retry", ms.ToString ( provider: null ), breakLineAfter: true );
    }

    /// <summary>
    /// Wait until the connection is closed by the server or until some message is not delivered to the client.
    /// </summary>
    public void KeepAlive () {
        if (!IsActive) {
            throw new InvalidOperationException ( SR.HttpRequestEventSource_KeepAliveDisposed );
        }
        terminatingMutex.WaitOne ();
    }

    /// <summary>
    /// Wait until the connection is closed by the server or until some message is not delivered to the client,
    /// with a specified maximum idle tolerance.
    /// </summary>
    /// <param name="maximumIdleTolerance">The maximum idle tolerance.</param>
    public void WaitForFail ( in TimeSpan maximumIdleTolerance ) {
        if (!IsActive) {
            throw new InvalidOperationException ( SR.HttpRequestEventSource_KeepAliveDisposed );
        }
        keepAlive = maximumIdleTolerance;

        new Task ( KeepAliveTask ).Start ();

        terminatingMutex.WaitOne ();
    }

    /// <summary>
    /// Closes the event listener and it's connection.
    /// </summary>
    public HttpResponse Close () {
        if (!isClosed) {
            isClosed = true;
            hostServer._eventCollection.UnregisterEventSource ( this );
        }
        return new HttpResponse ( HttpResponse.HTTPRESPONSE_SERVER_CLOSE );
    }

    /// <summary>
    /// Cancels the sending queue from sending pending messages and clears the queue.
    /// </summary>
    [Obsolete ( "This method doens't do anything and should not be used." )]
    public void Cancel () {
        ;
    }

    /// <summary>
    /// Flushes and releases the used resources of this class instance.
    /// </summary>
    public void Dispose () {
        if (!isDisposed) {
            Close ();
            writer.Dispose ();
            terminatingMutex.Dispose ();
            isDisposed = true;
        }
    }
}
