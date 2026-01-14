// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   FileContent.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using Sisk.Core.Helpers;

namespace Sisk.Core.Http;

/// <summary>
/// Provides HTTP content based on a file.
/// </summary>
public sealed class FileContent : HttpContent {

    const string DefaultMimeType = "application/octet-stream";

    /// <summary>
    /// Gets the file information for the content.
    /// </summary>
    public FileInfo File { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileContent"/> class with the specified file.
    /// </summary>
    /// <param name="file">The file to be used as content.</param>
    public FileContent ( FileInfo file ) {
        if (!file.Exists)
            throw new FileNotFoundException ( SR.Format ( SR.FileContent_FileNotFound, file.FullName ) );

        File = file;

        var mimeType = MimeHelper.GetMimeType ( File.Extension, DefaultMimeType );
        if (mimeType == DefaultMimeType) {
            Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue ( "attachment" ) {
                FileName = File.Name
            };
        }
        else {
            Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue ( "inline" );
        }

        Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ( mimeType );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileContent"/> class with the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the file to be used as content.</param>
    public FileContent ( string filePath ) : this ( new FileInfo ( filePath ) ) {
    }

    void ThrowIfFileNotFound () {
        if (!File.Exists)
            throw new FileNotFoundException ( "The specified file was not found.", File.FullName );
    }

    /// <inheritdoc/>
    protected override async Task SerializeToStreamAsync ( Stream stream, TransportContext? context, CancellationToken cancellationToken ) {
        ThrowIfFileNotFound ();

        using var fs = File.OpenRead ();
        await fs.CopyToAsync ( stream, cancellationToken );
    }

    /// <inheritdoc/>
    protected override Task SerializeToStreamAsync ( Stream stream, TransportContext? context ) {
        return SerializeToStreamAsync ( stream, context, default );
    }

    /// <inheritdoc/>
    protected override void SerializeToStream ( Stream stream, TransportContext? context, CancellationToken cancellationToken ) {
        ThrowIfFileNotFound ();

        using var fs = File.OpenRead ();
        fs.CopyTo ( stream );
    }

    /// <inheritdoc/>
    protected override Stream CreateContentReadStream ( CancellationToken cancellationToken ) {
        ThrowIfFileNotFound ();

        return File.OpenRead ();
    }

    /// <inheritdoc/>
    protected override Task<Stream> CreateContentReadStreamAsync () {
        return Task.FromResult ( CreateContentReadStream ( default ) );
    }

    /// <inheritdoc/>
    protected override Task<Stream> CreateContentReadStreamAsync ( CancellationToken cancellationToken ) {
        return Task.FromResult ( CreateContentReadStream ( cancellationToken ) );
    }

    /// <inheritdoc/>
    protected override bool TryComputeLength ( out long length ) {
        ThrowIfFileNotFound ();

        length = File.Length;
        return true;
    }
}
