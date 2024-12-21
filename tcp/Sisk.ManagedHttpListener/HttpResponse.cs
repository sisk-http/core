// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponse.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.ManagedHttpListener;

public sealed class HttpResponse {
    public int StatusCode { get; set; }
    public string StatusDescription { get; set; }
    public List<(string, string)> Headers { get; set; }
    public Stream? ResponseStream { get; set; }

    internal HttpResponse () {
        this.StatusCode = 200;
        this.StatusDescription = "Ok";
        this.Headers = new List<(string, string)>
            {
                ("Date", DateTime.Now.ToString("R")),
                ("Server", "Sisk")
            };
    }
}