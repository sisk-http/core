using Sisk.Core.Http;
using Sisk.Core.Routing;
using System.Collections.Specialized;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Sisk.Provider
{
    internal class ConfigParser
    {
        internal static void ParseConfiguration(ServiceProvider prov, bool softReload)
        {
            string filename = Path.GetFullPath(prov.ConfigurationFile);
            if (!File.Exists(filename))
            {
                throw new ArgumentException($"Configuration file {prov.ConfigurationFile} was not found.");
            }

            string fileContents = File.ReadAllText(filename);
            ConfigStructureFile? config = System.Text.Json.JsonSerializer.Deserialize(fileContents, typeof(ConfigStructureFile),
                new SourceGenerationContext(new System.Text.Json.JsonSerializerOptions()
                {
                    AllowTrailingCommas = true,
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip
                })) as ConfigStructureFile;

            if (config is null)
            {
                throw new Exception("Couldn't read the configuration file.");
            }

            if (prov.ServerConfiguration == null)
            {
                prov.ServerConfiguration = new HttpServerConfiguration();
            }

            prov.ServerConfiguration.ResolveForwardedOriginAddress = config.Server.ResolveForwardedOriginAddress;
            prov.ServerConfiguration.ResolveForwardedOriginHost = config.Server.ResolveForwardedOriginHost;
            prov.ServerConfiguration.DefaultEncoding = Encoding.GetEncoding(config.Server.DefaultEncoding);
            prov.ServerConfiguration.MaximumContentLength = config.Server.MaximumContentLength;
            prov.ServerConfiguration.IncludeRequestIdHeader = config.Server.IncludeRequestIdHeader;
            prov.ServerConfiguration.ThrowExceptions = config.Server.ThrowExceptions;


            if (config.Server.AccessLogsStream?.ToLower() == "console")
            {
                prov.ServerConfiguration.AccessLogsStream = Console.Out;
            }
            else if (config.Server.AccessLogsStream != null)
            {
                prov.ServerConfiguration.AccessLogsStream = new StreamWriter(config.Server.AccessLogsStream, true, prov.ServerConfiguration.DefaultEncoding)
                {
                    AutoFlush = true
                };
            }
            else
            {
                prov.ServerConfiguration.AccessLogsStream = null;
            }

            if (config.Server.ErrorsLogsStream?.ToLower() == "console")
            {
                prov.ServerConfiguration.ErrorsLogsStream = Console.Out;
            }
            else if (config.Server.ErrorsLogsStream != null)
            {
                prov.ServerConfiguration.ErrorsLogsStream = new StreamWriter(config.Server.ErrorsLogsStream, true, prov.ServerConfiguration.DefaultEncoding)
                {
                    AutoFlush = true
                };
            }
            else
            {
                prov.ServerConfiguration.ErrorsLogsStream = null;
            }

            // build parameters
            NameValueCollection parameters = new NameValueCollection();
            if (config.ListeningHost.Parameters != null)
            {
                foreach (var prop in config.ListeningHost.Parameters)
                {
                    parameters.Add(prop.Key, prop.Value?.AsValue().GetValue<object>().ToString());
                }
            }

            if (config.ListeningHost.Ports is null || config.ListeningHost.Ports.Length == 0)
            {
                throw new InvalidOperationException("The configuration file must define at least one listening host port.");
            }

            RouterFactory fac = prov.RouterFactoryInstance;
            fac.Setup(parameters);
            var router = fac.BuildRouter();

            ListeningHost host = new ListeningHost(config.ListeningHost.Hostname, config.ListeningHost.Ports);
            host.Router = router;
            host.Label = config.ListeningHost.Label;

            if (config.ListeningHost.CrossOriginResourceSharingPolicy?.MaxAge != null)
                host.CrossOriginResourceSharingPolicy.MaxAge = TimeSpan.FromSeconds((double)config.ListeningHost.CrossOriginResourceSharingPolicy.MaxAge);
            if (config.ListeningHost.CrossOriginResourceSharingPolicy?.AllowOrigins != null)
                host.CrossOriginResourceSharingPolicy.AllowOrigins = config.ListeningHost.CrossOriginResourceSharingPolicy.AllowOrigins;
            if (config.ListeningHost.CrossOriginResourceSharingPolicy?.AllowMethods != null)
                host.CrossOriginResourceSharingPolicy.AllowMethods = config.ListeningHost.CrossOriginResourceSharingPolicy.AllowMethods;
            if (config.ListeningHost.CrossOriginResourceSharingPolicy?.AllowCredentials != null)
                host.CrossOriginResourceSharingPolicy.AllowCredentials = config.ListeningHost.CrossOriginResourceSharingPolicy.AllowCredentials;
            if (config.ListeningHost.CrossOriginResourceSharingPolicy?.AllowHeaders != null)
                host.CrossOriginResourceSharingPolicy.AllowHeaders = config.ListeningHost.CrossOriginResourceSharingPolicy.AllowHeaders;
            if (config.ListeningHost.CrossOriginResourceSharingPolicy?.ExposeHeaders != null)
                host.CrossOriginResourceSharingPolicy.ExposeHeaders = config.ListeningHost.CrossOriginResourceSharingPolicy.ExposeHeaders;

            if (softReload)
            {
                prov.ServerConfiguration.ListeningHosts.Clear();
            }

            prov.ServerConfiguration.ListeningHosts.Add(host);

            if (prov.HttpServer == null)
            {
                prov.HttpServer = new HttpServer(prov.ServerConfiguration);
            }

            try
            {
                prov.HttpServer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't start the Sisk service: " + ex.Message);
                Environment.Exit(14);
            }

            if (prov.Verbose)
            {
                foreach (ListeningPort p in config.ListeningHost.Ports)
                {
                    string portStr = "";
                    if (p.Port != 443 && p.Port != 80) portStr = ":" + p.Port;
                    Console.WriteLine($"{config.ListeningHost.Label ?? "Sisk"} service is listening on {(p.Secure ? "https" : "http")}://{config.ListeningHost.Hostname}{portStr}/");
                }
            }
        }
    }

    [JsonSerializable(typeof(ConfigStructureFile))]
    internal class ConfigStructureFile
    {
        public ConfigStructureFile__ServerConfiguration Server { get; set; } = null!;
        public ConfigStructureFile__ListeningHost ListeningHost { get; set; } = null!;
    }

    [JsonSerializable(typeof(ConfigStructureFile__ServerConfiguration))]
    internal class ConfigStructureFile__ServerConfiguration
    {
        public string? AccessLogsStream { get; set; } = "console";
        public string? ErrorsLogsStream { get; set; }
        public bool ResolveForwardedOriginAddress { get; set; } = false;
        public bool ResolveForwardedOriginHost { get; set; } = false;
        public string DefaultEncoding { get; set; } = "UTF-8";
        public long MaximumContentLength { get; set; } = 0;
        public bool IncludeRequestIdHeader { get; set; } = false;
        public bool ThrowExceptions { get; set; } = true;
    }

    [JsonSerializable(typeof(ConfigStructureFile__ListeningHost__CrossOriginResourceSharingPolicy))]
    internal class ConfigStructureFile__ListeningHost__CrossOriginResourceSharingPolicy
    {
        public bool? AllowCredentials { get; set; } = null;
        public string[]? ExposeHeaders { get; set; }
        public string[]? AllowOrigins { get; set; }
        public string[]? AllowMethods { get; set; }
        public string[]? AllowHeaders { get; set; }
        public int? MaxAge { get; set; } = null;
    }

    [JsonSerializable(typeof(ConfigStructureFile__ListeningHost))]
    internal class ConfigStructureFile__ListeningHost
    {
        public string Hostname { get; set; } = "";
        public string? Label { get; set; }
        public ListeningPort[]? Ports { get; set; }

        public ConfigStructureFile__ListeningHost__CrossOriginResourceSharingPolicy? CrossOriginResourceSharingPolicy { get; set; }

        public JsonObject? Parameters { get; set; }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ConfigStructureFile))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}
