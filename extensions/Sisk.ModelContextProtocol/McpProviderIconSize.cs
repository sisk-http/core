// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   McpProviderIconSize.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;

namespace Sisk.ModelContextProtocol;

/// <summary>
/// Represents the pixel dimensions at which an MCP provider icon can be displayed.
/// </summary>
public readonly struct McpProviderIconSize : IEquatable<McpProviderIconSize> {

    readonly int w, h;

    /// <summary>
    /// Represents an icon that can be displayed at any size.
    /// </summary>
    public static readonly McpProviderIconSize Any = new ();

    /// <summary>
    /// Gets the icon width in pixels, or <c>0</c> when the size is <see cref="Any"/>.
    /// </summary>
    public int Width => w;

    /// <summary>
    /// Gets the icon height in pixels, or <c>0</c> when the size is <see cref="Any"/>.
    /// </summary>
    public int Height => h;

    /// <summary>
    /// Creates an icon size with the specified dimensions.
    /// </summary>
    /// <param name="width">The icon width in pixels.</param>
    /// <param name="height">The icon height in pixels.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is less than or equal to zero.</exception>
    public McpProviderIconSize ( int width, int height ) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( width );
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( height );

        this.w = width;
        this.h = height;
    }

    /// <summary>
    /// Creates an icon size that represents any dimensions.
    /// </summary>
    public McpProviderIconSize () {
        w = 0;
        h = 0;
    }

    /// <inheritdoc/>
    public override string ToString () {
        if (Width == 0 && Height == 0) {
            return "any";
        }

        return $"{Width}x{Height}";
    }

    /// <inheritdoc/>
    public override int GetHashCode () {
        return w ^ h;
    }

    /// <inheritdoc/>
    public override bool Equals ( [NotNullWhen ( true )] object? obj ) {
        return obj is McpProviderIconSize iconSize && Equals ( iconSize );
    }

    /// <inheritdoc/>
    public bool Equals ( McpProviderIconSize other ) {
        return other.w == w && other.h == h;
    }
}