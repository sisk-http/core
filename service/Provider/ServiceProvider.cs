using Sisk.Core.Entity;
using Sisk.Core.Http;
using Sisk.Core.Routing;
using System.Globalization;
using System.Reflection.Metadata;

[assembly: MetadataUpdateHandler(typeof(Sisk.Provider.ServiceReloadManager))]

namespace Sisk.Provider
{
    /// <summary>
    /// Provides an access to manage assembly hot reloads.
    /// </summary>
    /// <definition>
    /// public static class ServiceReloadManager
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public static class ServiceReloadManager
    {
        private static List<ServiceProvider> _services = new List<ServiceProvider>();

        /// <summary>
        /// Registers a <see cref="ServiceProvider"/> to be recompiled every time the assembly is reloaded.
        /// </summary>
        /// <param name="reloadAwareService">The service provider which will be registered.</param>
        /// <definition>
        /// public static void RegisterServiceProvider(ServiceProvider reloadAwareService)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public static void RegisterServiceProvider(ServiceProvider reloadAwareService)
        {
            _services.Add(reloadAwareService);
        }

        /// <summary>
        /// Clears all registrations from the assembly.
        /// </summary>
        /// <definition>
        /// public static void Clear()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public static void Clear()
        {
            _services.Clear();
        }

        static void ClearCache(Type[]? types)
        {
            ;
        }

        static void UpdateApplication(Type[]? types)
        {
            foreach (ServiceProvider pr in _services)
            {
                pr.Rebuild();
            }
        }
    }

    /// <summary>
    /// Provides a class that organizes and facilitates the porting management of a service or application that uses Sisk.
    /// </summary>
    /// <definition>
    /// public class ServiceProvider
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Provider
    /// </namespace>
    public class ServiceProvider
    {
        internal ServiceProviderConfiguratorHandler? __cfg = null;
        internal bool __handleHault = true;

        /// <summary>
        /// Gets the configured access log stream. This property is inherited from <see cref="ServerConfiguration"/>.
        /// </summary>
        /// <definition>
        /// public TextWriter? AccessLogs { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Provider
        /// </namespace>
        public LogStream? AccessLogs { get => ServerConfiguration?.AccessLogsStream; }

        /// <summary>
        /// Gets the configured error log stream. This property is inherited from <see cref="ServerConfiguration"/>.
        /// </summary>
        /// <definition>
        /// public TextWriter? ErrorLogs { get; } 
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Provider
        /// </namespace>
        public LogStream? ErrorLogs { get => ServerConfiguration?.ErrorsLogsStream; }

        /// <summary>
        /// Gets or sets the Sisk server portable configuration file.
        /// </summary>
        /// <definition>
        /// public string ConfigurationFile { get; set; } 
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Provider
        /// </namespace>
        public string ConfigurationFile { get; set; } = "service-config.json";

        /// <summary>
        /// Gets the emitted server configuration object interpreted from the configuration file.
        /// </summary>
        /// <definition>
        /// public HttpServerConfiguration? ServerConfiguration { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Provider
        /// </namespace>
        public HttpServerConfiguration? ServerConfiguration { get; internal set; }

        /// <summary>
        /// Gets the emitted HTTP server object instance interpreted from the configuration file.
        /// </summary>
        /// <definition>
        /// public HttpServer? HttpServer { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Provider
        /// </namespace>
        public HttpServer? HttpServer { get; internal set; }

        /// <summary>
        /// Gets or sets advanced configuration settings for the HTTP server initialization.
        /// </summary>
        /// <definition>
        /// public HttpServerFlags Flags { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Provider
        /// </namespace>
        public HttpServerFlags Flags { get; set; } = new HttpServerFlags();

        /// <summary>
        /// Gets an boolean indicating if the configuration was successfully interpreted and the server is functional.
        /// </summary>
        /// <definition>
        /// public bool Initialized { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Provider
        /// </namespace>
        public bool Initialized { get; internal set; }

