using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Sisk.Cadente;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

if (args.Length == 0 || args[0] is "--help" or "-h") {
    PrintHelp ();
    return;
}

switch (args[0]) {
    case "server" when args.Length >= 3:
        await RunServerAsync ( args[1], int.Parse ( args[2], CultureInfo.InvariantCulture ) );
        break;
    case "run":
        await RunBenchmarksAsync ( args[1..] );
        break;
    default:
        PrintHelp ();
        Environment.ExitCode = 1;
        break;
}

static void PrintHelp () {
    Console.WriteLine ( """
        Usage:
          dotnet run -c Release --project cadente/benchmark -- run [options]
          dotnet run -c Release --project cadente/benchmark -- server <cadente|aspnet> <port>

        Options:
          --runs <number>          Repetitions per scenario and server. Default: 5.
          --connections <number>   Bombardier concurrent connections. Default: 125.
          --duration <value>       Bombardier duration. Default: 8s.
          --warmup <value>         Warmup duration before measured runs. Default: 2s.
          --bombardier <path>      Bombardier executable. Default: bombardier.
        """ );
}

static async Task RunServerAsync ( string server, int port ) {
    if (server.Equals ( "cadente", StringComparison.OrdinalIgnoreCase )) {
        using var host = new HttpHost ( port ) {
            Handler = new CadenteBenchmarkHandler ()
        };

        host.Start ();
        Console.WriteLine ( $"READY http://127.0.0.1:{port}" );
        await Task.Delay ( Timeout.InfiniteTimeSpan );
    }
    else if (server.Equals ( "aspnet", StringComparison.OrdinalIgnoreCase )) {
        var builder = WebApplication.CreateSlimBuilder ();
        builder.WebHost.ConfigureKestrel ( options => {
            options.ListenLocalhost ( port, listen => listen.Protocols = HttpProtocols.Http1 );
        } );
        builder.Logging.ClearProviders ();

        WebApplication app = builder.Build ();
        app.MapGet ( "/hello", static () => Results.Text ( "Hello, World!", "text/plain" ) );
        app.MapPost ( "/read", static async context => await ReadRequestBodyAsync ( context.Request.Body, context.Response ) );
        app.MapGet ( "/write", static async context => await WriteFixedResponseAsync ( context.Response ) );
        app.MapPost ( "/chunked-read", static async context => await ReadRequestBodyAsync ( context.Request.Body, context.Response ) );
        app.MapGet ( "/chunked-write", static async context => await WriteChunkedResponseAsync ( context.Response ) );

        await app.StartAsync ();
        Console.WriteLine ( $"READY http://127.0.0.1:{port}" );
        await Task.Delay ( Timeout.InfiniteTimeSpan );
    }
    else {
        Console.Error.WriteLine ( $"Unknown server '{server}'." );
        Environment.ExitCode = 1;
    }
}

