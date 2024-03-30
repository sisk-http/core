// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RequestTimeoutException.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http;

/// <summary>
/// Represents an exception which is thrown when an Http request total time exceeds
/// the <see cref="HttpServerFlags.RouteActionTimeout"/>.
/// </summary>
/// <definition>
/// public sealed class HttpRequestException : Exception
/// </definition>
/// <type>
/// Class
/// </type>
public sealed class RequestTimeoutException : Exception
{
    /// <inheritdoc/>
    /// <nodoc/>
    public RequestTimeoutException() : base(SR.Httpserver_Commons_RouterTimeout) { }
}
