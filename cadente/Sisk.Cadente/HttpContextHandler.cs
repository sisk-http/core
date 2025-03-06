// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpContextHandler.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Cadente;

/// <summary>
/// Represents a method that takes an <see cref="HttpHostContext"/> as a parameter and does not return a value.
/// </summary>
/// <param name="sender">The <see cref="HttpHost"/> which created the <see cref="HttpHostContext"/> object.</param>
/// <param name="session">The HTTP session associated with the action.</param>
public delegate Task HttpContextHandler ( HttpHost sender, HttpHostContext session );

/// <summary>
/// Represents a method that takes an <see cref="HttpHostClientContext"/> as a parameter and does not return a value.
/// </summary>
/// <param name="sender">The <see cref="HttpHost"/> which created the <see cref="HttpHostContext"/> object.</param>
/// <param name="context">The HTTP context associated with the action.</param>
public delegate Task HttpClientContextHandler ( HttpHost sender, HttpHostClientContext context );