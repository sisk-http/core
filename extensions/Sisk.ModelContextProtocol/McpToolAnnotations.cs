// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   McpToolAnnotations.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.ModelContextProtocol;

/// <summary>
/// Provides optional hints describing the behavior of an MCP tool.
/// </summary>
public sealed class McpToolAnnotations {

    /// <summary>
    /// Gets or sets a human-readable title for the tool.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets whether the tool does not modify its environment.
    /// </summary>
    public bool? ReadOnlyHint { get; set; }

    /// <summary>
    /// Gets or sets whether the tool may perform destructive updates.
    /// </summary>
    public bool? DestructiveHint { get; set; }

    /// <summary>
    /// Gets or sets whether repeated calls with the same arguments have no additional effect.
    /// </summary>
    public bool? IdempotentHint { get; set; }

    /// <summary>
    /// Gets or sets whether the tool may interact with external entities.
    /// </summary>
    public bool? OpenWorldHint { get; set; }
}
