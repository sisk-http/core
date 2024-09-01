// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SslProxy.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.SslProxy;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Sisk.Ssl;

/// <summary>
/// Represents a proxy server that securely forwards traffic over SSL/HTTPS.
/// </summary>
public sealed class SslProxy : IDisposable
{
    private readonly TcpListener listener;
    private readonly IPEndPoint remoteEndpoint;
    private bool disposedValue;

    /// <summary>
    /// Gets a unique, static digest string used to verify trusted proxies.
    /// </summary>
    public static string ProxyDigest { get; } = Guid.NewGuid().ToString();

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
    /// Gets or sets the timeout duration for the proxy connection.
    /// </summary>
    public TimeSpan ProxyTimeout { get; set; } = TimeSpan.FromSeconds(120);

    /// <summary>
    /// Initializes a new instance of the <see cref="SslProxy"/> class.
    /// </summary>
    /// <param name="listenOn">The port number on which the proxy server listens for incoming connections.</param>
    /// <param name="certificate">The SSL/TLS certificate used by the proxy server.</param>
    /// <param name="remoteEndpoint">The remote endpoint to which the proxy server forwards traffic.</param>
    public SslProxy(int listenOn, X509Certificate certificate, IPEndPoint remoteEndpoint)
    {
        this.listener = new TcpListener(IPAddress.Any, listenOn);
        this.remoteEndpoint = remoteEndpoint;
        this.ServerCertificate = certificate;
    }

    /// <summary>
    /// Starts the <see cref="SslProxy"/> and start routing traffic to the set remote endpoint.
    /// </summary>
    public void Start()
    {
        this.listener.Start();
        this.listener.BeginAcceptTcpClient(this.ReceiveClientAsync, null);

        this.listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
        this.listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 2);
        this.listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2);
        this.listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    }

    void ReceiveClientAsync(IAsyncResult ar)
    {
        this.listener.BeginAcceptTcpClient(this.ReceiveClientAsync, null);
        var client = this.listener.EndAcceptTcpClient(ar);

        client.NoDelay = true;

        if (this.disposedValue)
            return;

        using (var tcpStream = client.GetStream())
        using (var sslStream = new SslStream(tcpStream, true))
        using (var httpClient = new TcpClient())
        {
            try
            {
                httpClient.Connect(this.remoteEndpoint);
                httpClient.SendTimeout = (int)(this.ProxyTimeout.TotalSeconds);
                httpClient.ReceiveTimeout = (int)(this.ProxyTimeout.TotalSeconds);
            }
            catch
            {
                HttpResponseWriter.TryWriteHttp1Response(sslStream, "502", "Bad Gateway", HttpResponseWriter.GetDefaultHeaders());
                sslStream.Flush();
                return;
            }

            using (var clientStream = httpClient.GetStream())
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
                            out var method,
                            out var path,
                            out var proto,
                            out var reqContentLength,
                            out var headers))
                        {
                            return;
                        }

                        headers.Add((Constants.XDigestHeaderName, ProxyDigest.ToString()));
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

                        if (!isConnectionKeepAlive)
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
        if (!this.disposedValue)
        {
            if (disposing)
            {
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
