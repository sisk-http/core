﻿// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonConfigParser.cs
// Repository:  https://github.com/sisk-http/core

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;

namespace Sisk.Core.Internal.ServiceProvider {
    internal sealed class JsonConfigParser : IConfigurationReader {

        public void ReadConfiguration ( ConfigurationContext prov ) {
            string filename = prov.ConfigurationFile;
            string fileContents = File.ReadAllText ( filename );

            if (JsonSerializer.Deserialize<ConfigStructureFile> ( fileContents, JsonConfigJsonSerializer.Default.ConfigStructureFile )
                is not { } config) {

                throw new InvalidOperationException ( SR.Provider_ConfigParser_ConfigFileInvalid );
            }

            if (config.Server != null) {
                prov.Host.ServerConfiguration.MaximumContentLength = config.Server.MaximumContentLength;
                prov.Host.ServerConfiguration.IncludeRequestIdHeader = config.Server.IncludeRequestIdHeader;
                prov.Host.ServerConfiguration.ThrowExceptions = config.Server.ThrowExceptions;

                if (config.Server.AccessLogsStream?.ToLowerInvariant () == "console") {
                    prov.Host.ServerConfiguration.AccessLogsStream = LogStream.ConsoleOutput;
                }
                else if (config.Server.AccessLogsStream != null) {
                    prov.Host.ServerConfiguration.AccessLogsStream = new LogStream ( config.Server.AccessLogsStream );
                }
                else {
                    prov.Host.ServerConfiguration.AccessLogsStream = null;
                }

                if (config.Server.ErrorsLogsStream?.ToLowerInvariant () == "console") {
                    prov.Host.ServerConfiguration.ErrorsLogsStream = LogStream.ConsoleOutput;
                }
                else if (config.Server.ErrorsLogsStream != null) {
                    prov.Host.ServerConfiguration.ErrorsLogsStream = new LogStream ( config.Server.ErrorsLogsStream );
                }
                else {
                    prov.Host.ServerConfiguration.ErrorsLogsStream = null;
                }
            }

            if (config.Parameters != null) {
                foreach (var prop in config.Parameters) {
                    prov.Host.Parameters.Add ( prop.Key, prop.Value );
                }
                prov.Host.Parameters.MakeReadonly ();
            }

            if (config.ListeningHost != null) {
                if (config.ListeningHost.Ports is null || config.ListeningHost.Ports.Length == 0) {
                    throw new InvalidOperationException ( SR.Provider_ConfigParser_NoListeningHost );
                }

                ListeningHost host = prov.TargetListeningHost;

                host.Label = config.ListeningHost.Label;
                host.Ports = config.ListeningHost.Ports.Select ( s => new ListeningPort ( s ) ).ToArray ();

                if (config.ListeningHost.CrossOriginResourceSharingPolicy?.MaxAge != null)
                    host.CrossOriginResourceSharingPolicy.MaxAge = TimeSpan.FromSeconds ( (double) config.ListeningHost.CrossOriginResourceSharingPolicy.MaxAge );
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

    [JsonSourceGenerationOptions ( AllowTrailingCommas = true,
        ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
        DictionaryKeyPolicy = JsonKnownNamingPolicy.Unspecified )]
    [JsonSerializable ( typeof ( JsonObject ) )]
    [JsonSerializable ( typeof ( ConfigStructureFile ) )]
    [JsonSerializable ( typeof ( ConfigStructureFile__ServerConfiguration ) )]
    [JsonSerializable ( typeof ( ConfigStructureFile__ListeningHost ) )]
    [JsonSerializable ( typeof ( ConfigStructureFile__ListeningHost__CrossOriginResourceSharingPolicy ) )]
    internal sealed partial class JsonConfigJsonSerializer : JsonSerializerContext {
    }

    internal sealed class ConfigStructureFile {
        public ConfigStructureFile__ServerConfiguration? Server { get; set; } = null!;
        public ConfigStructureFile__ListeningHost? ListeningHost { get; set; } = null!;
        public Dictionary<string, string>? Parameters { get; set; }
    }

    internal sealed class ConfigStructureFile__ServerConfiguration {
        public string? AccessLogsStream { get; set; } = "console";
        public string? ErrorsLogsStream { get; set; }
        public int MaximumContentLength { get; set; }
        public bool IncludeRequestIdHeader { get; set; }
        public bool ThrowExceptions { get; set; } = true;
    }

    internal sealed class ConfigStructureFile__ListeningHost__CrossOriginResourceSharingPolicy {
        public bool? AllowCredentials { get; set; }
        public string []? ExposeHeaders { get; set; }
        public string? AllowOrigin { get; set; }
        public string []? AllowOrigins { get; set; }
        public string []? AllowMethods { get; set; }
        public string []? AllowHeaders { get; set; }
        public int? MaxAge { get; set; }
    }

    internal sealed class ConfigStructureFile__ListeningHost {
        public string? Label { get; set; }
        public string []? Ports { get; set; }

        public ConfigStructureFile__ListeningHost__CrossOriginResourceSharingPolicy? CrossOriginResourceSharingPolicy { get; set; }
    }
}