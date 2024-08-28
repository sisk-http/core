// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   PortableConfigurationBuilder.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal.ServiceProvider;
using System.ComponentModel;

namespace Sisk.Core.Http.Hosting;

/// <summary>
/// Represents the portable configuration builder for <see cref="HttpServerHostContextBuilder"/>.
/// </summary>
public sealed class PortableConfigurationBuilder
{
    private readonly HttpServerHostContext _context;
    private string _filename = "service-config.json";
    bool _createIfDontExists = false;
    IConfigurationReader? _pipeline = null;

    Action<InitializationParameterCollection>? _initializerHandler;

    internal PortableConfigurationBuilder(HttpServerHostContext context)
    {
        this._context = context;
    }

    internal void Build()
    {
        if (this._createIfDontExists && !File.Exists(this._filename))
        {
            File.Create(this._filename).Close();
        }

        ConfigurationContext provider = new ConfigurationContext(this._filename, this._context, this._context.ServerConfiguration.ListeningHosts[0]);

        var pipelineReader = this._pipeline ?? new JsonConfigParser();
        pipelineReader.ReadConfiguration(provider);

        if (this._initializerHandler != null)
            this._initializerHandler(this._context.Parameters);

        this._context.Parameters.MakeReadonly();
    }

    /// <summary>
    /// Defines an custom <see cref="IConfigurationReader"/> configuration pipeline to the builder.
    /// </summary>
    /// <param name="reader">The <see cref="IConfigurationReader"/> object.</param>
    public PortableConfigurationBuilder WithConfigurationReader(IConfigurationReader reader)
    {
        this._pipeline = reader;
        return this;
    }

    /// <summary>
    /// Defines an custom <see cref="IConfigurationReader"/> configuration pipeline to the builder.
    /// </summary>
    /// <typeparam name="TPipeline">The <see cref="IConfigurationReader"/> type.</typeparam>
    public PortableConfigurationBuilder WithConfigurationPipeline<TPipeline>() where TPipeline : IConfigurationReader, new()
    {
        this._pipeline = new TPipeline();
        return this;
    }

    /// <summary>
    /// Specifies the name of the server configuration file.
    /// </summary>
    /// <param name="filename">The name of the JSON configuration file.</param>
    /// <param name="createIfDontExists">Optional. Determines if the configuration file should be created if it doens't exists.</param>
    public PortableConfigurationBuilder WithConfigFile(string filename, bool createIfDontExists = false)
    {
        this._filename = Path.GetFullPath(filename);
        this._createIfDontExists = createIfDontExists;
        return this;
    }

    /// <summary>
    /// Invokes a method on the initialization parameter collection.
    /// </summary>
    /// <param name="handler">The handler of <see cref="InitializationParameterCollection"/>.</param>
    public PortableConfigurationBuilder WithParameters(Action<InitializationParameterCollection> handler)
    {
        this._initializerHandler = handler;
        return this;
    }

    /// <summary>
    /// Specifies the required configuration sections in the configuration file.
    /// </summary>
    /// <param name="requiredSections">One or more required sections.</param>
    /// <remarks>
    /// This method is obsolete and won't be used anywhere in the code, and will be removed in later Sisk versions.
    /// </remarks>
    [Obsolete("This method is obsolete and won't be used anywhere in the code, and will be removed in later Sisk versions.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public void WithRequiredSections(PortableConfigurationRequireSection requiredSections)
    {
        ;
    }
}
