using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
// Add any other necessary using statements as tests are developed,
// e.g., for JSON parsing if needed, or specific Sisk types.

namespace tests.Tests // Ensure this namespace matches other test files
{
    // Helper class for SSE Events
    public class SseEvent
    {
        public string EventName { get; set; } = "message"; // Default SSE event name
        public string Data { get; set; } = "";

        public override string ToString() => $"(Event=\"{EventName}\" Data=\"{Data}\")";

        public override bool Equals(object? obj)
        {
            return obj is SseEvent other &&
                   EventName == other.EventName &&
                   Data == other.Data;
        }
        public override int GetHashCode() => HashCode.Combine(EventName, Data);
    }

    [TestClass]
    public class HttpEventSourceTests
    {
        private async Task<List<SseEvent>> ReadSseEventsAsync(HttpResponseMessage response)
        {
            var events = new List<SseEvent>();
            response.EnsureSuccessStatusCode();

            Assert.AreEqual("text/event-stream", response.Content.Headers.ContentType?.MediaType, "Content-Type should be text/event-stream.");
            if (response.Content.Headers.ContentType?.CharSet != null && !string.IsNullOrEmpty(response.Content.Headers.ContentType.CharSet))
            {
                Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet.ToLowerInvariant(), "Charset should be utf-8 if specified.");
            }

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream, Encoding.UTF8)) // SSE is always UTF-8
            {
                string? line;
                var currentEvent = new SseEvent();
                StringBuilder dataBuffer = new StringBuilder();

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) // Blank line: dispatch event
                    {
                        if (dataBuffer.Length > 0) // Only dispatch if we have some data
                        {
                            currentEvent.Data = dataBuffer.ToString().TrimEnd('\n'); // Remove trailing newline if any
                            events.Add(currentEvent);
                        }
                        currentEvent = new SseEvent(); // Reset for next event
                        dataBuffer.Clear();
                    }
                    else if (line.Contains(':'))
                    {
                        var parts = line.Split(':', StringSplitOptions.TrimEntries);
                        events.Add(new SseEvent()
                        {
                            Data = parts[1],
                            EventName = parts[0]
                        });
                    }
                    else
                    {
                        Assert.Fail("invalid SSE line");
                    }
                }
                // In case the stream ends without a final blank line but there's data buffered
                if (dataBuffer.Length > 0)
                {
                    currentEvent.Data = dataBuffer.ToString().TrimEnd('\n');
                    events.Add(currentEvent);
                }
            }
            return events;
        }

        [TestMethod]
        public async Task TestSyncSseFeatures()
        {
            using (var client = Server.GetHttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "tests/sse/sync");
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    Assert.IsTrue(response.Headers.TryGetValues("X-Test-SSE", out var headerValues), "X-Test-SSE header missing.");
                    Assert.AreEqual("sync", headerValues?.FirstOrDefault(), "X-Test-SSE header value incorrect.");

                    var receivedEvents = await ReadSseEventsAsync(response);
                    var expectedEvents = new List<SseEvent>
                    {
                        new SseEvent { EventName = "data", Data = "message 1 part 1" },
                        new SseEvent { EventName = "data", Data = "message 1 part 2" },
                        new SseEvent { EventName = "customSync", Data = "message 2 part 1" },
                        new SseEvent { EventName = "customSync", Data = "message 2 part 2" }
                    };

                    CollectionAssert.AreEqual(expectedEvents, receivedEvents, "The received SSE events do not match the expected events.");
                }
            }
        }

        [TestMethod]
        public async Task TestAsyncSseFeatures()
        {
            using (var client = Server.GetHttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "tests/sse/async");
                // Use HttpCompletionOption.ResponseHeadersRead to start processing as soon as headers are available
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    // Check for the custom header
                    Assert.IsTrue(response.Headers.TryGetValues("X-Test-SSE", out var headerValues), "X-Test-SSE header missing for async.");
                    Assert.AreEqual("async", headerValues?.FirstOrDefault(), "X-Test-SSE header value incorrect for async.");

                    var receivedEvents = await ReadSseEventsAsync(response);

                    var expectedEvents = new List<SseEvent>
                    {
                        new SseEvent { EventName = "data", Data = "async message 1" },
                        new SseEvent { EventName = "customAsync", Data = "async message 2" },
                        new SseEvent { EventName = "data", Data = "async message 3" }
                    };

                    CollectionAssert.AreEqual(expectedEvents, receivedEvents, "The received asynchronous SSE events do not match the expected events.");
                }
            }
        }

        [TestMethod]
        public async Task TestSseWithCorsHeaders()
        {
            using (var client = Server.GetHttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "tests/sse/cors");
                request.Headers.Add("Origin", "http://example.com");

                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode(); // Ensure the request itself was successful

                    // Check for the custom test header
                    Assert.IsTrue(response.Headers.TryGetValues("X-Test-SSE", out var testHeaderValues), "X-Test-SSE header missing for CORS test.");
                    Assert.AreEqual("cors", testHeaderValues?.FirstOrDefault(), "X-Test-SSE header value incorrect for CORS test.");

                    // Check for CORS header
                    Assert.IsTrue(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var corsHeaderValues), "Access-Control-Allow-Origin header missing.");
                    Assert.AreEqual("*", corsHeaderValues?.FirstOrDefault(), "Access-Control-Allow-Origin header value incorrect.");

                    var receivedEvents = await ReadSseEventsAsync(response);

                    var expectedEvents = new List<SseEvent>
                    {
                        new SseEvent { EventName = "data", Data = "cors message 1" }
                    };

                    CollectionAssert.AreEqual(expectedEvents, receivedEvents, "The received SSE events for CORS test do not match the expected events.");
                }
            }
        }

        [TestMethod]
        public async Task TestSseEmptyNullDataEvents()
        {
            using (var client = Server.GetHttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "tests/sse/empty");
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode(); // Ensure the request itself was successful

                    var receivedEvents = await ReadSseEventsAsync(response);

                    var expectedEvents = new List<SseEvent>
                    {
                        new SseEvent { EventName = "data", Data = "" },
                        new SseEvent { EventName = "data", Data = "" },
                        new SseEvent { EventName = "customEmpty", Data = "" },
                        new SseEvent { EventName = "customNull", Data = "" }
                    };

                    // Ensure SseEvent.Data is not null by default in constructor if data can be truly empty
                    // The SseEvent class initializes Data = "" so this is fine.
                    CollectionAssert.AreEqual(expectedEvents, receivedEvents, "The received SSE events for empty/null data do not match expected.");
                }
            }
        }
    }
}
