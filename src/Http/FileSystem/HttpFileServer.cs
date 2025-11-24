// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpFileServer.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Helpers;
using Sisk.Core.Routing;

namespace Sisk.Core.Http.FileSystem;

/// <summary>
/// Provides static factory methods for creating route actions that serve files and directories from the local file system.
/// </summary>
public static class HttpFileServer {

    /// <summary>
    /// Creates a <see cref="Route"/> that serves files and directories from the specified base path using the provided <see cref="HttpFileServerHandler"/>.
    /// </summary>
    /// <param name="basePath">The base path under which the route will respond.</param>
    /// <param name="ioHandler">The handler responsible for processing file-system requests.</param>
    /// <param name="requestHandlers">Optional array of additional request handlers to apply to the route.</param>
    /// <returns>A configured <see cref="Route"/> instance.</returns>
    public static Route CreateServingRoute ( string basePath, HttpFileServerHandler ioHandler, IRequestHandler []? requestHandlers = null ) {

        ioHandler.RoutePrefix = PathHelper.NormalizePath ( basePath );
        var action = CreateFileSystemRouteAction ( ioHandler );
        var route = new RegexRoute ( RouteMethod.Get, $@"^\/{basePath.Trim ( '/' )}(\/.*)?$", string.Empty, action, requestHandlers );

        return route;
    }

    /// <summary>
    /// Creates a <see cref="Route"/> that serves files and directories from the specified base path and root directory.
    /// </summary>
    /// <param name="basePath">The base path under which the route will respond.</param>
    /// <param name="rootDirectory">The root directory path to serve files from.</param>
    /// <returns>A configured <see cref="Route"/> instance.</returns>
    public static Route CreateServingRoute ( string basePath, string rootDirectory ) {
        return CreateServingRoute ( basePath, new HttpFileServerHandler ( rootDirectory ), null );
    }

    /// <summary>
    /// Creates a <see cref="RouteAction"/> delegate that uses the specified <see cref="HttpFileServerHandler"/> to handle requests.
    /// </summary>
    /// <param name="ioHandler">The handler responsible for processing file-system requests.</param>
    /// <returns>A <see cref="RouteAction"/> that invokes <see cref="HttpFileServerHandler.HandleRequest"/>.</returns>
    public static RouteAction CreateFileSystemRouteAction ( HttpFileServerHandler ioHandler ) {
        return ioHandler.HandleRequest;
    }

    /// <summary>
    /// Creates a <see cref="RouteAction"/> delegate that serves files from the specified root directory,
    /// optionally allowing directory listings when no index file is present.
    /// </summary>
    /// <param name="rootDirectory">The root directory path to serve files from.</param>
    /// <param name="allowDirectoryListing"><see langword="true"/> to enable directory listing; otherwise, <see langword="false"/>.</param>
    /// <returns>A <see cref="RouteAction"/> configured to handle file-system requests.</returns>
    public static RouteAction CreateFileSystemRouteAction ( string rootDirectory, bool allowDirectoryListing = false ) {
        var handler = new HttpFileServerHandler ( rootDirectory );
        handler.AllowDirectoryListing = allowDirectoryListing;

        return CreateFileSystemRouteAction ( handler );
    }
}
