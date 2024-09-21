// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MultipartObjectCommonFormat.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Entity;

/// <summary>
/// Represents an image format for Multipart objects.
/// </summary>
public enum MultipartObjectCommonFormat
{
    /// <summary>
    /// Represents that the object is not a recognized image.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Represents an JPEG/JPG image.
    /// </summary>
    JPEG = 100,

    /// <summary>
    /// Represents an GIF image.
    /// </summary>
    GIF = 101,

    /// <summary>
    /// Represents an PNG image.
    /// </summary>
    PNG = 102,

    /// <summary>
    /// Represents an TIFF image.
    /// </summary>
    TIFF = 103,

    /// <summary>
    /// Represents an bitmap image.
    /// </summary>
    BMP = 104,

    /// <summary>
    /// Represents an WebP image.
    /// </summary>
    WEBP = 105,

    /// <summary>
    /// Represents an PDF file.
    /// </summary>
    PDF = 200
}
