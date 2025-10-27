// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   TcpConnectionMonitor.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http;

// This current implementation of TcpConnectionMonitor is fragile and can lead to certain problems:
// 
// When run through a proxy, it may not work correctly, as it will depend on many proxy properties. For example: the proxy may not terminate a connection with the gateway if it is pooling connections
// and have a connection terminated between the client and the proxy. Another example is that the proxy may not terminate the connection between
// the proxy and the gateway if the client is with an HTTP/2 connection and wants to cancel a connection through a RST_STREAM frame.
// 
// Not always a discarded request indicates that your TCP connection has been terminated.
// 
// In addition, this implementation adds a lot of pressure to the GC while in use. When a disconnection token is created,
// the pooling starts in a separate thread checking if that TCP connection is still active. When it is no longer active, the token
// is canceled.
// 
// When delivering a response, the token is also canceled; it is the default behavior to prevent pooling from continuing to live after
// the delivery of the response.
// 
// The problem is that, each pooling iteration an allocation is made for all active TCP connections, and with each iteration this array
// is discarded and replaced by a new array. The discarded array becomes garbage and depending on the generation the GC collects it. Therefore,
// the GC keeps doing this all the time for every connection that is being monitored for disconnection.
// 
// The definitive solution should cover:
// - compatibility with the use of proxies, gateways, cdns...
// - cross-platform between Windows, Linux and MacOS.
// 
// As Sisk works through HTTP/1.1 of HttpListener, one possibility would be to detect when one of the input streams
// or output stops providing information. But guess what? They don't always stop providing information when a connection is
// terminated, because in Windows they are controlled by an external interface (http.sys).

///// <summary>
///// Provides methods for monitoring TCP connections.
///// </summary>
//public static class TcpConnectionMonitor {

//    static int _poolingPrecision = 500; //ms
//    static TimeSpan timeout = TimeSpan.FromHours ( 24 );
//    static IPGlobalProperties? ipGlobal;
//    static long lelapsed;
//    static TcpConnectionInformation [] cachedData = Array.Empty<TcpConnectionInformation> ();

//    /// <summary>
//    /// Gets or sets the maximum time that a disconnection cancellation token 
//    /// should last before being automatically canceled.
//    /// </summary>
//    public static TimeSpan Timeout {
//        get {
//            return timeout;
//        }
//        set {
//            ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( timeout.TotalSeconds );
//            timeout = value;
//        }
//    }

//    /// <summary>
//    /// Gets or sets the pooling precision.
//    /// </summary>
//    /// <value>
//    /// The pooling precision in milliseconds. Must be a positive value.
//    /// </value>
//    /// <exception cref="ArgumentOutOfRangeException">
//    /// Thrown when the value is negative or zero.
//    /// </exception>
//    public static int PoolingPrecision {
//        get {
//            return _poolingPrecision;
//        }
//        set {
//            ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( value );
//            _poolingPrecision = value;
//        }
//    }

//    /// <summary>
//    /// Gets a cancellation token that is triggered when a TCP connection on the specified local port is disconnected.
//    /// </summary>
//    /// <param name="tcpLocalPort">The local TCP port to monitor.</param>
//    /// <returns>A <see cref="CancellationToken"/> that is triggered when the TCP connection is no longer in the <see cref="TcpState.Established"/> state.</returns>
//    /// <exception cref="ArgumentOutOfRangeException">
//    /// Thrown when <paramref name="tcpLocalPort"/> is negative, zero, or greater than <see cref="ushort.MaxValue"/>.
//    /// </exception>
//    public static CancellationTokenSource GetDisconnectTokenSource ( int tcpLocalPort ) {

//        ArgumentOutOfRangeException.ThrowIfGreaterThan ( tcpLocalPort, ushort.MaxValue );
//        ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( tcpLocalPort );

//        if (ipGlobal is null) {
//            ipGlobal = IPGlobalProperties.GetIPGlobalProperties ();
//        }

//        CancellationTokenSource source = new CancellationTokenSource ( timeout );

//        _ = Task.Run ( () => {
//            while (true) {
//                Thread.Sleep ( PoolingPrecision );

//                if (source.IsCancellationRequested)
//                    break;
//                if (GetTcpConnectionState ( tcpLocalPort ) != TcpState.Established) {
//                    source.Cancel ();
//                    break;
//                }
//            }
//        } );

//        return source;
//    }

//    static TcpConnectionInformation [] GetConnections () {

//        if (ipGlobal is null)
//            return Array.Empty<TcpConnectionInformation> ();

//        long tick = Environment.TickCount64;
//        if (tick - lelapsed >= (PoolingPrecision * 2 * TimeSpan.TicksPerMillisecond)) {
//            cachedData = ipGlobal.GetActiveTcpConnections ();
//            lelapsed = tick;
//        }
//        return cachedData;
//    }

//    static TcpState GetTcpConnectionState ( int localPort ) {
//        try {
//            var tcpConnections = GetConnections ();
//            var connection = tcpConnections.FirstOrDefault ( c => c.LocalEndPoint.Port == localPort );

//            if (connection is not null) {
//                return connection.State;
//            }
//            return TcpState.Unknown;
//        }
//        catch {
//            Debugger.Break ();
//            return TcpState.Unknown;
//        }
//    }
//}
