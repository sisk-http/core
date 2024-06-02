﻿// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ConfigurationContext.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;

namespace Sisk.Core.Internal.ServiceProvider;

/// <summary>
/// Represents a reading context for a portable configuration file.
/// </summary>
public sealed class ConfigurationContext
{
    /// <summary>
    /// Gets the absolute path to the configuration file. This does not guarantee that the file will exist.
    /// </summary>
    public string ConfigurationFile { get; }

    /// <summary>
    /// Gets the <see cref="HttpServerHostContext"/> which are configuring this context.
    /// </summary>
    public HttpServerHostContext Host { get; }

    /// <summary>
    /// Gets the target <see cref="ListeningHost"/> which are configuring this context.
    /// </summary>
    public ListeningHost TargetListeningHost { get; }

    /// <summary>
    /// Gets the <see cref="InitializationParameterCollection"/> collection for defining configuration parameters of the host application.
    /// </summary>
    public InitializationParameterCollection Parameters { get => Host.Parameters; }

    internal ConfigurationContext(string configurationFile, HttpServerHostContext host, ListeningHost targetListeningHost)
    {
        ConfigurationFile = configurationFile;
        Host = host;
        TargetListeningHost = targetListeningHost;
    }
}
