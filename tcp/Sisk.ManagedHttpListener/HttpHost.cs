using System.Net;
using System.Net.Sockets;

namespace Sisk.ManagedHttpListener;

public sealed class HttpHost : IDisposable
{
    private readonly TcpListener _listener;
    private bool disposedValue;

    public HttpAction ActionHandler { get; }
    public bool IsDisposed { get => disposedValue; }
    public int Port { get; set; } = 8080;

    public HttpHost(int port, HttpAction actionHandler)
    {
        _listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
        ActionHandler = actionHandler;
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(disposedValue, this);

        _listener.Start();
        _listener.BeginAcceptTcpClient(ReceiveClient, null);
    }

    private void ReceiveClient(IAsyncResult result)
    {
        _listener.BeginAcceptTcpClient(ReceiveClient, null);
        using (TcpClient client = _listener.EndAcceptTcpClient(result))
        {
            Stream clientStream = client.GetStream();

            using (HttpConnection connection = new HttpConnection(clientStream, ActionHandler))
            {
                connection.HandleConnectionEvents();
            }
        }
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _listener.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
