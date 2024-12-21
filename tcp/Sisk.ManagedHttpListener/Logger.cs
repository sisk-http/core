// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Logger.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sisk.ManagedHttpListener;

static class Logger {
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static void LogInformation ( string? message ) {
#if VERBOSE && DEBUG
        Debug.WriteLine ( $"Sisk.ManagedHttpListener~ {message}" );
#endif
    }
}
