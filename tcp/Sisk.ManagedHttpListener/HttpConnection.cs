using Sisk.ManagedHttpListener.HttpSerializer;

namespace Sisk.ManagedHttpListener;

public sealed class HttpConnection : IDisposable
{
    private readonly Stream _connectionStream;
    private bool disposedValue;

    public HttpAction Action { get; set; }

    public HttpConnection(Stream connectionStream, HttpAction action)
    {
        _connectionStream = connectionStream;
        Action = action;
    }

    public int HandleConnectionEvents()
    {
        ObjectDisposedException.ThrowIf(disposedValue, this);

        Span<byte> memRequestLine = stackalloc byte[8192];

        while (_connectionStream.CanRead && !disposedValue)
        {
            //try
            //{
            using var bufferedStreamSession = new Streams.HttpBufferedReadStream(_connectionStream);

            if (!HttpRequestSerializer.TryReadHttp1Request(
                        bufferedStreamSession,
                        memRequestLine,
                out var method,
                out var path,
                out var reqContentLength,
                out var messageSize,
                out var headers,
                out var expectContinue))
            {
                Logger.LogInformation($"couldn't read request");
                return 1;
            }

            HttpSession.HttpRequest managedRequest = new HttpSession.HttpRequest(method, path, reqContentLength, headers, _connectionStream);
            HttpSession managedSession = new HttpSession(managedRequest, _connectionStream);

            Action(managedSession);

            if (!managedSession.KeepAlive)
                managedSession.Response.Headers.Set(("Connection", "Close"));

            Stream? responseStream = managedSession.Response.ResponseStream;
            if (responseStream is not null)
            {
                if (responseStream.CanSeek)
                {
                    managedSession.Response.Headers.Set(("Content-Length", responseStream.Length.ToString()));
                }
                else
                {
                    // implement chunked-encodind
                }
            }
            else
            {
                managedSession.Response.Headers.Set(("Content-Length", "0"));
            }

            if (!HttpResponseSerializer.TryWriteHttp1Response(
                _connectionStream,
                managedSession.Response.StatusCode,
                managedSession.Response.StatusDescription,
                managedSession.Response.Headers))
            {
                Logger.LogInformation($"couldn't write response");
                return 2;
            }

            if (responseStream is not null)
            {
                responseStream.CopyTo(_connectionStream);
                responseStream.Dispose();
            }

            _connectionStream.Flush();

            if (!managedSession.KeepAlive)
            {
                break;
            }
            //}
            //catch (Exception ex)
            //{
            //    Logger.LogInformation($"unhandled exception: {ex.Message}");
            //    return 3;
            //}
        }

        return 0;
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _connectionStream.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
