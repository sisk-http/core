// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   TransferEncoding.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP transfer-encoding algorithm.
/// </summary>
[Flags]
public enum TransferEncoding {
    /// <summary>
    /// Indicates that the response is sent in a series of chunks.
    /// </summary>
    Chunked = 1 << 1,

    /// <summary>
    /// Indicates that the response is compressed using GZip encoding.
    /// </summary>
    GZip = 1 << 2,

    /// <summary>
    /// Indicates that the response is compressed using Deflate encoding.
    /// </summary>
    Deflate = 1 << 3
}
