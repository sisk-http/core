using Sisk.ManagedHttpListener.Streams;

namespace Sisk.ManagedHttpListener;

public sealed class HttpSession
{
    public HttpRequest Request { get; }
    public HttpResponse Response { get; }

    public bool KeepAlive { get; set; } = true;

    internal HttpSession(HttpRequest request, Stream contentStream)
    {
        Request = request;
        Response = new HttpResponse();
    }

    public sealed class HttpRequest
    {
        public string Method { get; }
        public string Path { get; }
        public List<(string, string)> Headers { get; }
        public Stream RequestStream { get; }

        internal HttpRequest(string method, string path, long contentLength, List<(string, string)> headers, Stream networkStream)
        {
            RequestStream = new HttpRequestStream(networkStream, contentLength);
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
        }
    }

    public sealed class HttpResponse
    {
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public List<(string, string)> Headers { get; set; }
        public Stream? ResponseStream { get; set; }

        internal HttpResponse()
        {
            StatusCode = 200;
            StatusDescription = "Ok";
            Headers = new List<(string, string)>
            {
                ("Date", DateTime.Now.ToString("R")),
                ("Server", "Sisk")
            };
        }
    }
}
