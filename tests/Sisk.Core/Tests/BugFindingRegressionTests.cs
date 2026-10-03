// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   BugFindingRegressionTests.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Text;
using Sisk.Core.Entity;
using Sisk.Core.Http;
using Sisk.Core.Http.FileSystem;
using Sisk.Core.Routing;
using tests.TestUtils;

namespace tests.Tests;

[TestClass]
public sealed class BugFindingRegressionTests {

    [TestMethod]
    public async Task MapOptionsRegisteredAfterAnotherMethodIsExecuted () {
        using var host = TestHost.Start ( r => {
            r.MapGet ( "/both", ( HttpRequest req ) => new HttpResponse ( "get" ) );
            r.MapOptions ( "/both", ( HttpRequest req ) => new HttpResponse () {
                Headers = new () { [ "X-Route-Action" ] = "options" }
            } );
        } );
        using var client = TestHost.CreateClient ( host );

        var response = await client.SendAsync ( new HttpRequestMessage ( HttpMethod.Options, "both" ) );

        Assert.AreEqual ( HttpStatusCode.OK, response.StatusCode );
        Assert.AreEqual ( "options", response.Headers.GetValues ( "X-Route-Action" ).Single () );
    }

    [TestMethod]
    public async Task SyntheticOptionsRespectsRouteUseCors () {
        using var host = TestHost.Start ( r => {
            r.Map ( new Route ( RouteMethod.Get, "/no-cors", ( HttpRequest req ) => new HttpResponse () ) { UseCors = false } );
            r.MapGet ( "/cors", ( HttpRequest req ) => new HttpResponse () );
        }, cors: new CrossOriginResourceSharingHeaders ( allowOrigin: "*", allowMethods: [ "GET" ] ) );
        using var client = TestHost.CreateClient ( host );

        var noCors = await client.SendAsync ( new HttpRequestMessage ( HttpMethod.Options, "no-cors" ) );
        Assert.AreEqual ( HttpStatusCode.OK, noCors.StatusCode );
        Assert.IsFalse ( noCors.Headers.Contains ( "Access-Control-Allow-Origin" ) );

        var withCors = await client.SendAsync ( new HttpRequestMessage ( HttpMethod.Options, "cors" ) );
        Assert.AreEqual ( HttpStatusCode.OK, withCors.StatusCode );
        Assert.IsTrue ( withCors.Headers.Contains ( "Access-Control-Allow-Origin" ) );
    }

    [TestMethod]
    [DataRow ( true )]
    [DataRow ( false )]
    public async Task OriginDependentCorsSendsVaryOrigin ( bool autoOrigin ) {
        var cors = autoOrigin
            ? new CrossOriginResourceSharingHeaders ( allowOrigin: CrossOriginResourceSharingHeaders.AutoAllowOrigin )
            : new CrossOriginResourceSharingHeaders ( allowOrigins: [ "https://good.example" ] );

        using var host = TestHost.Start ( r => r.MapGet ( "/api", ( HttpRequest req ) => new HttpResponse ( "ok" ) ), cors: cors );
        using var client = TestHost.CreateClient ( host );

        var request = new HttpRequestMessage ( HttpMethod.Get, "api" );
        request.Headers.Add ( "Origin", "https://good.example" );
        var response = await client.SendAsync ( request );

        Assert.AreEqual ( "https://good.example", response.Headers.GetValues ( "Access-Control-Allow-Origin" ).Single () );
        CollectionAssert.Contains ( response.Headers.Vary.ToArray (), "Origin" );
    }

    [TestMethod]
    [DataRow ( true )]
    [DataRow ( false )]
    public async Task RequestStreamRespectsMaximumContentLength ( bool sendContentLength ) {
        using var host = TestHost.Start ( r => {
            r.MapPost ( "/stream", ( HttpRequest req ) => {
                try {
                    using var ms = new MemoryStream ();
                    req.GetRequestStream ().CopyTo ( ms );
                    return new HttpResponse ( $"read={ms.Length}" );
                }
                catch (InsufficientMemoryException) {
                    return new HttpResponse ( HttpStatusCode.RequestEntityTooLarge );
                }
            } );
        }, config => config.MaximumContentLength = 1024 );
        using var client = TestHost.CreateClient ( host );

        var small = await client.PostAsync ( "stream", new DeterministicPayloadContent ( 512, sendContentLength ) );
        Assert.AreEqual ( "read=512", await small.Content.ReadAsStringAsync () );

        var large = await client.PostAsync ( "stream", new DeterministicPayloadContent ( 4096, sendContentLength ) );
        Assert.AreEqual ( HttpStatusCode.RequestEntityTooLarge, large.StatusCode );
    }

    [TestMethod]
    public async Task RequestStreamIsUnlimitedWithoutMaximumContentLength () {
        using var host = TestHost.Start ( r => {
            r.MapPost ( "/stream", ( HttpRequest req ) => {
                using var ms = new MemoryStream ();
                req.GetRequestStream ().CopyTo ( ms );
                return new HttpResponse ( $"read={ms.Length}" );
            } );
        }, config => config.MaximumContentLength = 0 );
        using var client = TestHost.CreateClient ( host );

        var response = await client.PostAsync ( "stream", new DeterministicPayloadContent ( 4096, sendContentLength: false ) );

        Assert.AreEqual ( "read=4096", await response.Content.ReadAsStringAsync () );
    }

    [TestMethod]
    public async Task RawBodyReadsDeclaredContentLength () {
        using var host = TestHost.Start ( r => {
            r.MapPost ( "/body", ( HttpRequest req ) => new HttpResponse ( $"read={req.RawBody.Length}" ) );
        } );
        using var client = TestHost.CreateClient ( host );

        var response = await client.PostAsync ( "body", new DeterministicPayloadContent ( 200_000, sendContentLength: true ) );

        Assert.AreEqual ( "read=200000", await response.Content.ReadAsStringAsync () );
    }

    [TestMethod]
    public async Task FileServerKeepsPlusSignInPath () {
        string root = Directory.CreateTempSubdirectory ( "sisk-file-decode-" ).FullName;
        File.WriteAllText ( Path.Combine ( root, "a+b.txt" ), "plus" );
        File.WriteAllText ( Path.Combine ( root, "a b.txt" ), "space" );

        using var host = TestHost.Start ( r => r.Map ( HttpFileServer.CreateServingRoute ( "/static", root ) ) );
        using var client = TestHost.CreateClient ( host );

        Assert.AreEqual ( "plus", await client.GetStringAsync ( "static/a+b.txt" ) );
        Assert.AreEqual ( "space", await client.GetStringAsync ( "static/a%20b.txt" ) );
    }
}
