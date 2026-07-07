// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   DisconnectAwareStream.cs
// Repository:  https://github.com/sisk-http/core

using System.Net.Sockets;

namespace Sisk.Cadente.Streams;

sealed class DisconnectAwareStream : Stream {
    private readonly Stream baseStream;
    private readonly CancellationTokenSource disconnectCts;

    public DisconnectAwareStream ( Stream baseStream, CancellationTokenSource disconnectCts ) {
        this.baseStream = baseStream;
        this.disconnectCts = disconnectCts;
    }

    public override bool CanRead => baseStream.CanRead;

    public override bool CanSeek => baseStream.CanSeek;

    public override bool CanWrite => baseStream.CanWrite;

    public override long Length => baseStream.Length;

    public override long Position { get => baseStream.Position; set => baseStream.Position = value; }

    public override void Flush () {
        try {
            baseStream.Flush ();
        }
        catch (IOException) {
            MarkDisconnected ();
            throw;
        }
        catch (SocketException) {
            MarkDisconnected ();
            throw;
        }
        catch (ObjectDisposedException) {
            MarkDisconnected ();
            throw;
        }
    }

    public override async Task FlushAsync ( CancellationToken cancellationToken ) {
        try {
            await baseStream.FlushAsync ( cancellationToken ).ConfigureAwait ( false );
        }
        catch (IOException) {
            MarkDisconnected ();
            throw;
        }
        catch (SocketException) {
            MarkDisconnected ();
            throw;
        }
        catch (ObjectDisposedException) {
            MarkDisconnected ();
            throw;
        }
    }

    public override int Read ( byte [] buffer, int offset, int count ) {
        try {
            int read = baseStream.Read ( buffer, offset, count );
            return CompleteRead ( read, count );
        }
        catch (IOException) {
            MarkDisconnected ();
            throw;
        }
        catch (SocketException) {
            MarkDisconnected ();
            throw;
        }
        catch (ObjectDisposedException) {
            MarkDisconnected ();
            throw;
        }
    }

    public override int Read ( Span<byte> buffer ) {
        try {
            int read = baseStream.Read ( buffer );
            return CompleteRead ( read, buffer.Length );
        }
        catch (IOException) {
            MarkDisconnected ();
            throw;
        }
        catch (SocketException) {
            MarkDisconnected ();
            throw;
        }
        catch (ObjectDisposedException) {
            MarkDisconnected ();
            throw;
        }
    }

    public override Task<int> ReadAsync ( byte [] buffer, int offset, int count, CancellationToken cancellationToken ) {
        return ReadAsync ( buffer.AsMemory ( offset, count ), cancellationToken ).AsTask ();
    }

    public override async ValueTask<int> ReadAsync ( Memory<byte> buffer, CancellationToken cancellationToken = default ) {
        try {
            int read = await baseStream.ReadAsync ( buffer, cancellationToken ).ConfigureAwait ( false );
            return CompleteRead ( read, buffer.Length );
        }
        catch (IOException) {
            MarkDisconnected ();
            throw;
        }
        catch (SocketException) {
            MarkDisconnected ();
            throw;
        }
        catch (ObjectDisposedException) {
            MarkDisconnected ();
            throw;
        }
    }

    public override long Seek ( long offset, SeekOrigin origin ) {
        return baseStream.Seek ( offset, origin );
    }

    public override void SetLength ( long value ) {
        baseStream.SetLength ( value );
    }

    public override void Write ( byte [] buffer, int offset, int count ) {
        Write ( buffer.AsSpan ( offset, count ) );
    }

    public override void Write ( ReadOnlySpan<byte> buffer ) {
        try {
            baseStream.Write ( buffer );
        }
        catch (IOException) {
            MarkDisconnected ();
            throw;
        }
        catch (SocketException) {
            MarkDisconnected ();
            throw;
        }
        catch (ObjectDisposedException) {
            MarkDisconnected ();
            throw;
        }
    }

    public override Task WriteAsync ( byte [] buffer, int offset, int count, CancellationToken cancellationToken ) {
        return WriteAsync ( buffer.AsMemory ( offset, count ), cancellationToken ).AsTask ();
    }

    public override async ValueTask WriteAsync ( ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default ) {
        try {
            await baseStream.WriteAsync ( buffer, cancellationToken ).ConfigureAwait ( false );
        }
        catch (IOException) {
            MarkDisconnected ();
            throw;
        }
        catch (SocketException) {
            MarkDisconnected ();
            throw;
        }
        catch (ObjectDisposedException) {
            MarkDisconnected ();
            throw;
        }
    }

    protected override void Dispose ( bool disposing ) {
        if (disposing) {
            MarkDisconnected ();
            baseStream.Dispose ();
        }
    }

    public override async ValueTask DisposeAsync () {
        MarkDisconnected ();
        await baseStream.DisposeAsync ().ConfigureAwait ( false );
    }

    private int CompleteRead ( int read, int requestedLength ) {
        if (requestedLength > 0 && read == 0) {
            MarkDisconnected ();
        }

        return read;
    }

    private void MarkDisconnected () {
        if (disconnectCts.IsCancellationRequested) {
            return;
        }

        try {
            disconnectCts.Cancel ();
        }
        catch (ObjectDisposedException) {
        }
        catch (AggregateException) {
        }
    }
}
