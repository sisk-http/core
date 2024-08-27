// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Constants.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Ssl;

static class Constants
{
    public const int CH_SPACE = 0x20; // ' '
    public const int CH_RETURN = 0x0D; // '\r'
    public const int CH_LINEFEED = 0x0A; // '\n'
    public const int CH_HSEP = 0x3A; // ':'

    public const string RESPONSE_PROTOCOL = "HTTP/1.1 ";
    public const string XDigestHeaderName = "X-Sisk-Proxy-Digest";
    public const string XClientIpHeaderName = "X-Sisk-Proxy-Client-Ip";

    public static readonly byte[] CHUNKED_EOF = [CH_RETURN, CH_LINEFEED, CH_RETURN, CH_LINEFEED];
}
