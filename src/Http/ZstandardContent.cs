// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ZstandardContent.cs
// Repository:  https://github.com/sisk-http/core

using System.IO.Compression;
using Sisk.Core.Helpers;

namespace Sisk.Core.Http;
#if NET11_0_OR_GREATER

/// <summary>
/// Represents an HTTP content that is compressed using the ZStandard algorithm.
/// </summary>
public sealed class ZstandardContent : CompressedContent {

    /// <inheritdoc/>
    public ZstandardContent ( HttpContent innerContent ) : base ( innerContent ) {
    }

    /// <inheritdoc/>
    public ZstandardContent ( byte [] byteArrayContent ) : base ( byteArrayContent ) {
    }

    /// <inheritdoc/>
    public ZstandardContent ( Stream baseContent ) : base ( baseContent ) {
    }

    /// <inheritdoc/>
    public override Stream GetCompressingStream ( Stream outputStream ) {
        return new ZstandardStream ( outputStream, CompressionMode.Compress, leaveOpen: true );
    }

    /// <inheritdoc/>
    public override void Setup () {
        HeaderHelper.CopyHttpHeaders ( InnerContent.Headers, Headers );
        Headers.ContentEncoding.Add ( "zstd" );
    }
}

#endif