// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CompressedContent.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;

namespace Sisk.Core.Http;

/// <summary>
/// Represents a base class for HTTP contents served over an compressing stream.
/// </summary>
public abstract class CompressedContent : HttpContent {

    /// <summary>
    /// Gets the inner HTTP content.
    /// </summary>
    public HttpContent InnerContent { get; }

    /// <summary>
    /// Initializes a new instance of compressing stream with the specified inner HTTP content.
    /// </summary>
    /// <param name="innerContent">The inner HTTP content.</param>
    public CompressedContent ( HttpContent innerContent ) {
        this.InnerContent = innerContent;
        this.Setup ();
    }

    /// <summary>
    /// Initializes a new instance of compressing stream with the specified byte array content.
    /// </summary>
    /// <param name="byteArrayContent">The byte array content.</param>
    public CompressedContent ( byte [] byteArrayContent ) {
        this.InnerContent = new ByteArrayContent ( byteArrayContent );
        this.Setup ();
    }

    /// <summary>
    /// Initializes a new instance of compressing stream with the specified stream content.
    /// </summary>
    /// <param name="baseContent">The stream content.</param>
    public CompressedContent ( Stream baseContent ) {
        this.InnerContent = new StreamContent ( baseContent );
        this.Setup ();
    }

    /// <summary>
    /// Gets a stream that compresses the output stream.
    /// </summary>
    /// <param name="outputStream">The output stream to compress.</param>
    /// <returns>A stream that compresses the output stream.</returns>
    public abstract Stream GetCompressingStream ( Stream outputStream );

    /// <summary>
    /// Represents the method that is invoked once within the constructor to setup
    /// this compressor. This method is indeeded to add the missing Content-Encoding headers
    /// used by this compressor.
    /// </summary>
    public abstract void Setup ();

    /// <inheritdoc/>
    protected override sealed void SerializeToStream ( Stream stream, TransportContext? context, CancellationToken cancellationToken ) {
        using var compressStream = this.GetCompressingStream ( stream );
        using var contentStream = this.InnerContent.ReadAsStream ( cancellationToken );

        if (contentStream.CanSeek)
            contentStream.Seek ( 0, SeekOrigin.Begin );

        contentStream.CopyTo ( compressStream );
    }

    /// <inheritdoc/>
    protected override sealed async Task SerializeToStreamAsync ( Stream stream, TransportContext? context ) {
        using var compressStream = this.GetCompressingStream ( stream );
        using var contentStream = await this.InnerContent.ReadAsStreamAsync ();

        if (contentStream.CanSeek)
            contentStream.Seek ( 0, SeekOrigin.Begin );

        await contentStream.CopyToAsync ( compressStream );
    }

    /// <inheritdoc/>
    protected override sealed async Task SerializeToStreamAsync ( Stream stream, TransportContext? context, CancellationToken cancellationToken ) {
        using var compressStream = this.GetCompressingStream ( stream );
        using var contentStream = await this.InnerContent.ReadAsStreamAsync ( cancellationToken );

        if (contentStream.CanSeek)
            contentStream.Seek ( 0, SeekOrigin.Begin );

        await contentStream.CopyToAsync ( compressStream, cancellationToken );
    }

    /// <inheritdoc/>
    protected override sealed bool TryComputeLength ( out long length ) {
        length = -1;
        return false;
    }

    /// <inheritdoc/>
    protected override void Dispose ( bool disposing ) {
        base.Dispose ( disposing );
        if (disposing) {
            this.InnerContent?.Dispose ();
        }
    }
}
