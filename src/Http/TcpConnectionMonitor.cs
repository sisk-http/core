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

static class TcpConnectionMonitor {

    static IPGlobalProperties ipGlobal = IPGlobalProperties.GetIPGlobalProperties ();

    public static CancellationToken GetDisconnectToken ( int localPort ) {

        CancellationTokenSource source = new CancellationTokenSource ( TimeSpan.FromHours ( 24 ) );
        _ = Task.Run ( () => {
            while (GetTcpConnectionState ( localPort ) == TcpState.Established) {
                Thread.Sleep ( 100 );
            }
            source.Cancel ();
        } );

        return source.Token;
    }

    static TcpState GetTcpConnectionState ( int localPort ) {
        try {
            var tcpConnections = ipGlobal.GetActiveTcpConnections ();
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
