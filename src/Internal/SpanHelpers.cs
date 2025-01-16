// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SpanHelpers.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sisk.Core.Internal;
static class SpanHelpers {

    public static bool Contains<T> ( in ReadOnlySpan<T> search, T value, IEqualityComparer<T> comparer ) {
        ref T first = ref MemoryMarshal.GetReference ( search );
        return Contains ( ref first, value, search.Length, comparer );
    }

    // Adapted from System.Private.CoreLib/src/System/SpanHelpers.T.cs
    // checks if searchSpace contains value using comparer
    static bool Contains<T> ( ref T searchSpace, T value, int length, IEqualityComparer<T> comparer ) {

        if (length == 0)
            return false;

        nint index = 0; // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations

        if (default ( T ) != null || value != null) {

            while (length >= 8) {
                length -= 8;

                if (comparer.Equals ( value, Unsafe.Add ( ref searchSpace, index + 0 ) ) ||
                    comparer.Equals ( value, Unsafe.Add ( ref searchSpace, index + 1 ) ) ||
                    comparer.Equals ( value, Unsafe.Add ( ref searchSpace, index + 2 ) ) ||
                    comparer.Equals ( value, Unsafe.Add ( ref searchSpace, index + 3 ) ) ||
                    comparer.Equals ( value, Unsafe.Add ( ref searchSpace, index + 4 ) ) ||
                    comparer.Equals ( value, Unsafe.Add ( ref searchSpace, index + 5 ) ) ||
                    comparer.Equals ( value, Unsafe.Add ( ref searchSpace, index + 6 ) ) ||
                    comparer.Equals ( value, Unsafe.Add ( ref searchSpace, index + 7 ) )) {
                    goto Found;
                }

                index += 8;
            }

            if (length >= 4) {
                length -= 4;

                if (comparer.Equals ( value, Unsafe.Add ( ref searchSpace, index + 0 ) ) ||
                    comparer.Equals ( value, Unsafe.Add ( ref searchSpace, index + 1 ) ) ||
                    comparer.Equals ( value, Unsafe.Add ( ref searchSpace, index + 2 ) ) ||
                    comparer.Equals ( value, Unsafe.Add ( ref searchSpace, index + 3 ) )) {
                    goto Found;
                }

                index += 4;
            }

            while (length > 0) {
                length--;

                if (comparer.Equals ( value, Unsafe.Add ( ref searchSpace, index ) ))
                    goto Found;

                index += 1;
            }
        }
        else {
            nint len = length;
            for (index = 0; index < len; index++) {
                if ((object?) Unsafe.Add ( ref searchSpace, index ) is null) {
                    goto Found;
                }
            }
        }

        return false;

Found:
        return true;
    }
}
