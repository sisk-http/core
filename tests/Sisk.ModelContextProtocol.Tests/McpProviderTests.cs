using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using LightJson;
using LightJson.Schema;
using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;
using Sisk.ModelContextProtocol;

namespace Sisk.ModelContextProtocol.Tests;

[TestClass]
public sealed class McpProviderTests {

    [TestMethod]
    public async Task Discover_ReturnsModernResultWithoutSession () {
        using var host = new TestHost ( new McpProvider ( "test", "Test", new Version ( 2, 0 ) ) );
        using HttpResponseMessage response = await host.SendModernAsync ( "server/discover", "{}" );
        using JsonDocument document = await JsonDocument.ParseAsync ( await response.Content.ReadAsStreamAsync () );

        Assert.AreEqual ( HttpStatusCode.OK, response.StatusCode );
        Assert.IsFalse ( response.Headers.Contains ( "Mcp-Session-Id" ) );
        JsonElement result = document.RootElement.GetProperty ( "result" );
        Assert.AreEqual ( "complete", result.GetProperty ( "resultType" ).GetString () );
        CollectionAssert.AreEqual (
            new [] { McpProvider.PROTOCOL_VERSION },
            result.GetProperty ( "supportedVersions" ).EnumerateArray ().Select ( x => x.GetString () ).ToArray () );
        Assert.AreEqual ( "test", result.GetProperty ( "_meta" ).GetProperty ( "io.modelcontextprotocol/serverInfo" ).GetProperty ( "name" ).GetString () );
        Assert.AreEqual ( 0, result.GetProperty ( "ttlMs" ).GetInt32 () );
        Assert.IsFalse ( result.GetProperty ( "capabilities" ).TryGetProperty ( "extensions", out _ ) );
    }

    [TestMethod]
    public async Task ModernRequest_RejectsMissingOrUnsupportedVersion () {
        using var host = new TestHost ( new McpProvider () );
        using HttpRequestMessage missingHeader = host.CreateRequest ( "tools/list", "{}", McpProvider.PROTOCOL_VERSION );
        missingHeader.Headers.Remove ( "MCP-Protocol-Version" );
        using HttpResponseMessage missingResponse = await host.Client.SendAsync ( missingHeader );
        using JsonDocument missingDocument = await JsonDocument.ParseAsync ( await missingResponse.Content.ReadAsStreamAsync () );

        Assert.AreEqual ( HttpStatusCode.BadRequest, missingResponse.StatusCode );
        Assert.AreEqual ( -32020, missingDocument.RootElement.GetProperty ( "error" ).GetProperty ( "code" ).GetInt32 () );

        using HttpResponseMessage unsupportedResponse = await host.SendModernAsync ( "tools/list", "{}", "2099-01-01" );
        using JsonDocument unsupportedDocument = await JsonDocument.ParseAsync ( await unsupportedResponse.Content.ReadAsStreamAsync () );

        Assert.AreEqual ( HttpStatusCode.BadRequest, unsupportedResponse.StatusCode );
        Assert.AreEqual ( -32022, unsupportedDocument.RootElement.GetProperty ( "error" ).GetProperty ( "code" ).GetInt32 () );
        Assert.AreEqual ( 1, unsupportedDocument.RootElement.GetProperty ( "error" ).GetProperty ( "data" ).GetProperty ( "supported" ).GetArrayLength () );
    }

