// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerHostContext.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using Sisk.Core.Routing;

namespace Sisk.Core.Http.Hosting;

/// <summary>
/// Represents the class that hosts most of the components needed to run a Sisk application.
/// </summary>
public sealed class HttpServerHostContext : IDisposable {
    internal List<Func<string>> startupMessages = new ();

    /// <summary>
    /// Gets the initialization parameters from the portable configuration file.
    /// </summary>
    public InitializationParameterCollection Parameters { get; } = new InitializationParameterCollection ();

    /// <summary>
    /// Gets the host HTTP server.
    /// </summary>
    public HttpServer HttpServer { get; private set; }

    /// <summary>
    /// Gets the host server configuration.
    /// </summary>
    public HttpServerConfiguration ServerConfiguration { get => this.HttpServer.ServerConfiguration; }

    /// <summary>
    /// Gets the host <see cref="CrossOriginResourceSharingPolicy"/>.
    /// </summary>
    public CrossOriginResourceSharingHeaders CrossOriginResourceSharingPolicy {
        get => this.HttpServer.ServerConfiguration.ListeningHosts [ 0 ].CrossOriginResourceSharingPolicy;
        set => this.HttpServer.ServerConfiguration.ListeningHosts [ 0 ].CrossOriginResourceSharingPolicy = value;
    }

    /// <summary>
    /// Gets the host router.
    /// </summary>
    public Router Router {
        get => this.ServerConfiguration.ListeningHosts [ 0 ].Router!;
        set => this.ServerConfiguration.ListeningHosts [ 0 ].Router = value;
    }

    /// <summary>
    /// Gets the configured access log stream. This property is inherited from <see cref="ServerConfiguration"/>.
    /// </summary>
    public LogStream? AccessLogs { get => this.ServerConfiguration?.AccessLogsStream; }

    /// <summary>
    /// Gets the configured error log stream. This property is inherited from <see cref="ServerConfiguration"/>.
    /// </summary>
    public LogStream? ErrorLogs { get => this.ServerConfiguration?.ErrorsLogsStream; }

    internal HttpServerHostContext ( HttpServer httpServer ) {
        this.HttpServer = httpServer ?? throw new ArgumentNullException ( nameof ( httpServer ) );
    }

    /// <summary>
    /// Starts the HTTP server.
    /// </summary>
    /// <param name="preventHault">Optional. Specifies if the application should pause the main application loop.</param>
    /// <param name="verbose">Optional. Specifies if the application should write the listening prefix welcome message.</param>
    public void Start ( bool verbose = true, bool preventHault = true ) {
        this.HttpServer.Start ();

        if (verbose) {
            Console.WriteLine ( SR.Httpserver_StartMessage );
            foreach (string prefix in this.HttpServer.ListeningPrefixes)
                Console.WriteLine ( "- {0}", prefix );

            foreach (var startupMessage in this.startupMessages) {
                Console.WriteLine ( startupMessage () );
            }
        }

        if (this.Router.GetDefinedRoutes ().Length == 0) {
            Console.WriteLine ( $"Warning: {SR.Httpserver_Warning_NoRoutes}" );
        }

        if (preventHault)
            Thread.Sleep ( -1 );
    }

    /// <summary>
    /// Asynchronously starts the HTTP server.
    /// </summary>
    /// <param name="preventHault">Optional. Specifies if the application should pause the main application loop.</param>
    /// <param name="verbose">Optional. Specifies if the application should write the listening prefix welcome message.</param>
    public Task StartAsync ( bool verbose = true, bool preventHault = true ) {
        return Task.Run ( () => this.Start ( verbose, preventHault ) );
    }

    /// <summary>
    /// Invalidates this class and releases the resources used by it, and permanently closes the HTTP server.
    /// </summary>
    public void Dispose () {
        this.HttpServer.Dispose ();
    }
}
