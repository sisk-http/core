// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ChunkParseException.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Cadente.Streams;

/// <summary>
/// The exception that is thrown when a chunked transfer encoding stream contains malformed data.
/// </summary>
public sealed class ChunkParseException : IOException {
    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkParseException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the parsing error.</param>
    public ChunkParseException ( string message ) : base ( message ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkParseException"/> class with a specified error message and a reference to the inner exception that caused it.
    /// </summary>
    /// <param name="message">The message that describes the parsing error.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public ChunkParseException ( string message, Exception inner ) : base ( message, inner ) { }
}
