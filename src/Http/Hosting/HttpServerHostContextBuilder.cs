// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerHostContextBuilder.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Sisk.Core.Entity;
using Sisk.Core.Http.Handlers;
using Sisk.Core.Routing;

namespace Sisk.Core.Http.Hosting;

/// <summary>
/// Represents a context constructor for <see cref="HttpServerHostContext"/>.
/// </summary>
[SuppressMessage ( "Design", "CA1001:Types that own disposable fields should be disposable",
    Justification = "This class is a builder class and IDisposable members is disposed within the result of the Build() function." )]
public sealed class HttpServerHostContextBuilder {
    private readonly HttpServerHostContext _context;
    private PortableConfigurationBuilder? _portableConfiguration;

    private readonly Router router;
    private readonly HttpServerConfiguration configuration;
    private readonly ListeningHost listeningHost;
    private readonly HttpServer server;

    /// <summary>
    /// Defines how the constructor should capture errors thrown within
    /// <see cref="UsePortableConfiguration"/> and display in the Console.
    /// </summary>
    public static HttpServerHostContextBuilderExceptionMode CatchConfigurationExceptions { get; set; } = HttpServerHostContextBuilderExceptionMode.Normal;

    internal HttpServerHostContextBuilder () {
        this.router = new Router ();
        this.configuration = new HttpServerConfiguration ();
        this.listeningHost = new ListeningHost {
            Router = this.router
        };

        this.configuration.ListeningHosts.Add ( this.listeningHost );

        this.server = new HttpServer ( this.configuration );

        this._context = new HttpServerHostContext ( this.server );
    }

    /// <summary>
    /// Gets the Server Configuration object.
    /// </summary>
    public HttpServerConfiguration ServerConfiguration { get => this._context.ServerConfiguration; }

    /// <summary>
    /// Builds an <see cref="HttpServerHostContext"/> with the specified parameters.
    /// </summary>
    public HttpServerHostContext Build () {

        this._context.HttpServer.handler._default.hostContext = this._context;

        if (this.listeningHost.Ports.Count == 0)
            this.listeningHost.Ports.Add ( ListeningPort.GetRandomPort () );

        return this._context;
    }

    /// <summary>
    /// Adds an function that will be executed immediately before starting the HTTP server.
    /// </summary>
    /// <param name="bootstrapAction">The action which will be executed before the HTTP server start.</param>
    public HttpServerHostContextBuilder UseBootstraper ( Action bootstrapAction ) {
        this._context.HttpServer.handler._default._serverBootstrapingFunctions.Add ( (bootstrapAction, "Bootstrap action") );
        return this;
    }

    /// <summary>
    /// Adds an function that will be executed immediately before starting the HTTP server.
    /// </summary>
    /// <param name="name">Defines an custom label for the bootstraping action name.</param>
    /// <param name="bootstrapAction">The action which will be executed before the HTTP server start.</param>
    public HttpServerHostContextBuilder UseBootstraper ( string name, Action bootstrapAction ) {
        this._context.HttpServer.handler._default._serverBootstrapingFunctions.Add ( (bootstrapAction, name) );
        return this;
    }

    /// <summary>
    /// Enables the portable configuration for this application, which imports settings, parameters,
    /// and other information from a JSON settings file.
    /// </summary>
    /// <remarks>
    /// This method overrides almost all of your <see cref="HttpServer.CreateBuilder()"/> configuration. To avoid this,
    /// call this method at the beginning of your builder, as the first immediate method.
    /// </remarks>
    /// <param name="portableConfigHandler">The handler of <see cref="PortableConfigurationBuilder"/>.</param>
    public HttpServerHostContextBuilder UsePortableConfiguration ( Action<PortableConfigurationBuilder> portableConfigHandler ) {
        this._portableConfiguration = new PortableConfigurationBuilder ( this._context );
        try {
            portableConfigHandler ( this._portableConfiguration );
            this._portableConfiguration.Build ();
        }
        catch (Exception ex) {
            if (CatchConfigurationExceptions == HttpServerHostContextBuilderExceptionMode.Normal) {
                Console.WriteLine ( SR.Provider_ConfigParser_CaughtException );
                Console.WriteLine ( $"{ex.GetType ().Name}: {ex.Message}" );
            }
            else if (CatchConfigurationExceptions == HttpServerHostContextBuilderExceptionMode.Silent) {
                ;
            }
            else if (CatchConfigurationExceptions == HttpServerHostContextBuilderExceptionMode.Detailed) {
                Console.WriteLine ( SR.Provider_ConfigParser_CaughtException );
                Console.WriteLine ( ex.ToString () );
            }
            else if (CatchConfigurationExceptions == HttpServerHostContextBuilderExceptionMode.Throw) {
                throw;
            }
            Environment.Exit ( 2 );
        }
        return this;
    }