static async Task RunBenchmarksAsync ( string[] args ) {
    int runs = GetIntOption ( args, "--runs", 5 );
    int connections = GetIntOption ( args, "--connections", 125 );
    string duration = GetStringOption ( args, "--duration", "8s" );
    string warmup = GetStringOption ( args, "--warmup", "2s" );
    string bombardier = GetStringOption ( args, "--bombardier", "bombardier" );
    string payloadFile = Path.Combine ( Path.GetTempPath (), $"cadente-benchmark-{BenchmarkSettings.PayloadSize}.bin" );
    File.WriteAllBytes ( payloadFile, BenchmarkSettings.CreatePayload ( BenchmarkSettings.PayloadSize ) );

    var scenarios = new [] {
        new Scenario ( "hello", "GET", "/hello", BodyMode.None ),
        new Scenario ( "stream-read", "POST", "/read", BodyMode.Fixed ),
        new Scenario ( "stream-write", "GET", "/write", BodyMode.None ),
        new Scenario ( "chunked-read", "POST", "/chunked-read", BodyMode.Chunked ),
        new Scenario ( "chunked-write", "GET", "/chunked-write", BodyMode.None )
    };

    Console.WriteLine ( "scenario|server|run|rps|latency_mean_ms|latency_p95_ms" );

    var allResults = new List<RunResult> ();
    foreach (Scenario scenario in scenarios) {
        foreach (string server in new [] { "aspnet", "cadente" }) {
            int port = GetFreePort ();
            using Process process = StartServerProcess ( server, port );
            try {
                await WaitForReadyAsync ( process, port );

                await RunBombardierAsync ( bombardier, scenario, server, run: 0, port, connections, warmup, payloadFile, printOutput: false );

                for (int run = 1; run <= runs; run++) {
                    RunResult result = await RunBombardierAsync ( bombardier, scenario, server, run, port, connections, duration, payloadFile, printOutput: true );
                    allResults.Add ( result );
                }
            }
            finally {
                if (!process.HasExited) {
                    process.Kill ( entireProcessTree: true );
                    await process.WaitForExitAsync ();
                }
            }
        }

        RunResult cadenteBest = allResults
            .Where ( r => r.Scenario == scenario.Name && r.Server == "cadente" )
            .OrderByDescending ( r => r.RequestsPerSecond )
            .First ();
        RunResult aspnetBest = allResults
            .Where ( r => r.Scenario == scenario.Name && r.Server == "aspnet" )
            .OrderByDescending ( r => r.RequestsPerSecond )
            .First ();
        double ratio = cadenteBest.RequestsPerSecond / aspnetBest.RequestsPerSecond;

        Console.WriteLine (
            $"BEST|{scenario.Name}|cadente={cadenteBest.RequestsPerSecond:F2}|aspnet={aspnetBest.RequestsPerSecond:F2}|ratio={ratio:F3}|within20={(ratio >= 0.8 && ratio <= 1.2).ToString ().ToLowerInvariant ()}" );
    }
}

static Process StartServerProcess ( string server, int port ) {
    string executable = Environment.ProcessPath ?? throw new InvalidOperationException ( "Unable to find current executable." );
    var info = new ProcessStartInfo ( executable, $"server {server} {port}" ) {
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };

    Process process = Process.Start ( info ) ?? throw new InvalidOperationException ( "Unable to start server process." );
    process.ErrorDataReceived += (_, e) => {
        if (!string.IsNullOrWhiteSpace ( e.Data ))
            Console.Error.WriteLine ( $"[{server}] {e.Data}" );
    };
    process.BeginErrorReadLine ();
    return process;
}

static async Task WaitForReadyAsync ( Process process, int port ) {
    using var timeout = new CancellationTokenSource ( TimeSpan.FromSeconds ( 15 ) );
    while (!timeout.IsCancellationRequested) {
        if (process.HasExited)
            throw new InvalidOperationException ( $"Server process exited with code {process.ExitCode}." );

        try {
            using var client = new TcpClient ();
            await client.ConnectAsync ( IPAddress.Loopback, port, timeout.Token );
            return;
        }
        catch {
            await Task.Delay ( 100, timeout.Token );
        }
    }

    throw new TimeoutException ( $"Server on port {port} did not become ready." );
}

static async Task<RunResult> RunBombardierAsync (
    string bombardier,
    Scenario scenario,
    string server,
    int run,
    int port,
    int connections,
    string duration,
    string payloadFile,
    bool printOutput
) {
    string arguments = BuildBombardierArguments ( scenario, port, connections, duration, payloadFile );
    var info = new ProcessStartInfo ( bombardier, arguments ) {
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };

    using Process process = Process.Start ( info ) ?? throw new InvalidOperationException ( "Unable to start bombardier." );
    string output = await process.StandardOutput.ReadToEndAsync ();
    string error = await process.StandardError.ReadToEndAsync ();
    await process.WaitForExitAsync ();

    if (process.ExitCode != 0)
        throw new InvalidOperationException ( $"bombardier failed with code {process.ExitCode}: {error}" );

    using JsonDocument document = JsonDocument.Parse ( output );
    JsonElement result = document.RootElement.GetProperty ( "result" );
    double rps = ReadDouble ( result, "rps", "mean" );
    double mean = ReadDouble ( result, "latency", "mean" ) / 1_000d;
    double p95 = ReadDouble ( result.GetProperty ( "latency" ), "percentiles", "95" ) / 1_000d;

    if (printOutput)
        Console.WriteLine ( $"{scenario.Name}|{server}|{run}|{rps:F2}|{mean:F3}|{p95:F3}" );

    return new RunResult ( scenario.Name, server, run, rps, mean, p95 );
}

