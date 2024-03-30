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
/// <definition>
/// public sealed class HttpServerHostContextBuilder
/// </definition>
/// <type>
/// Class
/// </type>
public sealed class HttpServerHostContextBuilder
{
    private readonly HttpServerHostContext _context;
    private PortableConfigurationBuilder? _portableConfiguration;

    /// <summary>
    /// Defines how the constructor should capture errors thrown within
    /// <see cref="UsePortableConfiguration"/> and display in the Console.
    /// </summary>
    /// <definition>
    /// public static HttpServerHostContextBuilderExceptionMode CatchConfigurationExceptions { get; set; }
    /// </definition>
    /// <type>
    /// Static property
    /// </type>
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

        _context = new HttpServerHostContext(server);
    }

    /// <summary>
    /// Gets or sets the Server Configuration object.
    /// </summary>
    /// <definition>
    /// public HttpServerConfiguration ServerConfiguration { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public HttpServerConfiguration ServerConfiguration { get => _context.ServerConfiguration; set => value = _context.ServerConfiguration; }

    /// <summary>
    /// Builds an <see cref="HttpServerHostContext"/> with the specified parameters.
    /// </summary>
    /// <definition>
    /// public HttpServerHostContext Build()
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public HttpServerHostContext Build()
    {
        return _context;
    }

    /// <summary>
    /// Defines a function that will be executed immediately before starting the Http server.
    /// </summary>
    /// <param name="bootstrapAction">The action which will be executed before the Http server start.</param>
    /// <definition>
    /// public void UseBootstraper(Action bootstrapAction)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseBootstraper(Action bootstrapAction)
    {
        DefaultHttpServerHandler._serverBootstraping = bootstrapAction;
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
    /// <definition>
    /// public void UsePortableConfiguration(Action{{PortableConfigurationBuilder}} portableConfigHandler)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UsePortableConfiguration(Action<PortableConfigurationBuilder> portableConfigHandler)
    {
        _portableConfiguration = new PortableConfigurationBuilder(_context);
        try
        {
            portableConfigHandler(_portableConfiguration);
            _portableConfiguration.Build();
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
    }

    /// <summary>
    /// Sets the main <see cref="ListeningPort"/> of this host builder.
    /// </summary>
    /// <param name="port">The port the server will listen on.</param>
    /// <definition>
    /// public void UseListeningPort(ushort port)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseListeningPort(ushort port)
    {
        _context.ServerConfiguration.ListeningHosts[0].Ports[0] = new ListeningPort(port);
    }

    /// <summary>
    /// Sets the main <see cref="ListeningPort"/> of this host builder.
    /// </summary>
    /// <param name="uri">The URI component that will be parsed to the listening port format.</param>
    /// <definition>
    /// public void UseListeningPort(string uri)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseListeningPort(string uri)
    {
        _context.ServerConfiguration.ListeningHosts[0].Ports[0] = new ListeningPort(uri);
    }

    /// <summary>
    /// Sets the main <see cref="ListeningPort"/> of this host builder.
    /// </summary>
    /// <param name="listeningPort">The <see cref="ListeningPort"/> object which the Http server will listen to.</param>
    /// <definition>
    /// public void UseListeningPort(ListeningPort listeningPort)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseListeningPort(ListeningPort listeningPort)
    {
        _context.ServerConfiguration.ListeningHosts[0].Ports[0] = listeningPort;
    }

    /// <summary>
    /// Overrides the <see cref="HttpServerConfiguration.DefaultCultureInfo"/> property in the HTTP server configuration.
    /// </summary>
    /// <param name="locale">The default <see cref="CultureInfo"/> object which the HTTP server will apply to the request handlers and callbacks thread.</param>
    /// <definition>
    /// public void UseLocale(CultureInfo locale)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseLocale(CultureInfo locale)
    {
        _context.ServerConfiguration.DefaultCultureInfo = locale;
    }

    /// <summary>
    /// Overrides the HTTP server flags with the provided flags.
    /// </summary>
    /// <param name="flags">The flags that will be set on the HTTP server.</param>
    /// <definition>
    /// public void UseFlags(HttpServerFlags flags)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseFlags(HttpServerFlags flags)
    {
        _context.ServerConfiguration.Flags = flags;
    }

    /// <summary>
    /// Calls an action that has the HTTP server configuration as an argument.
    /// </summary>
    /// <param name="handler">An action where the first argument is an <see cref="HttpServerConfiguration"/>.</param>
    /// <definition>
    /// public void UseOverrides(Action{{HttpServerConfiguration}} handler)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseConfiguration(Action<HttpServerConfiguration> handler)
    {
        handler(_context.ServerConfiguration);
    }

    /// <summary>
    /// Calls an action that has the HTTP server instance as an argument.
    /// </summary>
    /// <param name="handler">An action where the first argument is the main <see cref="HttpServer"/> object.</param>
    /// <definition>
    /// public void UseHttpServer(Action{{HttpServer}} handler)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseHttpServer(Action<HttpServer> handler)
    {
        handler(_context.HttpServer);
    }

    /// <summary>
    /// Calls an action that has an <see cref="CrossOriginResourceSharingHeaders"/> instance from the main listening host as an argument.
    /// </summary>
    /// <param name="handler">An action where the first argument is the main <see cref="CrossOriginResourceSharingHeaders"/> object.</param>
    /// <definition>
    /// public void UseCors(Action{{CrossOriginResourceSharingHeaders}} handler)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseCors(Action<CrossOriginResourceSharingHeaders> handler)
    {
        if (_context.CrossOriginResourceSharingPolicy is null)
            _context.CrossOriginResourceSharingPolicy = CrossOriginResourceSharingHeaders.Empty;

        handler(_context.CrossOriginResourceSharingPolicy);
    }

    /// <summary>
    /// Calls an action that has an <see cref="Router"/> instance from the host HTTP server.
    /// </summary>
    /// <param name="handler">An action where the first argument is the main <see cref="Router"/> object.</param>
    /// <definition>
    /// public void UseRouter(Action{{Router}} handler)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseRouter(Action<Router> handler)
    {
        DefaultHttpServerHandler._routerSetup = handler;
    }

    /// <summary>
    /// This method is an shortcut for calling <see cref="Router.AutoScanModules{T}()"/>.
    /// </summary>
    /// <typeparam name="TModule">An class which implements <see cref="RouterModule"/>, or the router module itself.</typeparam>
    /// <param name="activateInstances">Optional. Determines whether found types should be defined as instances or static members.</param>
    /// <definition>
    /// public void UseAutoScan{{TModule}}() where TModule : RouterModule
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    [RequiresUnreferencedCode(SR.Router_AutoScanModules_RequiresUnreferencedCode)]
    public void UseAutoScan<TModule>(bool activateInstances = true) where TModule : RouterModule
    {
        _context.Router.AutoScanModules<TModule>(typeof(TModule).Assembly, activateInstances);
    }

    /// <summary>
    /// This method is an shortcut for calling <see cref="Router.AutoScanModules{T}()"/>.
    /// </summary>
    /// <typeparam name="TModule">An class which implements <see cref="RouterModule"/>, or the router module itself.</typeparam>
    /// <param name="t">The assembly where the scanning types are.</param>
    /// <param name="activateInstances">Optional. Determines whether found types should be defined as instances or static members.</param>
    /// <definition>
    /// public void UseAutoScan{{TModule}}() where TModule : RouterModule
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    [RequiresUnreferencedCode(SR.Router_AutoScanModules_RequiresUnreferencedCode)]
    public void UseAutoScan<TModule>(Assembly t, bool activateInstances = true) where TModule : RouterModule
    {
        _context.Router.AutoScanModules<TModule>(t, activateInstances);
    }

    /// <summary>
    /// This method is an shortcut for calling <see cref="HttpServer.RegisterHandler{T}"/>.
    /// </summary>
    /// <typeparam name="THandler">The handler which implements <see cref="HttpServerHandler"/>.</typeparam>
    /// <definition>
    /// public void UseHandler{{[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] THandler}}() where THandler : HttpServerHandler, new()
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] THandler>() where THandler : HttpServerHandler, new()
    {
        _context.HttpServer.RegisterHandler<THandler>();
    }

    /// <summary>
    /// This method is an shortcut for calling <see cref="HttpServer.RegisterHandler"/>.
    /// </summary>
    /// <param name="handler">The instance of the server handler.</param>
    /// <definition>
    /// public void UseHandler(HttpServerHandler handler)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseHandler(HttpServerHandler handler)
    {
        _context.HttpServer.RegisterHandler(handler);
    }
}
