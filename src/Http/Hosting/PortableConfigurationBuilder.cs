// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   PortableConfigurationBuilder.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal.ServiceProvider;

namespace Sisk.Core.Http.Hosting;

/// <summary>
/// Represents the portable configuration builder for <see cref="HttpServerHostContextBuilder"/>.
/// </summary>
/// <definition>
/// public sealed class PortableConfigurationBuilder
/// </definition>
/// <type>
/// Class
/// </type>
public sealed class PortableConfigurationBuilder
{
    private readonly HttpServerHostContext _context;
    private string _filename = "service-config.json";
    bool _createIfDontExists = false;
    PortableConfigurationRequireSection _requiredSections = default;

    Action<InitializationParameterCollection>? _initializerHandler;

    internal PortableConfigurationBuilder(HttpServerHostContext context)
    {
        _context = context;
    }

    internal void Build()
    {
        ProviderContext provider = new ProviderContext(_filename)
        {
            Host = _context,
            TargetListeningHost = _context.ServerConfiguration.ListeningHosts[0],
            CreateConfigurationFileIfDoenstExists = _createIfDontExists
        };

        ConfigParser.ParseConfiguration(provider);

        if (_initializerHandler != null)
            _initializerHandler(_context.Parameters);
    }

    /// <summary>
    /// Specifies the name of the server configuration file.
    /// </summary>
    /// <param name="filename">The name of the JSON configuration file.</param>
    /// <param name="createIfDontExists">Optional. Determines if the configuration file should be created if it doens't exists.</param>
    /// <definition>
    /// public void WithConfigFile(string filename, bool createIfDontExists = false)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void WithConfigFile(string filename, bool createIfDontExists = false)
    {
        _filename = filename;
        _createIfDontExists = createIfDontExists;
    }

    /// <summary>
    /// Invokes a method on the initialization parameter collection.
    /// </summary>
    /// <param name="handler">The handler of <see cref="InitializationParameterCollection"/>.</param>
    /// <definition>
    /// public void WithParameters(Action{{InitializationParameterCollection}} handler)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void WithParameters(Action<InitializationParameterCollection> handler)
    {
        _initializerHandler = handler;
    }

    /// <summary>
    /// Specifies the required configuration sections in the configuration file.
    /// </summary>
    /// <param name="requiredSections">One or more required sections.</param>
    /// <definition>
    /// public void WithRequiredSections(PortableConfigurationRequireSection requiredSections)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void WithRequiredSections(PortableConfigurationRequireSection requiredSections)
    {
        _requiredSections = requiredSections;
    }
}