    [TestMethod]
    public async Task Origin_RejectsCrossOriginUnlessExplicitlyAllowed () {
        var provider = new McpProvider ();
        using var host = new TestHost ( provider );
        using HttpRequestMessage rejectedRequest = host.CreateRequest ( "server/discover", "{}", McpProvider.PROTOCOL_VERSION );
        rejectedRequest.Headers.Host = "evil.example";
        rejectedRequest.Headers.TryAddWithoutValidation ( "Origin", "http://evil.example" );
        using HttpResponseMessage rejectedResponse = await host.Client.SendAsync ( rejectedRequest );

        Assert.AreEqual ( HttpStatusCode.Forbidden, rejectedResponse.StatusCode );

        provider.OriginValidator = ( _, origin ) => origin == "https://trusted.example";
        using HttpRequestMessage allowedRequest = host.CreateRequest ( "server/discover", "{}", McpProvider.PROTOCOL_VERSION );
        allowedRequest.Headers.TryAddWithoutValidation ( "Origin", "https://trusted.example" );
        using HttpResponseMessage allowedResponse = await host.Client.SendAsync ( allowedRequest );

        Assert.AreEqual ( HttpStatusCode.OK, allowedResponse.StatusCode );
    }

    [TestMethod]
    public async Task ToolsList_EmitsIconsOutputSchemaAndAnnotations () {
        var provider = new McpProvider ();
        provider.Tools.Add ( new McpTool (
            "inspect",
            "Inspects an item.",
            JsonSchema.CreateObjectSchema (),
            _ => Task.FromResult ( new McpToolResult ( new JsonObject () {
                [ "type" ] = "text",
                [ "text" ] = "ok"
            } ) {
                StructuredContent = new JsonObject ()
            } ) ) {
            Icons = [new McpProviderIcon ( "https://example.test/icon.png", "image/png" )],
            OutputSchema = JsonSchema.CreateObjectSchema (),
            Annotations = new McpToolAnnotations () {
                Title = "Inspect item",
                ReadOnlyHint = true,
                DestructiveHint = false,
                IdempotentHint = true,
                OpenWorldHint = false
            }
        } );
        using var host = new TestHost ( provider );
        using HttpResponseMessage response = await host.SendModernAsync ( "tools/list", "{}" );
        using JsonDocument document = await JsonDocument.ParseAsync ( await response.Content.ReadAsStreamAsync () );

        JsonElement result = document.RootElement.GetProperty ( "result" );
        JsonElement tool = result.GetProperty ( "tools" )[ 0 ];
        Assert.AreEqual ( "complete", result.GetProperty ( "resultType" ).GetString () );
        Assert.AreEqual ( "https://example.test/icon.png", tool.GetProperty ( "icons" )[ 0 ].GetProperty ( "src" ).GetString () );
        Assert.AreEqual ( JsonValueKind.Object, tool.GetProperty ( "outputSchema" ).ValueKind );
        Assert.IsTrue ( tool.GetProperty ( "annotations" ).GetProperty ( "readOnlyHint" ).GetBoolean () );
        Assert.IsFalse ( tool.GetProperty ( "annotations" ).GetProperty ( "destructiveHint" ).GetBoolean () );

        using HttpResponseMessage callResponse = await host.SendModernAsync ( "tools/call", "\"name\":\"inspect\",\"arguments\":{}", name: "inspect" );
        using JsonDocument callDocument = await JsonDocument.ParseAsync ( await callResponse.Content.ReadAsStreamAsync () );
        JsonElement callResult = callDocument.RootElement.GetProperty ( "result" );
        Assert.AreEqual ( JsonValueKind.Object, callResult.GetProperty ( "structuredContent" ).ValueKind );
        Assert.IsFalse ( callResult.GetProperty ( "isError" ).GetBoolean () );

        provider.Tools [ 0 ].OutputSchema = JsonSchema.CreateObjectSchema (
            properties: new Dictionary<string, JsonSchema> {
                [ "requiredValue" ] = JsonSchema.CreateStringSchema ()
            },
            requiredProperties: ["requiredValue"] );
        using HttpResponseMessage invalidCallResponse = await host.SendModernAsync ( "tools/call", "\"name\":\"inspect\",\"arguments\":{}", name: "inspect" );
        using JsonDocument invalidCallDocument = await JsonDocument.ParseAsync ( await invalidCallResponse.Content.ReadAsStreamAsync () );
        Assert.IsTrue ( invalidCallDocument.RootElement.GetProperty ( "result" ).GetProperty ( "isError" ).GetBoolean () );
        Assert.IsFalse ( invalidCallDocument.RootElement.GetProperty ( "result" ).TryGetProperty ( "structuredContent", out _ ) );
    }

