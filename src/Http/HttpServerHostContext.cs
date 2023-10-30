// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerBuilder.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using Sisk.Core.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Http;

/// <summary>
/// Represents the class that hosts most of the components needed to run a Sisk application.
/// </summary>
/// <definition>
/// public class HttpServerHostContext
/// </definition>
/// <type>
/// Class
/// </type>
public class HttpServerHostContext
{
    /// <summary>
    /// Gets the host Http server.
    /// </summary>
    /// <definition>
    /// public HttpServer HttpServer { get; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public HttpServer HttpServer { get; private set; }

    /// <summary>
    /// Gets the host server configuration.
    /// </summary>
    /// <definition>
    /// public HttpServerConfiguration ServerConfiguration { get; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public HttpServerConfiguration ServerConfiguration { get => HttpServer.ServerConfiguration; }

    /// <summary>
    /// Gets the host <see cref="CrossOriginResourceSharingPolicy"/>.
    /// </summary>
    /// <definition>
    /// public CrossOriginResourceSharingHeaders CrossOriginResourceSharingPolicy { get; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public CrossOriginResourceSharingHeaders CrossOriginResourceSharingPolicy { get => HttpServer.ServerConfiguration.ListeningHosts[0].CrossOriginResourceSharingPolicy; }

    /// <summary>
    /// Gets the host router.
    /// </summary>
    /// <definition>
    /// public Router Router { get; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public Router Router { get => ServerConfiguration.ListeningHosts[0].Router!; }

    /// <summary>
    /// Gets the configured access log stream. This property is inherited from <see cref="ServerConfiguration"/>.
    /// </summary>
    /// <definition>
    /// public TextWriter? AccessLogs { get; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public LogStream? AccessLogs { get => ServerConfiguration?.AccessLogsStream; }

    /// <summary>
    /// Gets the configured error log stream. This property is inherited from <see cref="ServerConfiguration"/>.
    /// </summary>
    /// <definition>
    /// public TextWriter? ErrorLogs { get; } 
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public LogStream? ErrorLogs { get => ServerConfiguration?.ErrorsLogsStream; }

    internal HttpServerHostContext(HttpServer httpServer)
    {
        HttpServer = httpServer ?? throw new ArgumentNullException(nameof(httpServer));
    }

    /// <summary>
    /// Starts the Http server.
    /// </summary>
    /// <param name="preventHault">Optional. Specifies if the application should pause the main application loop.</param>
    /// <param name="verbose">Optional. Specifies if the application should write the listening prefix welcome message.</param>
    /// <definition>
    /// public void Start(bool verbose = true, bool preventHault = true)
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public void Start(bool verbose = true, bool preventHault = true)
    {
        HttpServer.Start();
        if (verbose)
            Console.WriteLine("The HTTP server is listening at {0}", HttpServer.ListeningPrefixes[0]);
        if (preventHault)
            Thread.Sleep(-1);
    }
}
