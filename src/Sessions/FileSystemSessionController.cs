using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sisk.Core.Sessions;

public class FileSystemSessionController : ISessionController
{
    public string DirectoryPath { get; set; }
    public TimeSpan SessionExpirity { get; set; } = TimeSpan.FromDays(7);

    public FileSystemSessionController(string directoryPath)
    {
        DirectoryPath = directoryPath;
    }

    public Boolean TryGetSession(Guid sessionId, out UserSession? session)
    {
        string sessionFile = Path.Combine(DirectoryPath, sessionId.ToString() + ".json");
        if (File.Exists(sessionFile))
        {
            try
            {
                byte[] fileContents = File.ReadAllBytes(sessionFile);
                session = JsonSerializer.Deserialize<UserSession>(fileContents);
                if (session == null)
                {
                    return false;
                }
                return true;
            }
            catch
            {
                session = null;
                return false;
            }
        }
        else
        {
            session = null;
            return false;
        }
    }

    public Boolean StoreSession(UserSession session)
    {
        string sessionFile = Path.Combine(DirectoryPath, session.Id.ToString() + ".json");
        try
        {
            string json = JsonSerializer.Serialize<UserSession>(session);
            File.WriteAllText(sessionFile, json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void RunSessionGC()
    {
        var shiftingExpirity = DateTime.Now.Subtract(SessionExpirity);
        var files = Directory.GetFiles(DirectoryPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            if (shiftingExpirity > fileInfo.LastAccessTime)
            {
                File.Delete(file);
            }
        }
    }

    public void Initialize()
    {
        Directory.CreateDirectory(DirectoryPath);
    }
}
