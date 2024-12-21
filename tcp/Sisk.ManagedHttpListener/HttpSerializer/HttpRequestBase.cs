// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestBase.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.ManagedHttpListener.HttpSerializer;

// TODO make this class internal
public class HttpRequestBase {
    public string Method;
    public string Path;
    public string Version;

    public List<(string, string)> Headers;

    public long ContentLength;

    /// <summary>
    /// Gets the index in the <see cref="BufferedContent"/> where the header parsing is terminated, including
    /// the lasts CRLF.
    /// </summary>
    public int BufferHeaderIndex;

    public byte [] BufferedContent;

    public HttpRequestBase ( string method, string path, string version, List<(string, string)> headers, int headerEnd, long contentLength, ref byte [] bufferedContent ) {
        this.Method = method ?? throw new ArgumentNullException ( nameof ( method ) );
        this.Path = path ?? throw new ArgumentNullException ( nameof ( path ) );
        this.Version = version ?? throw new ArgumentNullException ( nameof ( version ) );
        this.Headers = headers ?? throw new ArgumentNullException ( nameof ( headers ) );
        this.BufferHeaderIndex = headerEnd;
        this.ContentLength = contentLength;
        this.BufferedContent = bufferedContent;
    }
}
