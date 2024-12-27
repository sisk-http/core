// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpConnectionState.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Cadente;

internal enum HttpConnectionState {
    ConnectionClosed = 0,
    ConnectionClosedByStreamRead = 1,

    UnhandledException = 10,

    BadRequest = 20,

    ResponseWriteException = 30,
}

internal enum HttpRequestReadState {
    RequestRead = 0,
    StreamZero = 1,
    StreamError = 2
}