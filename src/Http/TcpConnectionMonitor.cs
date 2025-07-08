// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   TcpConnectionMonitor.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Http;

/// <summary>
/// Provides methods for monitoring TCP connections.
/// </summary>
public static class TcpConnectionMonitor {

    static int _poolingPrecision;
    static TimeSpan timeout = TimeSpan.FromHours ( 24 );
    static IPGlobalProperties? ipGlobal;
    static long lelapsed;
    static TcpConnectionInformation [] cachedData = Array.Empty<TcpConnectionInformation> ();

    /// <summary>
    /// Gets or sets the maximum time that a disconnection cancellation token 
    /// should last before being automatically canceled.
    /// </summary>
    public static TimeSpan Timeout {
        get {
            return timeout;
        }
        set {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( timeout.TotalSeconds );
            timeout = value;
        }
    }

    /// <summary>
    /// Gets or sets the pooling precision.
    /// </summary>
    /// <value>
    /// The pooling precision in milliseconds. Must be a positive value.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is negative or zero.
    /// </exception>
    public static int PoolingPrecision {
        get {
            return _poolingPrecision;
        }
        set {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( value );
            _poolingPrecision = value;
        }
    }

    /// <summary>
    /// Gets a cancellation token that is triggered when a TCP connection on the specified local port is disconnected.
    /// </summary>
    /// <param name="tcpLocalPort">The local TCP port to monitor.</param>
    /// <returns>A <see cref="CancellationToken"/> that is triggered when the TCP connection is no longer in the <see cref="TcpState.Established"/> state.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="tcpLocalPort"/> is negative, zero, or greater than <see cref="ushort.MaxValue"/>.
    /// </exception>
    public static CancellationToken GetDisconnectToken ( int tcpLocalPort ) {

        ArgumentOutOfRangeException.ThrowIfGreaterThan ( tcpLocalPort, ushort.MaxValue );
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( tcpLocalPort );

        if (ipGlobal is null) {
            ipGlobal = IPGlobalProperties.GetIPGlobalProperties ();
        }

        CancellationTokenSource source = new CancellationTokenSource ( timeout );

        _ = Task.Run ( () => {
            while (source.IsCancellationRequested == false && GetTcpConnectionState ( tcpLocalPort ) == TcpState.Established) {
                Thread.Sleep ( PoolingPrecision );
            }
            source.Cancel ();
        } );

        return source.Token;
    }

    static TcpConnectionInformation [] GetConnections () {

        if (ipGlobal is null)
            return Array.Empty<TcpConnectionInformation> ();

        long tick = Environment.TickCount64;
        if (tick - lelapsed >= (PoolingPrecision * TimeSpan.TicksPerMillisecond)) {
            cachedData = ipGlobal.GetActiveTcpConnections ();
            lelapsed = tick;
        }
        return cachedData;
    }

    static TcpState GetTcpConnectionState ( int localPort ) {
        try {
            var tcpConnections = GetConnections ();
            var connection = tcpConnections.FirstOrDefault ( c => c.LocalEndPoint.Port == localPort );

            if (connection is not null) {
                return connection.State;
            }
            return TcpState.Unknown;
        }
        catch {
            Debugger.Break ();
            return TcpState.Unknown;
        }
    }
}
