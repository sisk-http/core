// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MitmproxyProvider.cs
// Repository:  https://github.com/sisk-http/core

using Asmichi.ProcessManagement;
using Sisk.Core.Http;
using Sisk.Core.Http.Handlers;

namespace Sisk.Helpers.Mitmproxy;

/// <summary>
/// Represents a handler for integrating mitmproxy into an HTTP server.
/// </summary>
public sealed class MitmproxyProvider : HttpServerHandler
{
    /// <summary>
    /// Gets the <see cref="IChildProcess"/> instance of the mitmdump process.
    /// </summary>
    public IChildProcess MitmdumpProcess { get; private set; } = null!;

    /// <summary>
    /// Gets the port on which the mitmproxy is listening.
    /// </summary>
    public ushort ProxyPort { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the mitmproxy should run in silent mode.
    /// </summary>
    public bool Silent { get; set; }

    private ChildProcessStartInfo MitmdumpProcessInfo = null!;
    private readonly Action<ChildProcessStartInfo>? setupAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="MitmproxyProvider"/> class.
    /// </summary>
    public MitmproxyProvider()
    {
        this.ProxyPort = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MitmproxyProvider"/> class with a specified proxy port and optional process setup action.
    /// </summary>
    /// <param name="proxyPort">The port on which the mitmproxy will listen.</param>
    /// <param name="processSetupAction">Optional. An action to configure the child process start information.</param>
    public MitmproxyProvider(ushort proxyPort, Action<ChildProcessStartInfo>? processSetupAction = null)
    {
        this.ProxyPort = proxyPort;
        this.setupAction = processSetupAction;
    }

    /// <inheritdoc/>
    protected override void OnServerStarting(HttpServer server)
    {
        ChildProcessStartInfo pinfo = new ChildProcessStartInfo()
        {
            FileName = "mitmdump"
        };

        OutputRedirection outputRedirection = this.Silent ? OutputRedirection.NullDevice : OutputRedirection.ParentOutput;
        pinfo.StdOutputRedirection = outputRedirection;
        pinfo.StdErrorRedirection = outputRedirection;

        if (this.ProxyPort == 0)
        {
            this.ProxyPort = (ushort)(server.ServerConfiguration.ListeningHosts[0].Ports[0].Port + 1);
        }

        pinfo.Arguments = ["--mode", $"reverse:{server.ListeningPrefixes[0]}", "-p", this.ProxyPort.ToString()];

        if (this.setupAction != null)
            this.setupAction(pinfo);

        this.MitmdumpProcessInfo = pinfo;
    }

    /// <inheritdoc/>
    protected override void OnServerStarted(HttpServer server)
    {
        try
        {
            this.MitmdumpProcess = ChildProcess.Start(this.MitmdumpProcessInfo);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to start the mitmproxy. Perhaps you forgot to install it and make " +
                "mitmdump executable available in your PATH?");
            Console.WriteLine($"Inner exception: {e.Message}");
            Environment.Exit(-1);
        }
    }

    /// <inheritdoc/>
    protected override void OnServerStopping(HttpServer server)
    {
        this.MitmdumpProcess.Kill();
    }

    /// <inheritdoc/>
    protected override void OnServerStopped(HttpServer server)
    {
        this.MitmdumpProcess.Dispose();
    }
}