    /// <summary>
    /// Sets the main <see cref="ListeningPort"/> of this host builder.
    /// </summary>
    /// <param name="port">The port the server will listen on.</param>
    public HttpServerHostContextBuilder UseListeningPort ( ushort port ) {
        var lport = new ListeningPort ( port );
        this.listeningHost.Ports.Add ( lport );
        if (!lport.IsPathRoot) {
            this.router.Prefix = lport.Path;
        }
        return this;
    }

    /// <summary>
    /// Sets the main <see cref="ListeningPort"/> of this host builder.
    /// </summary>
    /// <param name="uri">The URI component that will be parsed to the listening port format.</param>
    public HttpServerHostContextBuilder UseListeningPort ( string uri ) {
        var port = new ListeningPort ( uri );
        this.listeningHost.Ports.Add ( port );
        if (port.Path != "/") {
            this.router.Prefix = port.Path;
        }
        return this;
    }

    /// <summary>
    /// Sets the main <see cref="ListeningPort"/> of this host builder.
    /// </summary>
    /// <param name="listeningPort">The <see cref="ListeningPort"/> object which the HTTP server will listen to.</param>
    public HttpServerHostContextBuilder UseListeningPort ( ListeningPort listeningPort ) {
        this.listeningHost.Ports.Add ( listeningPort );
        if (listeningPort.Path != "/") {
            this.router.Prefix = listeningPort.Path;
        }
        return this;
    }

    /// <summary>
    /// Changes the default thread current culture through <see cref="CultureInfo.DefaultThreadCurrentCulture"/>.
    /// </summary>
    /// <param name="locale">The default <see cref="CultureInfo"/> object which the HTTP server will apply to the request handlers and callbacks thread.</param>
    public HttpServerHostContextBuilder UseLocale ( CultureInfo locale ) {
        CultureInfo.DefaultThreadCurrentCulture = locale;
        CultureInfo.DefaultThreadCurrentUICulture = locale;
        return this;
    }

    /// <summary>
    /// This method is a shortcut for setting <see cref="HttpServerConfiguration.ForwardingResolver"/>.
    /// </summary>
    /// <param name="resolver">The <see cref="ForwardingResolver"/> object.</param>
    public HttpServerHostContextBuilder UseForwardingResolver ( ForwardingResolver resolver ) {
        this._context.ServerConfiguration.ForwardingResolver = resolver;
        return this;
    }

    /// <summary>
    /// This method is a shortcut for setting <see cref="HttpServerConfiguration.ForwardingResolver"/>.
    /// </summary>
    /// <typeparam name="TForwardingResolver">The type which implements <see cref="ForwardingResolver"/>.</typeparam>
    public HttpServerHostContextBuilder UseForwardingResolver<TForwardingResolver> () where TForwardingResolver : ForwardingResolver, new() {
        return this.UseForwardingResolver ( new TForwardingResolver () );
    }

    /// <summary>
    /// Calls an action that has the HTTP server configuration as an argument.
    /// </summary>
    /// <param name="handler">An action where the first argument is an <see cref="HttpServerConfiguration"/>.</param>
    public HttpServerHostContextBuilder UseConfiguration ( Action<HttpServerConfiguration> handler ) {
        handler ( this._context.ServerConfiguration );
        return this;
    }

    /// <summary>
    /// Calls an action that has the HTTP server instance as an argument.
    /// </summary>
    /// <param name="handler">An action where the first argument is the main <see cref="HttpServer"/> object.</param>
    public HttpServerHostContextBuilder UseHttpServer ( Action<HttpServer> handler ) {
        handler ( this._context.HttpServer );
        return this;
    }

