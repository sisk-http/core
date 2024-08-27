// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SecureProxy.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.SslProxy;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Sisk.Ssl;

#pragma warning disable CS1591

public sealed class SecureProxy : IDisposable
{
    public static string ProxyDigest { get; } = Guid.NewGuid().ToString();

    private TcpListener listener;
    private IPEndPoint remoteEndpoint;
    private bool disposedValue;

    public X509Certificate2 ServerCertificate { get; }
    public bool ClientCertificateRequired { get; set; } = false;
    public SslProtocols AllowedProtocols { get; set; } = SslProtocols.Tls12 | SslProtocols.Tls13;
    public bool CheckCertificateRevocation { get; set; } = false;
    public TimeSpan ProxyTimeout { get; set; } = TimeSpan.FromSeconds(120);

    public SecureProxy(int listenOn, X509Certificate2 certificate, IPEndPoint remoteEndpoint)
    {
        this.listener = new TcpListener(IPAddress.Any, listenOn);
        this.remoteEndpoint = remoteEndpoint;
        ServerCertificate = certificate;
    }

    public void Start()
    {
        listener.Start();
        listener.BeginAcceptTcpClient(ReceiveClientAsync, null);

        listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
        listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 2);
        listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2);
        listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    }

    void ReceiveClientAsync(IAsyncResult ar)
    {
        var client = listener.EndAcceptTcpClient(ar);
        listener.BeginAcceptTcpClient(ReceiveClientAsync, null);

        client.NoDelay = true;

        if (disposedValue)
            return;

        bool isKeepAliveEnabled = true;

        using (var tcpStream = client.GetStream())
        using (var sslStream = new SslStream(tcpStream, true))
        using (var httpClient = new TcpClient())
        {
            httpClient.Connect(remoteEndpoint);
            httpClient.SendTimeout = (int)(ProxyTimeout.TotalSeconds);
            httpClient.ReceiveTimeout = (int)(ProxyTimeout.TotalSeconds);

            using (var clientStream = httpClient.GetStream())
            {
                try
                {
                    sslStream.AuthenticateAsServer(ServerCertificate, ClientCertificateRequired, AllowedProtocols, CheckCertificateRevocation);
                }
                catch
                {
                    return;
                }

                while (client.Connected && !disposedValue && isKeepAliveEnabled)
                {
                    if (!HttpRequestReader.TryReadHttp1Request(sslStream, out var method, out var path, out var proto, out var reqContentLength, out var headers))
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

                    if (!HttpResponseReader.TryReadHttp1Response(clientStream, out var resStatusCode, out var resStatusDescr, out var resHeaders, out var resContentLength, out var isChunked))
                    {
                        HttpResponseWriter.TryWriteHttp1Response(sslStream, "502", "Bad Gateway", HttpResponseWriter.GetDefaultHeaders());
                        sslStream.Flush();
                        return;
                    }

                    // TODO: check if client wants to keep alive
                    resHeaders.Add(("Connection", "keep-alive"));

                    HttpResponseWriter.TryWriteHttp1Response(sslStream, resStatusCode, resStatusDescr, resHeaders);
                    if (resContentLength > 0)
                    {
                        SerializerUtils.CopyStream(clientStream, sslStream, resContentLength);
                    }
                    else if (isChunked)
                    {
                        SerializerUtils.CopyUntil(clientStream, sslStream, Constants.CHUNKED_EOF);
                    }

                    tcpStream.Flush();
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

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
