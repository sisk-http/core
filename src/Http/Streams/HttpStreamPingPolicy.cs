// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
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
public sealed class HttpStreamPingPolicy : IDisposable {
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
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds ( 5 );

    internal HttpStreamPingPolicy ( HttpRequestEventSource parent ) {
        __sse_parent = parent;
    }
    internal HttpStreamPingPolicy ( HttpWebSocket parent ) {
        __ws_parent = parent;
    }

    /// <summary>
    /// Starts sending periodic pings to the client.
    /// </summary>
    public void Start () {
        _timer = new Timer ( new TimerCallback ( OnCallback ), null, 0, (int) Interval.TotalMilliseconds );
    }

    /// <summary>
    /// Configures and starts sending periodic pings to the client.
    /// </summary>
    /// <param name="dataMessage">The payload message that is sent to the server as a ping message.</param>
    /// <param name="interval">The sending interval for each ping message.</param>
    public void Start ( string dataMessage, TimeSpan interval ) {
        DataMessage = dataMessage;
        Interval = interval;

        Start ();
    }

    private void OnCallback ( object? state ) {
        if (__sse_parent != null) {
            if (!__sse_parent.IsActive) {
                _timer!.Dispose ();
                return;
            }
            __sse_parent.Send ( $":{DataMessage}" );
        }
        else if (__ws_parent != null) {
            if (__ws_parent.IsClosed) {
                _timer!.Dispose ();
                return;
            }
            __ws_parent.Send ( DataMessage );
        }
    }

    /// <inheritdoc/>
    public void Dispose () {
        _timer?.Dispose ();
    }
}
