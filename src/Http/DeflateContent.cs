// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   DeflateContent.cs
// Repository:  https://github.com/sisk-http/core

using System.IO.Compression;
using Sisk.Core.Helpers;

namespace Sisk.Core.Http;

/// <summary>
/// Represents an HTTP content that is compressed using the Deflate algorithm.
/// </summary>
public sealed class DeflateContent : CompressedContent {

    /// <inheritdoc/>
    public DeflateContent ( HttpContent innerContent ) : base ( innerContent ) {
    }

    /// <inheritdoc/>
    public DeflateContent ( byte [] byteArrayContent ) : base ( byteArrayContent ) {
    }

    /// <inheritdoc/>
    public DeflateContent ( Stream baseContent ) : base ( baseContent ) {
    }

    /// <inheritdoc/>
    public override Stream GetCompressingStream ( Stream outputStream ) {
        return new DeflateStream ( outputStream, CompressionMode.Compress, leaveOpen: true );
    }

    /// <inheritdoc/>
    public override void Setup () {
        HeaderHelper.CopyHttpHeaders ( this.InnerContent.Headers, this.Headers );
        this.Headers.ContentEncoding.Add ( "deflate" );
    }
}
