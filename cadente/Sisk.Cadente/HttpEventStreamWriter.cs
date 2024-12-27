// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpEventStreamWriter.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.Cadente;

/// <summary>
/// Provides methods to write data and events to an underlying stream in the Server-Sent Events (SSE) format.
/// </summary>
public sealed class HttpEventStreamWriter {
    private Stream _innerStream;
    private Encoding _messageEncoding;

    internal HttpEventStreamWriter ( Stream innerStream, Encoding encoding ) {
        this._innerStream = innerStream;
        this._messageEncoding = encoding;
    }

    /// <summary>
    /// Asynchronously writes a data message to the underlying stream.
    /// </summary>
    /// <param name="data">The data to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public async Task WriteDataAsync ( string data ) {
        byte [] payload = this._messageEncoding.GetBytes ( $"data: {data}\n\n" );
        await this._innerStream.WriteAsync ( payload );
    }

    /// <summary>
    /// Asynchronously writes an event message to the underlying stream.
    /// </summary>
    /// <param name="eventName">The name of the event to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public async Task WriteEventAsync ( string eventName ) {
        byte [] payload = this._messageEncoding.GetBytes ( $"event: {eventName}\n\n" );
        await this._innerStream.WriteAsync ( payload );
    }
}
