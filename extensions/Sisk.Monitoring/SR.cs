// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SR.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.Monitoring;

internal static class SR {
    public const string ApplicationMonitor_RoutePrefixChanged = "Route prefix cannot be changed once set. Current prefix: '{0}', attempted new prefix: '{1}'. Please, use a new instance of the ApplicationMonitor class for multiple servers.";
    public const string ApplicationMonitor_AuthenticationRequired = "Authenticate using your credentials to access this page.";

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static string Format ( string format, params object? [] items ) {
        return String.Format ( provider: null, format, items );
    }
}
