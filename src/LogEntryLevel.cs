// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   LogEntryLevel.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core;

/// <summary>
/// Defines the severity levels used for logging entries.
/// </summary>
public enum LogEntryLevel {
    /// <summary>
    /// Informational messages that highlight the progress of the application.
    /// </summary>
    Information,
    /// <summary>
    /// Detailed information useful during development or debugging.
    /// </summary>
    Trace,
    /// <summary>
    /// Debug-level events used for diagnosing issues during development.
    /// </summary>
    Debug,
    /// <summary>
    /// Warnings for potentially harmful situations that do not prevent the application from running.
    /// </summary>
    Warning,
    /// <summary>
    /// Error events that might still allow the application to continue running.
    /// </summary>
    Error,
    /// <summary>
    /// Critical errors that may force the application to terminate.
    /// </summary>
    Fatal
}