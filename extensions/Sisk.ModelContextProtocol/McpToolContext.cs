// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   McpToolContext.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.ModelContextProtocol;


/// <summary>
/// Represents a delegate that handles the execution of an MCP tool.
/// </summary>
/// <param name="context">The context in which the tool is executed.</param>
/// <returns>A task that, when completed, yields the <see cref="McpToolResult"/> of the tool execution.</returns>
public delegate Task<McpToolResult> McpToolHandler ( McpToolContext context );

/// <summary>
/// Provides context for the execution of an MCP tool.
/// </summary>
public sealed class McpToolContext {
    /// <summary>
    /// Gets a token to observe for cancellation requests.
    /// </summary>
    public required CancellationToken Cancellation { get; init; }

    /// <summary>
    /// Gets the MCP server instance associated with the current context.
    /// </summary>
    public required McpProvider Server { get; init; }

    /// <summary>
    /// Gets the incoming HTTP request that triggered the tool execution.
    /// </summary>
    public required HttpRequest Request { get; init; }

    /// <summary>
    /// Gets the name of the tool being executed.
    /// </summary>
    public required string ToolName { get; init; }

    /// <summary>
    /// Gets the arguments provided for the tool execution as a JSON object.
    /// </summary>
    public required JsonObject Arguments { get; init; }

    /// <summary>
    /// Gets any additional metadata associated with the tool execution.
    /// </summary>
    public required JsonObject Metadata { get; init; }
}