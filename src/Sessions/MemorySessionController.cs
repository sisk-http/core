// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MemorySessionController.cs
// Repository:  https://github.com/sisk-http/core

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
    private List<Session> _sessions = new List<Session>();

    /// <inheritdoc/>
    /// <nodocs/>
    public TimeSpan SessionExpirity { get; set; } = TimeSpan.FromDays(7);

    /// <inheritdoc/>
    /// <nodocs/>
    public Boolean DestroySession(Session session)
    {
        lock (_sessions)
        {
            return _sessions.Remove(session);
        }
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
        lock (_sessions)
        {
            var expirity = DateTime.Now.Subtract(SessionExpirity);
            foreach (Session s in _sessions.ToArray())
            {
                if (expirity > s.memAccessAt)
                {
                    _sessions.Remove(s);
                }
            }
        }
    }

    /// <inheritdoc/>
    /// <nodocs/>
    public Boolean StoreSession(Session session)
    {
        lock (_sessions)
            _sessions.Add(session);
        return true;
    }

    /// <inheritdoc/>
    /// <nodocs/>
    public Boolean TryGetSession(Guid sessionId, out Session? session)
    {
        Session? find = null;
        lock (_sessions)
            find = _sessions.FirstOrDefault(s => s.Id == sessionId);

        if (find != null)
        {
            find.memAccessAt = DateTime.Now;
            session = find;
            return true;
        }
        else
        {
            session = null;
            return false;
        }
    }
}
