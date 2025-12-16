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

namespace Sisk.Cadente;

static class Logger {

    [Conditional ( "DEBUG" )]
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static void LogInformation ( ref DefaultInterpolatedStringHandler message ) {
#if DEBUG
        // Note: Connection ID logging is only available in DEBUG builds and may be uninitialized.
        var connectionId = HttpConnection.Id.Value != 0 ? HttpConnection.Id.Value.ToString () : "N/A";
        Console.WriteLine ( $"Sisk.ManagedHttpListener [{DateTime.Now:R}] [CON {connectionId}] {message.ToString ()}" );
#endif
    }
}
