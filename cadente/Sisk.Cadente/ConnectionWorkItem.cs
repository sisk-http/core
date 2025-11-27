// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ConnectionWorkItem.cs
// Repository:  https://github.com/sisk-http/core

using System.Net.Sockets;

namespace Sisk.Cadente;

sealed class ConnectionWorkItem : IThreadPoolWorkItem {
    public HttpHost? Host;
    public Socket? Socket;

    public void Execute () {
        var host = Host!;
        var socket = Socket!;

        Host = null;
        Socket = null;

        _ = host.ProcessConnectionCoreAsync ( socket );
    }
}