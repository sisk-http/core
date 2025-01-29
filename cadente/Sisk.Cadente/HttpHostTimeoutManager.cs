// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHostTimeoutManager.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Cadente;

/// <summary>
/// Manages timeouts for HTTP hosts.
/// </summary>
public sealed class HttpHostTimeoutManager {

    internal int _ClientReadTimeoutSeconds = 30;
    internal int _ClientWriteTimeoutSeconds = 30;

    /// <summary>
    /// Gets or sets the timeout for client read operations.
    /// </summary>
    public TimeSpan ClientReadTimeout {
        get => TimeSpan.FromSeconds ( this._ClientReadTimeoutSeconds );
        set => this._ClientReadTimeoutSeconds = (int) value.TotalSeconds;
    }

    /// <summary>
    /// Gets or sets the timeout for client write operations.
    /// </summary>
    public TimeSpan ClientWriteTimeout {
        get => TimeSpan.FromSeconds ( this._ClientWriteTimeoutSeconds );
        set => this._ClientWriteTimeoutSeconds = (int) value.TotalSeconds;
    }

    /// <summary>
    /// Gets or sets the maximum duration for the context.
    /// </summary>
    public TimeSpan ContextMaxDuration { get; set; } = TimeSpan.Zero;

    internal HttpHostTimeoutManager () { }
}