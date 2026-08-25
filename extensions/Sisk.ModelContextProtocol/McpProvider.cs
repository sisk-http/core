// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   McpProvider.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Text;

namespace Sisk.ModelContextProtocol;

/// <summary>
/// Represents a server that hosts a Model Context Protocol server.
/// </summary>
public sealed class McpProvider {

    /// <summary>
    /// Represents the current supported version of the Model Context Protocol.
    /// </summary>
    public const string PROTOCOL_VERSION = "2026-07-28";

    /// <summary>
    /// Represents the error code for missing or mismatched HTTP metadata headers.
    /// </summary>
    public const int HEADER_MISMATCH_ERROR = -32020;

    /// <summary>
    /// Represents the error code for a required client capability that was not declared.
    /// </summary>
    public const int MISSING_REQUIRED_CLIENT_CAPABILITY_ERROR = -32021;

    /// <summary>
    /// Represents the error code for a protocol version that the provider does not support.
    /// </summary>
    public const int UNSUPPORTED_PROTOCOL_VERSION_ERROR = -32022;

    /// <summary>
    /// Gets the stateless protocol versions accepted by this provider.
    /// </summary>
    /// <remarks>
    /// Initialization-based versions remain available through the legacy initialize request.
    /// </remarks>
    public static IReadOnlyList<string> SupportedProtocolVersions { get; } = [PROTOCOL_VERSION];

    static IReadOnlyList<string> LegacyProtocolVersions { get; } = ["2025-11-25", "2025-06-18", "2025-03-26"];

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
    /// Gets or sets a human-readable description of the server.
    /// </summary>
    public string? ServerDescription { get; set; } = "An example MCP server providing tools";

    /// <summary>
    /// Gets or sets the URL of the server website.
    /// </summary>
    public string? ServerWebsiteUrl { get; set; }

    /// <summary>
    /// Gets or sets instructions that help clients understand how to use the server.
    /// </summary>
    public string? ClientInstructions { get; set; }

    /// <summary>
    /// Gets or sets the icons that clients can display for the server.
    /// </summary>
    public IList<McpProviderIcon> Icons { get; set; } = [];

    /// <summary>
    /// Gets or sets a validator for explicitly trusted origins.
    /// </summary>
    /// <remarks>
    /// Requests without an Origin header are accepted. Requests that include Origin are rejected unless this validator accepts them.
    /// </remarks>
    public Func<HttpRequest, string, bool>? OriginValidator { get; set; }

    /// <summary>
    /// Gets or sets the version of the MCP server.
    /// </summary>
    public Version ServerVersion { get; set; } = new Version ( 1, 0 );

    /// <summary>
    /// Gets or sets the list of MCP tools hosted by this server.
    /// </summary>
    public IList<McpTool> Tools { get; set; } = [];

    JsonObject GetServerInfo () {
        JsonObject serverInfo = new JsonObject () {
            [ "name" ] = ServerName,
            [ "title" ] = ServerTitle,
            [ "version" ] = ServerVersion.ToString ()
        };

        if (Icons.Any ())
            serverInfo [ "icons" ] = JsonArray.Create ( Icons );
        if (!string.IsNullOrWhiteSpace ( ServerDescription ))
            serverInfo [ "description" ] = ServerDescription;
        if (!string.IsNullOrWhiteSpace ( ServerWebsiteUrl ))
            serverInfo [ "websiteUrl" ] = ServerWebsiteUrl;

        return serverInfo;
    }

    JsonObject GetCapabilities () {
        JsonObject capabilities = [];
        if (Tools.Any ())
            capabilities [ "tools" ] = new JsonObject () {
                [ "listChanged" ] = false
            };
        return capabilities;
    }

