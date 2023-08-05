using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sisk.Core.Sessions;

public sealed class UserSession
{
    public Guid Id { get; set; }
    public Hashtable Bag { get; set; } = new Hashtable();

    public UserSession()
    {
        Id = Guid.NewGuid();
    }

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
