// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   McpProvider.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;


namespace Sisk.ModelContextProtocol;

/// <summary>
/// Represents a server that hosts a Model Context Protocol server.
/// </summary>
public sealed class McpProvider {

    /// <summary>
    /// Represents the current supported version of the Model Context Protocol.
    /// </summary>
    public const string PROTOCOL_VERSION = "2025-06-18";

    internal static JsonOptions Json = new JsonOptions () {
        PropertyNameComparer = StringComparer.OrdinalIgnoreCase,
        AllowNumbersAsStrings = true,
        SerializerContext = McpSerializerContext.Default,
        StringEncoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        InfinityHandler = JsonInfinityHandleOption.WriteNull
    };

    /// <summary>
    /// Creates a new instance of the <see cref="McpProvider"/> class.
    /// </summary>
    public McpProvider () {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="McpProvider"/> class with the specified server details.
    /// </summary>
    /// <param name="serverName">The name of the server.</param>
    /// <param name="serverTitle">The title of the server.</param>
    /// <param name="serverVersion">The version of the server.</param>
    public McpProvider ( string serverName, string serverTitle, Version serverVersion ) {
        ServerName = serverName;
        ServerTitle = serverTitle;
        ServerVersion = serverVersion;
    }

    /// <summary>
    /// Gets or sets the internal name of the server. 
    /// </summary>
    public string? ServerName { get; set; } = "ExampleServer";

    /// <summary>
    /// Gets or sets the display name of the server.
    /// </summary>
    public string? ServerTitle { get; set; } = "Example Server Display Name";

    /// <summary>
    /// Gets or sets the version of the MCP server.
    /// </summary>
    public Version ServerVersion { get; set; } = new Version ( 1, 0 );

    /// <summary>
    /// Gets or sets the list of MCP tools hosted by this server.
    /// </summary>
    public IList<McpTool> Tools { get; set; } = [];

    object GetToolsListResponse ( JsonValue id ) {
        JsonObject rootObject = new JsonObject () {
            [ "tools" ] = new JsonArray ( [
                ..Tools.Select(tool => new JsonObject() {
                    ["name"] = tool.Name,
                    ["title"] = tool.Title ?? tool.Name,
                    ["description"] = tool.Description,
                    ["inputSchema"] = tool.Schema
                })
            ] )
        };
        return new JsonRpcResponse ( rootObject, id );
    }

    object GetInitializeResponse ( JsonValue id ) {
        JsonObject rootObject = new JsonObject () {
            [ "protocolVersion" ] = PROTOCOL_VERSION,
            [ "serverInfo" ] = new JsonObject () {
                [ "name" ] = ServerName,
                [ "title" ] = ServerTitle,
                [ "version" ] = ServerVersion.ToString ()
            }
        };

        JsonObject capabilities = rootObject.Add ( "capabilities", new JsonObject () );

        if (Tools.Any ())
            capabilities.Add ( "tools", new JsonObject () {
                [ "listChanged" ] = false
            } );

        return new JsonRpcResponse ( rootObject, id );
    }

    /// <summary>
    /// Handles an incoming HTTP request for MCP operations asynchronously.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <param name="cancellation">A token to observe for cancellation requests.</param>
    /// <returns>An HTTP response representing the result of the request handling.</returns>
    public async Task<HttpResponse> HandleRequestAsync ( HttpRequest request, CancellationToken cancellation = default ) {

        string sessionId = request.Headers [ "Mcp-Session-Id" ] ?? Guid.NewGuid ().ToString ();

        if (request.Method == HttpMethod.Get) {
            var initializeResponse = GetInitializeResponse ( id: JsonValue.Null );
            return new McpJsonResponse ( initializeResponse, sessionId );
        }
        else if (request.Method == HttpMethod.Post) {
            Memory<byte> contentBytes = await request.GetBodyContentsAsync ( cancellation );
            JsonObject requestObject = await Json.DeserializeAsync<JsonObject> ( contentBytes, request.RequestEncoding, cancellation );

            JsonValue id = requestObject [ "id" ];
            string method = requestObject [ "method" ].GetString ();

            if (method == "initialize") {
                var clientVersion = requestObject [ "params" ] [ "protocolVersion" ].GetString ();

                if (clientVersion != PROTOCOL_VERSION) {
                    return new HttpResponse ( System.Net.HttpStatusCode.NotAcceptable );
                }

                var initializeResponse = GetInitializeResponse ( id );
                return new McpJsonResponse ( initializeResponse, sessionId );
            }
            else if (method == "tools/list") {
                var toolsResponse = GetToolsListResponse ( id );
                return new McpJsonResponse ( toolsResponse, sessionId );
            }
            else if (method == "tools/call") {
                string toolName = requestObject [ "params" ] [ "name" ].GetString ();
                JsonObject toolInput = requestObject [ "params" ] [ "arguments" ].GetJsonObject ();
                var tool = Tools.FirstOrDefault ( t => t.Name == toolName );

                if (tool == null) {
                    return new HttpResponse ( System.Net.HttpStatusCode.NotFound );
                }

                JsonObject result = [];

                if (tool.Schema.Validate ( toolInput ) is { IsValid: false } validationError) {
                    result [ "content" ] = new JsonArray ( [new JsonObject()
                    {
                        ["type"] = "text",
                        ["text"] = "Error executing tool: failed to validate the JSON schema of the function call. See errors below.\r\n" +
                            string.Join("\r\n", validationError.Errors.Select(e => $"- [{e.Path}] {e.Message}"))
                    }] );
                    result [ "isError" ] = true;
                }
                else {
                    try {
                        var context = new McpToolContext () {
                            Cancellation = cancellation,
                            Server = this,
                            Request = request,
                            Arguments = toolInput,
                            ToolName = toolName,
                            Metadata = requestObject [ "params" ] [ "_meta" ].MaybeNull ()?.GetJsonObject () ?? []
                        };

                        var toolResult = await tool.ExecuteAsync ( context );
                        if (toolResult.Result.IsJsonArray) {
                            result [ "content" ] = toolResult.Result;
                        }
                        else {
                            result [ "content" ] = new JsonArray ( [ toolResult.Result ] );
                        }
                        result [ "isError" ] = false;
                    }
                    catch (Exception ex) {
                        result [ "content" ] = new JsonArray ( [new JsonObject()
                        {
                            ["type"] = "text",
                            ["text"] = "Error executing tool: " + ex.Message
                        }] );
                        result [ "isError" ] = true;
                    }
                }

                var toolCallResponse = new JsonRpcResponse ( result, id );
                return new McpJsonResponse ( toolCallResponse, sessionId );
            }
            else if (method == "ping") {
                return new McpJsonResponse ( new JsonRpcResponse ( new JsonObject (), id ), sessionId );
            }
            else if (method.StartsWith ( "notifications/" )) {
                return new HttpResponse ( HttpStatusCode.Accepted );
            }
            else {
                return new McpJsonResponse ( new JsonObject () {
                    [ "code" ] = -32000,
                    [ "message" ] = "Method not found or not supported"
                }, sessionId: null );
            }
        }
        else {
            return new HttpResponse ( System.Net.HttpStatusCode.MethodNotAllowed );
        }
    }
}