        /// <summary>
        /// Gets or sets the <see cref="RouterFactory"/> object instance which will provide an entry point for this service.
        /// </summary>
        /// <definition>
        /// public RouterFactory RouterFactoryInstance { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Provider
        /// </namespace>
        public RouterFactory RouterFactoryInstance { get; set; }

        /// <summary>
        /// Gets or sets whether this <see cref="ServiceProvider"/> should write mensagens to console indicating if the server is listening or not.
        /// </summary>
        /// <definition>
        /// public bool Verbose { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Provider
        /// </namespace>
        public bool Verbose { get; set; } = true;

        internal void Throw(Exception exception)
        {
            if (Verbose)
            {
                Console.WriteLine($"error: {exception.Message}");
            }
            else
            {
                throw exception;
            }
        }

        /// <summary>
        /// Creates an new <see cref="ServiceProvider"/> instance with given router factory.
        /// </summary>
        /// <param name="instance">Specifies the <see cref="RouterFactory"/> object instance which will provide an entry point for this service.</param>
        /// <definition>
        /// public ServiceProvider(RouterFactory instance)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Provider
        /// </namespace>
        public ServiceProvider(RouterFactory instance)
        {
            this.RouterFactoryInstance = instance;
        }

        /// <summary>
        /// Creates an new <see cref="ServiceProvider"/> instance with given router factory and custom settings file name.
        /// </summary>
        /// <param name="instance">Specifies the <see cref="RouterFactory"/> object instance which will provide an entry point for this service.</param>
        /// <param name="configurationFile">Specifies the Sisk server portable configuration file.</param>
        /// <definition>
        /// public ServiceProvider(RouterFactory instance, string configurationFile)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Provider
        /// </namespace>
        public ServiceProvider(RouterFactory instance, string configurationFile)
        {
            this.RouterFactoryInstance = instance;
            this.ConfigurationFile = configurationFile;
        }

        /// <summary>
        /// Creates an new <see cref="ServiceProvider"/> instance with given router factory, custom settings file name and constructor callback.
        /// </summary>
        /// <param name="instance">Specifies the <see cref="RouterFactory"/> object instance which will provide an entry point for this service.</param>
        /// <param name="configurationFile">Specifies the Sisk server portable configuration file.</param>
        /// <param name="configurator">Defines the generation callback executed after the JSON file is interpreted by the server.</param>
        /// <definition>
        /// public ServiceProvider(RouterFactory instance, string configurationFile, ServiceProviderConfiguratorHandler configurator)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public ServiceProvider(RouterFactory instance, string configurationFile, ServiceProviderConfiguratorHandler configurator)
        {
            this.RouterFactoryInstance = instance;
            this.ConfigurationFile = configurationFile;
            this.__cfg = configurator;
        }

        /// <summary>
        /// Defines the generation callback executed after the JSON file is interpreted by the server and then initializes the application.
        /// </summary>
        /// <param name="configurator">The generation callback executed after the JSON file is interpreted by the server.</param>
        /// <definition>
        /// public void ConfigureInit(ServiceProviderConfiguratorHandler configurator)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void ConfigureInit(ServiceProviderConfiguratorHandler configurator)
        {
            this.__cfg = configurator;
            Initialize();
        }

        /// <summary>
        /// Opens and reads the configuration file, parses it and starts the HTTP server with the router and settings parsed from the file.
        /// </summary>
        /// <definition>
        /// public void Initialize()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Provider
        /// </namespace>
        public void Initialize()
        {
            ConfigParser.ParseConfiguration(this, false);
            if (__handleHault)
            {
                Thread.Sleep(-1);
            }
        }

        internal void Rebuild()
        {
            this.AccessLogs?.Close();
            this.ErrorLogs?.Close();
            this.ServerConfiguration?.ListeningHosts.Clear();
            ConfigParser.ParseConfiguration(this, true);
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
        /// public void UseOverrides(Action overrideCallback)
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
        /// public void UseHttpServer(Action serverCallback)
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
        /// public void UseCors(Action corsCallback)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void UseCors(Action<CrossOriginResourceSharingHeaders> corsCallback)
        {
            corsCallback(_config.ListeningHosts[0].CrossOriginResourceSharingPolicy);
        }
    }
}