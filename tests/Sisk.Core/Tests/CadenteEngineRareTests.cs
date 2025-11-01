// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CadenteEngineRareTests.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sisk.Cadente.CoreEngine;

namespace tests.Tests;

[TestClass]
public sealed class CadenteEngineRareTests {
    private static void AssumeCadenteEngine () {
        if (Server.Instance.HttpServer.ServerConfiguration.Engine is not CadenteHttpServerEngine) {
            Assert.Inconclusive ( "Cadente HTTP engine is not active for this test run." );
        }
    }

    [TestMethod]
    public async Task ExpectContinue_LargePayload_Succeeds () {
        AssumeCadenteEngine ();
        using var client = Server.GetHttpClient ();
        client.DefaultRequestHeaders.ExpectContinue = true;

        try {
            string payload = new string ( 'x', 256 * 1024 );
            using var content = new StringContent ( payload, Encoding.UTF8, "text/plain" );

            using var response = await client.PostAsync ( "tests/httprequest/getBodyContents", content );
            response.EnsureSuccessStatusCode ();
            string echoed = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( payload.Length, echoed.Length, "Echoed payload should match the original length." );
            Assert.AreEqual ( payload, echoed, "Expect-Continue negotiation should not alter payload content." );
        }
        finally {
            client.DefaultRequestHeaders.ExpectContinue = false;
        }
    }

    [TestMethod]
    public async Task FormContent_ThousandFields_RoundTrips () {
        AssumeCadenteEngine ();
        using var client = Server.GetHttpClient ();
        var formData = Enumerable.Range ( 0, 1000 ).ToDictionary ( i => $"field{i:0000}", i => $"value-{i}" );
        using var content = new FormUrlEncodedContent ( formData );

        using var response = await client.PostAsync ( "tests/httprequest/getFormContent", content );
        response.EnsureSuccessStatusCode ();
        Dictionary<string, string?>? echoed = await response.Content.ReadFromJsonAsync<Dictionary<string, string?>> ();
        Assert.IsNotNull ( echoed, "Server should echo form data as a dictionary." );
        var dictionary = echoed!;
        Assert.AreEqual ( formData.Count, dictionary.Count, "Field count should round trip." );
        foreach (var kv in formData) {
            Assert.AreEqual ( kv.Value, dictionary [ kv.Key ], $"Mismatch for field '{kv.Key}'." );
        }
    }

    [TestMethod]
    public async Task JsonContent_LargeStringPayload_RoundTrips () {
        AssumeCadenteEngine ();
        using var client = Server.GetHttpClient ();
        var nameBuilder = new StringBuilder ( 128 * 1024 );
        for (int i = 0; i < 2048; i++) {
            nameBuilder.Append ( "cadente-" ).Append ( i ).Append ( ';' );
        }
        var expected = new TestPoco { Name = nameBuilder.ToString (), Value = 4242 };

        using var response = await client.PostAsJsonAsync ( "tests/httprequest/getJsonContent", expected );
        response.EnsureSuccessStatusCode ();
        TestPoco? echoed = await response.Content.ReadFromJsonAsync<TestPoco> ();
        Assert.IsNotNull ( echoed, "Server should echo JSON body as TestPoco." );
        var poco = echoed!;
        Assert.AreEqual ( expected.Value, poco.Value );
        Assert.AreEqual ( expected.Name, poco.Name );
    }

    [TestMethod]
    public async Task ResponseStreamChunked_ReadIncrementally () {
        AssumeCadenteEngine ();
        using var client = Server.GetHttpClient ();

        using var response = await client.GetAsync ( "tests/responsestream/chunked", HttpCompletionOption.ResponseHeadersRead );
        response.EnsureSuccessStatusCode ();
        Assert.AreEqual ( true, response.Headers.TransferEncodingChunked, "Response should be chunked." );

        using var stream = await response.Content.ReadAsStreamAsync ();
        using var reader = new StreamReader ( stream, Encoding.UTF8 );
        var builder = new StringBuilder ( 128 );
        char [] buffer = new char [ 7 ];
        int readCount = 0;
        int read;
        while ((read = await reader.ReadAsync ( buffer, 0, buffer.Length )) > 0) {
            builder.Append ( buffer, 0, read );
            readCount++;
        }

        const string expectedContent = "This is the first chunk. This is the second chunk. This is the final chunk.";
        Assert.IsTrue ( readCount > 1, "Chunked response should be read in multiple iterations." );
        Assert.AreEqual ( expectedContent, builder.ToString () );
    }

    [TestMethod]
    public async Task ServerSentEvents_CancelAfterFirstEvent () {
        AssumeCadenteEngine ();
        using var client = Server.GetHttpClient ();
        using var cts = new CancellationTokenSource ( TimeSpan.FromSeconds ( 5 ) );

        using var response = await client.GetAsync ( "tests/sse/async", HttpCompletionOption.ResponseHeadersRead, cts.Token );
        response.EnsureSuccessStatusCode ();

        using var stream = await response.Content.ReadAsStreamAsync ( cts.Token );
        using var reader = new StreamReader ( stream, Encoding.UTF8 );
        var lines = new List<string> ();

        while (!cts.IsCancellationRequested) {
            string? line = await reader.ReadLineAsync ( cts.Token );
            if (line is null) {
                break;
            }

            if (line.Length == 0) {
                if (lines.Count > 0) {
                    break;
                }
                continue;
            }

            lines.Add ( line );
            if (lines.Count >= 2) {
                cts.Cancel ();
            }
        }

        Assert.IsTrue ( lines.Any ( l => l.Contains ( "async message 1" ) ), "Should receive the first SSE message before cancellation." );
    }

    [TestMethod]
    public async Task Plaintext_ManyConcurrentRequests_Succeed () {
        AssumeCadenteEngine ();
        using var client = Server.GetHttpClient ();

        var tasks = Enumerable.Range ( 0, 12 ).Select ( async _ => {
            using var response = await client.GetAsync ( "tests/plaintext" );
            response.EnsureSuccessStatusCode ();
            string body = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( "Hello, world!", body );
        } );

        await Task.WhenAll ( tasks );
    }

    [TestMethod]
    public async Task Plaintext_LongQueryString_Succeeds () {
        AssumeCadenteEngine ();
        using var client = Server.GetHttpClient ();
        var uriBuilder = new StringBuilder ( "tests/plaintext?" );
        for (int i = 0; i < 200; i++) {
            if (i > 0) {
                uriBuilder.Append ( '&' );
            }
            uriBuilder.Append ( "param" ).Append ( i ).Append ( '=' );
            uriBuilder.Append ( new string ( 'a', 24 ) );
        }

        using var response = await client.GetAsync ( uriBuilder.ToString () );
        response.EnsureSuccessStatusCode ();
        string body = await response.Content.ReadAsStringAsync ();
        Assert.AreEqual ( "Hello, world!", body );
    }
}
