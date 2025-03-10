// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHostHandler.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Cadente;

/// <summary>
/// Provides a base class for handling HTTP host events.
/// </summary>
public abstract class HttpHostHandler {

    /// <summary>
    /// Called when a new context is created for the specified HTTP host.
    /// </summary>
    /// <param name="host">The HTTP host that created the context.</param>
    /// <param name="context">The newly created context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public virtual Task OnContextCreatedAsync ( HttpHost host, HttpHostContext context ) {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when a new client connects to the specified HTTP host.
    /// </summary>
    /// <param name="host">The HTTP host that the client connected to.</param>
    /// <param name="client">The client that connected to the host.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public virtual Task OnClientConnectedAsync ( HttpHost host, HttpHostClient client ) {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when a client disconnects from the specified HTTP host.
    /// </summary>
    /// <param name="host">The HTTP host that the client disconnected from.</param>
    /// <param name="client">The client that disconnected from the host.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public virtual Task OnClientDisconnectedAsync ( HttpHost host, HttpHostClient client ) {
        return Task.CompletedTask;
    }
}
