// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   EndableStream.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;

namespace Sisk.Cadente.Streams;

abstract class EndableStream : Stream {
    volatile bool _ended = false;

    public bool IsEnded => _ended;

    protected void FinishReading () {
        _ended = true;
    }

    public async Task<bool> DrainAsync ( CancellationToken cancellation ) {

        byte [] discardBuffer = ArrayPool<byte>.Shared.Rent ( 8192 );
        try {
            while (!cancellation.IsCancellationRequested) {
                if (IsEnded) {
                    return true;
                }

                int read = await ReadAsync ( discardBuffer, 0, discardBuffer.Length, cancellation );
                if (read == 0) {
                    return true;
                }
            }
            return false;
        }
        catch (OperationCanceledException) {
            return false;
        }
        finally {
            ArrayPool<byte>.Shared.Return ( discardBuffer );
        }
    }
}