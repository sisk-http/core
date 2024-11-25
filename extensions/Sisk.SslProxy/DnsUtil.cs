// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   DnsUtil.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using System.Net;

namespace Sisk.Ssl;

static class DnsUtil
{
    public static IPEndPoint ResolveEndpoint(ListeningPort port, bool onlyUseIPv4 = false)
    {
        var hostEntry = Dns.GetHostEntry(port.Hostname);
        if (hostEntry.AddressList.Length == 0)
        {
            throw new InvalidOperationException($"Couldn't resolve DNS IP-address for {port}.");
        }
        else
        {
            if (onlyUseIPv4)
            {
                return new IPEndPoint(hostEntry.AddressList.Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Last(), port.Port);
            }
            else
            {
                var ipv6AddressList = hostEntry.AddressList.Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
                if (ipv6AddressList.Any())
                {
                    return new IPEndPoint(ipv6AddressList.Last(), port.Port);
                }
                else
                    return new IPEndPoint(hostEntry.AddressList.Last(), port.Port);
            }
        }
    }
}
