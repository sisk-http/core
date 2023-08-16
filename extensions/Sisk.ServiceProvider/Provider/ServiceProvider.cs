// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ServiceProvider.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Routing;


namespace Sisk.ServiceProvider
{
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
}