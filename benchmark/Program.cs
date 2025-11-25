using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Sisk.Cadente;

internal class Program
{
    static async Task Main(string[] args)
    {
        var cadenteHost = new HttpHost(15000)
        {
            Handler = new HostHandler()
        };

        cadenteHost.Start();
        Console.WriteLine("Server started on port 15000");
        Thread.Sleep(-1);
    }
}

sealed class HostHandler : HttpHostHandler
{
    public override async Task OnContextCreatedAsync(HttpHost host, HttpHostContext context)
    {
        context.Response.StatusCode = 200;
        // Don't use chunked for benchmark simplicity and performance, unless necessary.
        // But the example used it, so maybe I should stick to it or just Content-Length.
        // Let's use Content-Length for better raw throughput comparison if possible, or stick to default.
        // The example uses chunked: true.

        using var contentStream = await context.Response.GetResponseStreamAsync(chunked: true);
        using var writer = new StreamWriter(contentStream);
        await writer.WriteAsync("Hello, world!");
    }
}
