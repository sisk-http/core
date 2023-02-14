using Sisk.Core.Http;
using Sisk.Core.Routing;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace Sisk.Provider
{
    /// <summary>
    /// Provides an access to manage assembly hot reloads.
    /// </summary>
    public static class ServiceReloadManager
    {
        private static List<ServiceProvider> _services = new List<ServiceProvider>();

        /// <summary>
        /// Registers a <see cref="ServiceProvider"/> to be recompiled every time the assembly is reloaded.
        /// </summary>
        /// <param name="reloadAwareService">The service provider which will be registered.</param>
        public static void RegisterServiceProvider(ServiceProvider reloadAwareService)
        {
            _services.Add(reloadAwareService);
        }

        /// <summary>
        /// Clears all registrations from the assembly.
        /// </summary>
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
        public TextWriter? AccessLogs { get => ServerConfiguration?.AccessLogsStream; }

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
        public TextWriter? ErrorLogs { get => ServerConfiguration?.AccessLogsStream; }

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
            ConfigurationFile = configurationFile;
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
            ConfigParser.ParseConfiguration(this, true);
        }

        /// <summary>
        /// Opens and reads the configuration file, parses it and starts the HTTP server with the router and settings parsed from the file.
        /// </summary>
        /// <param name="registerReloadManager">If true, this <see cref="ServiceProvider"/> is registered in the <see cref="ServiceReloadManager"/> to support hot reloads.</param>
        public void Initialize(bool registerReloadManager)
        {
            Initialize();
            if (registerReloadManager)
            {
                ServiceReloadManager.RegisterServiceProvider(this);
            }
        }

        internal void Rebuild()
        {
            this.ServerConfiguration?.ListeningHosts.Clear();
            ConfigParser.ParseConfiguration(this, false);
        }

        /// <summary>
        /// Prevents the executable from closing automatically after starting the executable.
        /// </summary>
        /// <definition>
        /// public void Wait()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Provider
        /// </namespace>
        public void Wait()
        {
            Thread.Sleep(-1);
        }
    }
}