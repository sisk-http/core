using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Sessions;

/// <summary>
/// Represents a class which handles specific settings for sessions.
/// </summary>
/// <definition>
/// public sealed class SessionConfiguration
/// </definition>
/// <type>
/// Class
/// </type>
public sealed class SessionConfiguration
{
    /// <summary>
    /// Gets or sets the session handler that will be used on this HTTP server.
    /// </summary>
    /// <definition>
    /// public ISessionController SessionController { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public ISessionController SessionController { get; set; } = new MemorySessionController();

    /// <summary>
    /// Gets or sets whether the HTTP server should use sessions in its requests.
    /// </summary>
    /// <definition>
    /// public bool Enabled { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public bool Enabled { get; set; } = false;

    internal SessionConfiguration() { }
}
