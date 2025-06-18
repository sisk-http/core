// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   DnsUtil.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Net.Sockets;
using Sisk.Core.Http;

namespace Sisk.Ssl;

static class DnsUtil {
    public static IPEndPoint ResolveEndpoint ( ListeningPort port, bool onlyUseIPv4 = false ) {

        // Check if port.Hostname is already an IP address
        if (IPAddress.TryParse ( port.Hostname, out IPAddress? ipAddress )) {
            return new IPEndPoint ( ipAddress, port.Port );
        }

        var hostEntry = Dns.GetHostEntry ( port.Hostname );

        if (hostEntry.AddressList.Length == 0)
            throw new InvalidOperationException ( $"Couldn't resolve any IP addresses for {port}." );

        IPAddress? resolvedAddress;

        if (onlyUseIPv4) {
            resolvedAddress =
                // only resolves IPv4
                hostEntry.AddressList.LastOrDefault ( a => a.AddressFamily == AddressFamily.InterNetwork );
        }
        else {
            resolvedAddress =
                // try to return the last IPv6 or IPv4
                hostEntry.AddressList.LastOrDefault ( a => a.AddressFamily == AddressFamily.InterNetwork || a.AddressFamily == AddressFamily.InterNetworkV6 );
        }

        if (resolvedAddress is null)
            throw new InvalidOperationException ( $"Couldn't resolve any IP addresses for {port}." );

        return new IPEndPoint ( resolvedAddress, port.Port );
    }
}
