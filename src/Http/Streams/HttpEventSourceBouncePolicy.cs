using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Http.Streams;

/// <summary>
/// Provides an automatic ping sender for HTTP Event Source connections.
/// </summary>
/// <definition>
/// public class HttpEventSourceBouncePolicy
/// </definition>
/// <type>
/// Class
/// </type>
public sealed class HttpEventSourceBouncePolicy
{
    private HttpRequestEventSource _parent;
    private Timer? _timer;

    /// <summary>
    /// Gets or sets the payload message that is sent to the server as a ping message.
    /// </summary>
    /// <definition>
    /// public string DataMessage { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string DataMessage { get; set; } = "%ping%";

    /// <summary>
    /// Gets or sets the sending interval for each ping message.
    /// </summary>
    /// <definition>
    /// public TimeSpan Interval { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(1);

    internal HttpEventSourceBouncePolicy(HttpRequestEventSource parent)
    {
        this._parent = parent;
    }

    /// <summary>
    /// Starts sending periodic pings to the client.
    /// </summary>
    /// <definition>
    /// public void Start()
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void Start()
    {
        _timer = new Timer(new TimerCallback(OnCallback), null, 0, (int)Interval.TotalMilliseconds);
    }

    private void OnCallback(object? state)
    {
        if (!_parent.IsActive)
        {
            _timer!.Dispose();
            return;
        }
        Console.WriteLine($"online: {_parent.Identifier}");
        _parent.hasSentData = true;
        _parent.sendQueue.Add($"event:ping\ndata: {DataMessage}\n\n");
        _parent.Flush();
    }
}