    [TestMethod]
    public async Task ToolNotFound_ReturnsJsonRpcInvalidParams () {
        using var host = new TestHost ( new McpProvider () );
        using HttpResponseMessage response = await host.SendModernAsync ( "tools/call", "\"name\":\"missing\",\"arguments\":{}", name: "missing" );
        using JsonDocument document = await JsonDocument.ParseAsync ( await response.Content.ReadAsStreamAsync () );

        Assert.AreEqual ( HttpStatusCode.OK, response.StatusCode );
        Assert.AreEqual ( "2.0", document.RootElement.GetProperty ( "jsonrpc" ).GetString () );
        Assert.AreEqual ( -32602, document.RootElement.GetProperty ( "error" ).GetProperty ( "code" ).GetInt32 () );
    }

    [TestMethod]
    public async Task UnknownModernMethod_ReturnsHttp404AndJsonRpcError () {
        using var host = new TestHost ( new McpProvider () );
        using HttpResponseMessage response = await host.SendModernAsync ( "unknown/method", "{}" );
        using JsonDocument document = await JsonDocument.ParseAsync ( await response.Content.ReadAsStreamAsync () );

        Assert.AreEqual ( HttpStatusCode.NotFound, response.StatusCode );
        Assert.AreEqual ( -32601, document.RootElement.GetProperty ( "error" ).GetProperty ( "code" ).GetInt32 () );
    }

    [DataTestMethod]
    [DataRow ( "2025-11-25" )]
    [DataRow ( "2025-06-18" )]
    [DataRow ( "2025-03-26" )]
    public async Task LegacyInitializeAndGet_RemainSupported ( string protocolVersion ) {
        using var host = new TestHost ( new McpProvider () );
        using var initializeRequest = new HttpRequestMessage ( HttpMethod.Post, host.Endpoint ) {
            Content = new StringContent ( $"{{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{{\"protocolVersion\":\"{protocolVersion}\"}}}}", Encoding.UTF8, "application/json" )
        };
        initializeRequest.Headers.TryAddWithoutValidation ( "MCP-Protocol-Version", protocolVersion );
        using HttpResponseMessage initializeResponse = await host.Client.SendAsync ( initializeRequest );
        using JsonDocument initializeDocument = await JsonDocument.ParseAsync ( await initializeResponse.Content.ReadAsStreamAsync () );

        Assert.AreEqual ( protocolVersion, initializeDocument.RootElement.GetProperty ( "result" ).GetProperty ( "protocolVersion" ).GetString () );
        Assert.IsTrue ( initializeResponse.Headers.TryGetValues ( "Mcp-Session-Id", out IEnumerable<string>? sessionIds ) );
        Assert.IsFalse ( initializeDocument.RootElement.GetProperty ( "result" ).TryGetProperty ( "resultType", out _ ) );

        using var listRequest = new HttpRequestMessage ( HttpMethod.Post, host.Endpoint ) {
            Content = new StringContent ( "{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"tools/list\",\"params\":{}}", Encoding.UTF8, "application/json" )
        };
        listRequest.Headers.TryAddWithoutValidation ( "MCP-Protocol-Version", protocolVersion );
        listRequest.Headers.TryAddWithoutValidation ( "Mcp-Session-Id", sessionIds.Single () );
        using HttpResponseMessage listResponse = await host.Client.SendAsync ( listRequest );
        using JsonDocument listDocument = await JsonDocument.ParseAsync ( await listResponse.Content.ReadAsStreamAsync () );

        Assert.AreEqual ( HttpStatusCode.OK, listResponse.StatusCode );
        Assert.IsTrue ( listDocument.RootElement.TryGetProperty ( "result", out _ ) );
        Assert.IsFalse ( listDocument.RootElement.TryGetProperty ( "error", out _ ) );

        using HttpResponseMessage getResponse = await host.Client.GetAsync ( host.Endpoint );
        Assert.AreEqual ( HttpStatusCode.MethodNotAllowed, getResponse.StatusCode );
    }

