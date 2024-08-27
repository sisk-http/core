// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SecureProxyServerHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Http.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Ssl;

public sealed class SecureProxyServerHandler : HttpServerHandler
{
    public SecureProxy SecureProxy { get; }

    public SecureProxyServerHandler(SecureProxy secureProxy)
    {
        SecureProxy = secureProxy;
    }

    protected override void OnServerStarting(HttpServer server)
    {
        SecureProxy.Start();
    }

    protected override void OnServerStopping(HttpServer server)
    {
        SecureProxy.Dispose();
    }
}
