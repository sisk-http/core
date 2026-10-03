// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   TestHost.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;
using Sisk.Core.Routing;

namespace tests.TestUtils;

public static class TestHost {

    public static HttpServerHostContext Start ( Action<Router> setup, Action<HttpServerConfiguration>? configure = null, CrossOriginResourceSharingHeaders? cors = null ) {
        var router = new Router ();
        setup ( router );

        var builder = HttpServer.CreateBuilder ( ListeningPort.GetRandomPort ().Port )
            .UseConfiguration ( config => configure?.Invoke ( config ) )
            .UseRouter ( router );

        if (cors is not null)
            builder.UseCors ( cors );

        var host = builder.Build ();
        host.Start ( verbose: false, preventHault: false );
        return host;
    }

    public static HttpClient CreateClient ( HttpServerHostContext host ) => new HttpClient ( new HttpClientHandler { AllowAutoRedirect = false } ) {
        BaseAddress = new Uri ( host.HttpServer.ListeningPrefixes [ 0 ] ),
        Timeout = TimeSpan.FromSeconds ( 20 )
    };
}
