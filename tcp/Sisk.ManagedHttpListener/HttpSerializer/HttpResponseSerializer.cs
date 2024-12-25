// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseSerializer.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.ManagedHttpListener.HttpSerializer;

internal static class HttpResponseSerializer {

    public static async Task<bool> WriteHttpResponseHeaders ( Stream outgoingStream, HttpResponse response ) {
        try {
            const int BUFFER_SIZE = 2048;

            using var ms = new MemoryStream ( BUFFER_SIZE );
            const byte SPACE = 0x20;

            ms.Write ( "HTTP/1.1 "u8 );
            ms.Write ( Encoding.UTF8.GetBytes ( response.StatusCode.ToString () ) );
            ms.WriteByte ( SPACE );
            ms.Write ( Encoding.UTF8.GetBytes ( response.StatusDescription.ToString () ) );
            ms.Write ( "\r\n"u8 );

            for (int i = 0; i < response.Headers.Count; i++) {
                var header = response.Headers [ i ];

                ms.Write ( header.NameBytes.Span );
                ms.Write ( ": "u8 );
                ms.Write ( header.ValueBytes.Span );
                ms.Write ( "\r\n"u8 );
            }

            ms.Write ( "\r\n"u8 );

            ms.Position = 0;
            await ms.CopyToAsync ( outgoingStream, BUFFER_SIZE );

            return true;
        }
        catch (Exception ex) {
            //Logger.LogInformation ( $"HttpResponseSerializer finished with exception: {ex.Message}" );
            return false;
        }
    }
}