    [TestMethod]
    public async Task ModernInitializeWithoutRequestMetadata_IsRejected () {
        using var host = new TestHost ( new McpProvider () );
        using var request = new HttpRequestMessage ( HttpMethod.Post, host.Endpoint ) {
            Content = new StringContent ( $"{{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{{\"protocolVersion\":\"{McpProvider.PROTOCOL_VERSION}\"}}}}", Encoding.UTF8, "application/json" )
        };
        request.Headers.TryAddWithoutValidation ( "MCP-Protocol-Version", McpProvider.PROTOCOL_VERSION );
        using HttpResponseMessage response = await host.Client.SendAsync ( request );
        using JsonDocument document = await JsonDocument.ParseAsync ( await response.Content.ReadAsStreamAsync () );

        Assert.AreEqual ( HttpStatusCode.BadRequest, response.StatusCode );
        Assert.AreEqual ( -32602, document.RootElement.GetProperty ( "error" ).GetProperty ( "code" ).GetInt32 () );
    }

    [TestMethod]
    public async Task ModernMessageWithoutId_IsAcceptedAsNotificationWithoutBody () {
        using var host = new TestHost ( new McpProvider () );
        string body = $"{{\"jsonrpc\":\"2.0\",\"method\":\"tools/list\",\"params\":{{\"_meta\":{{\"io.modelcontextprotocol/protocolVersion\":\"{McpProvider.PROTOCOL_VERSION}\",\"io.modelcontextprotocol/clientCapabilities\":{{}}}}}}}}";
        using var request = new HttpRequestMessage ( HttpMethod.Post, host.Endpoint ) {
            Content = new StringContent ( body, Encoding.UTF8, "application/json" )
        };
        request.Headers.TryAddWithoutValidation ( "MCP-Protocol-Version", McpProvider.PROTOCOL_VERSION );
        request.Headers.TryAddWithoutValidation ( "Mcp-Method", "tools/list" );
        using HttpResponseMessage response = await host.Client.SendAsync ( request );

        Assert.AreEqual ( HttpStatusCode.Accepted, response.StatusCode );
        Assert.AreEqual ( string.Empty, await response.Content.ReadAsStringAsync () );
    }

    [TestMethod]
    public async Task InvalidJson_ReturnsJsonRpcParseError () {
        using var host = new TestHost ( new McpProvider () );
        using var request = new HttpRequestMessage ( HttpMethod.Post, host.Endpoint ) {
            Content = new StringContent ( "{", Encoding.UTF8, "application/json" )
        };
        using HttpResponseMessage response = await host.Client.SendAsync ( request );
        using JsonDocument document = await JsonDocument.ParseAsync ( await response.Content.ReadAsStreamAsync () );

        Assert.AreEqual ( HttpStatusCode.BadRequest, response.StatusCode );
        Assert.AreEqual ( -32700, document.RootElement.GetProperty ( "error" ).GetProperty ( "code" ).GetInt32 () );
        Assert.IsFalse ( document.RootElement.TryGetProperty ( "id", out _ ) );
    }

