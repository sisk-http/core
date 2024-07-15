// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpStreamPingPolicy.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http.Streams;

/// <summary>
/// Provides an automatic ping sender for HTTP Event Source connections.
/// </summary>
public sealed class HttpStreamPingPolicy
{
    private readonly HttpWebSocket? __ws_parent;
    private readonly HttpRequestEventSource? __sse_parent;
    private Timer? _timer;

    /// <summary>
    /// Gets or sets the payload message that is sent to the server as a ping message.
    /// </summary>
    public string DataMessage { get; set; } = "%ping%";

    /// <summary>
    /// Gets or sets the sending interval for each ping message.
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);

    internal HttpStreamPingPolicy(HttpRequestEventSource parent)
    {
        __sse_parent = parent;
    }
    internal HttpStreamPingPolicy(HttpWebSocket parent)
    {
        __ws_parent = parent;
    }

    /// <summary>
    /// Starts sending periodic pings to the client.
    /// </summary>
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