    JsonObject GetResult ( string method, bool modern ) {
        if (method == "server/discover") {
            JsonObject discoverResult = new JsonObject () {
                [ "resultType" ] = "complete",
                [ "supportedVersions" ] = new JsonArray ( [.. SupportedProtocolVersions] ),
                [ "capabilities" ] = GetCapabilities (),
                [ "ttlMs" ] = 0,
                [ "cacheScope" ] = "private",
                [ "_meta" ] = new JsonObject () {
                    [ "io.modelcontextprotocol/serverInfo" ] = GetServerInfo ()
                }
            };
            if (!string.IsNullOrWhiteSpace ( ClientInstructions ))
                discoverResult [ "instructions" ] = ClientInstructions;
            return discoverResult;
        }

        JsonArray tools = [];
        foreach (var tool in Tools) {
            JsonObject description = new JsonObject () {
                [ "name" ] = tool.Name,
                [ "title" ] = tool.Title ?? tool.Name,
                [ "description" ] = tool.Description,
                [ "inputSchema" ] = tool.Schema
            };

            if (tool.Icons.Any ())
                description [ "icons" ] = JsonArray.Create ( tool.Icons );
            if (tool.OutputSchema is { })
                description [ "outputSchema" ] = tool.OutputSchema;
            if (tool.Annotations is { } annotations) {
                JsonObject annotationsObject = [];
                if (!string.IsNullOrWhiteSpace ( annotations.Title ))
                    annotationsObject [ "title" ] = annotations.Title;
                if (annotations.ReadOnlyHint is { } readOnlyHint)
                    annotationsObject [ "readOnlyHint" ] = readOnlyHint;
                if (annotations.DestructiveHint is { } destructiveHint)
                    annotationsObject [ "destructiveHint" ] = destructiveHint;
                if (annotations.IdempotentHint is { } idempotentHint)
                    annotationsObject [ "idempotentHint" ] = idempotentHint;
                if (annotations.OpenWorldHint is { } openWorldHint)
                    annotationsObject [ "openWorldHint" ] = openWorldHint;
                description [ "annotations" ] = annotationsObject;
            }
            tools.Add ( description );
        }

        JsonObject toolsResult = new JsonObject () {
            [ "tools" ] = tools
        };
        if (modern) {
            toolsResult [ "resultType" ] = "complete";
            toolsResult [ "ttlMs" ] = 0;
            toolsResult [ "cacheScope" ] = "private";
            toolsResult [ "_meta" ] = new JsonObject () {
                [ "io.modelcontextprotocol/serverInfo" ] = GetServerInfo ()
            };
        }
        return toolsResult;
    }

    JsonObject GetInitializeResult ( string protocolVersion ) {
        JsonObject result = new JsonObject () {
            [ "protocolVersion" ] = protocolVersion,
            [ "serverInfo" ] = GetServerInfo (),
            [ "capabilities" ] = GetCapabilities ()
        };
        if (!string.IsNullOrWhiteSpace ( ClientInstructions ))
            result [ "instructions" ] = ClientInstructions;
        return result;
    }

