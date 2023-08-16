using Sisk.Core.Http;
using Sisk.Core.Http.Streams;
using Sisk.Core.Routing;
using System.Text;

namespace NativeAOT_Test;

static class Callbacks
{
    [Route(RouteMethod.Get, "/")]
    public static object Index(HttpRequest request)
    {
        // return PostIndex(request);
        return request.SendTo(PostIndex);
    }

    [Route(RouteMethod.Post, "/")]
    public static object PostIndex(HttpRequest request)
    {
        // return Index(request);
        return request.SendTo(Index);
    }

    [Route(RouteMethod.Get, "/st")]
    public static HttpResponse ServeText(HttpRequest request)
    {
        HttpResponseStream responseStream = request.GetResponseStream();

        string message = "Hello, world!";
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        responseStream.SendChunked = true;
        responseStream.SetHeader("Content-Type", "text/plain");
        responseStream.SetStatus(new HttpStatusInformation(200, "TUDO CERTO POR AQUI"));
        responseStream.Write(messageBytes);

        return responseStream.Close();
    }

    [Route(RouteMethod.Get, "/video")]
    public static HttpResponse ServeVideo(HttpRequest request)
    {
        var res = request.GetResponseStream();
        Stream fs = File.OpenRead("D:\\video.mp4");

        int chunkSize = 10000;
        int rangeStart = 0;

        string? rangeHeader = request.Headers["Range"];
        Console.WriteLine("-> Range: " + rangeHeader);

        if (rangeHeader?.Contains("bytes") == true)
        {
            rangeHeader = rangeHeader.Substring(rangeHeader.IndexOf('=') + 1);
            rangeHeader = rangeHeader.Substring(0, rangeHeader.IndexOf('-'));
            if (rangeHeader != "")
            {
                rangeStart = Int32.Parse(rangeHeader);
            }
        }

        int rangeEnd = ((int)Math.Min(fs.Length, chunkSize + rangeStart)) - 1;
        string contentRange = $"bytes {rangeStart}-{rangeEnd}/{fs.Length}";

        res.SendChunked = true;
        res.SetStatus(206);
        res.SetHeader("Content-Type", "video/mp4");
        res.SetHeader("Content-Range", contentRange);

        Span<byte> outputBytes = new Span<byte>(new byte[chunkSize]);
        fs.Position = rangeStart;

        fs.Read(outputBytes);
        res.Write(outputBytes);

        Console.WriteLine("<- Content-Range: " + contentRange);

        return res.Close();
    }

    [Route(RouteMethod.Get, "/file")]
    public static HttpResponse ServeStreamFile(HttpRequest request)
    {
        HttpResponseStream responseStream = request.GetResponseStream();
        Stream fs = File.OpenRead("D:\\big-file.zip");

        responseStream.SetHeader("Content-Disposition", "attachment; filename=\"big-file.zip\"");
        responseStream.SetHeader("Content-Length", fs.Length.ToString());
        fs.CopyTo(responseStream.ResponseStream);

        return responseStream.Close();
    }
}
