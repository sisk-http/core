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
    public const string ApplicationMonitor_StoreSaveFailed = "Failed to save the monitoring state to the configured store. Further failures are suppressed until a save succeeds.";
    public const string ApplicationMonitor_StoreRestoreFailed = "Failed to restore the monitoring state from the configured store. The monitor started with an empty state and will not save to the store during this run, preserving the stored state.";
    public const string Meter_InvalidPeriods = "A meter requires at least one period, and every period must be greater than zero.";

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static string Format ( string format, params object? [] items ) {
        return String.Format ( provider: null, format, items );
    }
}
