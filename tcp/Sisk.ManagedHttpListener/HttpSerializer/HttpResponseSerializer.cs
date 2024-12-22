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

    const int RESPONSE_HEADERS_INITIAL_CAPACITY = 128;

    public static async ValueTask<bool> WriteHttpResponseHeaders ( Stream outgoingStream, HttpResponse response ) {
        try {

            StringBuilder responseHeaderBuilder = new StringBuilder ( capacity: RESPONSE_HEADERS_INITIAL_CAPACITY );
            responseHeaderBuilder.Append ( "HTTP/1.1 " );
            responseHeaderBuilder.Append ( response.StatusCode );
            responseHeaderBuilder.Append ( ' ' );
            responseHeaderBuilder.Append ( response.StatusDescription );
            responseHeaderBuilder.Append ( "\r\n" );

            var headerCount = response.Headers.Count;
            for (int i = 0; i < headerCount; i++) {
                var header = response.Headers [ i ];
                responseHeaderBuilder.Append ( header.Item1 );
                responseHeaderBuilder.Append ( ": " );
                responseHeaderBuilder.Append ( header.Item2 );
                responseHeaderBuilder.Append ( "\r\n" );
            }

            responseHeaderBuilder.Append ( "\r\n" );

            byte [] responseHeaderBytes = Encoding.UTF8.GetBytes ( responseHeaderBuilder.ToString () );

            await outgoingStream.WriteAsync ( responseHeaderBytes );

            return true;
        }
        catch (Exception ex) {
            Logger.LogInformation ( $"HttpResponseSerializer finished with exception: {ex.Message}" );
            return false;
        }
    }
}
