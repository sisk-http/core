// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   UriScheme.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Internal.Net;
static class UriScheme
{
    public const string File = "file";
    public const string Ftp = "ftp";
    public const string Gopher = "gopher";
    public const string Http = "http";
    public const string Https = "https";
    public const string News = "news";
    public const string NetPipe = "net.pipe";
    public const string NetTcp = "net.tcp";
    public const string Nntp = "nntp";
    public const string Mailto = "mailto";
    public const string Ws = "ws";
    public const string Wss = "wss";

    public const string SchemeDelimiter = "://";
}