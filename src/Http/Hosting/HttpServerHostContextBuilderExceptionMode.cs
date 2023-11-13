// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerHostContextBuilderExceptionMode.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http.Hosting;

/// <summary>
/// Represents how the builder event error message should be displayed.
/// </summary>
/// <definition>
/// public enum HttpServerHostContextBuilderExceptionMode
/// </definition>
/// <type>
/// Enum
/// </type>
public enum HttpServerHostContextBuilderExceptionMode
{
    /// <summary>
    /// No message should be displayed.
    /// </summary>
    Silent,

    /// <summary>
    /// Normal messages, including their exception type and message, should be displayed.
    /// </summary>
    Normal,

    /// <summary>
    /// Detailed messages, including detailed exception trace and information, should be displayed.
    /// </summary>
    Detailed,

    /// <summary>
    /// No message should be displayed and exceptions should be thrown instead being caughts.
    /// </summary>
    Throw
}
