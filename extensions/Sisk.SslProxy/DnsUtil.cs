// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   DnsUtil.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Ssl;

static class DnsUtil
{
    public static IPEndPoint ResolveEndpoint(ListeningPort port)
    {
        var hostEntry = Dns.GetHostEntry(port.Hostname);
        if (hostEntry.AddressList.Length == 0)
        {
            throw new InvalidOperationException($"Couldn't resolve DNS IP-address for {port}.");
        }
        else
        {
            return new IPEndPoint(hostEntry.AddressList[0], port.Port);
        }
    }
}
