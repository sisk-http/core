// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RouteMetadataAttribute.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Routing;

/// <summary>
/// Represents metadata associated with a route. This attribute can be applied to classes or methods
/// to add key-value pairs to the <see cref="Route.Bag"/> dictionary.
/// </summary>
/// <remarks>
/// This attribute allows for the extensibility of route definitions by providing a mechanism
/// to attach arbitrary metadata. The metadata can be used for various purposes, such as
/// authorization checks, logging, or custom route processing logic.
/// </remarks>
[AttributeUsage ( validOn: AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true )]
public sealed class RouteMetadataAttribute : Attribute {

    /// <summary>
    /// Gets the key of the metadata.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the value of the metadata.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RouteMetadataAttribute"/> class with the specified key and value.
    /// </summary>
    /// <param name="key">The key for the metadata.</param>
    /// <param name="value">The value for the metadata. Defaults to <see langword="null"/> if not provided.</param>
    public RouteMetadataAttribute ( string key, object? value = null ) {
        Key = key;
        Value = value;
    }
}