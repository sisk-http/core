// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   PortableConfigurationBuilder.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Helpers;
using Sisk.Core.Internal.ServiceProvider;

namespace Sisk.Core.Http.Hosting;

/// <summary>
/// Represents the portable configuration builder for <see cref="HttpServerHostContextBuilder"/>.
/// </summary>
public sealed class PortableConfigurationBuilder
{
    private readonly HttpServerHostContext _context;
    private string _filename = "service-config.json";
    private bool _createIfDontExists = false;
    private IConfigurationReader? _pipeline = null;
    private ConfigurationFileLookupDirectory _fileLookupDirectory = ConfigurationFileLookupDirectory.CurrentDirectory;

    Action<InitializationParameterCollection>? _initializerHandler;

    internal PortableConfigurationBuilder(HttpServerHostContext context)
    {
        this._context = context;
    }

    internal void Build()
    {
        string absoluteFilePath = Path.GetFullPath(this._filename);
        string filename = Path.GetFileName(this._filename);

        Stack<string> searchFileLocations = new Stack<string>();
        searchFileLocations.Push(absoluteFilePath);

        if (this._fileLookupDirectory.HasFlag(ConfigurationFileLookupDirectory.CurrentDirectory))
            searchFileLocations.Push(PathHelper.FilesystemCombinePaths(Directory.GetCurrentDirectory(), filename));

        if (this._fileLookupDirectory.HasFlag(ConfigurationFileLookupDirectory.AppDirectory))
            searchFileLocations.Push(PathHelper.FilesystemCombinePaths(AppDomain.CurrentDomain.BaseDirectory, filename));

        string? currentTestingPath, foundFile = null;
        while (searchFileLocations.TryPop(out currentTestingPath))
        {
            if (File.Exists(currentTestingPath))
            {
                foundFile = currentTestingPath;
                break;
            }
        }

        if (foundFile is null)
        {
            if (this._createIfDontExists)
            {
                File.Create(absoluteFilePath).Close();
                foundFile = absoluteFilePath;
            }
            else
            {
                throw new ArgumentException(string.Format(SR.Provider_ConfigParser_ConfigFileNotFound, this._filename));
            }
        }

        ConfigurationContext provider = new ConfigurationContext(foundFile, this._context, this._context.ServerConfiguration.ListeningHosts[0]);

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
    public PortableConfigurationBuilder WithConfigReader(IConfigurationReader reader)
    {
        this._pipeline = reader;
        return this;
    }

    /// <summary>
    /// Defines an custom <see cref="IConfigurationReader"/> configuration pipeline to the builder.
    /// </summary>
    /// <typeparam name="TReader">The <see cref="IConfigurationReader"/> type.</typeparam>
    public PortableConfigurationBuilder WithConfigReader<TReader>() where TReader : IConfigurationReader, new()
    {
        this._pipeline = new TReader();
        return this;
    }

    /// <summary>
    /// Specifies the name of the server configuration file.
    /// </summary>
    /// <param name="filename">The name of the JSON configuration file.</param>
    /// <param name="createIfDontExists">Optional. Determines if the configuration file should be created if it doens't exists.</param>
    /// <param name="lookupDirectories">Optional. Specifies the directories which the <see cref="IConfigurationReader"/> should search for the configuration file.</param>
    public PortableConfigurationBuilder WithConfigFile(string filename, bool createIfDontExists = false, ConfigurationFileLookupDirectory lookupDirectories = ConfigurationFileLookupDirectory.CurrentDirectory)
    {
        this._filename = Path.GetFullPath(filename);
        this._createIfDontExists = createIfDontExists;
        this._fileLookupDirectory = lookupDirectories;
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
}
