﻿// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Constants.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Ssl;

static class Constants {
    public const int CH_SPACE = 0x20; // ' '
    public const int CH_RETURN = 0x0D; // '\r'
    public const int CH_LINEFEED = 0x0A; // '\n'
    public const int CH_HSEP = 0x3A; // ':'

    public const string RESPONSE_PROTOCOL = "HTTP/1.1 ";

    public const string Server = "siskproxy/0.1";

    // pre-allocate 32 headers for each request
    public const int HEADER_LINE_ALLOCATION = 32;

    public static readonly byte [] CHUNKED_EOF = [
        0x30, // ascii 0
        CH_RETURN, CH_LINEFEED,
        CH_RETURN, CH_LINEFEED];
}