    /// <summary>
    /// Calls an action that has an <see cref="CrossOriginResourceSharingHeaders"/> instance from the main listening host as an argument.
    /// </summary>
    /// <param name="handler">An action where the first argument is the main <see cref="CrossOriginResourceSharingHeaders"/> object.</param>
    public HttpServerHostContextBuilder UseCors ( Action<CrossOriginResourceSharingHeaders> handler ) {
        this._context.CrossOriginResourceSharingPolicy ??= CrossOriginResourceSharingHeaders.Empty;

        handler ( this._context.CrossOriginResourceSharingPolicy );
        return this;
    }

    /// <summary>
    /// Sets an <see cref="CrossOriginResourceSharingHeaders"/> instance in the current listening host.
    /// </summary>
    /// <param name="cors">The <see cref="CrossOriginResourceSharingHeaders"/> to the current host builder.</param>
    public HttpServerHostContextBuilder UseCors ( CrossOriginResourceSharingHeaders cors ) {
        this._context.CrossOriginResourceSharingPolicy = cors;
        return this;
    }

    /// <summary>
    /// Calls an action that has an <see cref="Router"/> instance from the host HTTP server.
    /// </summary>
    /// <param name="handler">An action where the first argument is the main <see cref="Router"/> object.</param>
    public HttpServerHostContextBuilder UseRouter ( Action<Router> handler ) {
        this._context.HttpServer.handler._default._routerSetup = handler;
        return this;
    }

    /// <summary>
    /// Sets an <see cref="Router"/> instance in the current listening host.
    /// </summary>
    /// <param name="r">The <see cref="Router"/> to the current host builder.</param>
    public HttpServerHostContextBuilder UseRouter ( Router r ) {
        this._context.Router = r;
        return this;
    }

    /// <summary>
    /// This method is an shortcut for calling <see cref="Router.AutoScanModules{T}()"/>.
    /// </summary>
    /// <typeparam name="TModule">An class which implements <see cref="RouterModule"/>, or the router module itself.</typeparam>
    [RequiresUnreferencedCode ( SR.RequiresUnreferencedCode )]
    public HttpServerHostContextBuilder UseAutoScan<TModule> () where TModule : RouterModule {
        this._context.Router.AutoScanModules<TModule> ( typeof ( TModule ).Assembly );
        return this;
    }

    /// <summary>
    /// This method is an shortcut for calling <see cref="Router.AutoScanModules{T}()"/>.
    /// </summary>
    /// <typeparam name="TModule">An class which implements <see cref="RouterModule"/>, or the router module itself.</typeparam>
    /// <param name="t">The assembly where the scanning types are.</param>
    [RequiresUnreferencedCode ( SR.RequiresUnreferencedCode )]
    public HttpServerHostContextBuilder UseAutoScan<TModule> ( Assembly t ) where TModule : RouterModule {
        this._context.Router.AutoScanModules<TModule> ( t );
        return this;
    }

    /// <summary>
    /// This method is an shortcut for calling <see cref="HttpServer.RegisterHandler{T}"/>.
    /// </summary>
    /// <typeparam name="THandler">The handler which implements <see cref="HttpServerHandler"/>.</typeparam>
    public HttpServerHostContextBuilder UseHandler<THandler> () where THandler : HttpServerHandler, new() {
        this._context.HttpServer.RegisterHandler<THandler> ();
        return this;
    }

    /// <summary>
    /// This method is an shortcut for calling <see cref="HttpServer.RegisterHandler"/>.
    /// </summary>
    /// <param name="handler">The instance of the server handler.</param>
    public HttpServerHostContextBuilder UseHandler ( HttpServerHandler handler ) {
        this._context.HttpServer.RegisterHandler ( handler );
        return this;
    }

    /// <summary>
    /// Add an optional message to the <see cref="HttpServerHostContext"/> output verbose.
    /// </summary>
    /// <param name="startupMessage">The startup message.</param>
    public HttpServerHostContextBuilder UseStartupMessage ( string startupMessage ) {
        this._context.startupMessages.Add ( () => startupMessage );
        return this;
    }

    /// <summary>
    /// Adds a function that returns an optional initialization message to the <see cref="HttpServerHostContext"/> output verbose.
    /// </summary>
    /// <param name="startupMessage">The startup message function.</param>
    public HttpServerHostContextBuilder UseStartupMessage ( Func<string> startupMessage ) {
        this._context.startupMessages.Add ( startupMessage );
        return this;
    }
}
