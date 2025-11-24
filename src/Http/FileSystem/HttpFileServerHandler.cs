// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpFileServerHandler.cs
// Repository:  https://github.com/sisk-http/core

using System.Globalization;
using System.Text;
using System.Web;
using Sisk.Core.Helpers;
using Sisk.Core.Http.FileSystem.Converters;

namespace Sisk.Core.Http.FileSystem;

/// <summary>
/// Provides HTTP file-serving capabilities for a specified root directory, including optional directory listing and file conversion.
/// </summary>
public class HttpFileServerHandler {

    /// <summary>
    /// Gets or sets the absolute or relative path to the root directory from which files are served.
    /// </summary>
    public string RootDirectoryPath { get; set; }

    /// <summary>
    /// Gets or sets the optional route prefix that must be matched for requests to be handled by this instance.
    /// Matched prefix will be trimmed from the request path when resolving files.
    /// </summary>
    public string RoutePrefix { get; set; } = "/";

    /// <summary>
    /// Gets or sets a value indicating whether directory listing is enabled when an index file is not present.
    /// </summary>
    public bool AllowDirectoryListing { get; set; }

    /// <summary>
    /// Gets the list of converters used to transform files before they are sent to the client.
    /// </summary>
    public List<HttpFileServerFileConverter> FileConverters { get; set; } = new ();

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpFileServerHandler"/> class with the specified root directory.
    /// </summary>
    /// <param name="rootDirectory">The root directory path to serve files from.</param>
    public HttpFileServerHandler ( string rootDirectory ) {
        RootDirectoryPath = rootDirectory;
        AllowDirectoryListing = false;
        FileConverters = [ new HttpFileAudioConverter (), new HttpFileVideoConverter () ];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpFileServerHandler"/> class using the current working directory as the root.
    /// </summary>
    public HttpFileServerHandler () : this ( Directory.GetCurrentDirectory () ) {
    }

    /// <summary>
    /// Determines whether the specified file-system entry is allowed to be listed.
    /// </summary>
    /// <param name="entryPath">The absolute or relative path of the entry to check.</param>
    /// <returns><see langword="true"/> if the entry is inside the configured root directory; otherwise, <see langword="false"/>.</returns>
    protected virtual bool IsEntryAllowedToListing ( string entryPath ) {

        string normalizedEntryPath = PathHelper.NormalizePath ( entryPath );
        string normalizedRootPath = PathHelper.NormalizePath ( RootDirectoryPath );

        return
            normalizedEntryPath.Length >= normalizedRootPath.Length &&
            normalizedEntryPath.StartsWith ( normalizedRootPath, StringComparison.InvariantCultureIgnoreCase );
    }

    /// <summary>
    /// Determines whether the incoming HTTP request is allowed to proceed.
    /// </summary>
    /// <param name="request">The HTTP request to inspect.</param>
    /// <returns><see langword="true"/> to allow the request; otherwise, <see langword="false"/>.</returns>
    protected virtual bool IsRequestAllowed ( HttpRequest request ) {
        return true;
    }

    /// <summary>
    /// Resolves the specified virtual path to a physical file or directory within the root directory.
    /// </summary>
    /// <param name="path">The virtual path to resolve.</param>
    /// <returns>A <see cref="FileSystemInfo"/> instance for the resolved file or directory, or <see langword="null"/> if the path does not exist.</returns>
    protected virtual FileSystemInfo? ResolvePath ( string path ) {

        // trim route prefix
        string [] prefixParts = PathHelper.Split ( RoutePrefix );
        string [] pathParts = PathHelper.Split ( path );

        pathParts = pathParts.Skip ( prefixParts.Length ).ToArray ();
        path = PathHelper.CombinePaths ( pathParts );

        string fullPath = PathHelper.FilesystemCombinePaths ( allowRelativeReturn: false, Path.DirectorySeparatorChar, [ RootDirectoryPath, path ] );

        if (Directory.Exists ( fullPath )) {
            return new DirectoryInfo ( fullPath );
        }
        else if (File.Exists ( fullPath )) {
            return new FileInfo ( fullPath );
        }
        else {
            return null;
        }
    }

    /// <summary>
    /// Generates an HTML directory listing for the specified directory and returns it as an HTTP response.
    /// </summary>
    /// <param name="directory">The directory whose contents will be listed.</param>
    /// <param name="request">The current HTTP request.</param>
    /// <returns>An <see cref="HttpResponse"/> containing the HTML directory listing.</returns>
    protected virtual HttpResponse ServeDirectoryListing ( DirectoryInfo directory, HttpRequest request ) {

        var directories = directory.GetDirectories ( "*.*", SearchOption.TopDirectoryOnly )
            .OrderBy ( f => f.Name )
            .Where ( f => IsEntryAllowedToListing ( f.FullName ) );

        var files = directory.GetFiles ( "*.*", SearchOption.TopDirectoryOnly )
            .OrderBy ( f => f.Name )
            .Where ( f => IsEntryAllowedToListing ( f.FullName ) );

        bool hasUpDirectory = directory.Parent != null && IsEntryAllowedToListing ( directory.Parent.FullName );

        StringBuilder listingBuilder = new ();

        if (hasUpDirectory) {
            listingBuilder.AppendLine ( CultureInfo.InvariantCulture, $"""
                <tr>
                    <td></td>
                    <td>
                        <a href="{PathHelper.NormalizePath ( PathHelper.Pop ( request.Path ), surroundWithDelimiters: true )}">
                            ..
                        </a>
                    </td>
                    <td></td>
                    <td></td>
                </tr>
                """ );
        }

        foreach (var dir in directories) {
            listingBuilder.AppendLine ( CultureInfo.InvariantCulture, $"""
                <tr>
                    <td>&lt;dir&gt;</td>
                    <td>
                        <a href="{PathHelper.CombinePaths ( request.Path, HttpUtility.UrlEncode ( dir.Name ) )}">
                            {HttpUtility.HtmlEncode ( dir.Name )}
                        </a>
                    </td>
                    <td></td>
                    <td>{dir.LastWriteTimeUtc.ToString ( "g", CultureInfo.InvariantCulture )}</td>
                </tr>
                """ );
        }
        foreach (var file in files) {
            listingBuilder.AppendLine ( CultureInfo.InvariantCulture, $"""
                <tr>
                    <td>&lt;file&gt;</td>
                    <td>
                        <a href="{PathHelper.CombinePaths ( request.Path, HttpUtility.UrlEncode ( file.Name ) )}">
                            {HttpUtility.HtmlEncode ( file.Name )}
                        </a>
                    </td>
                    <td>{SizeHelper.HumanReadableSize ( file.Length )}</td>
                    <td>{file.LastWriteTimeUtc.ToString ( "g", CultureInfo.InvariantCulture )}</td>
                </tr>
                """ );
        }

        string html = $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Directory listing</title>
            </head>
            <body>
                <h1>Index of {request.Path}</h1>
                <table>
                    <thead>
                        <tr>
                            <th style="width: 50px">Type</th>
                            <th style="min-width: 200px">Name</th>
                            <th style="width: 100px">Size</th>
                            <th>Date modified (UTC)</th>
                        </tr>
                    </thead>
                    <tbody>
                        {listingBuilder}
                    </tbody>
                </table>
            </body>
            </html>
            """;

        return new HttpResponse ( 200 ) {
            Content = new HtmlContent ( html )
        };
    }

    /// <summary>
    /// Serves the specified file as an HTTP response, applying the first compatible converter if available.
    /// </summary>
    /// <param name="file">The file to serve.</param>
    /// <param name="request">The current HTTP request.</param>
    /// <returns>An <see cref="HttpResponse"/> containing the file or its converted representation.</returns>
    protected virtual HttpResponse ServeFile ( FileInfo file, HttpRequest request ) {
        foreach (HttpFileServerFileConverter converter in FileConverters) {
            if (converter.CanConvert ( file )) {
                return converter.Convert ( file, request );
            }
        }

        return new HttpResponse ( 200 ) {
            Content = new FileContent ( file )
        };
    }

    /// <summary>
    /// Processes the incoming HTTP request and returns the appropriate file or directory response.
    /// </summary>
    /// <param name="request">The HTTP request to handle.</param>
    /// <returns>An <see cref="HttpResponse"/> containing the requested resource, a directory listing, or an error status.</returns>
    public virtual HttpResponse HandleRequest ( HttpRequest request ) {

        if (!IsRequestAllowed ( request )) {
            return new HttpResponse ( System.Net.HttpStatusCode.Forbidden );
        }

        var resolvedEntry = ResolvePath ( HttpUtility.UrlDecode ( request.Path ) );

        if (resolvedEntry is DirectoryInfo dirInfo) {
            if (!AllowDirectoryListing) {
                return new HttpResponse ( System.Net.HttpStatusCode.Forbidden );
            }

            return ServeDirectoryListing ( dirInfo, request );
        }
        else if (resolvedEntry is FileInfo fileInfo) {
            return ServeFile ( fileInfo, request );
        }
        else {
            return new HttpResponse ( System.Net.HttpStatusCode.NotFound );
        }
    }
}
