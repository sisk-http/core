// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestEventSourceWriter.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http.Streams;

/// <summary>
/// Represents a writer for HTTP request event sources.
/// </summary>
public sealed class HttpRequestEventSourceWriter : IDisposable {

    private TextWriter writer;
    private bool disposedValue;
    internal DateTime lastSuccessfullMessage;
    internal bool hasSentData;

    internal HttpRequestEventSourceWriter ( TextWriter writer ) {
        this.writer = writer;
    }

    /// <summary>
    /// Asynchronously writes a line to the output stream.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
    public ValueTask<bool> WriteLineAsync () {
        return WriteMessageAsync ( string.Empty );
    }

    /// <summary>
    /// Asynchronously sends a message to the output stream.
    /// </summary>
    /// <param name="fieldName">The name of the field to send.</param>
    /// <param name="parameter">The value of the field to send, or <c>null</c> if no value is provided.</param>
    /// <param name="breakLineAfter">Whether to write a line break after sending the message.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
    public async ValueTask<bool> SendMessageAsync ( string fieldName, string? parameter, bool breakLineAfter = false ) {
        if (await WriteMessageAsync ( $"{fieldName}: {parameter}\n" ) == false) {
            return false;
        }
        if (breakLineAfter && await WriteLineAsync () == false) {
            return false;
        }
        return true;
    }

    internal async ValueTask<bool> WriteMessageAsync ( string message ) {
        if (disposedValue)
            return false;
        try {
            await writer.WriteLineAsync ( message );
            lastSuccessfullMessage = DateTime.Now;
            hasSentData = true;

            return true;
        }
        catch {
            Dispose ();
            return false;
        }
    }

    private void Dispose ( bool disposing ) {
        if (!disposedValue) {
            if (disposing) {
                writer.Dispose ();
            }

            disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose () {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose ( disposing: true );
        GC.SuppressFinalize ( this );
    }
}
