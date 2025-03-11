// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
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
public sealed class PortableConfigurationBuilder {
    private readonly HttpServerHostContext _context;
    private string _filename = "service-config.json";
    private bool _createIfDontExists;
    private IConfigurationReader? _pipeline;
    private ConfigurationFileLookupDirectory _fileLookupDirectory = ConfigurationFileLookupDirectory.CurrentDirectory;

    Action<InitializationParameterCollection>? _initializerHandler;

    internal PortableConfigurationBuilder ( HttpServerHostContext context ) {
        _context = context;
    }

    internal void Build () {
        string absoluteFilePath = Path.GetFullPath ( _filename );
        string filename = Path.GetFileName ( _filename );

        Stack<string> searchFileLocations = new Stack<string> ();
        searchFileLocations.Push ( absoluteFilePath );

        if (_fileLookupDirectory.HasFlag ( ConfigurationFileLookupDirectory.CurrentDirectory ))
            searchFileLocations.Push ( PathHelper.FilesystemCombinePaths ( Directory.GetCurrentDirectory (), filename ) );

        if (_fileLookupDirectory.HasFlag ( ConfigurationFileLookupDirectory.AppDirectory ))
            searchFileLocations.Push ( PathHelper.FilesystemCombinePaths ( AppDomain.CurrentDomain.BaseDirectory, filename ) );

        string? currentTestingPath, foundFile = null;
        while (searchFileLocations.TryPop ( out currentTestingPath )) {
            if (File.Exists ( currentTestingPath )) {
                foundFile = currentTestingPath;
                break;
            }
        }

        if (foundFile is null) {
            if (_createIfDontExists) {
                File.Create ( absoluteFilePath ).Close ();
                foundFile = absoluteFilePath;
            }
            else {
                throw new ArgumentException ( SR.Format ( SR.Provider_ConfigParser_ConfigFileNotFound, _filename ) );
            }
        }

        ConfigurationContext provider = new ConfigurationContext ( foundFile, _context, _context.ServerConfiguration.ListeningHosts [ 0 ] );

        var pipelineReader = _pipeline ?? new JsonConfigParser ();
        pipelineReader.ReadConfiguration ( provider );

        _initializerHandler?.Invoke ( _context.Parameters );

        _context.Parameters.MakeReadonly ();
    }

    /// <summary>
    /// Defines an custom <see cref="IConfigurationReader"/> configuration pipeline to the builder.
    /// </summary>
    /// <param name="reader">The <see cref="IConfigurationReader"/> object.</param>
    public PortableConfigurationBuilder WithConfigReader ( IConfigurationReader reader ) {
        _pipeline = reader;
        return this;
    }

    /// <summary>
    /// Defines an custom <see cref="IConfigurationReader"/> configuration pipeline to the builder.
    /// </summary>
    /// <typeparam name="TReader">The <see cref="IConfigurationReader"/> type.</typeparam>
    public PortableConfigurationBuilder WithConfigReader<TReader> () where TReader : IConfigurationReader, new() {
        _pipeline = new TReader ();
        return this;
    }

    /// <summary>
    /// Specifies the name of the server configuration file.
    /// </summary>
    /// <param name="filename">The name of the JSON configuration file.</param>
    /// <param name="createIfDontExists">Optional. Determines if the configuration file should be created if it doens't exists.</param>
    /// <param name="lookupDirectories">Optional. Specifies the directories which the <see cref="IConfigurationReader"/> should search for the configuration file.</param>
    public PortableConfigurationBuilder WithConfigFile ( string filename, bool createIfDontExists = false, ConfigurationFileLookupDirectory lookupDirectories = ConfigurationFileLookupDirectory.CurrentDirectory ) {
        _filename = Path.GetFullPath ( filename );
        _createIfDontExists = createIfDontExists;
        _fileLookupDirectory = lookupDirectories;
        return this;
    }

    /// <summary>
    /// Invokes a method on the initialization parameter collection.
    /// </summary>
    /// <param name="handler">The handler of <see cref="InitializationParameterCollection"/>.</param>
    public PortableConfigurationBuilder WithParameters ( Action<InitializationParameterCollection> handler ) {
        _initializerHandler = handler;
        return this;
    }
}