    [TestMethod]
    public async Task InvalidParameterShapes_ReturnJsonRpcErrors () {
        using var host = new TestHost ( new McpProvider () );
        using var paramsRequest = new HttpRequestMessage ( HttpMethod.Post, host.Endpoint ) {
            Content = new StringContent ( "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/list\",\"params\":[]}", Encoding.UTF8, "application/json" )
        };
        using HttpResponseMessage paramsResponse = await host.Client.SendAsync ( paramsRequest );
        using JsonDocument paramsDocument = await JsonDocument.ParseAsync ( await paramsResponse.Content.ReadAsStreamAsync () );

        Assert.AreEqual ( HttpStatusCode.BadRequest, paramsResponse.StatusCode );
        Assert.AreEqual ( -32602, paramsDocument.RootElement.GetProperty ( "error" ).GetProperty ( "code" ).GetInt32 () );

        using var metadataRequest = new HttpRequestMessage ( HttpMethod.Post, host.Endpoint ) {
            Content = new StringContent ( "{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"tools/list\",\"params\":{\"_meta\":[]}}", Encoding.UTF8, "application/json" )
        };
        using HttpResponseMessage metadataResponse = await host.Client.SendAsync ( metadataRequest );
        using JsonDocument metadataDocument = await JsonDocument.ParseAsync ( await metadataResponse.Content.ReadAsStreamAsync () );

        Assert.AreEqual ( HttpStatusCode.BadRequest, metadataResponse.StatusCode );
        Assert.AreEqual ( -32602, metadataDocument.RootElement.GetProperty ( "error" ).GetProperty ( "code" ).GetInt32 () );
    }

    [TestMethod]
    public async Task InvalidEnvelopeShapes_ReturnInvalidRequest () {
        using var host = new TestHost ( new McpProvider () );
        foreach (string body in new [] {
            "[]",
            "{\"jsonrpc\":\"2.0\",\"id\":{},\"method\":\"tools/list\",\"params\":{}}"
        }) {
            using var request = new HttpRequestMessage ( HttpMethod.Post, host.Endpoint ) {
                Content = new StringContent ( body, Encoding.UTF8, "application/json" )
            };
            using HttpResponseMessage response = await host.Client.SendAsync ( request );
            using JsonDocument document = await JsonDocument.ParseAsync ( await response.Content.ReadAsStreamAsync () );

            Assert.AreEqual ( HttpStatusCode.BadRequest, response.StatusCode );
            Assert.AreEqual ( -32600, document.RootElement.GetProperty ( "error" ).GetProperty ( "code" ).GetInt32 () );
        }
    }

