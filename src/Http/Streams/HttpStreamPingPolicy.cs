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
/// public class HttpStreamPingPolicy
/// </definition>
/// <type>
/// Class
/// </type>
public sealed class HttpStreamPingPolicy
{
    private HttpWebSocket? __ws_parent;
    private HttpRequestEventSource? __sse_parent;
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

    internal HttpStreamPingPolicy(HttpRequestEventSource parent)
    {
        this.__sse_parent = parent;
    }
    internal HttpStreamPingPolicy(HttpWebSocket parent)
    {
        this.__ws_parent = parent;
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
        if (__sse_parent != null)
        {
            if (!__sse_parent.IsActive)
            {
                _timer!.Dispose();
                return;
            }
            __sse_parent.hasSentData = true;
            __sse_parent.sendQueue.Add($"event:ping\ndata: {DataMessage}\n\n");
            __sse_parent.Flush();
        }
        else if (__ws_parent != null)
        {
            if (__ws_parent.IsClosed)
            {
                _timer!.Dispose();
                return;
            }
            __ws_parent.Send(DataMessage);
        }
    }
}
