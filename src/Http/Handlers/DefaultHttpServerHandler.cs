// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   DefaultHttpServerHandler.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics;
using Sisk.Core.Http.Hosting;
using Sisk.Core.Routing;

namespace Sisk.Core.Http.Handlers;

internal class DefaultHttpServerHandler : HttpServerHandler {

    internal HttpServerHostContext? hostContext;
    internal Action<Router>? _routerSetup;
    internal List<(Action, string)> _serverBootstrapingFunctions = new ();

    protected override void OnServerStarting ( HttpServer server ) {
        for (int i = 0; i < this._serverBootstrapingFunctions.Count; i++) {
            var func = this._serverBootstrapingFunctions [ i ];

            Stopwatch sw = Stopwatch.StartNew ();
            if (this.hostContext?.verbose == true) {
                Console.Write ( $"Running server boostrapper <{func.Item2}>... " );
            }

            func.Item1 ();

            if (this.hostContext?.verbose == true) {
                Console.WriteLine ( $"OK! ({sw.ElapsedMilliseconds} ms)" );
            }
        }
    }

    protected override void OnSetupRouter ( Router router ) {
        if (this._routerSetup != null)
            this._routerSetup ( router );
    }
}