static string BuildBombardierArguments ( Scenario scenario, int port, int connections, string duration, string payloadFile ) {
    var arguments = new List<string> {
        "--http1",
        "--latencies",
        "--print", "result",
        "--format", "json",
        "--connections", connections.ToString ( CultureInfo.InvariantCulture ),
        "--duration", duration,
        "--method", scenario.Method
    };

    if (scenario.BodyMode is BodyMode.Fixed or BodyMode.Chunked) {
        arguments.Add ( "--body-file" );
        arguments.Add ( payloadFile );
    }
    if (scenario.BodyMode == BodyMode.Chunked)
        arguments.Add ( "--stream" );

    arguments.Add ( $"http://127.0.0.1:{port}{scenario.Path}" );
    return string.Join ( ' ', arguments.Select ( QuoteIfNeeded ) );
}

static string QuoteIfNeeded ( string value ) =>
    value.Contains ( ' ' ) || value.Contains ( '\t' )
        ? Quote ( value )
        : value;

static string Quote ( string value ) => "\"" + value.Replace ( "\"", "\\\"", StringComparison.Ordinal ) + "\"";

static int GetIntOption ( string[] args, string name, int fallback ) {
    string? value = GetOption ( args, name );
    return value is null ? fallback : int.Parse ( value, CultureInfo.InvariantCulture );
}

static string GetStringOption ( string[] args, string name, string fallback ) =>
    GetOption ( args, name ) ?? fallback;

static string? GetOption ( string[] args, string name ) {
    for (int i = 0; i < args.Length - 1; i++) {
        if (args[i] == name)
            return args[i + 1];
    }
    return null;
}

