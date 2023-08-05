using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Sessions;

public interface ISessionController
{
    public bool TryGetSession(Guid sessionId, out UserSession? session);
    public bool StoreSession(UserSession session);
    public void RunSessionGC();
    public void Initialize();
}
