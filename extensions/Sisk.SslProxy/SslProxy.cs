// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SslProxy.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Ssl.HttpSerializer;
using System.Net;
using System.Net.Security;
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
                new Thread(() => this.HandleTcpClient(client))
                    .Start();
            }
        }
    }

    void HandleTcpClient(TcpClient client)
    {
        client.NoDelay = true;
        bool keepAliveEnabled = this.KeepAliveEnabled;
        int clientId = clientIdGenerator.Next();
        int connectionCloseState = 0;
        int iteration = 0;

        if (this.disposedValue)
            return;

        Logger.LogInformation($"#{clientId} open");

        try
        {
            using (var tcpStream = client.GetStream())
            using (var sslStream = new SslStream(tcpStream, true))
            using (var gatewayClient = new TcpClient())
            {
                try
                {
                    gatewayClient.Connect(this.remoteEndpoint);
                    gatewayClient.NoDelay = true;
                    gatewayClient.SendTimeout = (int)(this.GatewayTimeout.TotalSeconds);
                    gatewayClient.ReceiveTimeout = (int)(this.GatewayTimeout.TotalSeconds);
                }
                catch (Exception ex)
                {
                    connectionCloseState = 1;
                    Logger.LogInformation($"#{clientId}: Gateway connect exception: {ex.Message}");

                    HttpResponseWriter.TryWriteHttp1DefaultResponse(
                        new HttpStatusInformation(502),
                        "The host service ins't working.",
                        sslStream);

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

                using (var gatewayStream = gatewayClient.GetStream())
                {
                    gatewayStream.ReadTimeout = (int)(this.GatewayTimeout.TotalSeconds);
                    gatewayStream.WriteTimeout = (int)(this.GatewayTimeout.TotalSeconds);

                    try
                    {
                        sslStream.AuthenticateAsServer(this.ServerCertificate, this.ClientCertificateRequired, this.AllowedProtocols, this.CheckCertificateRevocation);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogInformation($"#{clientId}: SslAuthentication failed: {ex.Message}");
                        connectionCloseState = 2;

                        // write an error on the http stream
                        HttpResponseWriter.TryWriteHttp1DefaultResponse(
                            new HttpStatusInformation(400),
                            "The plain HTTP request was sent to an HTTPS port.",
                            sslStream);

                        return;
                    }

                    while (client.Connected && !this.disposedValue)
                    {
                        try
                        {
                            if (!HttpRequestReader.TryReadHttp1Request(
                                        clientId,
                                        sslStream,
                                        reqReaderMemory,
                                        this.GatewayHostname,
                                        client,
                                out var method,
                                out var path,
                                out var proto,
                                out var forwardedFor,
                                out var reqContentLength,
                                out var headers,
                                out var expectContinue))
                            {
                                Logger.LogInformation($"#{clientId}: couldn't read request");
                                connectionCloseState = 9;

                                HttpResponseWriter.TryWriteHttp1DefaultResponse(
                                    new HttpStatusInformation(400),
                                    "The server received an invalid HTTP message.",
                                    sslStream);

                                return;
                            }

                            Logger.LogInformation($"#{clientId} >> {method} {path}");

                            if (this.ProxyAuthorization is not null)
                            {
                                headers.Add((HttpKnownHeaderNames.ProxyAuthorization, this.ProxyAuthorization.ToString()));
                            }

                            string clientIpAddress = ((IPEndPoint)client.Client.LocalEndPoint!).Address.ToString();
                            if (forwardedFor is not null)
                            {
                                headers.Add((HttpKnownHeaderNames.XForwardedFor, forwardedFor + ", " + clientIpAddress));
                            }
                            else
                            {
                                headers.Add((HttpKnownHeaderNames.XForwardedFor, clientIpAddress));
                            }

                            if (!HttpRequestWriter.TryWriteHttpV1Request(clientId, gatewayStream, method, path, headers))
                            {
                                HttpResponseWriter.TryWriteHttp1DefaultResponse(
                                    new HttpStatusInformation(502),
                                    "The host service ins't working.",
                                    sslStream);

                                connectionCloseState = 3;
                                return;
                            }

                            if (expectContinue)
                            {
                                goto readGatewayResponse;
                            }

                        redirClientContent:
                            if (reqContentLength > 0)
                            {
                                if (!SerializerUtils.CopyStream(sslStream, gatewayStream, reqContentLength, CancellationToken.None))
                                {
                                    // client couldn't send the full content
                                    break;
                                }
                            }

                        readGatewayResponse:
                            if (!HttpResponseReader.TryReadHttp1Response(
                                        clientId,
                                        gatewayStream,
                                        resReaderMemory,
                                out var resStatusCode,
                                out var resStatusDescr,
                                out var resHeaders,
                                out var resContentLength,
                                out var isChunked,
                                out var gatewayAllowsKeepAlive,
                                out var isWebSocket))
                            {
                                HttpResponseWriter.TryWriteHttp1DefaultResponse(
                                      new HttpStatusInformation(502),
                                      "The host service ins't working.",
                                      sslStream);

                                connectionCloseState = 4;
                                return;
                            }

                            Logger.LogInformation($"#{clientId} << {resStatusCode} {resStatusDescr}");

                            // TODO: check if client wants to keep alive
                            if (gatewayAllowsKeepAlive && keepAliveEnabled)
                            {
                                ;
                            }
                            else
                            {
                                resHeaders.Add(("Connection", "close"));
                            }
#if VERBOSE
                            if (!expectContinue)
                                resHeaders.Add(("X-Debug-Connection-Id", clientId.ToString()));
#endif

                            HttpResponseWriter.TryWriteHttp1Response(clientId, sslStream, resStatusCode, resStatusDescr, resHeaders);

                            if (expectContinue && resStatusCode == "100")
                            {
                                expectContinue = false;
                                goto redirClientContent;
                            }

                            if (isWebSocket)
                            {
                                AutoResetEvent waitEvent = new AutoResetEvent(false);

                                Logger.LogInformation($"#{clientId} << entering ws");

                                SerializerUtils.CopyBlocking(gatewayStream, sslStream, waitEvent);
                                SerializerUtils.CopyBlocking(sslStream, gatewayStream, waitEvent);

                                waitEvent.WaitOne();
                            }
                            else if (resContentLength > 0)
                            {
                                Logger.LogInformation($"#{clientId} << {resContentLength} bytes written");
                                SerializerUtils.CopyStream(gatewayStream, sslStream, resContentLength, CancellationToken.None);
                            }
                            else if (isChunked)
                            {
                                AutoResetEvent waitEvent = new AutoResetEvent(false);
                                Logger.LogInformation($"#{clientId} << entering chunked data");
                                SerializerUtils.CopyUntilBlocking(gatewayStream, sslStream, Constants.CHUNKED_EOF, waitEvent);
                                waitEvent.WaitOne();
                            }

                            tcpStream.Flush();

                            if (!gatewayAllowsKeepAlive || !keepAliveEnabled)
                            {
                                connectionCloseState = 5;
                                break;
                            }
                        }
                        catch
                        {
                            connectionCloseState = 6;
                            return;
                        }
                        finally
                        {
                            iteration++;
                        }
                    }
                }
            }
        }
        finally
        {
            Logger.LogInformation($"#{clientId} closed. State = {connectionCloseState}");
        }
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
