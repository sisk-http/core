// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SslProxy.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Ssl.HttpSerializer;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;

namespace Sisk.Ssl;

/// <summary>
/// Represents a HTTP/1.1 proxy server that forwards traffic over SSL/HTTPS into an insecure HTTP
/// gateway.
/// </summary>
public sealed class SslProxy : IDisposable
{
    private readonly static Random clientIdGenerator = new Random();
    private readonly TcpListener listener;
    private readonly IPEndPoint remoteEndpoint;
    private readonly Channel<TcpClient> clientQueue;
    private readonly Thread channelConsumerThread;
    private bool disposedValue;

    /// <summary>
    /// Gets or sets an boolean indicating if the <see cref="SslProxy"/> should trace the
    /// gateway client when starting the proxy.
    /// </summary>
    public bool CheckGatewayConnectionOnInit { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum of open TCP connections this <see cref="SslProxy"/> can mantain
    /// open at the same time.
    /// </summary>
    public int MaxOpenConnections { get; set; } = Int32.MaxValue;

    /// <summary>
    /// Gets or sets the Proxy-Authorization header value for creating an trusted gateway between
    /// the application and the proxy.
    /// </summary>
    public string? ProxyAuthorization { get; set; }

    /// <summary>
    /// Gets or sets whether keep-alive connections should be used.
    /// </summary>
    public bool KeepAliveEnabled { get; set; } = true;

    /// <summary>
    /// Gets the SSL certificate used by the proxy server.
    /// </summary>
    public X509Certificate ServerCertificate { get; }

    /// <summary>
    /// Gets or sets a value indicating whether client certificates are required for authentication.
    /// </summary>
    public bool ClientCertificateRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets the SSL/HTTPS protocols allowed for connections.
    /// </summary>
    public SslProtocols AllowedProtocols { get; set; } = SslProtocols.Tls12 | SslProtocols.Tls13;

    /// <summary>
    /// Gets or sets a value indicating whether to check for certificate revocation.
    /// </summary>
    public bool CheckCertificateRevocation { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum time that the gateway should take to
    /// respond to a connection or message from the proxy.
    /// </summary>
    public TimeSpan GatewayTimeout { get; set; } = TimeSpan.FromSeconds(120);

    /// <summary>
    /// Gets or sets an fixed proxy host header value for incoming requests.
    /// </summary>
    public string? GatewayHostname { get; set; }

    /// <summary>
    /// Gets the proxy endpoint.
    /// </summary>
    public IPEndPoint GatewayEndpoint { get => this.remoteEndpoint; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SslProxy"/> class.
    /// </summary>
    /// <param name="sslListeningPort">The port number on which the proxy server listens for incoming connections.</param>
    /// <param name="certificate">The SSL/TLS certificate used by the proxy server.</param>
    /// <param name="remoteEndpoint">The remote endpoint to which the proxy server forwards traffic.</param>
    public SslProxy(int sslListeningPort, X509Certificate certificate, IPEndPoint remoteEndpoint)
    {
        this.listener = new TcpListener(IPAddress.Any, sslListeningPort);
        this.remoteEndpoint = remoteEndpoint;
        this.ServerCertificate = certificate;
        this.channelConsumerThread = new Thread(this.ConsumerJobThread);
        this.clientQueue = Channel.CreateBounded<TcpClient>(
            new BoundedChannelOptions(this.MaxOpenConnections) { SingleReader = true, SingleWriter = false });
    }

    /// <summary>
    /// Starts the <see cref="SslProxy"/> and start routing traffic to the set remote endpoint.
    /// </summary>
    public void Start()
    {
        if (this.CheckGatewayConnectionOnInit)
        {
            using (var gatewayClient = new TcpClient())
            {
                gatewayClient.Connect(this.remoteEndpoint);

                using (var gatewayStream = gatewayClient.GetStream())
                {
                    bool sentRequest = HttpRequestWriter.TryWriteHttpV1Request(0, gatewayStream, "TRACE", "/", [
                        ("Host", this.GatewayHostname ?? "localhost")
                    ]);

                    if (!sentRequest)
                    {
                        throw new Exception("Couldn't connect to the gateway address.");
                    }
                }
            }
        }

        if (this.KeepAliveEnabled)
        {
            this.listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
            this.listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 120);
            this.listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3);
            this.listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        }

        this.channelConsumerThread.Start();
        this.listener.Start();
        this.listener.BeginAcceptTcpClient(this.ReceiveClientAsync, null);
    }

    async void ReceiveClientAsync(IAsyncResult ar)
    {
        var client = this.listener.EndAcceptTcpClient(ar);
        this.listener.BeginAcceptTcpClient(this.ReceiveClientAsync, null);

        if (!this.disposedValue)
            await this.clientQueue.Writer.WriteAsync(client);
    }

    async void ConsumerJobThread()
    {
        var reader = this.clientQueue.Reader;
        while (!this.disposedValue && await reader.WaitToReadAsync())
        {
            while (reader.TryRead(out var client))
            {
                new Thread(delegate () { this.HandleTcpClient(client); }).Start();
            }
        }
    }

    void HandleTcpClient(TcpClient client)
    {
        using var gatewayClient = new TcpClient();
        using var connection = new HttpConnection(this, client);

        connection.HandleConnection();
    }

    private void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.clientQueue.Writer.Complete();
                this.listener.Stop();
                this.listener.Dispose();
            }

            this.disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
