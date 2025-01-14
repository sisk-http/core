// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   GZipContent.cs
// Repository:  https://github.com/sisk-http/core

using System.IO.Compression;

namespace Sisk.Core.Http;

/// <summary>
/// Represents an HTTP content that is compressed using the GZip algorithm.
/// </summary>
public sealed class GZipContent : CompressedContent {

    /// <inheritdoc/>
    public GZipContent ( HttpContent innerContent ) : base ( innerContent ) {
    }

    /// <inheritdoc/>
    public GZipContent ( byte [] byteArrayContent ) : base ( byteArrayContent ) {
    }

    /// <inheritdoc/>
    public GZipContent ( Stream baseContent ) : base ( baseContent ) {
    }

    /// <inheritdoc/>
    public override Stream GetCompressingStream ( Stream outputStream ) {
        return new GZipStream ( outputStream, CompressionMode.Compress, leaveOpen: true );
    }

    /// <inheritdoc/>
    public override void Setup () {
        this.Headers.ContentEncoding.Add ( "gzip" );
    }
}
