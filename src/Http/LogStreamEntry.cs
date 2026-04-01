// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   LogStreamEntry.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;

namespace Sisk.Core.Http;

/// <summary>
/// Represents a single log entry with a timestamp, severity level, and message.
/// </summary>
public readonly struct LogStreamEntry : IEquatable<LogStreamEntry>, IComparable<LogStreamEntry>, IFormattable {

    /// <summary>
    /// Creates an informational log entry with the specified message.
    /// </summary>
    /// <param name="message">The informational message.</param>
    /// <returns>A new <see cref="LogStreamEntry"/> instance with <see cref="LogEntryLevel.Information"/>.</returns>
    public static LogStreamEntry CreateInformation ( string message ) => new ( LogEntryLevel.Information, message );

    /// <summary>
    /// Creates a debug log entry with the specified message.
    /// </summary>
    /// <param name="message">The debug message.</param>
    /// <returns>A new <see cref="LogStreamEntry"/> instance with <see cref="LogEntryLevel.Debug"/>.</returns>
    public static LogStreamEntry CreateDebug ( string message ) => new ( LogEntryLevel.Debug, message );

    /// <summary>
    /// Creates an error log entry with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new <see cref="LogStreamEntry"/> instance with <see cref="LogEntryLevel.Error"/>.</returns>
    public static LogStreamEntry CreateError ( string message ) => new ( LogEntryLevel.Error, message );

    /// <summary>
    /// Creates an error log entry from the specified exception.
    /// </summary>
    /// <param name="exception">The exception whose string representation will be used as the message.</param>
    /// <returns>A new <see cref="LogStreamEntry"/> instance with <see cref="LogEntryLevel.Error"/>.</returns>
    public static LogStreamEntry CreateError ( Exception exception ) => new ( LogEntryLevel.Error, exception.ToString () );

    /// <summary>
    /// Creates a warning log entry with the specified message.
    /// </summary>
    /// <param name="message">The warning message.</param>
    /// <returns>A new <see cref="LogStreamEntry"/> instance with <see cref="LogEntryLevel.Warning"/>.</returns>
    public static LogStreamEntry CreateWarning ( string message ) => new ( LogEntryLevel.Warning, message );

    /// <summary>
    /// Creates a warning log entry from the specified exception.
    /// </summary>
    /// <param name="exception">The exception whose string representation will be used as the message.</param>
    /// <returns>A new <see cref="LogStreamEntry"/> instance with <see cref="LogEntryLevel.Warning"/>.</returns>
    public static LogStreamEntry CreateWarning ( Exception exception ) => new ( LogEntryLevel.Warning, exception.ToString () );

    /// <summary>
    /// Gets the point in time when the log entry was created.
    /// </summary>
    public DateTimeOffset Moment { get; }

    /// <summary>
    /// Gets the severity level of the log entry.
    /// </summary>
    public LogEntryLevel Level { get; }

    /// <summary>
    /// Gets the log message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Initializes a new instance of the LogEntry class with the specified message and an information log level.
    /// </summary>
    /// <remarks>The log entry is timestamped with the current date and time, and its level is set to
    /// Information by default.</remarks>
    /// <param name="message">The log message to associate with this entry. Cannot be null.</param>
    public LogStreamEntry ( string message ) {
        ArgumentException.ThrowIfNullOrEmpty ( message );

        Moment = DateTimeOffset.Now;
        Level = LogEntryLevel.Information;
        Message = message;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogStreamEntry"/> struct with the specified level and message,
    /// setting the moment to the current time.
    /// </summary>
    /// <param name="level">The severity level of the log entry.</param>
    /// <param name="message">The log message.</param>
    public LogStreamEntry ( LogEntryLevel level, string message ) {
        ArgumentException.ThrowIfNullOrEmpty ( message );
        if (!Enum.IsDefined ( level ))
            throw new InvalidOperationException ( SR.LogEntry_InvalidLevel );

        Moment = DateTimeOffset.Now;
        Level = level;
        Message = message;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogStreamEntry"/> struct with the specified moment, level, and message.
    /// </summary>
    /// <param name="moment">The point in time when the log entry was created.</param>
    /// <param name="level">The severity level of the log entry.</param>
    /// <param name="message">The log message.</param>
    public LogStreamEntry ( DateTimeOffset moment, LogEntryLevel level, string message ) {
        ArgumentException.ThrowIfNullOrEmpty ( message );
        if (!Enum.IsDefined ( level ))
            throw new InvalidOperationException ( SR.LogEntry_InvalidLevel );

        Moment = moment;
        Level = level;
        Message = message;
    }

    /// <summary>
    /// Returns a string representation of the log entry in the default format.
    /// </summary>
    /// <returns>A string containing the moment, level, and message.</returns>
    public override string ToString () {
        return $"[{Moment:HH:mm:ss.fff}] [{Level}] {Message}";
    }

    /// <summary>
    /// Returns the hash code for this log entry.
    /// </summary>
    /// <returns>A hash code based on the moment, level, and message.</returns>
    public override int GetHashCode () {
        return HashCode.Combine ( Moment, Level, Message );
    }

    /// <summary>
    /// Determines whether this log entry is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with this log entry.</param>
    /// <returns><see langword="true"/> if the specified object is a <see cref="LogStreamEntry"/> and has the same hash code; otherwise, <see langword="false"/>.</returns>
    public override bool Equals ( [NotNullWhen ( true )] object? obj ) {
        if (obj is LogStreamEntry other) {
            return Equals ( other );
        }

        return false;
    }

    /// <summary>
    /// Determines whether this log entry is equal to another log entry.
    /// </summary>
    /// <param name="other">The log entry to compare with this log entry.</param>
    /// <returns><see langword="true"/> if the two log entries have the same hash code; otherwise, <see langword="false"/>.</returns>
    public bool Equals ( LogStreamEntry other ) {
        return GetHashCode () == other.GetHashCode ();
    }

    /// <summary>
    /// Compares this log entry to another log entry based on their moment.
    /// </summary>
    /// <param name="other">The log entry to compare with this log entry.</param>
    /// <returns>A value indicating the relative order of the log entries.</returns>
    public int CompareTo ( LogStreamEntry other ) {
        return Moment.CompareTo ( other.Moment );
    }

    /// <summary>
    /// Returns a string representation of the log entry using the specified format string and format provider.
    /// </summary>
    /// <param name="format">A format string containing placeholders (%moment, %level, %message).</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A formatted string representation of the log entry.</returns>
    public string ToString ( string? format, IFormatProvider? formatProvider ) {
        if (format is null)
            return ToString ();

        return format
            .Replace ( "%moment", Moment.ToString ( "HH:mm:ss.fff", formatProvider ) )
            .Replace ( "%level", Level.ToString () )
            .Replace ( "%message", Message );
    }

    /// <inheritdoc/>
    public static bool operator == ( LogStreamEntry left, LogStreamEntry right ) {
        return left.Equals ( right );
    }

    /// <inheritdoc/>
    public static bool operator != ( LogStreamEntry left, LogStreamEntry right ) {
        return !(left == right);
    }

    /// <inheritdoc/>
    public static bool operator < ( LogStreamEntry left, LogStreamEntry right ) {
        return left.CompareTo ( right ) < 0;
    }

    /// <inheritdoc/>
    public static bool operator <= ( LogStreamEntry left, LogStreamEntry right ) {
        return left.CompareTo ( right ) <= 0;
    }

    /// <inheritdoc/>
    public static bool operator > ( LogStreamEntry left, LogStreamEntry right ) {
        return left.CompareTo ( right ) > 0;
    }

    /// <inheritdoc/>
    public static bool operator >= ( LogStreamEntry left, LogStreamEntry right ) {
        return left.CompareTo ( right ) >= 0;
    }
}