    /// <summary>
    /// Handles an incoming HTTP request for MCP operations asynchronously.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <param name="cancellation">A token to observe for cancellation requests.</param>
    /// <returns>An HTTP response representing the result of the request handling.</returns>
    public async Task<HttpResponse> HandleRequestAsync ( HttpRequest request, CancellationToken cancellation = default ) {
        string? origin = request.Headers.Origin;
        if (origin is { } && !(OriginValidator?.Invoke ( request, origin ) ?? false))
            return new HttpResponse ( HttpStatusCode.Forbidden );

        string sessionId = request.Headers [ "Mcp-Session-Id" ] ?? Guid.NewGuid ().ToString ();

        if (request.Method == HttpMethod.Get)
            return new HttpResponse ( HttpStatusCode.MethodNotAllowed );
        if (request.Method != HttpMethod.Post)
            return new HttpResponse ( HttpStatusCode.MethodNotAllowed );

        Memory<byte> contentBytes = await request.GetBodyContentsAsync ( cancellation );
        JsonValue requestValue;
        try {
            requestValue = await Json.DeserializeAsync<JsonValue> ( contentBytes, cancellation );
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) {
            throw;
        }
        catch (Exception) {
            return new McpJsonResponse (
                new JsonRpcErrorResponse ( -32700, "Parse error", id: null ),
                sessionId: null ) {
                Status = HttpStatusInformation.BadRequest
            };
        }

        if (requestValue.Type != JsonValueType.Object) {
            return new McpJsonResponse (
                new JsonRpcErrorResponse ( -32600, "Invalid Request", id: null ),
                sessionId: null ) {
                Status = HttpStatusInformation.BadRequest
            };
        }

        JsonObject requestObject = requestValue.GetJsonObject ();
        JsonValue idValue = requestObject [ "id" ];
        bool isNotification = idValue.Type == JsonValueType.Undefined;
        JsonValue id = isNotification ? JsonValue.Null : idValue;
        JsonValue? errorId = isNotification ? (JsonValue?) null : id;
        JsonValue versionValue = requestObject [ "jsonrpc" ];
        JsonValue methodValue = requestObject [ "method" ];
        if (versionValue.Type != JsonValueType.String
            || versionValue.GetString () != "2.0"
            || methodValue.Type != JsonValueType.String
            || idValue.Type is not JsonValueType.String and not JsonValueType.Number and not JsonValueType.Undefined) {
            return new McpJsonResponse (
                new JsonRpcErrorResponse ( -32600, "Invalid Request", errorId ),
                sessionId: null ) {
                Status = HttpStatusInformation.BadRequest
            };
        }

        string method = methodValue.GetString ();
        JsonValue parametersValue = requestObject [ "params" ];
        if (parametersValue.Type is not JsonValueType.Object and not JsonValueType.Null and not JsonValueType.Undefined) {
            return new McpJsonResponse (
                new JsonRpcErrorResponse ( -32602, "Invalid params", errorId ),
                sessionId: null ) {
                Status = HttpStatusInformation.BadRequest
            };
        }

        JsonObject parameters = parametersValue.Type == JsonValueType.Object ? parametersValue.GetJsonObject () : [];
        JsonValue metadataValue = parameters [ "_meta" ];
        if (metadataValue.Type is not JsonValueType.Object and not JsonValueType.Null and not JsonValueType.Undefined) {
            return new McpJsonResponse (
                new JsonRpcErrorResponse ( -32602, "Invalid params: _meta must be an object", errorId ),
                sessionId: null ) {
                Status = HttpStatusInformation.BadRequest
            };
        }

        JsonObject metadata = metadataValue.Type == JsonValueType.Object ? metadataValue.GetJsonObject () : [];
        JsonValue metadataVersionValue = metadata [ "io.modelcontextprotocol/protocolVersion" ];
        string? metadataVersion = metadataVersionValue.Type == JsonValueType.String ? metadataVersionValue.GetString () : null;
        string? protocolHeader = request.Headers [ "MCP-Protocol-Version" ];
        bool modern = metadataVersion is { }
            || method == "server/discover"
            || (protocolHeader is { } && !LegacyProtocolVersions.Contains ( protocolHeader ));

        if (isNotification && !modern)
            return new HttpResponse ( HttpStatusCode.Accepted );

        if (method == "initialize"
            && metadataVersion is null
            && (protocolHeader is null || LegacyProtocolVersions.Contains ( protocolHeader ))) {
            JsonValue requestedLegacyVersionValue = parameters [ "protocolVersion" ];
            string requestedLegacyVersion = requestedLegacyVersionValue.Type == JsonValueType.String
                ? requestedLegacyVersionValue.GetString ()
                : "2025-11-25";
            string selectedLegacyVersion = LegacyProtocolVersions.Contains ( requestedLegacyVersion )
                ? requestedLegacyVersion
                : LegacyProtocolVersions [ 0 ];
            return new McpJsonResponse (
                new JsonRpcResponse ( GetInitializeResult ( selectedLegacyVersion ), id ),
                sessionId );
        }

        if (modern) {
            if (metadataVersion is null || metadata [ "io.modelcontextprotocol/clientCapabilities" ].Type != JsonValueType.Object) {
                return new McpJsonResponse (
                    new JsonRpcErrorResponse ( -32602, "Invalid params: required request metadata is missing", errorId ),
                    sessionId: null ) {
                    Status = HttpStatusInformation.BadRequest
                };
            }

            string? methodHeader = request.Headers [ "Mcp-Method" ];
            string? nameHeader = request.Headers [ "Mcp-Name" ];
            JsonValue bodyNameValue = parameters [ "name" ];
            string? bodyName = method == "tools/call" && bodyNameValue.Type == JsonValueType.String
                ? bodyNameValue.GetString ()
                : null;
            if (nameHeader is { } && nameHeader.StartsWith ( "=?base64?", StringComparison.Ordinal ) && nameHeader.EndsWith ( "?=", StringComparison.Ordinal )) {
                try {
                    nameHeader = Encoding.UTF8.GetString ( Convert.FromBase64String ( nameHeader [ 9..^2 ] ) );
                }
                catch (FormatException) {
                    nameHeader = null;
                }
            }

            if (protocolHeader != metadataVersion
                || methodHeader != method
                || (method == "tools/call" && (nameHeader is null || nameHeader != bodyName))) {
                return new McpJsonResponse (
                    new JsonRpcErrorResponse ( HEADER_MISMATCH_ERROR, "Required HTTP headers are missing or do not match the request body", errorId ),
                    sessionId: null ) {
                    Status = HttpStatusInformation.BadRequest
                };
            }

            if (metadataVersion != PROTOCOL_VERSION) {
                return new McpJsonResponse (
                    new JsonRpcErrorResponse ( UNSUPPORTED_PROTOCOL_VERSION_ERROR, "Unsupported protocol version", errorId, new JsonObject () {
                        [ "supported" ] = new JsonArray ( [.. SupportedProtocolVersions] ),
                        [ "requested" ] = metadataVersion
                    } ),
                    sessionId: null ) {
                    Status = HttpStatusInformation.BadRequest
                };
            }
        }

        if (isNotification)
            return new HttpResponse ( HttpStatusCode.Accepted );

        if (method == "server/discover") {
            return new McpJsonResponse (
                new JsonRpcResponse ( GetResult ( method, modern: true ), id ),
                sessionId: null );
        }
        if (method == "tools/list") {
            return new McpJsonResponse (
                new JsonRpcResponse ( GetResult ( method, modern ), id ),
                modern ? null : sessionId );
        }
        if (method == "tools/call") {
            JsonValue toolNameValue = parameters [ "name" ];
            JsonValue toolInputValue = parameters [ "arguments" ];
            if (toolNameValue.Type != JsonValueType.String
                || toolInputValue.Type is not JsonValueType.Object and not JsonValueType.Null and not JsonValueType.Undefined) {
                return new McpJsonResponse (
                    new JsonRpcErrorResponse ( -32602, "Invalid params", errorId ),
                    modern ? null : sessionId );
            }

            string toolName = toolNameValue.GetString ();
            JsonObject toolInput = toolInputValue.Type == JsonValueType.Object ? toolInputValue.GetJsonObject () : [];
            var tool = Tools.FirstOrDefault ( t => t.Name == toolName );

            if (tool is null) {
                return new McpJsonResponse (
                    new JsonRpcErrorResponse ( -32602, $"Unknown tool: {toolName}", id ),
                    modern ? null : sessionId );
            }

            JsonObject result = [];
            if (tool.Schema.Validate ( toolInput ) is { IsValid: false } validationError) {
                result [ "content" ] = new JsonArray ( [new JsonObject () {
                    [ "type" ] = "text",
                    [ "text" ] = "Error executing tool: failed to validate the JSON schema of the function call. See errors below.\r\n"
                        + string.Join ( "\r\n", validationError.Errors.Select ( e => $"- [{e.Path}] {e.Message}" ) )
                }] );
                result [ "isError" ] = true;
            }
            else {
                McpToolContext? context = null;
                try {
                    context = new McpToolContext () {
                        Cancellation = cancellation,
                        Server = this,
                        Request = request,
                        Arguments = toolInput,
                        ToolName = toolName!,
                        Metadata = metadata
                    };

                    var toolResult = await tool.ExecuteAsync ( context );
                    if (tool.OutputSchema is { } outputSchema
                        && (toolResult.StructuredContent is not { } structuredContent
                            || outputSchema.Validate ( structuredContent ) is { IsValid: false })) {
                        result [ "content" ] = new JsonArray ( [new JsonObject () {
                            [ "type" ] = "text",
                            [ "text" ] = "Error executing tool: structured content does not conform to the output schema"
                        }] );
                        result [ "isError" ] = true;
                    }
                    else {
                        result [ "content" ] = toolResult.Result.IsJsonArray
                            ? toolResult.Result
                            : new JsonArray ( [toolResult.Result] );
                        if (toolResult.StructuredContent is { } structuredResult)
                            result [ "structuredContent" ] = structuredResult;
                        result [ "isError" ] = false;
                    }
                }
                catch (Exception ex) {
                    if (request.Context.HttpServer.ServerConfiguration.ThrowExceptions)
                        throw;

                    request.Context.HttpServer.ServerConfiguration.ErrorsLogsStream?.WriteException ( ex,
                        $"Exception raised within MCP tool execution. MCP Provider={ToString ()} ToolName={toolName} Content={toolInput} Meta={context?.Metadata}" );
                    result [ "content" ] = new JsonArray ( [new JsonObject () {
                        [ "type" ] = "text",
                        [ "text" ] = "Error executing tool"
                    }] );
                    result [ "isError" ] = true;
                }
            }

            if (modern) {
                result [ "resultType" ] = "complete";
                result [ "_meta" ] = new JsonObject () {
                    [ "io.modelcontextprotocol/serverInfo" ] = GetServerInfo ()
                };
            }
            return new McpJsonResponse (
                new JsonRpcResponse ( result, id ),
                modern ? null : sessionId );
        }
        if (method == "ping" && !modern) {
            return new McpJsonResponse (
                new JsonRpcResponse ( new JsonObject (), id ),
                sessionId );
        }
        return new McpJsonResponse (
            new JsonRpcErrorResponse ( -32601, "Method not found", id ),
            modern ? null : sessionId ) {
            Status = modern ? HttpStatusInformation.NotFound : HttpStatusInformation.Ok
        };
    }
}
