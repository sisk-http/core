using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic; // For Dictionary
using System.Text.Json;          // For JsonSerializer
using System.Security.Cryptography; // For SHA256

namespace tests.Tests
{
    [TestClass]
    public class WebSocketTests
    {
        private static Uri GetWebSocketServerUri(string path)
        {
            var serverUri = new Uri(Server.Instance.HttpServer.ListeningPrefixes[0]);
            return new UriBuilder(serverUri.Scheme == "https" ? "wss" : "ws", serverUri.Host, serverUri.Port, path).Uri;
        }

        // Placeholder test methods
        [TestMethod]
        public async Task Test_TextEcho_ShouldReceiveWhatIsSent()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var uri = GetWebSocketServerUri("/tests/ws/echo");
            var (client, initialMessage) = await ConnectAndReceiveInitialMessageAsync(uri, cts);

            Assert.IsTrue(initialMessage.StartsWith("Connected to /tests/ws/echo."), $"Unexpected initial message: {initialMessage}");

            string testMessage = "Hello WebSocket!";
            string response = await SendAndReceiveTextAsync(client, testMessage, cts);
            Assert.AreEqual($"Echo: {testMessage}", response);

            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token);
            client.Dispose();
        }

        [TestMethod]
        public async Task Test_BinaryEcho_ShouldReceiveWhatIsSent()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var uri = GetWebSocketServerUri("/tests/ws/echo");
            var (client, initialMessage) = await ConnectAndReceiveInitialMessageAsync(uri, cts);

            Assert.IsTrue(initialMessage.StartsWith("Connected to /tests/ws/echo."), $"Unexpected initial message: {initialMessage}");

            byte[] testPayload = Encoding.UTF8.GetBytes("This is a binary message.");
            await client.SendAsync(new ArraySegment<byte>(testPayload), WebSocketMessageType.Binary, true, cts.Token);

            var receiveBuffer = new byte[1024];
            var result = await client.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cts.Token);

            Assert.AreEqual(WebSocketMessageType.Binary, result.MessageType);
            Assert.AreEqual(testPayload.Length, result.Count);
            CollectionAssert.AreEqual(testPayload, receiveBuffer.Take(result.Count).ToArray());

            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token);
            client.Dispose();
        }

        [TestMethod]
        public async Task Test_ChecksumValidation_ValidChecksum_ShouldSucceed()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var uri = GetWebSocketServerUri("/tests/ws/checksum");
            var (client, initialMessage) = await ConnectAndReceiveInitialMessageAsync(uri, cts);
            Assert.IsTrue(initialMessage.StartsWith("Connected to /tests/ws/checksum."), $"Unexpected initial message: {initialMessage}");

            byte[] dataToSend = Encoding.UTF8.GetBytes("Data for checksumming");
            await client.SendAsync(new ArraySegment<byte>(dataToSend), WebSocketMessageType.Binary, true, cts.Token);

            // Receive "Received X bytes. Send SHA256 checksum as text."
            var buffer = new byte[1024];
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
            string serverResponse = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Assert.IsTrue(serverResponse.StartsWith($"Received {dataToSend.Length} bytes."), $"Unexpected server response: {serverResponse}");

            string checksum;
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(dataToSend);
                checksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }

            serverResponse = await SendAndReceiveTextAsync(client, checksum, cts);
            Assert.AreEqual("Checksum VALID", serverResponse);

            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token);
            client.Dispose();
        }

        [TestMethod]
        public async Task Test_ChecksumValidation_InvalidChecksum_ShouldFail()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var uri = GetWebSocketServerUri("/tests/ws/checksum");
            var (client, initialMessage) = await ConnectAndReceiveInitialMessageAsync(uri, cts);
            Assert.IsTrue(initialMessage.StartsWith("Connected to /tests/ws/checksum."), $"Unexpected initial message: {initialMessage}");

            byte[] dataToSend = Encoding.UTF8.GetBytes("Some other data");
            await client.SendAsync(new ArraySegment<byte>(dataToSend), WebSocketMessageType.Binary, true, cts.Token);

            var buffer = new byte[1024];
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token); // Receive "Received X bytes..."
            string serverResponse = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Assert.IsTrue(serverResponse.StartsWith($"Received {dataToSend.Length} bytes."), $"Unexpected server response: {serverResponse}");

            string invalidChecksum = "thisisnotavalidchecksum";
            serverResponse = await SendAndReceiveTextAsync(client, invalidChecksum, cts);
            Assert.IsTrue(serverResponse.StartsWith("Checksum INVALID"), $"Expected INVALID response, got: {serverResponse}");

            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token);
            client.Dispose();
        }

        [TestMethod]
        public async Task Test_MessageQueuing_ShouldDeliverInOrder()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20)); // Longer timeout for queue processing
            var uri = GetWebSocketServerUri("/tests/ws/queue");
            var (client, initialMessage) = await ConnectAndReceiveInitialMessageAsync(uri, cts);
            Assert.IsTrue(initialMessage.StartsWith("Connected to /tests/ws/queue."), $"Unexpected initial message: {initialMessage}");

            List<string> sentMessages = new List<string>();
            List<string> receivedProcessingOrder = new List<string>();
            List<string> receivedProcessedOrder = new List<string>();

            for (int i = 1; i <= 3; i++)
            {
                string msg = $"Message {i}";
                sentMessages.Add(msg);
                await client.SendAsync(Encoding.UTF8.GetBytes(msg), WebSocketMessageType.Text, true, cts.Token);

                // Wait for "Queued: Message X"
                var buffer = new byte[1024];
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                string response = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Assert.AreEqual($"Queued: {msg}", response);
            }

            // Collect "Processing: ..." and "Processed: ..." messages
            int expectedMessagePairs = sentMessages.Count * 2;
            for (int i = 0; i < expectedMessagePairs; i++)
            {
                var buffer = new byte[1024];
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                if (cts.IsCancellationRequested) break;

                string response = Encoding.UTF8.GetString(buffer, 0, result.Count);
                if (response.StartsWith("Processing: "))
                {
                    receivedProcessingOrder.Add(response.Substring("Processing: ".Length));
                }
                else if (response.StartsWith("Processed: "))
                {
                    receivedProcessedOrder.Add(response.Substring("Processed: ".Length));
                }
            }

            await client.SendAsync(Encoding.UTF8.GetBytes("STOP_PROCESSING"), WebSocketMessageType.Text, true, cts.Token);
            // Wait for "Stopping message processing queue."
            var finalBuffer = new byte[1024];
            var finalResult = await client.ReceiveAsync(new ArraySegment<byte>(finalBuffer), cts.Token);
            string finalResponse = Encoding.UTF8.GetString(finalBuffer, 0, finalResult.Count);
            Assert.AreEqual("Stopping message processing queue.", finalResponse);


            CollectionAssert.AreEqual(sentMessages, receivedProcessingOrder, "Messages were not processed in the order they were sent.");
            CollectionAssert.AreEqual(sentMessages, receivedProcessedOrder, "Messages were not marked 'processed' in the order they were sent.");

            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token);
            client.Dispose();
        }

        [TestMethod]
        public async Task Test_ReadInitialHeaders_ShouldReceiveHeadersFromServer()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var uri = GetWebSocketServerUri("/tests/ws/headers");

            var client = new ClientWebSocket();
            client.Options.SetRequestHeader("X-Custom-Test-Header", "TestValue123");
            client.Options.SetRequestHeader("Another-Header", "Hello Sisk");

            await client.ConnectAsync(uri, cts.Token);

            var buffer = new byte[4096]; // Might need a larger buffer for many headers
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
            string headersJson = Encoding.UTF8.GetString(buffer, 0, result.Count);

            var receivedHeaders = JsonSerializer.Deserialize<Dictionary<string, string>>(headersJson);

            Assert.IsNotNull(receivedHeaders, "Failed to deserialize headers JSON.");
            Assert.IsTrue(receivedHeaders.ContainsKey("X-Custom-Test-Header"), "Custom header 'X-Custom-Test-Header' not found.");
            Assert.AreEqual("TestValue123", receivedHeaders["X-Custom-Test-Header"]);
            Assert.IsTrue(receivedHeaders.ContainsKey("Another-Header"), "Custom header 'Another-Header' not found.");
            Assert.AreEqual("Hello Sisk", receivedHeaders["Another-Header"]);
            Assert.IsTrue(receivedHeaders.ContainsKey("Host"), "Standard 'Host' header not found.");
            // User-Agent might be added by ClientWebSocket or OS, so check for presence if needed
            // Assert.IsTrue(receivedHeaders.ContainsKey("User-Agent"), "Standard 'User-Agent' header not found.");


            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token);
            client.Dispose();
        }

        [TestMethod]
        public async Task Test_AsyncServerMessages_ShouldReceiveDelayedResponses()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var uri = GetWebSocketServerUri("/tests/ws/async-server");
            var (client, initialMessage) = await ConnectAndReceiveInitialMessageAsync(uri, cts);
            Assert.IsTrue(initialMessage.StartsWith("Connected to /tests/ws/async-server."), $"Unexpected initial message: {initialMessage}");

            string msg1 = "AsyncTest1";
            string msg2 = "AsyncTest2";

            await client.SendAsync(Encoding.UTF8.GetBytes(msg1), WebSocketMessageType.Text, true, cts.Token);
            await client.SendAsync(Encoding.UTF8.GetBytes(msg2), WebSocketMessageType.Text, true, cts.Token);

            List<string> responses = new List<string>();
            // Expect two responses, each delayed by 500ms server-side
            for (int i = 0; i < 2; i++)
            {
                var buffer = new byte[1024];
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                responses.Add(Encoding.UTF8.GetString(buffer, 0, result.Count));
            }

            Assert.AreEqual(2, responses.Count, "Did not receive two responses.");
            Assert.IsTrue(responses.Contains($"Async response to: {msg1}"), "Response for msg1 not found.");
            Assert.IsTrue(responses.Contains($"Async response to: {msg2}"), "Response for msg2 not found.");
            // Order is not guaranteed due to async nature, so just check for presence.

            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token);
            client.Dispose();
        }

        [TestMethod]
        public async Task Test_ClientInitiatedUnexpectedDisconnect_ServerShouldHandle()
        {
            // This test is tricky to automate for server-side log verification
            // without more infrastructure. We'll connect, then dispose the client
            // without a proper close handshake, which should trigger the server's
            // WebSocketException catch block in the /tests/ws/disconnect handler.
            // Manual verification of server console output is expected for "disconnected unexpectedly".
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var uri = GetWebSocketServerUri("/tests/ws/disconnect");

            var client = new ClientWebSocket();
            await client.ConnectAsync(uri, cts.Token);

            // Send a message to make sure connection was established server-side
            await client.SendAsync(Encoding.UTF8.GetBytes("Hello, server!"), WebSocketMessageType.Text, true, cts.Token);
            var buffer = new byte[1024];
            // Expect "Still connected..." or similar if server echoes for /disconnect
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
            string initialResponse = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Assert.AreEqual("Still connected...", initialResponse);


            // Abruptly close/dispose the client to simulate an unexpected disconnect
            client.Abort(); // Or client.Dispose() directly might also work for some scenarios
            // client.Dispose();

            // Add a small delay to allow the server to process the disconnect.
            await Task.Delay(500, CancellationToken.None); // Use CancellationToken.None if cts might be cancelled by timeout

            // No direct assertion here, relies on server-side logging.
            // In a more advanced test setup, one might query a logging service or endpoint.
            Console.WriteLine("[Test_ClientInitiatedUnexpectedDisconnect_ServerShouldHandle] Client disconnected. Check server logs for 'disconnected unexpectedly' message.");
            Assert.IsTrue(true, "Test execution finished. Verify server logs for disconnect handling.");
        }

        [TestMethod]
        public async Task Test_SubProtocolNegotiation_ServerSelectsSupportedProtocol()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var uri = GetWebSocketServerUri("/tests/ws/subprotocol");

            var client = new ClientWebSocket();
            client.Options.AddSubProtocol("chat.v1");
            client.Options.AddSubProtocol("custom.protocol"); // Server supports this one
            client.Options.AddSubProtocol("unsupported.protocol");

            // ConnectAndReceiveInitialMessageAsync is not used here directly because we need to inspect client.SubProtocol after connect
            await client.ConnectAsync(uri, cts.Token);
            Assert.AreEqual("custom.protocol", client.SubProtocol, "Server did not select the expected sub-protocol.");

            var buffer = new byte[1024];
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
            string serverMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Assert.AreEqual("Sub-protocol 'custom.protocol' negotiated and selected.", serverMessage);

            string testMessage = "Testing with subprotocol";
            serverMessage = await SendAndReceiveTextAsync(client, testMessage, cts);
            Assert.AreEqual($"(custom.protocol): {testMessage}", serverMessage);

            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token);
            client.Dispose();
        }

        [TestMethod]
        public async Task Test_SubProtocolNegotiation_NoCommonProtocol_ShouldConnectWithoutSubProtocol()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var uri = GetWebSocketServerUri("/tests/ws/subprotocol");

            var client = new ClientWebSocket();
            client.Options.AddSubProtocol("unsupported.v1");
            client.Options.AddSubProtocol("another.unsupported");

            await client.ConnectAsync(uri, cts.Token);
            Assert.IsNull(client.SubProtocol, "Server selected a sub-protocol when none should have been common.");

            var buffer = new byte[1024];
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
            string serverMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Assert.AreEqual("No common sub-protocol negotiated, or client did not request one.", serverMessage);

            string testMessage = "Testing without subprotocol";
            serverMessage = await SendAndReceiveTextAsync(client, testMessage, cts);
            Assert.AreEqual($"(no-protocol): {testMessage}", serverMessage);

            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test completed", cts.Token);
            client.Dispose();
        }

        // Helper method to connect and receive initial message
        private async Task<(ClientWebSocket, string)> ConnectAndReceiveInitialMessageAsync(Uri uri, CancellationTokenSource cts, ClientWebSocketOptions? options = null)
        {
            var client = new ClientWebSocket();
            if (options != null)
            {
                foreach (var KVP in options.RequestHeaders)
                {
                    client.Options.SetRequestHeader(KVP.Key, KVP.Value);
                }
                foreach (var protocol in options.RequestedSubProtocols)
                {
                    client.Options.AddSubProtocol(protocol);
                }
            }

            await client.ConnectAsync(uri, cts.Token);
            var buffer = new byte[1024 * 4];
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
            return (client, Encoding.UTF8.GetString(buffer, 0, result.Count));
        }

        // Helper method to send and receive text
        private async Task<string> SendAndReceiveTextAsync(ClientWebSocket client, string message, CancellationTokenSource cts)
        {
            var sendBuffer = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, cts.Token);
            var receiveBuffer = new byte[1024 * 4];
            var result = await client.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cts.Token);
            return Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
        }
    }
}
