// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   McpTool.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightJson.Schema;

namespace Sisk.ModelContextProtocol;

/// <summary>
/// Represents a tool that can be hosted and executed by an MCP server.
/// </summary>
public sealed class McpTool {

    /// <summary>
    /// Gets the unique name of the tool.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the display title of the tool. If null, the <see cref="Name"/> will be used.
    /// </summary>
    public string? Title { get; }

    /// <summary>
    /// Gets a description of what the tool does.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the JSON schema that defines the expected input arguments for the tool.
    /// </summary>
    public JsonSchema Schema { get; }

    /// <summary>
    /// Gets or sets the handler function that will be executed when the tool is invoked.
    /// </summary>
    public McpToolHandler ExecuteAsync { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpTool"/> class.
    /// </summary>
    /// <param name="name">The unique name of the tool.</param>
    /// <param name="description">A description of what the tool does.</param>
    /// <param name="schema">The JSON schema defining the tool's input arguments.</param>
    /// <param name="executionHandler">The handler function that will be executed when the tool is invoked.</param>
    /// <param name="title">The optional display title of the tool.</param>
    public McpTool ( string name, string description, JsonSchema schema, McpToolHandler executionHandler, string? title = null ) {
        Name = name;
        Title = title;
        Description = description;
        Schema = schema;
        ExecuteAsync = executionHandler;
    }
}