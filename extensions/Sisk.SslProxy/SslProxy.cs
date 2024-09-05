// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SslProxy.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;
using Sisk.Core.Http;

namespace Sisk.Ssl;

/// <summary>
/// Represents a HTTP/1.1 proxy server that forwards traffic over SSL/HTTPS into an insecure HTTP
/// gateway.
/// </summary>
public sealed class SslProxy : IDisposable
{
    private readonly TcpListener listener;
    private readonly IPEndPoint remoteEndpoint;
    private readonly Channel<TcpClient> clientQueue = Channel.CreateBounded<TcpClient>(
        new BoundedChannelOptions(MaxOpenConnections) { SingleReader = true, SingleWriter = false });
    private Thread channelConsumerThread;
    private bool disposedValue;

    /// <summary>
    /// Gets or sets the maximum of open TCP connections this <see cref="SslProxy"/> can mantain
    /// open at the same time.
    /// </summary>
    public static int MaxOpenConnections { get; set; } = Int32.MaxValue;

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
    /// Gets or sets the proxy host header value for incoming requests.
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
    }

    /// <summary>
    /// Starts the <see cref="SslProxy"/> and start routing traffic to the set remote endpoint.
    /// </summary>
    public void Start()
    {
        if (this.KeepAliveEnabled)
        {
            this.listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
            this.listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 2);
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
                new Thread(() => this.HandleTcpClient(client))
                    .Start();
            }
        }
    }

    void HandleTcpClient(TcpClient client)
    {
        client.NoDelay = true;

        if (this.disposedValue)
            return;

        using (var tcpStream = client.GetStream())
        using (var sslStream = new SslStream(tcpStream, true))
        using (var gatewayClient = new TcpClient())
        {
            try
            {
                gatewayClient.Connect(this.remoteEndpoint);
                gatewayClient.SendTimeout = (int)(this.GatewayTimeout.TotalSeconds);
                gatewayClient.ReceiveTimeout = (int)(this.GatewayTimeout.TotalSeconds);
            }
            catch
            {
                HttpResponseWriter.TryWriteHttp1Response(sslStream, "502", "Bad Gateway", HttpResponseWriter.GetDefaultHeaders());
                sslStream.Flush();
                return;
            }

            // used by header values
            Span<byte> secHXl1 = stackalloc byte[4096];
            // used by request path
            Span<byte> secHXl2 = stackalloc byte[2048];
            // used by header name
            Span<byte> secHLg1 = stackalloc byte[512];
            // used by response reason message
            Span<byte> secHL21 = stackalloc byte[256];
            // used by request method and response status code
            Span<byte> secHSm1 = stackalloc byte[8];
            // used by request and respones protocol
            Span<byte> secHSm2 = stackalloc byte[8];

            HttpRequestReaderSpan reqReaderMemory = new HttpRequestReaderSpan()
            {
                MethodBuffer = secHSm1,
                PathBuffer = secHXl2,
                ProtocolBuffer = secHSm2,
                PsHeaderName = secHLg1,
                PsHeaderValue = secHXl1
            };

            HttpResponseReaderSpan resReaderMemory = new HttpResponseReaderSpan()
            {
                ProtocolBuffer = secHSm2,
                StatusCodeBuffer = secHSm1,
                StatusReasonBuffer = secHL21,
                PsHeaderName = secHLg1,
                PsHeaderValue = secHXl1
            };

            using (var clientStream = gatewayClient.GetStream())
            {
                try
                {
                    sslStream.AuthenticateAsServer(this.ServerCertificate, this.ClientCertificateRequired, this.AllowedProtocols, this.CheckCertificateRevocation);
                }
                catch (Exception)
                {
                    return;
                }

                while (client.Connected && !this.disposedValue)
                {
                    try
                    {
                        if (!HttpRequestReader.TryReadHttp1Request(sslStream,
                                    reqReaderMemory,
                                    this.GatewayHostname,
                                    client,
                            out var method,
                            out var path,
                            out var proto,
                            out var reqContentLength,
                            out var headers))
                        {
                            return;
                        }

                        if (this.ProxyAuthorization is not null)
                        {
                            headers.Add((HttpKnownHeaderNames.ProxyAuthorization, this.ProxyAuthorization.ToString()));
                        }
                        headers.Add((Constants.XClientIpHeaderName, ((IPEndPoint)client.Client.LocalEndPoint!).Address.ToString()));

                        if (!HttpRequestWriter.TryWriteHttpV1Request(clientStream, method, path, headers, reqContentLength))
                        {
                            HttpResponseWriter.TryWriteHttp1Response(sslStream, "502", "Bad Gateway", HttpResponseWriter.GetDefaultHeaders());
                            sslStream.Flush();
                            return;
                        }
                        if (reqContentLength > 0)
                        {
                            SerializerUtils.CopyStream(sslStream, clientStream, reqContentLength);
                        }

                        if (!HttpResponseReader.TryReadHttp1Response(clientStream,
                                    resReaderMemory,
                            out var resStatusCode,
                            out var resStatusDescr,
                            out var resHeaders,
                            out var resContentLength,
                            out var isChunked,
                            out var isConnectionKeepAlive,
                            out var isWebSocket))
                        {
                            HttpResponseWriter.TryWriteHttp1Response(sslStream, "502", "Bad Gateway", HttpResponseWriter.GetDefaultHeaders());
                            sslStream.Flush();
                            return;
                        }

                        // TODO: check if client wants to keep alive
                        if (isConnectionKeepAlive)
                        {
                            // not necessary in HTTP/1.1
                            // resHeaders.Add(("Connection", "keep-alive"));
                        }
                        else
                        {
                            resHeaders.Add(("Connection", "close"));
                        }

                        HttpResponseWriter.TryWriteHttp1Response(sslStream, resStatusCode, resStatusDescr, resHeaders);
                        if (isWebSocket)
                        {
                            AutoResetEvent waitEvent = new AutoResetEvent(false);

                            SerializerUtils.CopyBlocking(clientStream, sslStream, waitEvent);
                            SerializerUtils.CopyBlocking(sslStream, clientStream, waitEvent);

                            waitEvent.WaitOne();
                        }
                        else if (resContentLength > 0)
                        {
                            SerializerUtils.CopyStream(clientStream, sslStream, resContentLength);
                        }
                        else if (isChunked)
                        {
                            // SerializerUtils.CopyUntil(clientStream, sslStream, Constants.CHUNKED_EOF);

                            AutoResetEvent waitEvent = new AutoResetEvent(false);
                            SerializerUtils.CopyUntilBlocking(clientStream, sslStream, Constants.CHUNKED_EOF, waitEvent);
                            waitEvent.WaitOne();
                        }

                        tcpStream.Flush();

                        if (!isConnectionKeepAlive || !this.KeepAliveEnabled)
                        {
                            break;
                        }
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }
        ;//connection closed
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
