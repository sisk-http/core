using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Sessions;

public sealed class SessionConfiguration
{
    public ISessionController SessionController { get; set; } = new MemorySessionController();
    public bool Enabled { get; set; } = false;
}
