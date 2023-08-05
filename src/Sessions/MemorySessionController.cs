using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Sessions;

public class MemorySessionController : ISessionController
{
    private MemoryCache cache = new MemoryCache("sessioncontroller");
    public TimeSpan SessionExpirity { get; set; } = TimeSpan.FromDays(7);

    public void Initialize()
    {
        ;
    }

    public void RunSessionGC()
    {
        ; // cache does this automatically
    }

    public Boolean StoreSession(UserSession session)
    {
        cache.Set(session.Id.ToString(), session, new CacheItemPolicy()
        {
            AbsoluteExpiration = DateTime.Now.Add(SessionExpirity)
        });
        return true;
    }

    public Boolean TryGetSession(Guid sessionId, out UserSession? session)
    {
        var result = cache.Get(sessionId.ToString());
        if (result == null)
        {
            session = null;
            return false;
        }
        else
        {
            session = (UserSession)result;
            return true;
        }
    }
}
