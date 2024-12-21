// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   DefaultMessagePage.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;
using System.Web;
using Sisk.Core.Http;

namespace Sisk.Ssl;

static class DefaultMessagePage {
    public static string CreateDefaultPageHtml ( string firstHeader, string description ) {
        StringBuilder htmlBuilder = new StringBuilder ();
        htmlBuilder.Append ( "<!DOCTYPE html>" );
        htmlBuilder.Append ( "<html><head><title>" );
        htmlBuilder.Append ( HttpUtility.HtmlEncode ( firstHeader ) );
        htmlBuilder.Append ( "</title></head><body><h1>" );
        htmlBuilder.Append ( HttpUtility.HtmlEncode ( firstHeader ) );
        htmlBuilder.Append ( "</h1><p>" );
        htmlBuilder.Append ( HttpUtility.HtmlEncode ( description ) );
        htmlBuilder.Append ( "</p><hr><i>Sisk v." );
        htmlBuilder.Append ( HttpServer.SiskVersion );
        htmlBuilder.Append ( "</i></body></html>" );

        return htmlBuilder.ToString ();
    }
}
