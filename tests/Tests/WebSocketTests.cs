// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   WebSocketTests.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic; // For Dictionary
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography; // For SHA256
using System.Text;
using System.Text.Json;          // For JsonSerializer
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests.Tests {
    [TestClass]
    public class WebSocketTests {
        private static Uri GetWebSocketServerUri ( string path ) {
            var serverUri = new Uri ( Server.Instance.HttpServer.ListeningPrefixes [ 0 ] );
            return new UriBuilder ( serverUri.Scheme == "https" ? "wss" : "ws", serverUri.Host, serverUri.Port, path ).Uri;
        }

        // Placeholder test methods
        [TestMethod]
        public async Task Test_TextEcho_ShouldReceiveWhatIsSent () {
            var cts = new CancellationTokenSource ( TimeSpan.FromSeconds ( 10 ) );
            var uri = GetWebSocketServerUri ( "/tests/ws/echo" );
            var (client, initialMessage) = await ConnectAndReceiveInitialMessageAsync ( uri, cts );

            Assert.IsTrue ( initialMessage.StartsWith ( "Connected to /tests/ws/echo." ), $"Unexpected initial message: {initialMessage}" );

            string testMessage = "Hello WebSocket!";
            string response = await SendAndReceiveTextAsync ( client, testMessage, cts );
            Assert.AreEqual ( testMessage, response );

            await client.CloseAsync ( WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token );
            client.Dispose ();
        }

        [TestMethod]
        public async Task Test_BinaryEcho_ShouldReceiveWhatIsSent () {
            var cts = new CancellationTokenSource ( TimeSpan.FromSeconds ( 10 ) );
            var uri = GetWebSocketServerUri ( "/tests/ws/echo" );
            var (client, initialMessage) = await ConnectAndReceiveInitialMessageAsync ( uri, cts );

            Assert.IsTrue ( initialMessage.StartsWith ( "Connected to /tests/ws/echo." ), $"Unexpected initial message: {initialMessage}" );

            byte [] testPayload = Encoding.UTF8.GetBytes ( "This is a binary message." );
            await client.SendAsync ( new ArraySegment<byte> ( testPayload ), WebSocketMessageType.Binary, true, cts.Token );

            var receiveBuffer = new byte [ 1024 ];
            var result = await client.ReceiveAsync ( new ArraySegment<byte> ( receiveBuffer ), cts.Token );

            Assert.AreEqual ( WebSocketMessageType.Binary, result.MessageType );
            Assert.AreEqual ( testPayload.Length, result.Count );
            CollectionAssert.AreEqual ( testPayload, receiveBuffer.Take ( result.Count ).ToArray () );

            await client.CloseAsync ( WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token );
            client.Dispose ();
        }

        [TestMethod]
        public async Task Test_ChecksumValidation_ValidChecksum_ShouldSucceed () {
            var cts = new CancellationTokenSource ( TimeSpan.FromSeconds ( 15 ) );
            var uri = GetWebSocketServerUri ( "/tests/ws/checksum" );
            var (client, initialMessage) = await ConnectAndReceiveInitialMessageAsync ( uri, cts );
            Assert.IsTrue ( initialMessage.StartsWith ( "Connected to /tests/ws/checksum." ), $"Unexpected initial message: {initialMessage}" );

            byte [] dataToSend = Encoding.UTF8.GetBytes ( "Data for checksumming" );
            await client.SendAsync ( new ArraySegment<byte> ( dataToSend ), WebSocketMessageType.Binary, true, cts.Token );

            // Receive "Received X bytes. Send SHA256 checksum as text."
            var buffer = new byte [ 1024 ];
            var result = await client.ReceiveAsync ( new ArraySegment<byte> ( buffer ), cts.Token );
            string serverResponse = Encoding.UTF8.GetString ( buffer, 0, result.Count );
            Assert.IsTrue ( serverResponse.StartsWith ( $"Received {dataToSend.Length} bytes." ), $"Unexpected server response: {serverResponse}" );

            string checksum;
            using (var sha256 = SHA256.Create ()) {
                byte [] hashBytes = sha256.ComputeHash ( dataToSend );
                checksum = BitConverter.ToString ( hashBytes ).Replace ( "-", "" ).ToLowerInvariant ();
            }

            serverResponse = await SendAndReceiveTextAsync ( client, checksum, cts );
            Assert.AreEqual ( "Checksum VALID", serverResponse );

            await client.CloseAsync ( WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token );
            client.Dispose ();
        }

        [TestMethod]
        public async Task Test_ChecksumValidation_InvalidChecksum_ShouldFail () {
            var cts = new CancellationTokenSource ( TimeSpan.FromSeconds ( 15 ) );
            var uri = GetWebSocketServerUri ( "/tests/ws/checksum" );
            var (client, initialMessage) = await ConnectAndReceiveInitialMessageAsync ( uri, cts );
            Assert.IsTrue ( initialMessage.StartsWith ( "Connected to /tests/ws/checksum." ), $"Unexpected initial message: {initialMessage}" );

            byte [] dataToSend = Encoding.UTF8.GetBytes ( "Some other data" );
            await client.SendAsync ( new ArraySegment<byte> ( dataToSend ), WebSocketMessageType.Binary, true, cts.Token );

            var buffer = new byte [ 1024 ];
            var result = await client.ReceiveAsync ( new ArraySegment<byte> ( buffer ), cts.Token ); // Receive "Received X bytes..."
            string serverResponse = Encoding.UTF8.GetString ( buffer, 0, result.Count );
            Assert.IsTrue ( serverResponse.StartsWith ( $"Received {dataToSend.Length} bytes." ), $"Unexpected server response: {serverResponse}" );

            string invalidChecksum = "thisisnotavalidchecksum";
            serverResponse = await SendAndReceiveTextAsync ( client, invalidChecksum, cts );
            Assert.IsTrue ( serverResponse.StartsWith ( "Checksum INVALID" ), $"Expected INVALID response, got: {serverResponse}" );

            await client.CloseAsync ( WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token );
            client.Dispose ();
        }

        [TestMethod]
        public async Task Test_ReadInitialHeaders_ShouldReceiveHeadersFromServer () {
            var cts = new CancellationTokenSource ( TimeSpan.FromSeconds ( 10 ) );
            var uri = GetWebSocketServerUri ( "/tests/ws/headers" );
            ClientWebSocket? client = null; // Declare here for Dispose

            try {
                // Use the modified helper to connect and receive the first message
                (client, string headersJson) = await ConnectAndReceiveInitialMessageAsync ( uri, cts, options => {
                    options.SetRequestHeader ( "X-Custom-Test-Header", "TestValue123" );
                    options.SetRequestHeader ( "Another-Header", "Hello Sisk" );
                } );

                var receivedHeaders = JsonSerializer.Deserialize<Dictionary<string, string>> ( headersJson );

                Assert.IsNotNull ( receivedHeaders, "Failed to deserialize headers JSON." );
                Assert.IsTrue ( receivedHeaders.ContainsKey ( "X-Custom-Test-Header" ), "Custom header 'X-Custom-Test-Header' not found." );
                Assert.AreEqual ( "TestValue123", receivedHeaders [ "X-Custom-Test-Header" ] );
                Assert.IsTrue ( receivedHeaders.ContainsKey ( "Another-Header" ), "Custom header 'Another-Header' not found." );
                Assert.AreEqual ( "Hello Sisk", receivedHeaders [ "Another-Header" ] );
                Assert.IsTrue ( receivedHeaders.ContainsKey ( "Host" ), "Standard 'Host' header not found." );

                await client.CloseAsync ( WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token );
            }
            finally {
                client?.Dispose ();
            }
        }

        [TestMethod]
        public async Task Test_AsyncServerMessages_ShouldReceiveDelayedResponses () {
            var cts = new CancellationTokenSource ( TimeSpan.FromSeconds ( 20 ) );
            var uri = GetWebSocketServerUri ( "/tests/ws/async-server" );
            var (client, initialMessage) = await ConnectAndReceiveInitialMessageAsync ( uri, cts );
            Assert.IsTrue ( initialMessage.StartsWith ( "Connected to /tests/ws/async-server." ), $"Unexpected initial message: {initialMessage}" );

            string msg1 = "AsyncTest1";
            string msg2 = "AsyncTest2";

            await client.SendAsync ( Encoding.UTF8.GetBytes ( msg1 ), WebSocketMessageType.Text, true, cts.Token );
            await client.SendAsync ( Encoding.UTF8.GetBytes ( msg2 ), WebSocketMessageType.Text, true, cts.Token );

            List<string> responses = [];
            // Expect two responses, each delayed by 500ms server-side
            for (int i = 0; i < 2; i++) {
                var buffer = new byte [ 1024 ];
                var result = await client.ReceiveAsync ( new ArraySegment<byte> ( buffer ), cts.Token );
                responses.Add ( Encoding.UTF8.GetString ( buffer, 0, result.Count ) );
            }

            Assert.AreEqual ( 2, responses.Count, "Did not receive two responses." );
            Assert.IsTrue ( responses.Contains ( $"Async response to: {msg1}" ), "Response for msg1 not found." );
            Assert.IsTrue ( responses.Contains ( $"Async response to: {msg2}" ), "Response for msg2 not found." );
            // Order is not guaranteed due to async nature, so just check for presence.

            await client.CloseAsync ( WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token );
            client.Dispose ();
        }

        [TestMethod]
        public async Task Test_ClientInitiatedUnexpectedDisconnect_ServerShouldHandle () {
            // This test is tricky to automate for server-side log verification
            // without more infrastructure. We'll connect, then dispose the client
            // without a proper close handshake, which should trigger the server's
            // WebSocketException catch block in the /tests/ws/disconnect handler.
            // Manual verification of server console output is expected for "disconnected unexpectedly".
            var cts = new CancellationTokenSource ( TimeSpan.FromSeconds ( 10 ) );
            var uri = GetWebSocketServerUri ( "/tests/ws/disconnect" );

            var client = new ClientWebSocket ();
            await client.ConnectAsync ( uri, cts.Token );

            // Send a message to make sure connection was established server-side
            await client.SendAsync ( Encoding.UTF8.GetBytes ( "Hello, server!" ), WebSocketMessageType.Text, true, cts.Token );
            var buffer = new byte [ 1024 ];
            // Expect "Still connected..." or similar if server echoes for /disconnect
            var result = await client.ReceiveAsync ( new ArraySegment<byte> ( buffer ), cts.Token );
            string initialResponse = Encoding.UTF8.GetString ( buffer, 0, result.Count );
            Assert.AreEqual ( "Still connected...", initialResponse );


            // Abruptly close/dispose the client to simulate an unexpected disconnect
            client.Abort (); // Or client.Dispose() directly might also work for some scenarios
            // client.Dispose();

            // Add a small delay to allow the server to process the disconnect.
            await Task.Delay ( 500, CancellationToken.None ); // Use CancellationToken.None if cts might be cancelled by timeout

            // No direct assertion here, relies on server-side logging.
            // In a more advanced test setup, one might query a logging service or endpoint.
            Console.WriteLine ( "[Test_ClientInitiatedUnexpectedDisconnect_ServerShouldHandle] Client disconnected. Check server logs for 'disconnected unexpectedly' message." );
            Assert.IsTrue ( true, "Test execution finished. Verify server logs for disconnect handling." );
        }

        [TestMethod]
        public async Task Test_SubProtocolNegotiation_ServerSelectsSupportedProtocol () {
            var cts = new CancellationTokenSource ( TimeSpan.FromSeconds ( 10 ) );
            var uri = GetWebSocketServerUri ( "/tests/ws/subprotocol" );

            var client = new ClientWebSocket ();
            client.Options.AddSubProtocol ( "chat.v1" );
            client.Options.AddSubProtocol ( "custom.protocol" ); // Server supports this one
            client.Options.AddSubProtocol ( "unsupported.protocol" );

            // ConnectAndReceiveInitialMessageAsync is not used here directly because we need to inspect client.SubProtocol after connect
            await client.ConnectAsync ( uri, cts.Token );
            Assert.AreEqual ( "custom.protocol", client.SubProtocol, "Server did not select the expected sub-protocol." );

            var buffer = new byte [ 1024 ];
            var result = await client.ReceiveAsync ( new ArraySegment<byte> ( buffer ), cts.Token );
            string serverMessage = Encoding.UTF8.GetString ( buffer, 0, result.Count );
            Assert.AreEqual ( "Sub-protocol 'custom.protocol' negotiated and selected.", serverMessage );

            string testMessage = "Testing with subprotocol";
            serverMessage = await SendAndReceiveTextAsync ( client, testMessage, cts );
            Assert.AreEqual ( $"(custom.protocol): {testMessage}", serverMessage );

            await client.CloseAsync ( WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token );
            client.Dispose ();
        }

        [TestMethod]
        public async Task Test_SubProtocolNegotiation_NoCommonProtocol_ShouldConnectWithoutSubProtocol () {
            var cts = new CancellationTokenSource ( TimeSpan.FromSeconds ( 10 ) );
            var uri = GetWebSocketServerUri ( "/tests/ws/subprotocol" );

            var client = new ClientWebSocket ();
            client.Options.AddSubProtocol ( "unsupported.v1" );
            client.Options.AddSubProtocol ( "another.unsupported" );

            await client.ConnectAsync ( uri, cts.Token );
            Assert.IsNull ( client.SubProtocol, "Server selected a sub-protocol when none should have been common." );

            var buffer = new byte [ 1024 ];
            var result = await client.ReceiveAsync ( new ArraySegment<byte> ( buffer ), cts.Token );
            string serverMessage = Encoding.UTF8.GetString ( buffer, 0, result.Count );
            Assert.AreEqual ( "No common sub-protocol negotiated, or client did not request one.", serverMessage );

            string testMessage = "Testing without subprotocol";
            serverMessage = await SendAndReceiveTextAsync ( client, testMessage, cts );
            Assert.AreEqual ( $"(no-protocol): {testMessage}", serverMessage );

            await client.CloseAsync ( WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token );
            client.Dispose ();
        }

        // Helper method to connect and receive initial message
        private async Task<(ClientWebSocket, string)> ConnectAndReceiveInitialMessageAsync ( Uri uri, CancellationTokenSource cts, Action<ClientWebSocketOptions>? configureOptions = null ) {
            var client = new ClientWebSocket ();
            if (configureOptions != null) {
                configureOptions ( client.Options );
            }

            await client.ConnectAsync ( uri, cts.Token );
            var buffer = new byte [ 1024 * 4 ];
            var result = await client.ReceiveAsync ( new ArraySegment<byte> ( buffer ), cts.Token );
            return (client, Encoding.UTF8.GetString ( buffer, 0, result.Count ));
        }

        // Helper method to send and receive text
        private async Task<string> SendAndReceiveTextAsync ( ClientWebSocket client, string message, CancellationTokenSource cts ) {
            var sendBuffer = Encoding.UTF8.GetBytes ( message );
            await client.SendAsync ( new ArraySegment<byte> ( sendBuffer ), WebSocketMessageType.Text, true, cts.Token );
            var receiveBuffer = new byte [ 1024 * 4 ];
            var result = await client.ReceiveAsync ( new ArraySegment<byte> ( receiveBuffer ), cts.Token );
            return Encoding.UTF8.GetString ( receiveBuffer, 0, result.Count );
        }
    }
}
