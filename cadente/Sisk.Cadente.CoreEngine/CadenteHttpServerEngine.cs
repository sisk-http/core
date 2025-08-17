
// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CadenteHttpServerEngine.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Sisk.Core.Http.Engine;

namespace Sisk.Cadente.CoreEngine {

    /// <summary>
    /// Represents an HTTP server engine based on the Cadente host.
    /// This class implements <see cref="HttpServerEngine"/> and <see cref="IDisposable"/>
    /// to manage the lifecycle of the HTTP server.
    /// </summary>
    public sealed class CadenteHttpServerEngine : HttpServerEngine, IDisposable {
        private List<HttpHost> hosts = [];
        private List<string> prefixes = [];
        private TimeSpan idleConnectionTimeout = TimeSpan.FromSeconds ( 90 );
        private bool isDisposed;

        private Action<HttpHost>? setupHostAction;
        private readonly ConcurrentQueue<TaskCompletionSource<HttpServerEngineContext>> _pendingContextRequests = new ();
        private readonly ConcurrentQueue<HttpServerEngineContext> _readyContexts = new ();

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
        public override void AddListeningPrefix ( string prefix ) {
            prefixes.Add ( prefix );
        }

        /// <inheritdoc/>
        public override void ClearPrefixes () {
            prefixes.Clear ();
        }

        /// <inheritdoc/>
        public override void StartServer () {
            StopServer (); // clear host list
            foreach (string prefix in prefixes) {
                HttpHost host;
                var uri = new Uri ( prefix );

                if (IPAddress.TryParse ( uri.Host, out var ipaddress )) {
                    host = new HttpHost ( new IPEndPoint ( ipaddress, uri.Port ) );
                }
                else {
                    var dnsHost = Dns.GetHostEntry ( uri.Host );
                    if (dnsHost.AddressList.Length < 1)
                        throw new ArgumentException ( $"Failed to resolve DNS for host {uri.Host}." );

                    host = new HttpHost ( new IPEndPoint ( dnsHost.AddressList [ 0 ], uri.Port ) );
                }

                host.TimeoutManager.ClientReadTimeout = idleConnectionTimeout;
                host.TimeoutManager.ClientWriteTimeout = idleConnectionTimeout;
                host.Handler = new CadenteHttpEngineHostHandler ( this );
                setupHostAction?.Invoke ( host );
                host.Start ();
                hosts.Add ( host );
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

        internal void EnqueueContext ( CadenteHttpServerEngineContext context ) {
            if (_pendingContextRequests.TryDequeue ( out var tcs )) {
                tcs.SetResult ( context );
            }
            else {
                _readyContexts.Enqueue ( context );
            }
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginGetContext ( AsyncCallback? callback, object? state ) {
            if (_readyContexts.TryDequeue ( out var context )) {
                var tcs = new TaskCompletionSource<HttpServerEngineContext> ( state );
                tcs.SetResult ( context );
                callback?.Invoke ( tcs.Task );
                return tcs.Task;
            }
            else {
                var tcs = new TaskCompletionSource<HttpServerEngineContext> ( state );
                _pendingContextRequests.Enqueue ( tcs );
                if (callback != null) {
                    tcs.Task.ContinueWith ( t => callback ( t ) );
                }
                return tcs.Task;
            }
        }

        /// <inheritdoc/>
        public override HttpServerEngineContext EndGetContext ( IAsyncResult asyncResult ) {
            var task = (Task<HttpServerEngineContext>) asyncResult;
            return task.Result;
        }
    }
}
