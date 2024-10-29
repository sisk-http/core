// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   StaticRouter.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Helpers;
using Sisk.Core.Http;
using System.Text;

namespace Sisk.Mvc;

internal class StaticRouter
{
    public static HttpResponse ServeStaticAsset(HttpRequest request)
    {
        string filename = request.RouteParameters["filename"].GetString();

        var assets = AssetsManager.GetStaticAssets();
        if (filename == "dist/app.css")
        {
            return new HttpResponse()
                .WithContent(new StringContent(assets.css, Encoding.Default, "text/css"));
        }
        else
        {
            foreach (var r in AssemblyResourceHelper.ManifestResourceNames)
            {
                if (string.Compare(r[(r.IndexOf('.') + 1)..], request.Path.TrimStart('/'), true) == 0)
                {
                    using Stream resourceStream = AssemblyResourceHelper.EntryAssembly.GetManifestResourceStream(r)!;

                    var resStream = request.GetResponseStream();

                    string mimeType = MimeHelper.GetMimeType(Path.GetExtension(r));
                    resStream.SetHeader(HttpKnownHeaderNames.ContentType, mimeType);
                    resStream.SetContentLength(resourceStream.Length);

                    resourceStream.CopyTo(resStream.ResponseStream);

                    return resStream.Close();
                }
            }

            return DefaultMessagePage.CreateDefaultResponse(HttpStatusInformation.NotFound, "The requested resource was not found.");
        }
    }
}
