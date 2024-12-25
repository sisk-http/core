// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpEventStreamWriter.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.ManagedHttpListener;

public sealed class HttpEventStreamWriter {
    private Stream _innerStream;
    private Encoding _messageEncoding;

    internal HttpEventStreamWriter ( Stream innerStream, Encoding encoding ) {
        this._innerStream = innerStream;
        this._messageEncoding = encoding;
    }

    public async Task WriteDataAsync ( string data ) {
        byte [] payload = this._messageEncoding.GetBytes ( $"data: {data}\n\n" );
        await this._innerStream.WriteAsync ( payload );
    }

    public async Task WriteEventAsync ( string eventName ) {
        byte [] payload = this._messageEncoding.GetBytes ( $"event: {eventName}\n\n" );
        await this._innerStream.WriteAsync ( payload );
    }
}
