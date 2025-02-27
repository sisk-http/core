// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SpanReader.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.Cadente;

ref struct SpanReader<T> where T : IEquatable<T> {

    int readLength = 0;

    public ReadOnlySpan<T> UnreadSpan { get => this.Span [ this.readLength.. ]; }
    public ReadOnlySpan<T> Span { get; }

    public int Consumed { get => this.readLength; }

    public SpanReader ( in ReadOnlySpan<T> span ) {
        this.Span = span;
    }

    public bool TryReadToAny ( out ReadOnlySpan<T> result, scoped ReadOnlySpan<T> delimiters, bool advancePastDelimiter = false ) {

        ReadOnlySpan<T> remaining = this.UnreadSpan;

        int index = delimiters.Length switch {
            0 => -1,
            2 => remaining.IndexOfAny ( delimiters [ 0 ], delimiters [ 1 ] ),
            3 => remaining.IndexOfAny ( delimiters [ 0 ], delimiters [ 1 ], delimiters [ 3 ] ),
            _ => remaining.IndexOfAny ( delimiters )
        };

        if (index != -1) {
            result = remaining.Slice ( 0, index );
            this.Advance ( index + (advancePastDelimiter ? 1 : 0) );
            return true;
        }

        result = default;
        return false;
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public void Advance ( int count ) {
        this.readLength += count;
    }
}
