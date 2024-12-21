// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ConfigurationContext.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http.Hosting;

/// <summary>
/// Represents a reading context for a portable configuration file.
/// </summary>
public sealed class ConfigurationContext {
    /// <summary>
    /// Gets the absolute path to the configuration file. The file is guaranteed to exist
    /// when getting this property value.
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
    public InitializationParameterCollection Parameters { get => this.Host.Parameters; }

    internal ConfigurationContext ( string configurationFile, HttpServerHostContext host, ListeningHost targetListeningHost ) {
        this.ConfigurationFile = configurationFile;
        this.Host = host;
        this.TargetListeningHost = targetListeningHost;
    }
}
