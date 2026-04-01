// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   DeterministicPayloadContent.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Net;
using System.Security.Cryptography;

namespace tests.TestUtils;

public sealed class DeterministicPayloadContent : HttpContent {
    private readonly long _totalLength;
    private readonly bool _sendContentLength;
    private readonly int _blockSize;
    private ulong _state;
    private string? _computedSha256Hex;

    public DeterministicPayloadContent ( long totalLength, bool sendContentLength, ulong seed = 0xC0FFEE_1234_5678UL, int blockSize = 1024 * 1024 ) {
        ArgumentOutOfRangeException.ThrowIfNegative ( totalLength );
        if (blockSize <= 0) {
            throw new ArgumentOutOfRangeException ( nameof ( blockSize ) );
        }

        _totalLength = totalLength;
        _sendContentLength = sendContentLength;
        _blockSize = blockSize;
        _state = seed;

        Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ( "application/octet-stream" );
    }

    public string ComputedSha256Hex {
        get {
            return _computedSha256Hex ?? throw new InvalidOperationException ( "Checksum not available yet. The content must be sent first." );
        }
    }

    protected override bool TryComputeLength ( out long length ) {
        if (_sendContentLength) {
            length = _totalLength;
            return true;
        }

        length = 0;
        return false;
    }

    protected override async Task SerializeToStreamAsync ( Stream stream, TransportContext? context ) {
        await SerializeToStreamAsync ( stream, context, CancellationToken.None ).ConfigureAwait ( false );
    }

    protected override async Task SerializeToStreamAsync ( Stream stream, TransportContext? context, CancellationToken cancellationToken ) {
        if (_computedSha256Hex is not null) {
            throw new InvalidOperationException ( "This content instance was already serialized." );
        }

        using var hasher = IncrementalHash.CreateHash ( HashAlgorithmName.SHA256 );

        byte [] buffer = ArrayPool<byte>.Shared.Rent ( _blockSize );
        try {
            long remaining = _totalLength;
            while (remaining > 0) {
                cancellationToken.ThrowIfCancellationRequested ();

                int count = (int) Math.Min ( buffer.Length, remaining );
                FillDeterministic ( buffer.AsSpan ( 0, count ) );

                hasher.AppendData ( buffer, 0, count );
                await stream.WriteAsync ( buffer.AsMemory ( 0, count ), cancellationToken ).ConfigureAwait ( false );

                remaining -= count;
            }

            byte [] hash = hasher.GetHashAndReset ();
            _computedSha256Hex = Convert.ToHexString ( hash ).ToLowerInvariant ();
        }
        finally {
            ArrayPool<byte>.Shared.Return ( buffer );
        }
    }

    private void FillDeterministic ( Span<byte> destination ) {
        int i = 0;
        while (i < destination.Length) {
            ulong x = Next ( ref _state );
            for (int b = 0; b < 8 && i < destination.Length; b++, i++) {
                destination [ i ] = (byte) (x >> (8 * b));
            }
        }
    }

    private static ulong Next ( ref ulong state ) {
        state ^= state >> 12;
        state ^= state << 25;
        state ^= state >> 27;
        return state * 2685821657736338717UL;
    }
}
