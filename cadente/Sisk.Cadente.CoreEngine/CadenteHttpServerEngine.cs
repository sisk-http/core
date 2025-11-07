
// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CadenteHttpServerEngine.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Net;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Channels;
using Sisk.Core.Http;
using Sisk.Core.Http.Engine;

namespace Sisk.Cadente.CoreEngine {

    /// <summary>
    /// Represents an HTTP server engine based on the Cadente host.
    /// This class implements <see cref="HttpServerEngine"/> and <see cref="IDisposable"/>
    /// to manage the lifecycle of the HTTP server.
    /// </summary>
    public sealed class CadenteHttpServerEngine : HttpServerEngine, IDisposable {
        private List<HttpHost> hosts = [];
        private List<ListeningHost> prefixes = [];
        private TimeSpan idleConnectionTimeout = TimeSpan.FromSeconds ( 90 );
        private bool isDisposed;

        private Action<HttpHost>? setupHostAction;
        private Channel<CadenteHttpServerEngineContext> _pendingContexts = Channel.CreateBounded<CadenteHttpServerEngineContext> ( new BoundedChannelOptions ( capacity: Environment.ProcessorCount * 512 ) {
            AllowSynchronousContinuations = false,
            SingleReader = true,
            SingleWriter = false
        } );

        /// <summary>
        /// Initializes a new instance of the <see cref="CadenteHttpServerEngine"/> class.
        /// </summary>
        public CadenteHttpServerEngine () {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CadenteHttpServerEngine"/> class
        /// with a specified action to set up each HTTP host.
        /// </summary>
        /// <param name="hostSetupAction">An action that is executed for each <see cref="HttpHost"/> to configure it.</param>
        public CadenteHttpServerEngine ( Action<HttpHost> hostSetupAction ) {
            setupHostAction = hostSetupAction;
        }

        /// <inheritdoc/>
        public override TimeSpan IdleConnectionTimeout {
            get => idleConnectionTimeout;
            set => idleConnectionTimeout = value;
        }

        /// <inheritdoc/>
        public override string [] ListeningPrefixes => prefixes.SelectMany ( s => s.Ports ).Select ( s => s.ToString ( includePath: true ) ).ToArray ();

        /// <inheritdoc/>
        public override HttpServerEngineContextEventLoopMecanism EventLoopMecanism => HttpServerEngineContextEventLoopMecanism.InlineAsyncronousGetContext;

        /// <inheritdoc/>
        public override void SetListeningHosts ( IEnumerable<ListeningHost> hosts ) {
            prefixes = [ .. hosts ];
        }

        /// <inheritdoc/>
        public override void OnConfiguring ( HttpServer server, HttpServerConfiguration configuration ) {
            foreach (ListeningHost prefix in prefixes) {

                foreach (ListeningPort port in prefix.Ports) {
                    HttpHost host;

                    var rawUri = port.ToString ( includePath: false );
                    var uri = new Uri ( rawUri );

                    if (IPAddress.TryParse ( uri.Host, out var ipaddress )) {
                        host = new HttpHost ( new IPEndPoint ( ipaddress, uri.Port ) );
                    }
                    else {
                        var dnsHost = Dns.GetHostEntry ( uri.Host );
                        if (dnsHost.AddressList.Length < 1)
                            throw new ArgumentException ( $"Failed to resolve DNS for host {uri.Host}." );

                        host = new HttpHost ( new IPEndPoint ( dnsHost.AddressList [ 0 ], uri.Port ) );
                    }

                    if (prefix.SslOptions is { } sslOptions) {
                        host.HttpsOptions = new ( sslOptions.ServerCertificate ) {
                            AllowedProtocols = sslOptions.AllowedProtocols,
                            CheckCertificateRevocation = sslOptions.CheckCertificateRevocation,
                            ClientCertificateRequired = sslOptions.ClientCertificateRequired
                        };
                    }

                    host.TimeoutManager.ClientReadTimeout = idleConnectionTimeout;
                    host.TimeoutManager.ClientWriteTimeout = idleConnectionTimeout;

                    host.Handler = new CadenteHttpEngineHostHandler ( this );
                    setupHostAction?.Invoke ( host );

                    hosts.Add ( host );
                }
            }
        }

        /// <inheritdoc/>
        public override void StartServer () {
            foreach (var host in hosts) {
                host.Start ();
            }
        }

        /// <inheritdoc/>
        public override void StopServer () {
            foreach (var host in hosts) {
                host.Dispose ();
            }
            hosts.Clear ();
        }

        /// <inheritdoc/>
        public override void Dispose () {
            if (isDisposed)
                return;
            StopServer ();
            isDisposed = true;
            GC.SuppressFinalize ( this );
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginGetContext ( AsyncCallback? callback, object? state ) {
            throw new NotImplementedException ();
        }

        /// <inheritdoc/>
        public override HttpServerEngineContext EndGetContext ( IAsyncResult asyncResult ) {
            throw new NotImplementedException ();
        }

        internal void EnqueueContext ( CadenteHttpServerEngineContext context ) {
            if (!_pendingContexts.Writer.TryWrite ( context )) {
                throw new InvalidOperationException ( "Failed to enqueue HTTP context." );
            }
        }

        /// <inheritdoc/>
        public override async Task<HttpServerEngineContext> GetContextAsync ( CancellationToken cancellationToken = default ) {
            return await _pendingContexts.Reader.ReadAsync ( cancellationToken );
        }
    }
}
