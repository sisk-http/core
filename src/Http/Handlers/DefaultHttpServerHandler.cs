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

internal sealed class DefaultHttpServerHandler : AsyncHttpServerHandler {

    public override int Priority => -1;

    public record struct ServerBoostrapAction ( Func<Task> Action, string? Label, string GroupKey );

    internal HttpServerHostContext? hostContext;
    internal Action<Router>? _routerSetup;
    internal List<ServerBoostrapAction> _serverBootstrapingFunctions = new ();

    protected override async Task OnServerStartingAsync ( HttpServer server ) {

        List<Exception> raisedExceptions = new ();
        object syncLock = new ();
        void WriteIfVerbose ( string text ) {

            if (hostContext?.verbose != true)
                return;

            lock (syncLock) {
                Console.Out.WriteLine ( text );
            }
        }

        var queues = _serverBootstrapingFunctions
            .GroupBy ( fn => fn.GroupKey, StringComparer.Ordinal )
            .Select ( queue => Task.Run ( async delegate {

                foreach (var fn in queue) {
                    string label = string.IsNullOrEmpty ( fn.Label ) switch {
                        true => $"<action#{fn.GetHashCode ()}>",
                        false => $"<{fn.Label}>"
                    };

                    Stopwatch sw = Stopwatch.StartNew ();
                    WriteIfVerbose ( $"Running server boostrapper {label}... " );

                    try {
                        await fn.Action ().ConfigureAwait ( false );
                        WriteIfVerbose ( $"Running server boostrapper {label}... OK in {sw.ElapsedMilliseconds:N0}ms" );
                    }
                    catch (Exception ex) {
                        WriteIfVerbose ( $"Running server boostrapper {label}... Failed with {ex.GetType ().Name} in {sw.ElapsedMilliseconds:N0}ms" );

                        lock (syncLock) {
                            raisedExceptions.Add ( new InvalidOperationException ( SR.Format ( SR.HttpServerHandler_BootstrapperFailed, label ), ex ) );
                        }
                        return;
                    }
                }
            } ) );

        await Task.WhenAll ( queues ).ConfigureAwait ( false );

        if (raisedExceptions.Count > 0) {
            throw new AggregateException ( SR.HttpServerHandler_BootstrappersFailed, raisedExceptions );
        }
    }

    protected override Task OnSetupRouterAsync ( Router router ) {
        _routerSetup?.Invoke ( router );
        return Task.CompletedTask;
    }
}
