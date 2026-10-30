// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SR.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.ModelContextProtocol;

internal static class SR {
    public const string McpProvider_ParseError = "Parse error";
    public const string McpProvider_InvalidRequest = "Invalid Request";
    public const string McpProvider_InvalidParams = "Invalid params";
    public const string McpProvider_MetaNotObject = "Invalid params: _meta must be an object";
    public const string McpProvider_RequestMetadataMissing = "Invalid params: required request metadata is missing";
    public const string McpProvider_HeaderMismatch = "Required HTTP headers are missing or do not match the request body";
    public const string McpProvider_UnsupportedProtocolVersion = "Unsupported protocol version";
    public const string McpProvider_InvalidSkillCatalog = "Invalid skill catalog: duplicate skills, conflicting files or incomplete nested manifests";
    public const string McpProvider_UnknownSkillUri = "Unknown skill or resource URI";
    public const string McpProvider_UnknownTool = "Unknown tool: {0}";
    public const string McpProvider_MethodNotFound = "Method not found";
    public const string McpProvider_ToolSchemaValidationFailed = "Error executing tool: failed to validate the JSON schema of the function call. See errors below.\r\n";
    public const string McpProvider_ToolOutputSchemaMismatch = "Error executing tool: structured content does not conform to the output schema";
    public const string McpProvider_ToolExecutionFailed = "Error executing tool";
    public const string McpProvider_ToolExceptionLog = "Exception raised within MCP tool execution. MCP Provider={0} ToolName={1} Content={2} Meta={3}";

    public const string McpProviderExtensions_NotConfigured = "MCP provider is not configured. Please call UseMcp() on the HttpServerHost builder first.";

    public const string McpSkill_InvalidFrontmatter = "Frontmatter requires a valid skill name and a description of 1 to 1024 characters.";
    public const string McpSkill_InvalidPath = "The skill path must contain URI-safe segments and end in the skill name.";
    public const string McpSkill_InvalidSupportingFiles = "Supporting files must have unique relative paths and cannot replace SKILL.md.";
    public const string McpSkill_LimitExceeded = "A skill cannot exceed 512 files or 16 MiB.";

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static string Format ( string format, params object? [] items ) {
        return String.Format ( provider: null, format, items );
    }
}
