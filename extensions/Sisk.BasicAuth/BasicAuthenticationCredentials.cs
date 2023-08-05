using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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