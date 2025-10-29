// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerEngine.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http.Engine;
/// <summary>
/// Provides an abstract base class for HTTP server engines.
/// </summary>
public abstract class HttpServerEngine : IDisposable {

    /// <summary>
    /// Gets or sets the timeout for idle connections.
    /// </summary>
    /// <value>
    /// The <see cref="TimeSpan"/> representing the idle connection timeout.
    /// </value>
    public abstract TimeSpan IdleConnectionTimeout { get; set; }

    /// <summary>
    /// Gets the event loop mechanism used by the server.
    /// </summary>
    public abstract HttpServerEngineContextEventLoopMecanism EventLoopMecanism { get; }

    /// <summary>
    /// Adds a listening prefix to the server.
    /// </summary>
    /// <param name="prefix">The prefix to add.</param>
    public abstract void AddListeningPrefix ( string prefix );

    /// <summary>
    /// Clears all listening prefixes from the server.
    /// </summary>
    public abstract void ClearPrefixes ();

    /// <summary>
    /// Starts the HTTP server.
    /// </summary>
    public abstract void StartServer ();

    /// <summary>
    /// Stops the HTTP server.
    /// </summary>
    public abstract void StopServer ();

    /// <summary>
    /// Begins an asynchronous operation to get an HTTP context.
    /// </summary>
    /// <param name="callback">The <see cref="AsyncCallback"/> delegate.</param>
    /// <param name="state">An object that provides state information for the asynchronous operation.</param>
    /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous operation.</returns>
    public abstract IAsyncResult BeginGetContext ( AsyncCallback? callback, object? state );

    /// <summary>
    /// Ends an asynchronous operation to get an HTTP context.
    /// </summary>
    /// <param name="asyncResult">The <see cref="IAsyncResult"/> that references the pending asynchronous operation.</param>
    /// <returns>An <see cref="HttpServerEngineContext"/> representing the HTTP context.</returns>
    public abstract HttpServerEngineContext EndGetContext ( IAsyncResult asyncResult );

    /// <summary>
    /// Asynchronously obtains an <see cref="HttpServerEngineContext"/>.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to cancel the operation. The default value is <see langword="default"/>.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, containing the <see cref="HttpServerEngineContext"/>.</returns>
    public abstract Task<HttpServerEngineContext> GetContextAsync ( CancellationToken cancellationToken = default );

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public abstract void Dispose ();
}