// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   PayloadChecksumIntegrityTests.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using tests.TestUtils;

namespace tests.Tests;

[TestClass]
public sealed class PayloadChecksumIntegrityTests {

    private static bool IsHugePayloadTestsEnabled () {
        string? value = Environment.GetEnvironmentVariable ( "SISK_RUN_HUGE_PAYLOAD_TESTS" );
        return string.Equals ( value, "1", StringComparison.OrdinalIgnoreCase ) ||
               string.Equals ( value, "true", StringComparison.OrdinalIgnoreCase ) ||
               string.Equals ( value, "yes", StringComparison.OrdinalIgnoreCase );
    }

    private static HttpClient CreateHugePayloadClient () {
        return new HttpClient () {
            BaseAddress = new Uri ( Server.Instance.HttpServer.ListeningPrefixes [ 0 ] ),
            Timeout = TimeSpan.FromHours ( 2 )
        };
    }

    [DataTestMethod]
    [DataRow ( 10L * 1024 * 1024, false )] // 10MB
    [DataRow ( 10L * 1024 * 1024, true )] // 10MB with chunked transfer encoding
    [DataRow ( 100L * 1024 * 1024, false )] // 100MB
    [DataRow ( 100L * 1024 * 1024, true )] // 100MB with chunked transfer encoding
    [DataRow ( 500L * 1024 * 1024, false )] // 500MB
    [DataRow ( 500L * 1024 * 1024, true )] // 500MB with chunked transfer encoding
    [DataRow ( 1536L * 1024 * 1024, false )] // 1.5GB
    [DataRow ( 1536L * 1024 * 1024, true )] // 1.5GB with chunked transfer encoding
    public async Task PayloadChecksum_Sha256_RoundTrips ( long sizeBytes, bool chunked ) {
        if (sizeBytes > 10L * 1024 * 1024 && !IsHugePayloadTestsEnabled ()) {
            Assert.Inconclusive ( "Huge payload tests are disabled. Set SISK_RUN_HUGE_PAYLOAD_TESTS=1 to enable 100MB+ cases." );
        }

        using var client = CreateHugePayloadClient ();

        using var request = new HttpRequestMessage ( HttpMethod.Post, "tests/payload/sha256" ) {
            Version = HttpVersion.Version11
        };

        using var content = new DeterministicPayloadContent ( totalLength: sizeBytes, sendContentLength: !chunked );
        request.Content = content;

        if (chunked) {
            request.Headers.TransferEncodingChunked = true;
        }

        using var response = await client.SendAsync ( request, HttpCompletionOption.ResponseContentRead ).ConfigureAwait ( false );
        response.EnsureSuccessStatusCode ();

        string serverChecksum = (await response.Content.ReadAsStringAsync ().ConfigureAwait ( false )).Trim ();
        string clientChecksum = content.ComputedSha256Hex;

        Assert.AreEqual ( clientChecksum, serverChecksum, $"SHA256 mismatch for SizeBytes={sizeBytes} Chunked={chunked}." );
    }
}
