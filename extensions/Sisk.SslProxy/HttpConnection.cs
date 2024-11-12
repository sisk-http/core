// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpConnection.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Ssl.HttpSerializer;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;

namespace Sisk.Ssl;

internal class HttpConnection : IDisposable
{
    private static readonly Random clientIdRngGenerator = new Random();
    private int connectionCloseState = -1;
    private bool disposedValue;
    private readonly int clientId = clientIdRngGenerator.Next();
    private int iteration = 0;
    private readonly DateTime createdAt = DateTime.Now;

    // do not dispose parent on Dispose ()
    public SslProxy Parent { get; }
    public TcpClient Client { get; }

    public DateTime CreatedAt { get => this.createdAt; }
    public int ClientId { get => this.clientId; }
    public int Iteration { get => this.iteration; }
    public int CloseState { get => this.connectionCloseState; }

    public HttpConnection(SslProxy parent, TcpClient client)
    {
        this.Client = client;
        this.Parent = parent;
    }

    public void HandleConnection()
    {
        this.Client.NoDelay = true;

        if (this.disposedValue)
            return;

        Logger.LogInformation($"#{this.clientId} open");

        try
        {
            using (var tcpStream = this.Client.GetStream())
            using (var sslStream = new SslStream(tcpStream, true))
            using (var gatewayClient = new TcpClient())
            {
                try
                {
                    gatewayClient.Connect(this.Parent.GatewayEndpoint);
                    gatewayClient.NoDelay = true;
                    gatewayClient.SendTimeout = (int)(this.Parent.GatewayTimeout.TotalMilliseconds);
                    gatewayClient.ReceiveTimeout = (int)(this.Parent.GatewayTimeout.TotalMilliseconds);
                }
                catch (Exception ex)
                {
                    this.connectionCloseState = 1;
                    Logger.LogInformation($"#{this.clientId}: Gateway connect exception: {ex.Message}");

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
                    try
                    {
                        sslStream.AuthenticateAsServer(
                            serverCertificate: this.Parent.ServerCertificate,
                            clientCertificateRequired: this.Parent.ClientCertificateRequired,
                            enabledSslProtocols: this.Parent.AllowedProtocols,
                            checkCertificateRevocation: this.Parent.CheckCertificateRevocation);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogInformation($"#{this.clientId}: SslAuthentication failed: {ex.Message}");
                        this.connectionCloseState = 2;

                        // write an error on the http stream
                        HttpResponseWriter.TryWriteHttp1DefaultResponse(
                            new HttpStatusInformation(400),
                            "The plain HTTP request was sent to an HTTPS port.",
                            sslStream);

                        return;
                    }

                    while (this.Client.Connected && !this.disposedValue)
                    {
                        try
                        {
                            if (!HttpRequestReader.TryReadHttp1Request(
                                        this.clientId,
                                        sslStream,
                                        reqReaderMemory,
                                        this.Parent.GatewayHostname,
                                        this.Client,
                                out var method,
                                out var path,
                                out var proto,
                                out var forwardedFor,
                                out var reqContentLength,
                                out var headers,
                                out var expectContinue))
                            {
                                Logger.LogInformation($"#{this.clientId}: couldn't read request");
                                this.connectionCloseState = 9;

                                HttpResponseWriter.TryWriteHttp1DefaultResponse(
                                    new HttpStatusInformation(400),
                                    "The server received an invalid HTTP message.",
                                    sslStream);

                                return;
                            }

                            Logger.LogInformation($"#{this.clientId} >> {method} {path}");

                            if (this.Parent.ProxyAuthorization is not null)
                            {
                                headers.Add((HttpKnownHeaderNames.ProxyAuthorization, this.Parent.ProxyAuthorization.ToString()));
                            }

                            string clientIpAddress = ((IPEndPoint)this.Client.Client.LocalEndPoint!).Address.ToString();
                            if (forwardedFor is not null)
                            {
                                headers.Add((HttpKnownHeaderNames.XForwardedFor, forwardedFor + ", " + clientIpAddress));
                            }
                            else
                            {
                                headers.Add((HttpKnownHeaderNames.XForwardedFor, clientIpAddress));
                            }

                            if (!HttpRequestWriter.TryWriteHttpV1Request(this.clientId, gatewayStream, method, path, headers))
                            {
                                HttpResponseWriter.TryWriteHttp1DefaultResponse(
                                    new HttpStatusInformation(502),
                                    "The host service ins't working.",
                                    sslStream);

                                this.connectionCloseState = 3;
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
                                        this.clientId,
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

                                this.connectionCloseState = 4;
                                return;
                            }

                            Logger.LogInformation($"#{this.clientId} << {resStatusCode} {resStatusDescr}");

                            // TODO: check if client wants to keep alive
                            if (gatewayAllowsKeepAlive && this.Parent.KeepAliveEnabled)
                            {
                                ;
                            }
                            else
                            {
                                resHeaders.Add(("Connection", "close"));
                            }
#if VERBOSE
                            if (!expectContinue)
                                resHeaders.Add(("X-Debug-Connection-Id", this.clientId.ToString()));
#endif

                            HttpResponseWriter.TryWriteHttp1Response(this.clientId, sslStream, resStatusCode, resStatusDescr, resHeaders);

                            if (expectContinue && resStatusCode == "100")
                            {
                                expectContinue = false;
                                goto redirClientContent;
                            }

                            if (isWebSocket)
                            {
                                AutoResetEvent waitEvent = new AutoResetEvent(false);

                                Logger.LogInformation($"#{this.clientId} << entering ws");

                                SerializerUtils.CopyBlocking(gatewayStream, sslStream, waitEvent);
                                SerializerUtils.CopyBlocking(sslStream, gatewayStream, waitEvent);

                                waitEvent.WaitOne();
                            }
                            else if (resContentLength > 0)
                            {
                                Logger.LogInformation($"#{this.clientId} << {resContentLength} bytes written");
                                SerializerUtils.CopyStream(gatewayStream, sslStream, resContentLength, CancellationToken.None);
                            }
                            else if (isChunked)
                            {
                                AutoResetEvent waitEvent = new AutoResetEvent(false);
                                Logger.LogInformation($"#{this.clientId} << entering chunked data");
                                SerializerUtils.CopyUntilBlocking(gatewayStream, sslStream, Constants.CHUNKED_EOF, waitEvent);
                                waitEvent.WaitOne();
                            }

                            tcpStream.Flush();

                            if (!gatewayAllowsKeepAlive || !this.Parent.KeepAliveEnabled)
                            {
                                this.connectionCloseState = 5;
                                break;
                            }
                        }
                        catch
                        {
                            this.connectionCloseState = 6;
                            return;
                        }
                        finally
                        {
                            this.iteration++;
                        }
                    }
                }
            }
        }
        finally
        {
            Logger.LogInformation($"#{this.clientId} closed. State = {this.connectionCloseState}");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.Client.Close();
            }

            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Não altere este código. Coloque o código de limpeza no método 'Dispose(bool disposing)'
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
