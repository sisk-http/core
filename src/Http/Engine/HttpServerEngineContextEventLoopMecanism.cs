// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerEngineContextEventLoopMecanism.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http.Engine;

/// <summary>
/// Represents the mechanism used by the HTTP server engine context event loop.
/// </summary>
public enum HttpServerEngineContextEventLoopMecanism {

    /// <summary>
    /// The event loop is unbound and uses both asyncronous BeginGetContext and EndGetContext operations.
    /// </summary>
    UnboundAsyncronousGetContext,

    /// <summary>
    /// The event loop is inline and uses an asynchronous GetContextAsync operation.
    /// </summary>
    InlineAsyncronousGetContext
}