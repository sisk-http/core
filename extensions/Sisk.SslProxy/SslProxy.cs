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
    private bool disposedValue;

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
    public IPEndPoint GatewayEndpoint { get => remoteEndpoint; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SslProxy"/> class.
    /// </summary>
    /// <param name="sslListeningPort">The port number on which the proxy server listens for incoming connections.</param>
    /// <param name="certificate">The SSL/TLS certificate used by the proxy server.</param>
    /// <param name="remoteEndpoint">The remote endpoint to which the proxy server forwards traffic.</param>
    public SslProxy(int sslListeningPort, X509Certificate certificate, IPEndPoint remoteEndpoint)
    {
        listener = new TcpListener(IPAddress.Any, sslListeningPort);
        this.remoteEndpoint = remoteEndpoint;
        ServerCertificate = certificate;
    }

    /// <summary>
    /// Starts the <see cref="SslProxy"/> and start routing traffic to the set remote endpoint.
    /// </summary>
    public void Start()
    {
        listener.Start();
        listener.BeginAcceptTcpClient(ReceiveClientAsync, null);

        if (KeepAliveEnabled)
        {
            listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
            listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 2);
            listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2);
            listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        }
    }

    void ReceiveClientAsync(IAsyncResult ar)
    {
        listener.BeginAcceptTcpClient(ReceiveClientAsync, null);
        var client = listener.EndAcceptTcpClient(ar);

        client.NoDelay = true;

        if (disposedValue)
            return;

        using (var tcpStream = client.GetStream())
        using (var sslStream = new SslStream(tcpStream, true))
        using (var gatewayClient = new TcpClient())
        {
            try
            {
                gatewayClient.Connect(remoteEndpoint);
                gatewayClient.SendTimeout = (int)(GatewayTimeout.TotalSeconds);
                gatewayClient.ReceiveTimeout = (int)(GatewayTimeout.TotalSeconds);
            }
            catch
            {
                HttpResponseWriter.TryWriteHttp1Response(sslStream, "502", "Bad Gateway", HttpResponseWriter.GetDefaultHeaders());
                sslStream.Flush();
                return;
            }

            using (var clientStream = gatewayClient.GetStream())
            {
                try
                {
                    sslStream.AuthenticateAsServer(ServerCertificate, ClientCertificateRequired, AllowedProtocols, CheckCertificateRevocation);
                }
                catch (Exception)
                {
                    return;
                }

                while (client.Connected && !disposedValue)
                {
                    try
                    {
                        if (!HttpRequestReader.TryReadHttp1Request(sslStream,
                                    GatewayHostname,
                                    client,
                            out var method,
                            out var path,
                            out var proto,
                            out var reqContentLength,
                            out var headers))
                        {
                            return;
                        }

                        if (ProxyAuthorization is not null)
                        {
                            headers.Add((HttpKnownHeaderNames.ProxyAuthorization, ProxyAuthorization.ToString()));
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

                        if (!isConnectionKeepAlive || !KeepAliveEnabled)
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
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                listener.Stop();
                listener.Dispose();
            }

            disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
