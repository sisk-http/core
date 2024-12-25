// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestBase.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.ManagedHttpListener.HttpSerializer;

class HttpRequestBase {
    public required string Method;
    public required string Path;
    public required string Version;

    public required HttpHeader [] Headers;

    public long ContentLength;

    public bool CanKeepAlive;

    /// <summary>
    /// Gets the index in the <see cref="BufferedContent"/> where the header parsing is terminated, including
    /// the lasts CRLF.
    /// </summary>
    public int BufferHeaderIndex;
    public required byte [] BufferedContent;
}
