// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ChunkParseException.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Cadente.Streams;

#pragma warning disable CS1591
public sealed class ChunkParseException : IOException {
    public ChunkParseException ( string message ) : base ( message ) { }
    public ChunkParseException ( string message, Exception inner ) : base ( message, inner ) { }
}