static int GetFreePort () {
    using var socket = new Socket ( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
    socket.Bind ( new IPEndPoint ( IPAddress.Loopback, 0 ) );
    return ((IPEndPoint) socket.LocalEndPoint!).Port;
}

static double ReadDouble ( JsonElement root, string objectName, string propertyName ) =>
    root.GetProperty ( objectName ).GetProperty ( propertyName ).GetDouble ();

static async Task ReadRequestBodyAsync ( Stream requestBody, HttpResponse response ) {
    long total = 0;
    byte[] buffer = new byte[BenchmarkSettings.ChunkSize];
    while (true) {
        int read = await requestBody.ReadAsync ( buffer );
        if (read == 0)
            break;

        total += read;
    }

    byte[] output = System.Text.Encoding.ASCII.GetBytes ( total.ToString ( CultureInfo.InvariantCulture ) );
    response.ContentType = "text/plain";
    response.ContentLength = output.Length;
    await response.Body.WriteAsync ( output );
}

static async Task WriteFixedResponseAsync ( HttpResponse response ) {
    response.ContentType = "application/octet-stream";
    response.ContentLength = BenchmarkSettings.StreamResponseSize;

    byte[] chunk = SharedChunk.Value;
    for (int remaining = BenchmarkSettings.StreamResponseSize; remaining > 0;) {
        int count = Math.Min ( remaining, chunk.Length );
        await response.Body.WriteAsync ( chunk.AsMemory ( 0, count ) );
        remaining -= count;
    }
}

static async Task WriteChunkedResponseAsync ( HttpResponse response ) {
    response.ContentType = "application/octet-stream";

    byte[] chunk = SharedChunk.Value;
    for (int remaining = BenchmarkSettings.ChunkedResponseSize; remaining > 0;) {
        int count = Math.Min ( remaining, chunk.Length );
        await response.Body.WriteAsync ( chunk.AsMemory ( 0, count ) );
        remaining -= count;
    }
}

sealed class CadenteBenchmarkHandler : HttpHostHandler {
    private static readonly ReadOnlyMemory<byte> HelloBody = "Hello, World!"u8.ToArray ();
    private static readonly HttpHeader TextContentTypeHeader = new ( "Content-Type", "text/plain" );
    private static readonly HttpHeader OctetContentTypeHeader = new ( "Content-Type", "application/octet-stream" );
    private static readonly HttpHeader HelloContentLengthHeader = new ( "Content-Length", HelloBody.Length.ToString ( CultureInfo.InvariantCulture ) );
    private static readonly HttpHeader StreamContentLengthHeader = new ( "Content-Length", BenchmarkSettings.StreamResponseSize.ToString ( CultureInfo.InvariantCulture ) );

    public override async Task OnContextCreatedAsync ( HttpHost host, HttpHostContext context ) {
        switch (context.Request.Path) {
            case "/hello":
                context.Response.Headers.Set ( TextContentTypeHeader );
                context.Response.Headers.Set ( HelloContentLengthHeader );
                await using (Stream stream = await context.Response.GetResponseStreamAsync ()) {
                    await stream.WriteAsync ( HelloBody );
                }
                return;
            case "/read":
            case "/chunked-read":
                await ReadCadenteRequestBodyAsync ( context );
                return;
            case "/write":
                await WriteCadenteFixedResponseAsync ( context );
                return;
            case "/chunked-write":
                await WriteCadenteChunkedResponseAsync ( context );
                return;
            default:
                context.Response.StatusCode = 404;
                context.Response.StatusDescription = "Not Found";
                context.Response.Headers.Set ( new HttpHeader ( "Content-Length", "0" ) );
                await using (await context.Response.GetResponseStreamAsync ()) {
                }
                return;
        }
    }

    private static async Task ReadCadenteRequestBodyAsync ( HttpHostContext context ) {
        long total = 0;
        byte[] buffer = new byte[BenchmarkSettings.ChunkSize];
        Stream requestStream = context.Request.GetRequestStream ();
        while (true) {
            int read = await requestStream.ReadAsync ( buffer );
            if (read == 0)
                break;

            total += read;
        }

        byte[] output = System.Text.Encoding.ASCII.GetBytes ( total.ToString ( CultureInfo.InvariantCulture ) );
        context.Response.Headers.Set ( TextContentTypeHeader );
        context.Response.Headers.Set ( new HttpHeader ( "Content-Length", output.Length.ToString ( CultureInfo.InvariantCulture ) ) );
        await using Stream responseStream = await context.Response.GetResponseStreamAsync ();
        await responseStream.WriteAsync ( output );
    }

    private static async Task WriteCadenteFixedResponseAsync ( HttpHostContext context ) {
        context.Response.Headers.Set ( OctetContentTypeHeader );
        context.Response.Headers.Set ( StreamContentLengthHeader );

        await using Stream stream = await context.Response.GetResponseStreamAsync ();
        byte[] chunk = SharedChunk.Value;
        for (int remaining = BenchmarkSettings.StreamResponseSize; remaining > 0;) {
            int count = Math.Min ( remaining, chunk.Length );
            await stream.WriteAsync ( chunk.AsMemory ( 0, count ) );
            remaining -= count;
        }
    }

    private static async Task WriteCadenteChunkedResponseAsync ( HttpHostContext context ) {
        context.Response.Headers.Set ( OctetContentTypeHeader );

        await using Stream stream = await context.Response.GetResponseStreamAsync ( chunked: true );
        byte[] chunk = SharedChunk.Value;
        for (int remaining = BenchmarkSettings.ChunkedResponseSize; remaining > 0;) {
            int count = Math.Min ( remaining, chunk.Length );
            await stream.WriteAsync ( chunk.AsMemory ( 0, count ) );
            remaining -= count;
        }
    }
}

static class SharedChunk {
    public static readonly byte[] Value = BenchmarkSettings.CreatePayload ( BenchmarkSettings.ChunkSize );
}

static class BenchmarkSettings {
    public const int PayloadSize = 64 * 1024;
    public const int ChunkSize = 4 * 1024;
    public const int StreamResponseSize = 256 * 1024;
    public const int ChunkedResponseSize = 256 * 1024;

    public static byte[] CreatePayload ( int length ) {
        byte[] payload = new byte[length];
        for (int i = 0; i < payload.Length; i++)
            payload[i] = (byte) ('a' + i % 26);
        return payload;
    }
}

enum BodyMode {
    None,
    Fixed,
    Chunked
}

readonly record struct Scenario ( string Name, string Method, string Path, BodyMode BodyMode );

readonly record struct RunResult (
    string Scenario,
    string Server,
    int Run,
    double RequestsPerSecond,
    double MeanLatencyMs,
    double P95LatencyMs
);