    [TestMethod]
    public async Task Skills_AdvertiseListGetAndReadConsistentSnapshots () {
        byte[] attachment = [0, 255, 42];
        var frontmatter = new JsonObject () {
            [ "name" ] = "refunds",
            [ "description" ] = "Processar devoluções.",
            [ "metadata" ] = new JsonObject () { [ "custom" ] = "preserved" }
        };
        var provider = new McpProvider ();
        var skill = new McpSkill ( "acme/refunds", frontmatter, "# Devoluções\nUse references/data.bin.",
            new Dictionary<string, byte[]> { [ "references/data.bin" ] = attachment } );
        provider.Skills.Add ( skill );
        attachment [ 0 ] = 99;
        frontmatter [ "name" ] = "changed";
        using var host = new TestHost ( provider );

        using HttpResponseMessage discover = await host.SendModernAsync ( "server/discover", "{}" );
        using JsonDocument discovery = await JsonDocument.ParseAsync ( await discover.Content.ReadAsStreamAsync () );
        JsonElement capabilities = discovery.RootElement.GetProperty ( "result" ).GetProperty ( "capabilities" );
        Assert.IsTrue ( capabilities.TryGetProperty ( "resources", out _ ) );
        Assert.AreEqual ( 0, capabilities.GetProperty ( "extensions" ).GetProperty ( "io.modelcontextprotocol/skills" ).EnumerateObject ().Count () );

        using HttpResponseMessage list = await host.SendModernAsync ( "skills/list", "{}" );
        using JsonDocument listing = await JsonDocument.ParseAsync ( await list.Content.ReadAsStreamAsync () );
        JsonElement result = listing.RootElement.GetProperty ( "result" );
        Assert.AreEqual ( "complete", result.GetProperty ( "resultType" ).GetString () );
        Assert.AreEqual ( 0, result.GetProperty ( "ttlMs" ).GetInt32 () );
        Assert.AreEqual ( "private", result.GetProperty ( "cacheScope" ).GetString () );
        Assert.IsFalse ( result.TryGetProperty ( "nextCursor", out _ ) );
        JsonElement entry = result.GetProperty ( "skills" ) [ 0 ];
        Assert.AreEqual ( "refunds", entry.GetProperty ( "frontmatter" ).GetProperty ( "name" ).GetString () );
        Assert.AreEqual ( "preserved", entry.GetProperty ( "frontmatter" ).GetProperty ( "metadata" ).GetProperty ( "custom" ).GetString () );

        using HttpResponseMessage get = await host.SendModernAsync ( "skills/get", $"\"uri\":\"{skill.Uri}\"" );
        using JsonDocument fetched = await JsonDocument.ParseAsync ( await get.Content.ReadAsStreamAsync () );
        Assert.AreEqual ( entry.GetRawText (), fetched.RootElement.GetProperty ( "result" ).GetProperty ( "skill" ).GetRawText () );
        Assert.AreEqual ( 2, entry.GetProperty ( "resources" ).GetArrayLength () );
        foreach (JsonElement resource in entry.GetProperty ( "resources" ).EnumerateArray ()) {
            string uri = resource.GetProperty ( "uri" ).GetString ()!;
            using HttpResponseMessage read = await host.SendModernAsync ( "resources/read", $"\"uri\":\"{uri}\"" );
            using JsonDocument document = await JsonDocument.ParseAsync ( await read.Content.ReadAsStreamAsync () );
            JsonElement content = document.RootElement.GetProperty ( "result" ).GetProperty ( "contents" ) [ 0 ];
            byte[] bytes = content.TryGetProperty ( "text", out JsonElement text )
                ? Encoding.UTF8.GetBytes ( text.GetString ()! )
                : Convert.FromBase64String ( content.GetProperty ( "blob" ).GetString ()! );
            Assert.AreEqual ( resource.GetProperty ( "size" ).GetInt32 (), bytes.Length );
            Assert.AreEqual ( resource.GetProperty ( "digest" ).GetString (),
                "sha256:" + Convert.ToHexString ( System.Security.Cryptography.SHA256.HashData ( bytes ) ).ToLowerInvariant () );
            if (uri == skill.Uri) {
                string[] sections = text.GetString ()!.Split ( "---\n" );
                using JsonDocument yamlJson = JsonDocument.Parse ( sections [ 1 ] );
                Assert.AreEqual ( entry.GetProperty ( "frontmatter" ).GetRawText (), yamlJson.RootElement.GetRawText () );
            }
            else
                CollectionAssert.AreEqual ( new byte [] { 0, 255, 42 }, bytes );
        }

        using HttpResponseMessage resources = await host.SendModernAsync ( "resources/list", "{}" );
        using JsonDocument resourceList = await JsonDocument.ParseAsync ( await resources.Content.ReadAsStreamAsync () );
        Assert.AreEqual ( 2, resourceList.RootElement.GetProperty ( "result" ).GetProperty ( "resources" ).GetArrayLength () );
    }

