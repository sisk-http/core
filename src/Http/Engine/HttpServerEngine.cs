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
    /// Gets or sets the listening prefixes for the server.
    /// </summary>
    /// <value>
    /// An array of strings that specify the prefixes the server should listen on.
    /// </value>
    public virtual string [] ListeningPrefixes { get; } = Array.Empty<string> ();

    /// <summary>
    /// Gets the event loop mechanism used by the server.
    /// </summary>
    public abstract HttpServerEngineContextEventLoopMecanism EventLoopMecanism { get; }

    /// <summary>
    /// Sets the listening hosts for the server.
    /// </summary>
    /// <param name="hosts">The collection of <see cref="ListeningHost"/> instances that the server should listen on.</param>
    public abstract void SetListeningHosts ( IEnumerable<ListeningHost> hosts );

    /// <summary>
    /// Called when the server is being configured.
    /// </summary>
    /// <param name="server">The <see cref="HttpServer"/> instance that is being configured.</param>
    /// <param name="configuration">The <see cref="HttpServerConfiguration"/> that will be applied to the server.</param>
    /// <remarks>
    /// Override this method to customize the configuration of the server before it starts.
    /// The default implementation performs no action.
    /// </remarks>
    public virtual void OnConfiguring ( HttpServer server, HttpServerConfiguration configuration ) {
        ;
    }

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