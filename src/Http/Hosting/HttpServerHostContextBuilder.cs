// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerHostContextBuilder.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using Sisk.Core.Http.Handlers;
using Sisk.Core.Routing;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Sisk.Core.Http.Hosting;

/// <summary>
/// Represents a context constructor for <see cref="HttpServerHostContext"/>.
/// </summary>
public sealed class HttpServerHostContextBuilder
{
    private readonly HttpServerHostContext _context;
    private PortableConfigurationBuilder? _portableConfiguration;

    /// <summary>
    /// Defines how the constructor should capture errors thrown within
    /// <see cref="UsePortableConfiguration"/> and display in the Console.
    /// </summary>
    public static HttpServerHostContextBuilderExceptionMode CatchConfigurationExceptions { get; set; } = HttpServerHostContextBuilderExceptionMode.Normal;

    internal HttpServerHostContextBuilder()
    {
        Router router = new Router();
        HttpServerConfiguration configuration = new HttpServerConfiguration();
        ListeningHost listeningHost = new ListeningHost();
        ListeningPort listeningPort = ListeningPort.GetRandomPort();

        listeningHost.Ports = new ListeningPort[] { listeningPort };
        listeningHost.Router = router;
        configuration.ListeningHosts.Add(listeningHost);

        HttpServer server = new HttpServer(configuration);

        this._context = new HttpServerHostContext(server);
    }

    /// <summary>
    /// Gets the Server Configuration object.
    /// </summary>
    public HttpServerConfiguration ServerConfiguration { get => this._context.ServerConfiguration; }

    /// <summary>
    /// Builds an <see cref="HttpServerHostContext"/> with the specified parameters.
    /// </summary>
    public HttpServerHostContext Build()
    {
        return this._context;
    }

