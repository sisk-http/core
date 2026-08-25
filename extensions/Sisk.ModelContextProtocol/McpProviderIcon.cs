// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   McpProviderIcon.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.ModelContextProtocol;

/// <summary>
/// Represents an icon that clients can display for an MCP server.
/// </summary>
public sealed class McpProviderIcon : IJsonSerializable<McpProviderIcon> {

    /// <summary>
    /// Gets the URI of the icon resource.
    /// </summary>
    public string SourceUrl { get; }

    /// <summary>
    /// Gets the MIME type of the icon, if explicitly specified.
    /// </summary>
    public string? MimeType { get; }

    /// <summary>
    /// Gets the sizes at which the icon can be displayed.
    /// </summary>
    public IEnumerable<McpProviderIconSize> Sizes { get; }

    /// <summary>
    /// Gets the color theme for which the icon is intended.
    /// </summary>
    public McpProviderIconTheme? Theme { get; }

    /// <summary>
    /// Creates an icon with the specified source and optional display metadata.
    /// </summary>
    /// <param name="sourceUrl">The URI of the icon resource.</param>
    /// <param name="mimeType">The MIME type of the icon, or <see langword="null"/> to let the client infer it.</param>
    /// <param name="sizes">The supported icon sizes, or <see langword="null"/> if the icon can be used at any size.</param>
    /// <param name="theme">The color theme for which the icon is intended.</param>
    /// <exception cref="ArgumentException"><paramref name="sourceUrl"/> is empty or consists only of white-space characters.</exception>
    public McpProviderIcon (
        string sourceUrl,
        string? mimeType = null,
        IEnumerable<McpProviderIconSize>? sizes = null,
        McpProviderIconTheme? theme = null ) {

        ArgumentException.ThrowIfNullOrWhiteSpace ( sourceUrl );

        SourceUrl = sourceUrl;
        MimeType = mimeType;
        Sizes = sizes ?? [];
        Theme = theme;
    }

    static McpProviderIcon IJsonSerializable<McpProviderIcon>.DeserializeFromJson ( JsonValue json, JsonOptions options ) {
        throw new NotSupportedException ();
    }

    /// <summary>
    /// Serializes an MCP provider icon into its JSON representation.
    /// </summary>
    /// <param name="self">The icon to serialize.</param>
    /// <param name="options">The options used to serialize the icon.</param>
    /// <returns>The JSON representation of <paramref name="self"/>.</returns>
    public static JsonValue SerializeIntoJson ( McpProviderIcon self, JsonOptions options ) {
        var r = new JsonObject () {
            [ "src" ] = self.SourceUrl
        };

        if (!string.IsNullOrWhiteSpace ( self.MimeType ))
            r [ "mimeType" ] = self.MimeType;
        if (self.Sizes.Any ())
            r [ "sizes" ] = JsonArray.Create ( self.Sizes.Distinct () );
        if (self.Theme is { } theme)
            r [ "theme" ] = theme.ToString ().ToLowerInvariant ();

        return r;
    }
}