// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   TransferEncoding.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.ManagedHttpListener;

[Flags]
public enum TransferEncoding {
    Chunked = 1 << 1,
    GZip = 1 << 2,
    Deflate = 1 << 3
}
