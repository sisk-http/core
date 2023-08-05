using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sisk.Core.Sessions;

/// <summary>
/// Represents a session object that stores data referring to an HTTP state.
/// </summary>
/// <definition>
/// public sealed class UserSession
/// </definition>
/// <type>
/// Class
/// </type>
public sealed class Session
{
    internal bool willDestroy = false;

    /// <summary>
    /// Gets or sets the session ID.
    /// </summary>
    /// <definition>
    /// public Guid Id { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public Guid Id { get; set; }

    /// <summary>
    /// Represents the session storage, which allows you to store objects in the form of a value and key.
    /// </summary>
    /// <definition>
    /// public Hashtable Bag { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public Hashtable Bag { get; set; } = new Hashtable();

    /// <summary>
    /// Creates a new session with a random ID.
    /// </summary>
    /// <definition>
    /// public Session()
    /// </definition>
    /// <type>
    /// Constructor
    /// </type>
    public Session()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Destroys the session, deletes all its stored data and sends a cookie to the client to invalidate the stored cookie.
    /// </summary>
    /// <definition>
    /// public void Destroy()
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void Destroy()
    {
        willDestroy = true;
    }

    /// <summary>
    /// Gets or sets a bag property for this session.
    /// </summary>
    /// <nodocs/>
    /// <param name="index">The bag property key.</param>
    public object? this[string index]
    {
        get
        {
            return Bag[index];
        }
        set
        {
            Bag[index] = value;
        }
    }
}