    /// <summary>
    /// Defines a function that will be executed immediately before starting the HTTP server.
    /// </summary>
    /// <param name="bootstrapAction">The action which will be executed before the HTTP server start.</param>
    public HttpServerHostContextBuilder UseBootstraper(Action bootstrapAction)
    {
        this._context.HttpServer.handler._default._serverBootstraping = bootstrapAction;
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
    public HttpServerHostContextBuilder UsePortableConfiguration(Action<PortableConfigurationBuilder> portableConfigHandler)
    {
        this._portableConfiguration = new PortableConfigurationBuilder(this._context);
        try
        {
            portableConfigHandler(this._portableConfiguration);
            this._portableConfiguration.Build();
        }
        catch (Exception ex)
        {
            if (CatchConfigurationExceptions == HttpServerHostContextBuilderExceptionMode.Normal)
            {
                Console.WriteLine(SR.Provider_ConfigParser_CaughtException);
                Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            }
            else if (CatchConfigurationExceptions == HttpServerHostContextBuilderExceptionMode.Silent)
            {
                ;
            }
            else if (CatchConfigurationExceptions == HttpServerHostContextBuilderExceptionMode.Detailed)
            {
                Console.WriteLine(SR.Provider_ConfigParser_CaughtException);
                Console.WriteLine(ex.ToString());
            }
            else if (CatchConfigurationExceptions == HttpServerHostContextBuilderExceptionMode.Throw)
            {
                throw;
            }
            Environment.Exit(2);
        }
        return this;
    }

    /// <summary>
    /// Sets the main <see cref="ListeningPort"/> of this host builder.
    /// </summary>
    /// <param name="port">The port the server will listen on.</param>
    public HttpServerHostContextBuilder UseListeningPort(ushort port)
    {
        this._context.ServerConfiguration.ListeningHosts[0].Ports[0] = new ListeningPort(port);
        return this;
    }

    /// <summary>
    /// Sets the main <see cref="ListeningPort"/> of this host builder.
    /// </summary>
    /// <param name="uri">The URI component that will be parsed to the listening port format.</param>
    public HttpServerHostContextBuilder UseListeningPort(string uri)
    {
        this._context.ServerConfiguration.ListeningHosts[0].Ports[0] = new ListeningPort(uri);
        return this;
    }

    /// <summary>
    /// Sets the main <see cref="ListeningPort"/> of this host builder.
    /// </summary>
    /// <param name="listeningPort">The <see cref="ListeningPort"/> object which the HTTP server will listen to.</param>
    public HttpServerHostContextBuilder UseListeningPort(ListeningPort listeningPort)
    {
        this._context.ServerConfiguration.ListeningHosts[0].Ports[0] = listeningPort;
        return this;
    }

    /// <summary>
    /// Overrides the <see cref="HttpServerConfiguration.DefaultCultureInfo"/> property in the HTTP server configuration.
    /// </summary>
    /// <param name="locale">The default <see cref="CultureInfo"/> object which the HTTP server will apply to the request handlers and callbacks thread.</param>
    public HttpServerHostContextBuilder UseLocale(CultureInfo locale)
    {
        this._context.ServerConfiguration.DefaultCultureInfo = locale;
        return this;
    }

    /// <summary>
    /// Overrides the HTTP server flags with the provided flags.
    /// </summary>
    /// <param name="flags">The flags that will be set on the HTTP server.</param>
    public HttpServerHostContextBuilder UseFlags(HttpServerFlags flags)
    {
        this._context.ServerConfiguration.Flags = flags;
        return this;
    }

    /// <summary>
    /// This method is a shortcut for setting <see cref="HttpServerConfiguration.ForwardingResolver"/>.
    /// </summary>
    /// <param name="resolver">The <see cref="ForwardingResolver"/> object.</param>
    public HttpServerHostContextBuilder UseForwardingResolver(ForwardingResolver resolver)
    {
        this._context.ServerConfiguration.ForwardingResolver = resolver;
        return this;
    }

    /// <summary>
    /// This method is a shortcut for setting <see cref="HttpServerConfiguration.ForwardingResolver"/>.
    /// </summary>
    /// <typeparam name="TForwardingResolver">The type which implements <see cref="ForwardingResolver"/>.</typeparam>
    public HttpServerHostContextBuilder UseForwardingResolver<TForwardingResolver>() where TForwardingResolver : ForwardingResolver, new()
    {
        return this.UseForwardingResolver(new TForwardingResolver());
    }

    /// <summary>
    /// Calls an action that has the HTTP server configuration as an argument.
    /// </summary>
    /// <param name="handler">An action where the first argument is an <see cref="HttpServerConfiguration"/>.</param>
    public HttpServerHostContextBuilder UseConfiguration(Action<HttpServerConfiguration> handler)
    {
        handler(this._context.ServerConfiguration);
        return this;
    }

    /// <summary>
    /// Calls an action that has the HTTP server instance as an argument.
    /// </summary>
    /// <param name="handler">An action where the first argument is the main <see cref="HttpServer"/> object.</param>
    public HttpServerHostContextBuilder UseHttpServer(Action<HttpServer> handler)
    {
        handler(this._context.HttpServer);
        return this;
    }

    /// <summary>
    /// Calls an action that has an <see cref="CrossOriginResourceSharingHeaders"/> instance from the main listening host as an argument.
    /// </summary>
    /// <param name="handler">An action where the first argument is the main <see cref="CrossOriginResourceSharingHeaders"/> object.</param>
    public HttpServerHostContextBuilder UseCors(Action<CrossOriginResourceSharingHeaders> handler)
    {
        if (this._context.CrossOriginResourceSharingPolicy is null)
            this._context.CrossOriginResourceSharingPolicy = CrossOriginResourceSharingHeaders.Empty;

        handler(this._context.CrossOriginResourceSharingPolicy);
        return this;
    }

    /// <summary>
    /// Sets an <see cref="CrossOriginResourceSharingHeaders"/> instance in the current listening host.
    /// </summary>
    /// <param name="cors">The <see cref="CrossOriginResourceSharingHeaders"/> to the current host builder.</param>
    public HttpServerHostContextBuilder UseCors(CrossOriginResourceSharingHeaders cors)
    {
        this._context.CrossOriginResourceSharingPolicy = cors;
        return this;
    }

    /// <summary>
    /// Calls an action that has an <see cref="Router"/> instance from the host HTTP server.
    /// </summary>
    /// <param name="handler">An action where the first argument is the main <see cref="Router"/> object.</param>
    public HttpServerHostContextBuilder UseRouter(Action<Router> handler)
    {
        this._context.HttpServer.handler._default._routerSetup = handler;
        return this;
    }

    /// <summary>
    /// Sets an <see cref="Router"/> instance in the current listening host.
    /// </summary>
    /// <param name="r">The <see cref="Router"/> to the current host builder.</param>
    public HttpServerHostContextBuilder UseRouter(Router r)
    {
        this._context.Router = r;
        return this;
    }

    /// <summary>
    /// This method is an shortcut for calling <see cref="Router.AutoScanModules{T}()"/>.
    /// </summary>
    /// <typeparam name="TModule">An class which implements <see cref="RouterModule"/>, or the router module itself.</typeparam>
    /// <param name="activateInstances">Optional. Determines whether found types should be defined as instances or static members.</param>
    [RequiresUnreferencedCode(SR.Router_AutoScanModules_RequiresUnreferencedCode)]
    public HttpServerHostContextBuilder UseAutoScan<TModule>(bool activateInstances = true) where TModule : RouterModule
    {
        this._context.Router.AutoScanModules<TModule>(typeof(TModule).Assembly, activateInstances);
        return this;
    }

    /// <summary>
    /// This method is an shortcut for calling <see cref="Router.AutoScanModules{T}()"/>.
    /// </summary>
    /// <typeparam name="TModule">An class which implements <see cref="RouterModule"/>, or the router module itself.</typeparam>
    /// <param name="t">The assembly where the scanning types are.</param>
    /// <param name="activateInstances">Optional. Determines whether found types should be defined as instances or static members.</param>
    [RequiresUnreferencedCode(SR.Router_AutoScanModules_RequiresUnreferencedCode)]
    public HttpServerHostContextBuilder UseAutoScan<TModule>(Assembly t, bool activateInstances = true) where TModule : RouterModule
    {
        this._context.Router.AutoScanModules<TModule>(t, activateInstances);
        return this;
    }

    /// <summary>
    /// This method is an shortcut for calling <see cref="HttpServer.RegisterHandler{T}"/>.
    /// </summary>
    /// <typeparam name="THandler">The handler which implements <see cref="HttpServerHandler"/>.</typeparam>
    public HttpServerHostContextBuilder UseHandler<THandler>() where THandler : HttpServerHandler, new()
    {
        this._context.HttpServer.RegisterHandler<THandler>();
        return this;
    }

    /// <summary>
    /// This method is an shortcut for calling <see cref="HttpServer.RegisterHandler"/>.
    /// </summary>
    /// <param name="handler">The instance of the server handler.</param>
    public HttpServerHostContextBuilder UseHandler(HttpServerHandler handler)
    {
        this._context.HttpServer.RegisterHandler(handler);
        return this;
    }

    /// <summary>
    /// Add an optional message to the <see cref="HttpServerHostContext"/> output verbose.
    /// </summary>
    /// <param name="startupMessage">The startup message.</param>
    public HttpServerHostContextBuilder UseStartupMessage(string startupMessage)
    {
        this._context.startupMessages.AppendLine(startupMessage);
        return this;
    }
}
