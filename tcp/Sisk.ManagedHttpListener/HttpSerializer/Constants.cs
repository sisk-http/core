// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Constants.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.ManagedHttpListener.HttpSerializer;

static class Constants
{
    public const int CH_SPACE = 0x20; // ' '
    public const int CH_RETURN = 0x0D; // '\r'
    public const int CH_LINEFEED = 0x0A; // '\n'
    public const int CH_HSEP = 0x3A; // ':'

    // pre-allocate 32 headers for each request
    public const int HEADER_LINE_ALLOCATION = 32;
}
