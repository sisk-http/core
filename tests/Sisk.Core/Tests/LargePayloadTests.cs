using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sisk.Cadente;
using Sisk.Cadente.CoreEngine;
using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;
using Sisk.Core.Routing;

namespace tests;

[TestClass]
public class LargePayloadTests
{
    private HttpServerHostContext? _server;
    private int _port;

    private static int GetRandomPort()
    {
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }

    private async Task<string> CalculateHashAsync(Stream stream)
    {
        byte[] hashBytes = await SHA1.HashDataAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    [TestInitialize]
    public void Setup()
    {
        _port = GetRandomPort();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _server?.Dispose();
    }

    private async Task RunPayloadTest(long size)
    {
        var router = new Router();
        router.SetRoute(RouteMethod.Post, "/payload", async (HttpRequest req) =>
        {
            using var stream = req.GetRequestStream();
            string hash = await CalculateHashAsync(stream);
            return new HttpResponse(200) { Content = new StringContent(hash) };
        });

        var engine = new CadenteHttpServerEngine(host => {
            // Configure large timeouts for the test
            host.TimeoutManager.BodyDrainTimeout = TimeSpan.FromMinutes(10);
            host.TimeoutManager.ClientReadTimeout = TimeSpan.FromMinutes(10);
            host.TimeoutManager.ClientWriteTimeout = TimeSpan.FromMinutes(10);
        });

        _server = HttpServer.CreateBuilder()
            .UseListeningPort((ushort)_port)
            .UseRouter(router)
            .UseEngine(engine)
            .UseConfiguration(config =>
            {
                config.AccessLogsStream = null;
                config.ErrorsLogsStream = LogStream.ConsoleOutput;
            })
            .Build();

        _server.Start(verbose: false, preventHault: false);

        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromMinutes(10);

        int seed = 12345;
        using var payloadStream = new RandomStream(size, seed);
        using var content = new StreamContent(payloadStream);

        // Calculate expected hash
        using var calculationStream = new RandomStream(size, seed);
        string expectedHash = await CalculateHashAsync(calculationStream);

        var response = await client.PostAsync($"http://localhost:{_port}/payload", content);
        response.EnsureSuccessStatusCode();

        string serverHash = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(expectedHash, serverHash, $"Hash mismatch for payload size {size}");
    }

    [TestMethod]
    public async Task TestPayload_10MB()
    {
        await RunPayloadTest(10 * 1024 * 1024);
    }

    [TestMethod]
    public async Task TestPayload_100MB()
    {
        await RunPayloadTest(100 * 1024 * 1024);
    }

    [TestMethod]
    public async Task TestPayload_500MB()
    {
        await RunPayloadTest(500 * 1024 * 1024);
    }

    [TestMethod]
    [Timeout(600000)] // 10 minutes
    public async Task TestPayload_1_5GB()
    {
        long size = (long)(1.5 * 1024 * 1024 * 1024);
        await RunPayloadTest(size);
    }
}
