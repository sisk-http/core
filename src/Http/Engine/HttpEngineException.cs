// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpEngineException.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Http.Engine;

/// <summary>
/// Represents an exception that occurred during the execution of the HTTP engine.
/// </summary>
public class HttpEngineException : Exception {

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpEngineException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public HttpEngineException ( string message ) : base ( message ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpEngineException"/> class with a specified inner exception.
    /// </summary>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public HttpEngineException ( Exception inner ) : base ( SR.Httpengine_Default, inner ) { }
}