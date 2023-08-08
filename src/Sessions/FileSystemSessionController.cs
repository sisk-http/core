using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sisk.Core.Sessions;

/// <summary>
/// Represents a controller based on JSON files for storing sessions.
/// </summary>
/// <definition>
/// public class FileSystemSessionController : ISessionController
/// </definition>
/// <type>
/// Class
/// </type>
public class FileSystemSessionController : ISessionController
{
    /// <summary>
    /// Gets or sets the absolute path to the directory where the sessions will be stored. This folder will be created if it does not exist.
    /// </summary>
    /// <definition>
    /// public string DirectoryPath { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string DirectoryPath { get; set; }

    /// <inheritdoc/>
    /// <nodocs/>
    public TimeSpan SessionExpirity { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Creates an new <see cref="FileSystemSessionController"/> instance with given directory path.
    /// </summary>
    /// <param name="directoryPath">Indicates the absolute path to the directory where the sessions will be stored. This folder will be created if it does not exist.</param>
    /// <definition>
    /// public FileSystemSessionController(string directoryPath)
    /// </definition>
    /// <type>
    /// Constructor
    /// </type>
    public FileSystemSessionController(string directoryPath)
    {
        DirectoryPath = directoryPath;
    }

    /// <inheritdoc/>
    /// <nodocs/>
    public Boolean TryGetSession(Guid sessionId, out Session? session)
    {
        string sessionFile = Path.Combine(DirectoryPath, sessionId.ToString() + ".json");
        if (File.Exists(sessionFile))
        {
            try
            {
                byte[] fileContents = File.ReadAllBytes(sessionFile);
                session = JsonSerializer.Deserialize<Session>(fileContents);
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

    /// <inheritdoc/>
    /// <nodocs/>
    public Boolean StoreSession(Session session)
    {
        string sessionFile = Path.Combine(DirectoryPath, session.Id.ToString() + ".json");
        try
        {
            string json = JsonSerializer.Serialize<Session>(session);
            File.WriteAllText(sessionFile, json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    /// <nodocs/>
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

    /// <inheritdoc/>
    /// <nodocs/>
    public void Initialize()
    {
        Directory.CreateDirectory(DirectoryPath);
    }

    /// <inheritdoc/>
    /// <nodocs/>
    public Boolean DestroySession(Session session)
    {
        string sessionFile = Path.Combine(DirectoryPath, session.Id.ToString() + ".json");
        if (File.Exists(sessionFile))
        {
            try
            {
                File.Delete(sessionFile);
                return true;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }
}
