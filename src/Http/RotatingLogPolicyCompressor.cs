// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RotatingLogPolicyCompressor.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http;

/// <summary>
/// Provides a base class for implementing log compression policies.
/// </summary>
public abstract class RotatingLogPolicyCompressor {

    /// <summary>
    /// Gets the compressed file name based on the provided pre-formatted name.
    /// </summary>
    /// <param name="preFormattedName">The pre-formatted name of the log file.</param>
    /// <returns>The compressed file name.</returns>
    public abstract string GetCompressedFileName ( string preFormattedName );

    /// <summary>
    /// Gets a stream for compressing the log file.
    /// </summary>
    /// <param name="logFileStream">The stream of the log file to be compressed.</param>
    /// <returns>A stream for compressing the log file.</returns>
    public abstract Stream GetCompressingStream ( Stream logFileStream );
}