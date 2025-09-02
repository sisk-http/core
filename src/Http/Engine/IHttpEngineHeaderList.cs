// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IHttpEngineHeaderList.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Http.Engine;

/// <summary>
/// Represents a collection of HTTP headers.
/// </summary>
public interface IHttpEngineHeaderList {
    /// <summary>
    /// Gets the number of headers in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets an array of strings representing the names of all defined headers in the collection.
    /// </summary>
    string [] DefinedHeaderNames { get; }

    /// <summary>
    /// Removes all headers from the collection.
    /// </summary>
    void Clear ();

    /// <summary>
    /// Determines whether the collection contains a header with the specified name.
    /// </summary>
    /// <param name="name">The name of the header to locate.</param>
    /// <returns><see langword="true"/> if the collection contains the header; otherwise, <see langword="false"/>.</returns>
    bool Contains ( string name );

    /// <summary>
    /// Appends a header with the specified name and value to the collection.
    /// If a header with the same name already exists, the new value is added as an additional value for that header.
    /// </summary>
    /// <param name="name">The name of the header to append.</param>
    /// <param name="value">The value of the header to append.</param>
    void AppendHeader ( string name, string value );

    /// <summary>
    /// Sets a header with the specified name and value in the collection.
    /// If a header with the same name already exists, its existing values are replaced with the new value.
    /// </summary>
    /// <param name="name">The name of the header to set.</param>
    /// <param name="value">The value of the header to set.</param>
    void SetHeader ( string name, string value );

    /// <summary>
    /// Gets all values associated with the header with the specified name.
    /// </summary>
    /// <param name="name">The name of the header to retrieve.</param>
    /// <returns>An array of strings representing the values of the header. Returns an empty array if the header is not found.</returns>
    string [] GetHeader ( string name );
}