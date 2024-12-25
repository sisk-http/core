// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponse.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.ManagedHttpListener.HttpSerializer;

namespace Sisk.ManagedHttpListener;

public sealed class HttpResponse {
    public int StatusCode { get; set; }
    public string StatusDescription { get; set; }
    public List<HttpHeader> Headers { get; set; }

    public TransferEncoding TransferEncoding { get; set; }

    // MUST SPECIFY ResponseStream OR ResponseBytes, NOT BOTH
    public Stream? ResponseStream { get; set; }
    public byte []? ResponseBytes { get; set; }

    internal HttpResponse () {
        this.StatusCode = 200;
        this.StatusDescription = "Ok";
        this.Headers = new List<HttpHeader>
            {
                new HttpHeader ("Date", DateTime.Now.ToString("R")),
                new HttpHeader ("Server", "Sisk")
            };
    }
}