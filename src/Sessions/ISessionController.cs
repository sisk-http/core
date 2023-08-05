using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Sessions;

/// <summary>
/// Represents an interface that controls <see cref="Session"/> storages.
/// </summary>
/// <definition>
/// public interface ISessionController
/// </definition>
/// <type>
/// Interface
/// </type>
public interface ISessionController
{
    /// <summary>
    /// Tries to retrieve a session from its ID.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="session">The output session object.</param>
    /// <definition>
    /// public Boolean TryGetSession(Guid sessionId, out Session? session)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public bool TryGetSession(Guid sessionId, out Session? session);

    /// <summary>
    /// Creates or updates an session.
    /// </summary>
    /// <param name="session">The session object.</param>
    /// <definition>
    /// public Boolean StoreSession(Session session)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public bool StoreSession(Session session);

    /// <summary>
    /// Deletes an session.
    /// </summary>
    /// <param name="session">The session object.</param>
    /// <definition>
    /// public Boolean DestroySession(Session session)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public bool DestroySession(Session session);

    /// <summary>
    /// Searches for all expired sessions and deletes them.
    /// </summary>
    /// <definition>
    /// public void RunSessionGC()
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void RunSessionGC();

    /// <summary>
    /// Initialize this session controller.
    /// </summary>
    /// <definition>
    /// public void Initialize()
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void Initialize();
}
