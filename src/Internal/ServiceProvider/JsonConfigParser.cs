// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonConfigParser.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Sisk.Core.Internal.ServiceProvider
{
    internal sealed class JsonConfigParser : IConfigurationReader
    {
        public void ReadConfiguration(ConfigurationContext prov)
        {
            string filename = prov.ConfigurationFile;

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
                throw new Exception(SR.Provider_ConfigParser_ConfigFileInvalid);
            }

            if (config.Server != null)
            {
                prov.Host.ServerConfiguration.DefaultEncoding = Encoding.GetEncoding(config.Server.DefaultEncoding);
                prov.Host.ServerConfiguration.MaximumContentLength = config.Server.MaximumContentLength;
                prov.Host.ServerConfiguration.IncludeRequestIdHeader = config.Server.IncludeRequestIdHeader;
                prov.Host.ServerConfiguration.ThrowExceptions = config.Server.ThrowExceptions;

                if (config.Server.AccessLogsStream?.ToLower() == "console")
                {
                    prov.Host.ServerConfiguration.AccessLogsStream = LogStream.ConsoleOutput;
                }
                else if (config.Server.AccessLogsStream != null)
                {
                    prov.Host.ServerConfiguration.AccessLogsStream = new LogStream(config.Server.AccessLogsStream);
                }
                else
                {
                    prov.Host.ServerConfiguration.AccessLogsStream = null;
                }

                if (config.Server.ErrorsLogsStream?.ToLower() == "console")
                {
                    prov.Host.ServerConfiguration.ErrorsLogsStream = LogStream.ConsoleOutput;
                }
                else if (config.Server.ErrorsLogsStream != null)
                {
                    prov.Host.ServerConfiguration.ErrorsLogsStream = new LogStream(config.Server.ErrorsLogsStream);
                }
                else
                {
                    prov.Host.ServerConfiguration.ErrorsLogsStream = null;
                }
            }

            if (config.Parameters != null)
            {
                foreach (var prop in config.Parameters)
                {
                    prov.Host.Parameters.Add(prop.Key, prop.Value?.AsValue().GetValue<object>().ToString());
                }
                prov.Host.Parameters.MakeReadonly();
            }

            if (config.ListeningHost != null)
            {
                if (config.ListeningHost.Ports is null || config.ListeningHost.Ports.Length == 0)
                {
                    throw new InvalidOperationException(SR.Provider_ConfigParser_NoListeningHost);
                }

                ListeningHost host = prov.TargetListeningHost;

                host.Label = config.ListeningHost.Label;
                host.Ports = config.ListeningHost.Ports.Select(s => new ListeningPort(s)).ToArray();

                if (config.ListeningHost.CrossOriginResourceSharingPolicy?.MaxAge != null)
                    host.CrossOriginResourceSharingPolicy.MaxAge = TimeSpan.FromSeconds((double)config.ListeningHost.CrossOriginResourceSharingPolicy.MaxAge);
                if (config.ListeningHost.CrossOriginResourceSharingPolicy?.AllowOrigin != null)
                    host.CrossOriginResourceSharingPolicy.AllowOrigin = config.ListeningHost.CrossOriginResourceSharingPolicy.AllowOrigin;
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
            }
        }
    }

    [JsonSerializable(typeof(JsonObject))]
    [JsonSerializable(typeof(ConfigStructureFile__ServerConfiguration))]
    [JsonSerializable(typeof(ConfigStructureFile__ListeningHost))]
    internal partial class ConfigStructureFile : JsonSerializerContext
    {
        public ConfigStructureFile__ServerConfiguration? Server { get; set; } = null!;
        public ConfigStructureFile__ListeningHost? ListeningHost { get; set; } = null!;
        public JsonObject? Parameters { get; set; }
    }

    internal class ConfigStructureFile__ServerConfiguration
    {
        public string? AccessLogsStream { get; set; } = "console";
        public string? ErrorsLogsStream { get; set; }
        public string DefaultEncoding { get; set; } = "UTF-8";
        public int MaximumContentLength { get; set; } = 0;
        public bool IncludeRequestIdHeader { get; set; } = false;
        public bool ThrowExceptions { get; set; } = true;
    }

    [JsonSerializable(typeof(string[]))]
    internal partial class ConfigStructureFile__ListeningHost__CrossOriginResourceSharingPolicy : JsonSerializerContext
    {
        public bool? AllowCredentials { get; set; } = null;
        public string[]? ExposeHeaders { get; set; }
        public string? AllowOrigin { get; set; }
        public string[]? AllowOrigins { get; set; }
        public string[]? AllowMethods { get; set; }
        public string[]? AllowHeaders { get; set; }
        public int? MaxAge { get; set; } = null;
    }

    [JsonSerializable(typeof(ConfigStructureFile__ListeningHost__CrossOriginResourceSharingPolicy))]
    internal partial class ConfigStructureFile__ListeningHost : JsonSerializerContext
    {
        public string? Label { get; set; }
        public string[]? Ports { get; set; }

        public ConfigStructureFile__ListeningHost__CrossOriginResourceSharingPolicy? CrossOriginResourceSharingPolicy { get; set; }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ConfigStructureFile))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}