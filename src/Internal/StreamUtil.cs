// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   StreamUtil.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Internal;

internal sealed class StreamUtil {

    public static void CopyToLimited ( Stream source, Stream destination, int bufferSize, long maxLength ) {
        byte [] buffer = ArrayPool<byte>.Shared.Rent ( bufferSize );
        long copied = 0;
        try {
            int bytesRead;
            while ((bytesRead = source.Read ( buffer, 0, buffer.Length )) != 0) {
                destination.Write ( buffer, 0, bytesRead );
                copied += bytesRead;

                if (copied > maxLength) {
                    throw new InsufficientMemoryException ( SR.StreamUtil_CopyOverflow );
                }
            }
        }
        finally {
            ArrayPool<byte>.Shared.Return ( buffer );
        }
    }

    public static async Task CopyToLimitedAsync ( Stream source, Stream destination, int bufferSize, long maxLength, CancellationToken cancellation ) {
        using var bufferOwnership = MemoryPool<byte>.Shared.Rent ( bufferSize );
        long copied = 0;
        int bytesRead;
        while ((bytesRead = await source.ReadAsync ( bufferOwnership.Memory, cancellation )) != 0) {
            await destination.WriteAsync ( bufferOwnership.Memory [ 0..bytesRead ], cancellation );
            copied += bytesRead;

            if (copied > maxLength) {
                throw new InsufficientMemoryException ( SR.StreamUtil_CopyOverflow );
            }
        }
    }
}
