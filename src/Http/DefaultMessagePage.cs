// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   DefaultMessagePage.cs
// Repository:  https://github.com/sisk-http/core

using System.Web;

namespace Sisk.Core.Http;

/// <summary>
/// Provides methods for creating informative static pages used by Sisk.
/// </summary>
public static class DefaultMessagePage {
    /// <summary>
    /// Gets or sets the page CSS string used by the page code.
    /// </summary>
    public static string DefaultPageCSS { get; set; } =
        """
        body {
            background-color: #eeeeee;
            font-family: sans-serif;
        }
        
        main {
            background-color: white;
            padding: 25px;
            border-radius: 10px;
            border: 1px solid #dddddd;
            display: block;
            margin: 30px auto 0 auto;
            max-width: 600px;
        }
        
        h1 {
            margin: 0;
        }
        
        hr {
            border-top: 1px solid #bbbbbb;
            border-bottom: none;
        }
        
        small {
            color: #777777;
        }
        """;

    /// <summary>
    /// Creates an static default page with given header and description.
    /// </summary>
    /// <param name="firstHeader">The static page header text.</param>
    /// <param name="description">The static page description text.</param>
    public static string CreateDefaultPageHtml ( string firstHeader, string description ) {
        firstHeader = HttpUtility.HtmlEncode ( firstHeader );
        description = HttpUtility.HtmlEncode ( description );

        return $$"""
            <!DOCTYPE html>
            <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>{{firstHeader}}</title>
                </head>

                <body>
                    <style>
                        {{DefaultPageCSS}}
                    </style>
                    <main>
                        <h1>{{firstHeader}}</h1>
                        <p>{{description}}</p>
                        <hr>
                        <small>Sisk/{{HttpServer.SiskVersion.ToString ( 3 )}}</small>
                    </main>
                </body>
            </html>
            """;
    }

    /// <summary>
    /// Creates an static default page with given status code and description.
    /// </summary>
    /// <param name="status">The static page status code.</param>
    /// <param name="longDescription">The static page description text.</param>
    public static HttpResponse CreateDefaultResponse ( in HttpStatusInformation status, string longDescription ) {
        string html = CreateDefaultPageHtml ( status.Description, longDescription );
        return new HttpResponse () {
            Status = status,
            Content = new HtmlContent ( html )
        };
    }
}
