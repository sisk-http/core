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

public sealed class MitmproxyProvider : HttpServerHandler
{
    public IChildProcess MitmdumpProcess { get; private set; } = null!;
    public ushort ProxyPort { get; private set; }
    public bool Silent { get; set; }

    private ChildProcessStartInfo MitmdumpProcessInfo = null!;
    private readonly Action<ChildProcessStartInfo>? setupAction;

    public MitmproxyProvider()
    {
        this.ProxyPort = 0;
    }

    public MitmproxyProvider(ushort proxyPort, Action<ChildProcessStartInfo>? processSetupAction = null)
    {
        this.ProxyPort = proxyPort;
        this.setupAction = processSetupAction;
    }

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

    protected override void OnServerStarted(HttpServer server)
    {
        this.MitmdumpProcess = ChildProcess.Start(this.MitmdumpProcessInfo);
    }

    protected override void OnServerStopping(HttpServer server)
    {
        this.MitmdumpProcess.Kill();
    }

    protected override void OnServerStopped(HttpServer server)
    {
        this.MitmdumpProcess.Dispose();
    }
}