    [TestMethod]
    public async Task Skills_EmptyCatalogInvalidParametersAndLegacyIsolation () {
        var provider = new McpProvider ();
        using var host = new TestHost ( provider );
        using HttpResponseMessage list = await host.SendModernAsync ( "skills/list", "{}" );
        using JsonDocument listing = await JsonDocument.ParseAsync ( await list.Content.ReadAsStreamAsync () );
        Assert.AreEqual ( 0, listing.RootElement.GetProperty ( "result" ).GetProperty ( "skills" ).GetArrayLength () );
        foreach (var (method, parameters) in new [] {
            ("skills/get", "{}"),
            ("skills/get", "\"uri\":42"),
            ("skills/get", "\"uri\":\"skill://missing/SKILL.md\""),
            ("resources/read", "\"uri\":\"skill://missing/../secret\""),
            ("skills/list", "\"cursor\":\"invalid\"")
        }) {
            using HttpResponseMessage response = await host.SendModernAsync ( method, parameters );
            using JsonDocument document = await JsonDocument.ParseAsync ( await response.Content.ReadAsStreamAsync () );
            Assert.AreEqual ( -32602, document.RootElement.GetProperty ( "error" ).GetProperty ( "code" ).GetInt32 () );
        }
        provider.Skills.Add ( new McpSkill ( "example", new JsonObject () {
            [ "name" ] = "example", [ "description" ] = "Example skill."
        }, "Instructions." ) );
        using var initializeBody = new StringContent (
            "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{\"protocolVersion\":\"2025-06-18\"}}", Encoding.UTF8, "application/json" );
        using HttpResponseMessage initialize = await host.Client.PostAsync ( host.Endpoint, initializeBody );
        using JsonDocument initialized = await JsonDocument.ParseAsync ( await initialize.Content.ReadAsStreamAsync () );
        Assert.IsFalse ( initialized.RootElement.GetProperty ( "result" ).GetProperty ( "capabilities" ).TryGetProperty ( "extensions", out _ ) );
        using var legacyBody = new StringContent ( "{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"skills/list\"}", Encoding.UTF8, "application/json" );
        using HttpResponseMessage legacy = await host.Client.PostAsync ( host.Endpoint, legacyBody );
        using JsonDocument legacyResult = await JsonDocument.ParseAsync ( await legacy.Content.ReadAsStreamAsync () );
        Assert.AreEqual ( -32601, legacyResult.RootElement.GetProperty ( "error" ).GetProperty ( "code" ).GetInt32 () );
    }

    [TestMethod]
    public async Task Skills_RejectInconsistentCatalogs () {
        var frontmatter = new JsonObject () { [ "name" ] = "example", [ "description" ] = "Example." };
        var skill = new McpSkill ( "example", frontmatter, "Body" );
        var provider = new McpProvider () { Skills = [skill, skill] };
        using var host = new TestHost ( provider );
        foreach (string method in new [] { "skills/list", "resources/list" }) {
            using HttpResponseMessage response = await host.SendModernAsync ( method, "{}" );
            using JsonDocument document = await JsonDocument.ParseAsync ( await response.Content.ReadAsStreamAsync () );
            Assert.AreEqual ( -32603, document.RootElement.GetProperty ( "error" ).GetProperty ( "code" ).GetInt32 () );
        }
        provider.Skills = [skill, new McpSkill ( "example/example", frontmatter, "Nested" )];
        using HttpResponseMessage incomplete = await host.SendModernAsync ( "skills/list", "{}" );
        using JsonDocument incompleteDocument = await JsonDocument.ParseAsync ( await incomplete.Content.ReadAsStreamAsync () );
        Assert.AreEqual ( -32603, incompleteDocument.RootElement.GetProperty ( "error" ).GetProperty ( "code" ).GetInt32 () );

        provider.Skills = [new McpSkill ( "example", frontmatter, "Body", new Dictionary<string, byte[]> {
            [ "example/SKILL.md" ] = Encoding.UTF8.GetBytes ( "Different content" )
        } ), new McpSkill ( "example/example", frontmatter, "Nested" )];
        using HttpResponseMessage conflict = await host.SendModernAsync ( "skills/list", "{}" );
        using JsonDocument conflictDocument = await JsonDocument.ParseAsync ( await conflict.Content.ReadAsStreamAsync () );
        Assert.AreEqual ( -32603, conflictDocument.RootElement.GetProperty ( "error" ).GetProperty ( "code" ).GetInt32 () );
    }

