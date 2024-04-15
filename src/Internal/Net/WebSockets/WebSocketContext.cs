// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   WebSocketContext.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.WebSockets;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Internal.Net.WebSockets;

public abstract class WebSocketContext
{
    public abstract Uri RequestUri { get; }
    public abstract NameValueCollection Headers { get; }
    public abstract string Origin { get; }
    public abstract IEnumerable<string> SecWebSocketProtocols { get; }
    public abstract string SecWebSocketVersion { get; }
    public abstract string SecWebSocketKey { get; }
    public abstract CookieCollection CookieCollection { get; }
    public abstract IPrincipal? User { get; }
    public abstract bool IsAuthenticated { get; }
    public abstract bool IsLocal { get; }
    public abstract bool IsSecureConnection { get; }
    public abstract WebSocket WebSocket { get; }
}