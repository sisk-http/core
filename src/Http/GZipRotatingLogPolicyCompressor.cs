// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   GZipRotatingLogPolicyCompressor.cs
// Repository:  https://github.com/sisk-http/core

using System.IO.Compression;

namespace Sisk.Core.Http;

sealed class GZipRotatingLogPolicyCompressor : RotatingLogPolicyCompressor {

    public override string GetCompressedFileName ( string preFormattedName ) {
        return $"{preFormattedName}.gz";
    }

    public override Stream GetCompressingStream ( Stream logFileStream ) {
        return new GZipStream ( logFileStream, CompressionMode.Compress );
    }
}