    [TestMethod]
    public void Skill_RejectsInvalidNamesPathsAndFiles () {
        var frontmatter = new JsonObject () { [ "name" ] = "example", [ "description" ] = "Example." };
        foreach (string path in new [] { "different", "../example", "acme//example", "acme\\example" })
            Assert.ThrowsException<ArgumentException> ( () => new McpSkill ( path, frontmatter, "Body" ) );
        foreach (string file in new [] { "SKILL.md", "../secret", "/absolute", "a//b", "a\\b", "C:secret" })
            Assert.ThrowsException<ArgumentException> ( () => new McpSkill ( "example", frontmatter, "Body",
                new Dictionary<string, byte[]> { [ file ] = [] } ) );
        Assert.ThrowsException<ArgumentException> ( () => new McpSkill ( "example", new JsonObject (), "Body" ) );
        foreach (string name in new [] { "Upper", "two--hyphens", "trailing-", "example\n", new string ( 'a', 65 ) }) {
            var invalid = new JsonObject () { [ "name" ] = name, [ "description" ] = "Example." };
            Assert.ThrowsException<ArgumentException> ( () => new McpSkill ( name, invalid, "Body" ) );
        }
        Assert.ThrowsException<ArgumentException> ( () => new McpSkill ( "example", frontmatter, "Body",
            Enumerable.Range ( 0, 512 ).ToDictionary ( i => $"{i}.txt", _ => Array.Empty<byte> () ) ) );
        Assert.ThrowsException<ArgumentException> ( () => new McpSkill ( "example", frontmatter, "Body",
            new Dictionary<string, byte[]> { [ "large.bin" ] = new byte [ 16_777_216 ] } ) );
    }

    private sealed class TestHost : IDisposable {
        private readonly HttpServerHostContext server;

        public HttpClient Client { get; } = new () { Timeout = TimeSpan.FromSeconds ( 5 ) };
        public Uri Endpoint { get; }

        public TestHost ( McpProvider provider ) {
            int port;
            using (var listener = new TcpListener ( IPAddress.Loopback, 0 )) {
                listener.Start ();
                port = ((IPEndPoint) listener.LocalEndpoint).Port;
            }

            Endpoint = new Uri ( $"http://127.0.0.1:{port}/mcp" );
            server = HttpServer.CreateBuilder ()
                .UseListeningPort ( $"http://127.0.0.1:{port}/" )
                .UseConfiguration ( configuration => {
                    configuration.ThrowExceptions = true;
                    configuration.AccessLogsStream = null;
                    configuration.ErrorsLogsStream = null;
                } )
                .UseRouter ( router => router.MapAny ( "/mcp", (Func<HttpRequest, Task<HttpResponse>>) ( async request => await provider.HandleRequestAsync ( request ) ) ) )
                .Build ();
            server.Start ( verbose: false, preventHault: false );
        }

        public HttpRequestMessage CreateRequest ( string method, string additionalParameters, string version, string? name = null ) {
            string separator = additionalParameters == "{}" ? string.Empty : additionalParameters + ",";
            string body = $"{{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"{method}\",\"params\":{{{separator}\"_meta\":{{\"io.modelcontextprotocol/protocolVersion\":\"{version}\",\"io.modelcontextprotocol/clientCapabilities\":{{}}}}}}}}";
            var request = new HttpRequestMessage ( HttpMethod.Post, Endpoint ) {
                Content = new StringContent ( body, Encoding.UTF8, "application/json" )
            };
            request.Headers.TryAddWithoutValidation ( "Accept", "application/json, text/event-stream" );
            request.Headers.TryAddWithoutValidation ( "MCP-Protocol-Version", version );
            request.Headers.TryAddWithoutValidation ( "Mcp-Method", method );
            if (name is { })
                request.Headers.TryAddWithoutValidation ( "Mcp-Name", name );
            return request;
        }

        public async Task<HttpResponseMessage> SendModernAsync ( string method, string additionalParameters, string? version = null, string? name = null ) {
            using HttpRequestMessage request = CreateRequest ( method, additionalParameters, version ?? McpProvider.PROTOCOL_VERSION, name );
            return await Client.SendAsync ( request );
        }

        public void Dispose () {
            Client.Dispose ();
            server.Dispose ();
        }
    }
}
