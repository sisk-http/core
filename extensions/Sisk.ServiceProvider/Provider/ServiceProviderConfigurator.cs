using Sisk.Core.Entity;
using Sisk.Core.Http;
using Sisk.Core.Sessions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.ServiceProvider;

/// <summary>
/// Represents the configurator that associates post-interpretation generation functions on the server.
/// </summary>
/// <definition>
/// public class ServiceProviderConfigurator
/// </definition>
/// <type>
/// Class
/// </type>
public class ServiceProviderConfigurator
{
    private HttpServer _server;
    private HttpServerConfiguration _config;
    private ServiceProvider _provider;

    internal ServiceProviderConfigurator(HttpServer server, HttpServerConfiguration config, ServiceProvider provider)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>
    /// Calls a callback over the <see cref="SessionConfiguration"/> in the HTTP configuration.
    /// </summary>
    /// <param name="config">An action where the first argument is an <see cref="SessionConfiguration"/>.</param>
    /// <definition>
    /// public void UseSessions(Action{{SessionConfiguration}} config)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseSessions(Action<SessionConfiguration> config)
    {
        _config.SessionConfiguration.Enabled = true;
        config(_config.SessionConfiguration);
    }

    /// <summary>
    /// Determines if the HTTP server should handle the application hauting for blocking the main loop or not.
    /// </summary>
    /// <definition>
    /// public void UseHauting(bool preventHauting)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseHauting(bool preventHauting)
    {
        _provider.__handleHault = preventHauting;
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
        _config.DefaultCultureInfo = locale;
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
        _config.Flags = flags;
    }

    /// <summary>
    /// Calls a callback that has the HTTP server configuration as an argument.
    /// </summary>
    /// <param name="overrideCallback">An action where the first argument is an <see cref="HttpServerConfiguration"/>.</param>
    /// <definition>
    /// public void UseOverrides(Action{{HttpServerConfiguration}} overrideCallback)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseConfiguration(Action<HttpServerConfiguration> overrideCallback)
    {
        overrideCallback(_config);
    }

    /// <summary>
    /// Calls a callback that has the HTTP server instance as an argument.
    /// </summary>
    /// <param name="serverCallback">An action where the first argument is the main <see cref="HttpServer"/> object.</param>
    /// <definition>
    /// public void UseHttpServer(Action{{HttpServer}} serverCallback)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseHttpServer(Action<HttpServer> serverCallback)
    {
        serverCallback(_server);
    }

    /// <summary>
    /// Calls a callback that has <see cref="CrossOriginResourceSharingHeaders"/> instance from the main listening host as an argument.
    /// </summary>
    /// <param name="corsCallback">An action where the first argument is the main <see cref="CrossOriginResourceSharingHeaders"/> object.</param>
    /// <definition>
    /// public void UseCors(Action{{CrossOriginResourceSharingHeaders}} corsCallback)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void UseCors(Action<CrossOriginResourceSharingHeaders> corsCallback)
    {
        corsCallback(_config.ListeningHosts[0].CrossOriginResourceSharingPolicy);
    }
}

/// <summary>
/// Represents the callback that runs after the server interprets the settings file.
/// </summary>
/// <param name="configurator">The generation callback executed after the JSON file is interpreted by the server.</param>
/// <definition>
/// public delegate void ServiceProviderConfiguratorHandler(ServiceProviderConfigurator configurator);
/// </definition>
/// <type>
/// Delegate
/// </type>
public delegate void ServiceProviderConfiguratorHandler(ServiceProviderConfigurator configurator);