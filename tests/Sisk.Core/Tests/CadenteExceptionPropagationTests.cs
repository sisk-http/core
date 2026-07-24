// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CadenteExceptionPropagationTests.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Net.Sockets;
using Sisk.Cadente.CoreEngine;
using Sisk.Core.Http;
using Sisk.Core.Http.Handlers;

namespace tests.Tests;

[TestClass]
public sealed class CadenteExceptionPropagationTests {

    [DataTestMethod]
    [DataRow ( false )]
    [DataRow ( true )]
    public async Task RequestException_ReportsToHandlerAndErrorLog ( bool throwExceptions ) {
        int port;
        using (var listener = new TcpListener ( IPAddress.Loopback, 0 )) {
            listener.Start ();
            port = ((IPEndPoint) listener.LocalEndpoint).Port;
        }

        var exceptionHandler = new CapturingExceptionHandler ();
        using var errorWriter = new StringWriter ();
        using var server = HttpServer.CreateBuilder ()
            .UseEngine ( new CadenteHttpServerEngine () )
            .UseListeningPort ( $"http://127.0.0.1:{port}/" )
            .UseConfiguration ( configuration => {
                configuration.ThrowExceptions = throwExceptions;
                configuration.AccessLogsStream = null;
                configuration.ErrorsLogsStream = new LogStream ( errorWriter );
            } )
            .UseHandler ( exceptionHandler )
            .UseRouter ( router => {
                router.MapGet ( "/throw", _ => throw new InvalidOperationException ( "Cadente probe exception." ) );
                router.MapGet ( "/alive", _ => new HttpResponse ( "alive" ) );
            } )
            .Build ();

        server.Start ( verbose: false, preventHault: false );

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds ( 3 ) };
        try {
            using HttpResponseMessage response = await client.GetAsync ( $"http://127.0.0.1:{port}/throw" );
            if (!throwExceptions) {
                Assert.AreEqual ( HttpStatusCode.InternalServerError, response.StatusCode );
            }
        }
        catch (System.Net.Http.HttpRequestException) when (throwExceptions) {
        }

        Exception propagatedException = await exceptionHandler.ExceptionTask.WaitAsync ( TimeSpan.FromSeconds ( 3 ) );
        Assert.IsInstanceOfType<InvalidOperationException> ( propagatedException );
        Assert.AreEqual ( "Cadente probe exception.", propagatedException.Message );
        StringAssert.Contains ( errorWriter.ToString (), "Cadente probe exception." );

        using HttpResponseMessage aliveResponse = await client.GetAsync ( $"http://127.0.0.1:{port}/alive" );
        Assert.AreEqual ( HttpStatusCode.OK, aliveResponse.StatusCode );
        Assert.AreEqual ( "alive", await aliveResponse.Content.ReadAsStringAsync () );
    }

    private sealed class CapturingExceptionHandler : HttpServerHandler {
        private readonly TaskCompletionSource<Exception> exceptionSource = new ( TaskCreationOptions.RunContinuationsAsynchronously );

        public Task<Exception> ExceptionTask => exceptionSource.Task;

        protected override void OnException ( Exception exception ) {
            exceptionSource.TrySetResult ( exception );
        }
    }
}