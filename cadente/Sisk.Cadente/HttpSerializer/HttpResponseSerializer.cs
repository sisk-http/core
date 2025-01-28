// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseSerializer.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.Cadente.HttpSerializer;

internal static class HttpResponseSerializer {

    public static async ValueTask<bool> WriteHttpResponseHeaders ( Stream outgoingStream, MemoryStream ms, HttpSessionResponse response ) {
        try {
            // reset the MS
            ms.SetLength ( 0 );

            const byte SPACE = 0x20;

            ms.Write ( "HTTP/1.1 "u8 );
            ms.Write ( Encoding.ASCII.GetBytes ( response.StatusCode.ToString () ) );
            ms.WriteByte ( SPACE );
            ms.Write ( Encoding.ASCII.GetBytes ( response.StatusDescription.ToString () ) );
            ms.Write ( "\r\n"u8 );

            for (int i = 0; i < response.Headers.Count; i++) {
                var header = response.Headers [ i ];

                if (header.IsEmpty)
                    continue;

                ms.Write ( header.NameBytes.Span );
                ms.Write ( ": "u8 );
                ms.Write ( header.ValueBytes.Span );
                ms.Write ( "\r\n"u8 );
            }

            ms.Write ( "\r\n"u8 );

            ms.Position = 0;
            await ms.CopyToAsync ( outgoingStream, (int) ms.Length );

            return true;
        }
        catch (Exception) {
            //Logger.LogInformation ( $"HttpResponseSerializer finished with exception: {ex.Message}" );
            return false;
        }
    }
}
