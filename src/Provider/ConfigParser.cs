using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Sisk.Core.Entity;
using Sisk.Core.Http;
using Sisk.Core.Routing;
using System.Collections.Specialized;
using System.Text;

namespace Sisk.Provider
{
    internal class ConfigParser
    {
        private class IntTimeSpanConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(TimeSpan);
            }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                bool ok = Int32.TryParse(reader.Value?.ToString(), out int seconds);
                if (!ok) throw new Exception($"Couldn't parse {existingValue} to an valid numeric value.");
                return TimeSpan.FromSeconds(seconds);
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                ;
            }
        }

        public static void ParseConfiguration(ServiceProvider provider)
        {
            string filePath = Path.GetFullPath(provider.ConfigurationFile);
            if (!File.Exists(filePath))
            {
                provider.Throw(new Exception("Couldn't open configuration file " + provider.ConfigurationFile));
                return;
            }

            JsonLoadSettings loadSettings = new JsonLoadSettings()
            {
                CommentHandling = CommentHandling.Ignore
            };

            var serializerSettings = new JsonSerializer();
            serializerSettings.Converters.Add(new StringEnumConverter());
            serializerSettings.Converters.Add(new IntTimeSpanConverter());

            JObject jsonFile;

            try
            {
                jsonFile = JObject.Parse(File.ReadAllText(filePath), loadSettings);
            }
            catch (Exception ex)
            {
                provider.Throw(ex);
                return;
            }

            provider.ServerConfiguration = new HttpServerConfiguration();
            JToken? serverToken = jsonFile["Server"];
            if (serverToken != null)
            {
                string actualNode = "";
                #region SettingsParsing
                try
                {
                    {
                        actualNode = "DefaultEncoding";
                        string? parsingNode = serverToken[actualNode]?.Value<string>();
                        if (parsingNode != null)
                        {
                            provider.ServerConfiguration.DefaultEncoding = Encoding.GetEncoding(parsingNode);
                        }
                    };
                    {
                        actualNode = "AccessLogsStream";
                        string? parsingNode = serverToken[actualNode]?.Value<string>();
                        if (parsingNode != null)
                        {
                            if (parsingNode.ToLower() == "console")
                            {
                                provider.ServerConfiguration.AccessLogsStream = Console.Out;
                            }
                            else if (parsingNode.ToLower() == "none")
                            {
                                provider.ServerConfiguration.AccessLogsStream = null;
                            }
                            else
                            {
                                provider.ServerConfiguration.AccessLogsStream = new StreamWriter(parsingNode, true, provider.ServerConfiguration.DefaultEncoding)
                                {
                                    AutoFlush = true
                                };
                            }
                        }
                    };
                    {
                        actualNode = "ErrorsLogsStream";
                        string? parsingNode = serverToken[actualNode]?.Value<string>();
                        if (parsingNode != null)
                        {
                            if (parsingNode.ToLower() == "console")
                            {
                                provider.ServerConfiguration.ErrorsLogsStream = Console.Out;
                            }
                            else if (parsingNode.ToLower() == "none")
                            {
                                provider.ServerConfiguration.ErrorsLogsStream = null;
                            }
                            else
                            {
                                provider.ServerConfiguration.ErrorsLogsStream = new StreamWriter(parsingNode, true, provider.ServerConfiguration.DefaultEncoding)
                                {
                                    AutoFlush = true
                                };
                            }
                        }
                    };
                    {
                        actualNode = "ThrowExceptions";
                        string? parsingNode = serverToken[actualNode]?.Value<string>();
                        if (parsingNode != null)
                        {
                            provider.ServerConfiguration.ThrowExceptions = bool.Parse(parsingNode);
                        }
                    };
                    {
                        actualNode = "MaximumContentLength";
                        string? parsingNode = serverToken[actualNode]?.Value<string>();
                        if (parsingNode != null)
                        {
                            provider.ServerConfiguration.MaximumContentLength = Int64.Parse(parsingNode);
                        }
                    };
                    {
                        actualNode = "ResolveForwardedOriginAddress";
                        string? parsingNode = serverToken[actualNode]?.Value<string>();
                        if (parsingNode != null)
                        {
                            provider.ServerConfiguration.ResolveForwardedOriginAddress = bool.Parse(parsingNode);
                        }
                    };
                    {
                        actualNode = "ResolveForwardedOriginHost";
                        string? parsingNode = serverToken[actualNode]?.Value<string>();
                        if (parsingNode != null)
                        {
                            provider.ServerConfiguration.ResolveForwardedOriginHost = bool.Parse(parsingNode);
                        }
                    };
                    {
                        actualNode = "IncludeRequestIdHeader";
                        string? parsingNode = serverToken[actualNode]?.Value<string>();
                        if (parsingNode != null)
                        {
                            provider.ServerConfiguration.IncludeRequestIdHeader = bool.Parse(parsingNode);
                        }
                    };
                }
                catch (Exception ex)
                {
                    provider.Throw(new Exception($"Couldn't parse node Server.{actualNode}: {ex.Message}"));
                    return;
                }
                #endregion
            }

            JToken? hostNode = jsonFile["ListeningHost"];
            if (hostNode == null)
            {
                provider.Throw(new Exception("Couldn't find the ListeningHost node."));
                return;
            }

            string? hostname = hostNode["Hostname"]?.Value<string>();
            string? label = hostNode["Label"]?.Value<string>();
            JToken? portsNode = hostNode["Ports"];
            JToken? parameters = hostNode["Parameters"];
            JToken? corsPolicyObj = hostNode["CrossOriginResourceSharingPolicy"];
            if (portsNode == null)
            {
                provider.Throw(new Exception("Couldn't find the ListeningHost.Ports node."));
                return;
            }
            if (string.IsNullOrEmpty(hostname))
            {
                provider.Throw(new Exception("ListeningHost.Hostname node cannot be empty or null."));
                return;
            }

            CrossOriginResourceSharingHeaders? corsPolicy = new CrossOriginResourceSharingHeaders();
            if (corsPolicyObj != null)
            {
                corsPolicy = corsPolicyObj.ToObject<CrossOriginResourceSharingHeaders>(serializerSettings);
                if (corsPolicy == null)
                {
                    provider.Throw(new Exception("Failed to parse CrossOriginResourceSharingPolicy."));
                    return;
                }
            }


            NameValueCollection parametersCollection = new NameValueCollection();
            if (parameters != null)
            {
                foreach (JProperty property in parameters)
                {
                    parametersCollection.Add(property.Name, property.Value.ToString());
                }
            }

            provider.RouterFactoryInstance.Setup(parametersCollection);
            Router r = provider.RouterFactoryInstance.BuildRouter();

            List<ListeningPort> ports = new List<ListeningPort>();
            foreach (JToken obj in portsNode)
            {
                bool p0 = Int32.TryParse(obj["Port"]?.Value<string>(), out Int32 port);
                bool p1 = Boolean.TryParse(obj["Secure"]?.Value<string>(), out Boolean secure);

                if (p0 == false)
                {
                    provider.Throw(new Exception($"Listening port {p0} is an invalid port."));
                    return;
                }
                if (p1 == false)
                {
                    provider.Throw(new Exception($"Listening port {p1} secure status is invalid."));
                    return;
                }

                ListeningPort p = new ListeningPort(port, secure);
                ports.Add(p);
            }

            ListeningHost listeningHost = new ListeningHost(hostname, ports.ToArray(), r);
            listeningHost.Label = label;
            listeningHost.CrossOriginResourceSharingPolicy = corsPolicy ?? new CrossOriginResourceSharingHeaders();

            //listeningHost.CrossOriginResourceSharingPolicy

            provider.ServerConfiguration.ListeningHosts.Add(listeningHost);
            provider.HttpServer = new HttpServer(provider.ServerConfiguration);
            provider.HttpServer.Start();
            provider.Initialized = true;

            if (provider.Verbose)
            {
                foreach (ListeningPort port in ports)
                {
                    Console.WriteLine($"Service {label ?? "(unknown)"} is listening at {(port.Secure ? "https" : "http")}://{hostname}" +
                        $"{(port.Port == 443 || port.Port == 80 ? "" : ":" + port.Port)}/");
                }
            }
        }
    }
}
