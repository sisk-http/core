// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
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
/// Provides a MITM proxy server handler.
/// </summary>
public sealed class MitmproxyProvider : HttpServerHandler {
    private ChildProcessStartInfo MitmdumpProcessInfo = null!;
    private readonly Action<ChildProcessStartInfo>? setupAction;

    /// <summary>
    /// Gets the mitmdump process.
    /// </summary>
    public IChildProcess MitmdumpProcess { get; private set; } = null!;

    /// <summary>
    /// Gets or sets the proxy port.
    /// </summary>
    public ushort ProxyPort { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether to run the mitmdump process silently.
    /// </summary>
    public bool Silent { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MitmproxyProvider"/> class.
    /// </summary>
    public MitmproxyProvider () {
        ProxyPort = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MitmproxyProvider"/> class with a specified proxy port and optional process setup action.
    /// </summary>
    /// <param name="proxyPort">The port on which the mitmproxy will listen.</param>
    /// <param name="processSetupAction">Optional. An action to configure the child process start information.</param>
    public MitmproxyProvider ( ushort proxyPort, Action<ChildProcessStartInfo>? processSetupAction = null ) {
        ProxyPort = proxyPort;
        setupAction = processSetupAction;
    }

    /// <inheritdoc/>
    protected override void OnServerStarting ( HttpServer server ) {
        ChildProcessStartInfo pinfo = new ChildProcessStartInfo () {
            FileName = "mitmdump"
        };

        OutputRedirection outputRedirection = Silent ? OutputRedirection.NullDevice : OutputRedirection.ParentOutput;
        pinfo.StdOutputRedirection = outputRedirection;
        pinfo.StdErrorRedirection = outputRedirection;

        if (ProxyPort == 0) {
            ProxyPort = (ushort) (server.ServerConfiguration.ListeningHosts [ 0 ].Ports [ 0 ].Port + 1);
        }

        pinfo.Arguments = [ "--mode", $"reverse:{server.ListeningPrefixes [ 0 ]}", "-p", ProxyPort.ToString () ];

        if (setupAction != null)
            setupAction ( pinfo );

        MitmdumpProcessInfo = pinfo;
    }

    /// <inheritdoc/>
    protected override void OnServerStarted ( HttpServer server ) {
        try {
            MitmdumpProcess = ChildProcess.Start ( MitmdumpProcessInfo );
        }
        catch (Exception e) {
            Console.WriteLine ( "Failed to start the mitmproxy. Perhaps you forgot to install it and make " +
                "mitmdump executable available in your PATH?" );
            Console.WriteLine ( $"Inner exception: {e.Message}" );
            Environment.Exit ( -1 );
        }
    }

    /// <inheritdoc/>
    protected override void OnServerStopping ( HttpServer server ) {
        MitmdumpProcess.Kill ();
    }

    /// <inheritdoc/>
    protected override void OnServerStopped ( HttpServer server ) {
        MitmdumpProcess.Dispose ();
    }
}
