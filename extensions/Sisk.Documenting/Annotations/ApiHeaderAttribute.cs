﻿// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiHeaderAttribute.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting.Annotations;

/// <summary>
/// Specifies an attribute for an API header, allowing metadata such as header name, description, and requirement status to be associated with methods.
/// </summary>
[AttributeUsage ( AttributeTargets.Method, AllowMultiple = true )]
public sealed class ApiHeaderAttribute : Attribute {
    /// <summary>
    /// Gets the name of the header.
    /// </summary>
    public string HeaderName { get; }

    /// <summary>
    /// Gets or sets the description of the header.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the header is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiHeaderAttribute"/> class with the specified header name.
    /// </summary>
    /// <param name="headerName">The name of the header.</param>
    public ApiHeaderAttribute ( string headerName ) {
        HeaderName = headerName;
    }

    internal ApiEndpointHeader GetApiEndpointObject () {
        return new ApiEndpointHeader {
            HeaderName = HeaderName,
            Description = Description,
            IsRequired = IsRequired
        };
    }
}
