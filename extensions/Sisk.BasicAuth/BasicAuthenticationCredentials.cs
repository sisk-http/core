// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   BasicAuthenticationCredentials.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.BasicAuth;

/// <summary>
/// Represents basic authentication credentials for an HTTP request.
/// </summary>
public sealed class BasicAuthenticationCredentials : IEquatable<BasicAuthenticationCredentials> {

    /// <summary>
    /// Gets the user id component from this credentials.
    /// </summary>
    public string UserId { get; }

    /// <summary>
    /// Gets the plain password component from this credentials.
    /// </summary>
    public string Password { get; }

    internal BasicAuthenticationCredentials ( string username, string password ) {
        UserId = username;
        Password = password;
    }

    /// <inheritdoc/>
    public bool Equals ( BasicAuthenticationCredentials? other ) {
        return GetHashCode () == other?.GetHashCode ();
    }

    /// <inheritdoc/>
    public override bool Equals ( object? obj ) {
        return obj is BasicAuthenticationCredentials other && Equals ( other );
    }

    /// <inheritdoc/>
    public override int GetHashCode () {
        return HashCode.Combine ( UserId, Password );
    }

    /// <inheritdoc/>
    public override string ToString () {
        return $"BasicAuthenticationCredentials {{Uid={UserId}}}";
    }
}