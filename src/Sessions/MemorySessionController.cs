using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Sessions;

/// <summary>
/// Represents a controller for storing sessions in memory.
/// </summary>
/// <definition>
/// public class MemorySessionController : ISessionController
/// </definition>
/// <type>
/// Class
/// </type>
public class MemorySessionController : ISessionController
{
    private MemoryCache cache = new MemoryCache("sessioncontroller");

    /// <summary>
    /// Gets or sets the session lifespan has before it is deleted.
    /// </summary>
    /// <definition>
    /// public TimeSpan SessionExpirity { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public TimeSpan SessionExpirity { get; set; } = TimeSpan.FromDays(7);

    /// <inheritdoc/>
    /// <nodocs/>
    public Boolean DestroySession(Session session)
    {
        cache.Remove(session.Id.ToString());
        return true;
    }

    /// <inheritdoc/>
    /// <nodocs/>
    public void Initialize()
    {
        ;
    }

    /// <inheritdoc/>
    /// <nodocs/>
    public void RunSessionGC()
    {
        ; // cache does this automatically
    }

    /// <inheritdoc/>
    /// <nodocs/>
    public Boolean StoreSession(Session session)
    {
        cache.Set(session.Id.ToString(), session, new CacheItemPolicy()
        {
            AbsoluteExpiration = DateTime.Now.Add(SessionExpirity)
        });
        return true;
    }

    /// <inheritdoc/>
    /// <nodocs/>
    public Boolean TryGetSession(Guid sessionId, out Session? session)
    {
        var result = cache.Get(sessionId.ToString());
        if (result == null)
        {
            session = null;
            return false;
        }
        else
        {
            session = (Session)result;
            return true;
        }
    }
}
