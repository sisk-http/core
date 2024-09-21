using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;
using Sisk.IniConfiguration.Serializer;
using System.Text;

namespace Sisk.IniConfiguration;

/// <summary>
/// Provides an INI-Document based configuration-reader pipeline.
/// </summary>
public sealed class IniConfigurationPipeline : IConfigurationReader
{
    /// <inheritdoc/>
    public void ReadConfiguration(ConfigurationContext context)
    {
        IniDocument document = IniDocument.FromFile(context.ConfigurationFile);

        string parsingNode = "";
        try
        {
            var serverSection = document.GetSection("Server");
            if (serverSection is not null)
            {
                parsingNode = "Server.Listen";
                string[] listeningPorts = serverSection.GetMany("Listen");
                context.TargetListeningHost.Ports = listeningPorts.Select(n => ListeningPort.Parse(n, null)).ToArray();

                parsingNode = "Server.Encoding";
                if (serverSection.GetOne("Encoding") is { } encoding)
                    context.Host.ServerConfiguration.DefaultEncoding = Encoding.GetEncoding(encoding);

                parsingNode = "Server.MaximumContentLength";
                if (serverSection.GetOne("MaximumContentLength") is { } MaximumContentLength)
                    context.Host.ServerConfiguration.MaximumContentLength = Int64.Parse(MaximumContentLength);

                parsingNode = "Server.IncludeRequestIdHeader";
                if (serverSection.GetOne("IncludeRequestIdHeader") is { } IncludeRequestIdHeader)
                    context.Host.ServerConfiguration.IncludeRequestIdHeader = IniReader.IniNamingComparer.Compare(IncludeRequestIdHeader, "true") == 0;

                parsingNode = "Server.ThrowExceptions";
                if (serverSection.GetOne("ThrowExceptions") is { } ThrowExceptions)
                    context.Host.ServerConfiguration.ThrowExceptions = IniReader.IniNamingComparer.Compare(ThrowExceptions, "true") == 0;

                parsingNode = "Server.AccessLogsStream";
                if (serverSection.GetOne("AccessLogsStream") is { } AccessLogsStream)
                    context.Host.ServerConfiguration.AccessLogsStream = string.Compare(AccessLogsStream, "console", true) == 0 ?
                        LogStream.ConsoleOutput : new LogStream(AccessLogsStream);

                parsingNode = "Server.ErrorsLogsStream";
                if (serverSection.GetOne("ErrorsLogsStream") is { } ErrorsLogsStream)
                    context.Host.ServerConfiguration.ErrorsLogsStream = string.Compare(ErrorsLogsStream, "console", true) == 0 ?
                        LogStream.ConsoleOutput : new LogStream(ErrorsLogsStream);
            }

            var paramsSection = document.GetSection("Parameters");
            if (paramsSection is not null)
            {
                foreach (string key in paramsSection.Keys)
                {
                    string? value = paramsSection.GetOne(key);
                    if (value is not null)
                        context.Parameters.Add(key, value);
                }
            }

            parsingNode = "Cors";
            var corsSection = document.GetSection("CORS");
            if (corsSection is not null)
            {
                parsingNode = "Cors.AllowMethods";
                if (corsSection.GetOne("AllowMethods") is { } AllowMethods)
                    context.TargetListeningHost.CrossOriginResourceSharingPolicy.AllowMethods
                        = AllowMethods.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                parsingNode = "Cors.AllowHeaders";
                if (corsSection.GetOne("AllowHeaders") is { } AllowHeaders)
                    context.TargetListeningHost.CrossOriginResourceSharingPolicy.AllowHeaders
                        = AllowHeaders.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                parsingNode = "Cors.AllowOrigins";
                if (corsSection.GetOne("AllowOrigins") is { } AllowOrigins)
                    context.TargetListeningHost.CrossOriginResourceSharingPolicy.AllowOrigins
                        = AllowOrigins.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                parsingNode = "Cors.AllowOrigin";
                if (corsSection.GetOne("AllowOrigin") is { } AllowOrigin)
                    context.TargetListeningHost.CrossOriginResourceSharingPolicy.AllowOrigin
                        = AllowOrigin;

                parsingNode = "Cors.ExposeHeaders";
                if (corsSection.GetOne("ExposeHeaders") is { } ExposeHeaders)
                    context.TargetListeningHost.CrossOriginResourceSharingPolicy.ExposeHeaders
                        = ExposeHeaders.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                parsingNode = "Cors.AllowCredentials";
                if (corsSection.GetOne("AllowCredentials") is { } AllowCredentials)
                    context.TargetListeningHost.CrossOriginResourceSharingPolicy.AllowCredentials
                        = string.Compare(AllowCredentials, "True", true) == 0;

                parsingNode = "Cors.MaxAge";
                if (corsSection.GetOne("MaxAge") is { } MaxAge)
                    context.TargetListeningHost.CrossOriginResourceSharingPolicy.MaxAge
                        = TimeSpan.FromSeconds(int.Parse(MaxAge));
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Caught exception while trying to read the property {parsingNode}: {ex.Message}", ex);
        }
    }
}
