// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
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
/// <definition>
/// public class BasicAuthenticationCredentials
/// </definition> 
/// <type>
/// Class
/// </type>
public class BasicAuthenticationCredentials
{
    /// <summary>
    /// Gets the user id component from this credentials.
    /// </summary>
    /// <definition>
    /// public string UserId { get; }
    /// </definition> 
    /// <type>
    /// Property
    /// </type>
    public string UserId { get; private set; }

    /// <summary>
    /// Gets the plain password component from this credentials.
    /// </summary>
    /// <definition>
    /// public string Password { get; }
    /// </definition> 
    /// <type>
    /// Property
    /// </type>
    public string Password { get; private set; }

    internal BasicAuthenticationCredentials(string username, string password)
    {
        UserId = username;
        Password = password;
    }
}