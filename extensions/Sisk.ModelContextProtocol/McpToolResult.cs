// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   McpToolResult.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.ModelContextProtocol;


/// <summary>
/// Represents the result of executing an MCP tool.
/// </summary>
public sealed class McpToolResult {

    /// <summary>
    /// Gets the JSON representation of the tool result.
    /// </summary>
    public JsonValue Result { get; }

    /// <summary>
    /// Creates a text-based result for an MCP tool.
    /// </summary>
    /// <param name="text">The text content of the result.</param>
    /// <returns>An <see cref="McpToolResult"/> representing a text result.</returns>
    public static McpToolResult CreateText ( string text ) {
        return new McpToolResult ( new JsonObject () {
            [ "type" ] = "text",
            [ "text" ] = text
        } );
    }

    /// <summary>
    /// Creates an image-based result for an MCP tool.
    /// </summary>
    /// <param name="imageBytes">The byte array representing the image data.</param>
    /// <param name="mimeType">The MIME type of the image (e.g., "image/png"). Defaults to "image/png".</param>
    /// <returns>An <see cref="McpToolResult"/> representing an image result.</returns>
    public static McpToolResult CreateImage ( ReadOnlySpan<byte> imageBytes, string mimeType = "image/png" ) {
        return new McpToolResult ( new JsonObject () {
            [ "type" ] = "image",
            [ "data" ] = Convert.ToBase64String ( imageBytes, Base64FormattingOptions.None ),
            [ "mimeType" ] = mimeType
        } );
    }

    /// <summary>
    /// Creates an audio-based result for an MCP tool.
    /// </summary>
    /// <param name="audioBytes">The byte array representing the audio data.</param>
    /// <param name="mimeType">The MIME type of the audio (e.g., "audio/wav"). Defaults to "audio/wav".</param>
    /// <returns>An <see cref="McpToolResult"/> representing an audio result.</returns>
    public static McpToolResult CreateAudio ( ReadOnlySpan<byte> audioBytes, string mimeType = "audio/wav" ) {
        return new McpToolResult ( new JsonObject () {
            [ "type" ] = "audio",
            [ "data" ] = Convert.ToBase64String ( audioBytes, Base64FormattingOptions.None ),
            [ "mimeType" ] = mimeType
        } );
    }

    /// <summary>
    /// Combines multiple <see cref="McpToolResult"/> objects into a single result.
    /// If the input contains a single result, it is returned directly. Otherwise,
    /// all individual results are unpacked and combined into a JSON array.
    /// </summary>
    /// <param name="results">A collection of <see cref="McpToolResult"/> objects to combine.</param>
    /// <returns>A single <see cref="McpToolResult"/> containing the combined results.</returns>
    public static McpToolResult Combine ( params McpToolResult [] results ) {
        if (results.Length == 1)
            return results [ 0 ];

        JsonArray resultsValue = [];
        void Unpack ( JsonValue value ) {
            if (value.Type == JsonValueType.Object) {
                resultsValue.Add ( value );
            }
            else if (value.Type == JsonValueType.Array) {
                foreach (var item in value.GetJsonArray ()) {
                    Unpack ( item );
                }
            }
        }

        foreach (var item in results) {
            Unpack ( item.Result );
        }

        return new McpToolResult ( resultsValue );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpToolResult"/> class.
    /// </summary>
    /// <param name="result">The JSON value representing the tool result.</param>
    public McpToolResult ( JsonValue result ) {
        Result = result;
    }

    /// <inheritdoc/>
    public static implicit operator McpToolResult ( string text ) => CreateText ( text );
}