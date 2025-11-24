// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpFileServerFileConverter.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http.FileSystem;

/// <summary>
/// Base class for implementing custom file converters that transform files before they are served as HTTP responses.
/// </summary>
public abstract class HttpFileServerFileConverter {

    /// <summary>
    /// Determines whether this converter can process the specified file.
    /// </summary>
    /// <param name="file">The file to inspect.</param>
    /// <returns><see langword="true"/> if this converter can handle the file; otherwise, <see langword="false"/>.</returns>
    public abstract bool CanConvert ( FileInfo file );

    /// <summary>
    /// Converts the specified file into an HTTP response.
    /// </summary>
    /// <param name="file">The file to convert.</param>
    /// <param name="request">The current HTTP request.</param>
    /// <returns>An <see cref="HttpResponse"/> representing the converted file.</returns>
    public abstract HttpResponse Convert ( FileInfo file, HttpRequest request );
}
