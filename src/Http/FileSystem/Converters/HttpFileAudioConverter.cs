// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpFileAudioConverter.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http.FileSystem.Converters;

internal class HttpFileAudioConverter : HttpFileRangedContentStream {

    static string [] AllowedExtensions = [ ".mp3", ".ogg", ".wav", ".flac", ".ogv" ];

    public override bool CanConvert ( FileInfo file ) {
        return AllowedExtensions.Contains ( file.Extension.ToLowerInvariant () );
    }
}
