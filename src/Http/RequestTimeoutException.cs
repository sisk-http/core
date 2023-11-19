// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RequestTimeoutException.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Http;

/// <summary>
/// Represents an exception which is thrown when an Http request total time exceeds
/// the <see cref="HttpServerFlags.RouteActionTimeout"/>.
/// </summary>
/// <definition>
/// public class HttpRequestException : Exception
/// </definition>
/// <type>
/// Class
/// </type>
public class RequestTimeoutException : Exception
{
    /// <inheritdoc/>
    /// <nodoc/>
    public RequestTimeoutException() : base(SR.Httpserver_Commons_RouterTimeout) { }
}
