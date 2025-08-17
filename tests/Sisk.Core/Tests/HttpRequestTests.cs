// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestTests.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json; // For JsonSerializer
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests.Tests;

public class TestPoco {
    public string? Name { get; set; }
    public int Value { get; set; }
    public override bool Equals ( object? obj ) => obj is TestPoco other && Name == other.Name && Value == other.Value;
    public override int GetHashCode () => HashCode.Combine ( Name, Value );
}

[TestClass]
public class HttpRequestTests {
    // All previously refactored tests
    [TestMethod]
    public async Task GetBodyContents_ServerReadsAndEchoes_ClientVerifies () {
        using (var client = Server.GetHttpClient ()) {
            string testBody = "Hello, server using GetBodyContents!";
            var content = new StringContent ( testBody, Encoding.UTF8, "text/plain" );
            var response = await client.PostAsync ( "tests/httprequest/getBodyContents", content );
            response.EnsureSuccessStatusCode ();
            string responseBody = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( testBody, responseBody );
        }
    }

    [TestMethod]
    public async Task GetBodyContentsAsync_ServerReadsAndEchoes_ClientVerifies () {
        using (var client = Server.GetHttpClient ()) {
            string testBody = "Hello, server using GetBodyContentsAsync!";
            var content = new StringContent ( testBody, Encoding.UTF8, "application/xml" );
            var response = await client.PostAsync ( "tests/httprequest/getBodyContentsAsync", content );
            response.EnsureSuccessStatusCode ();
            string responseBody = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( testBody, responseBody );
        }
    }

    [TestMethod]
    public async Task RawBody_ServerReadsAndEchoes_ClientVerifies () {
        using (var client = Server.GetHttpClient ()) {
            string testBody = "Hello, server using RawBody!";
            var content = new StringContent ( testBody, Encoding.UTF8, "text/html" );
            var response = await client.PostAsync ( "tests/httprequest/rawBody", content );
            response.EnsureSuccessStatusCode ();
            string responseBody = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( testBody, responseBody );
        }
    }

    [TestMethod]
    public async Task GetBodyContents_EmptyBody_ServerReceivesEmpty () {
        using (var client = Server.GetHttpClient ()) {
            var content = new StringContent ( "", Encoding.UTF8, "text/plain" );
            var response = await client.PostAsync ( "tests/httprequest/getBodyContents", content );
            response.EnsureSuccessStatusCode ();
            string responseBody = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( "", responseBody );
        }
    }

    [TestMethod]
    public async Task GetBodyContentsAsync_EmptyBody_ServerReceivesEmpty () {
        using (var client = Server.GetHttpClient ()) {
            var content = new StringContent ( "", Encoding.UTF8, "text/plain" );
            var response = await client.PostAsync ( "tests/httprequest/getBodyContentsAsync", content );
            response.EnsureSuccessStatusCode ();
            string responseBody = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( "", responseBody );
        }
    }

    [TestMethod]
    public async Task RawBody_EmptyBody_ServerReceivesEmpty () {
        using (var client = Server.GetHttpClient ()) {
            var content = new StringContent ( "", Encoding.UTF8, "text/plain" );
            var response = await client.PostAsync ( "tests/httprequest/rawBody", content );
            response.EnsureSuccessStatusCode ();
            string responseBody = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( "", responseBody );
        }
    }

    [TestMethod]
    public async Task GetJsonContent_ServerDeserializesAndEchoes_ClientVerifies () {
        using (var client = Server.GetHttpClient ()) {
            var testData = new TestPoco { Name = "LiveServer Test", Value = 123 };
            var response = await client.PostAsJsonAsync ( "tests/httprequest/getJsonContent", testData );
            response.EnsureSuccessStatusCode ();
            TestPoco? echoedPoco = await response.Content.ReadFromJsonAsync<TestPoco> ();
            Assert.IsNotNull ( echoedPoco );
            Assert.AreEqual ( testData, echoedPoco );
        }
    }

    [TestMethod]
    public async Task GetJsonContentAsync_ServerDeserializesAndEchoes_ClientVerifies () {
        using (var client = Server.GetHttpClient ()) {
            var testData = new TestPoco { Name = "LiveServer AsyncTest", Value = 456 };
            var response = await client.PostAsJsonAsync ( "tests/httprequest/getJsonContentAsync", testData );
            response.EnsureSuccessStatusCode ();
            TestPoco? echoedPoco = await response.Content.ReadFromJsonAsync<TestPoco> ();
            Assert.IsNotNull ( echoedPoco );
            Assert.AreEqual ( testData, echoedPoco );
        }
    }

    [TestMethod]
    public async Task GetJsonContent_WithOptions_ServerUsesOptionsAndEchoes_ClientVerifies () {
        using (var client = Server.GetHttpClient ()) {
            var testData = new TestPoco { Name = "Options Test", Value = 789 };
            string rawJson = $"{{\"name\":\"{testData.Name}\",\"value\":{testData.Value}}}";
            var httpContent = new StringContent ( rawJson, Encoding.UTF8, "application/json" );
            var response = await client.PostAsync ( "tests/httprequest/getJsonContentWithOptions", httpContent );
            response.EnsureSuccessStatusCode ();
            TestPoco? echoedPoco = await response.Content.ReadFromJsonAsync<TestPoco> ();
            Assert.IsNotNull ( echoedPoco );
            Assert.AreEqual ( testData.Name, echoedPoco.Name );
            Assert.AreEqual ( testData.Value, echoedPoco.Value );
        }
    }

    [TestMethod]
    public async Task GetJsonContentAsync_WithOptions_ServerUsesOptionsAndEchoes_ClientVerifies () {
        using (var client = Server.GetHttpClient ()) {
            var testData = new TestPoco { Name = "Options Async Test", Value = 101 };
            string rawJson = $"{{\"name\":\"{testData.Name}\",\"value\":{testData.Value}}}";
            var httpContent = new StringContent ( rawJson, Encoding.UTF8, "application/json" );
            var response = await client.PostAsync ( "tests/httprequest/getJsonContentAsyncWithOptions", httpContent );
            response.EnsureSuccessStatusCode ();
            TestPoco? echoedPoco = await response.Content.ReadFromJsonAsync<TestPoco> ();
            Assert.IsNotNull ( echoedPoco );
            Assert.AreEqual ( testData.Name, echoedPoco.Name );
            Assert.AreEqual ( testData.Value, echoedPoco.Value );
        }
    }

    [TestMethod]
    public async Task GetJsonContent_InvalidJson_ServerReturnsBadRequest () {
        using (var client = Server.GetHttpClient ()) {
            var httpContent = new StringContent ( "this is not json", Encoding.UTF8, "application/json" );
            var response = await client.PostAsync ( "tests/httprequest/getJsonContent", httpContent );
            Assert.AreEqual ( System.Net.HttpStatusCode.BadRequest, response.StatusCode );
        }
    }

    [TestMethod]
    public async Task GetJsonContent_EmptyBody_ServerReturnsJsonNull () {
        using (var client = Server.GetHttpClient ()) {
            var httpContent = new StringContent ( "null", Encoding.UTF8, "application/json" );
            var response = await client.PostAsync ( "tests/httprequest/getJsonContent", httpContent );
            response.EnsureSuccessStatusCode ();
            string responseBody = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( "null", responseBody.Trim () );
        }
    }

    [TestMethod]
    public async Task GetJsonContentAsync_EmptyBody_ServerReturnsJsonNull () {
        using (var client = Server.GetHttpClient ()) {
            var httpContent = new StringContent ( "null", Encoding.UTF8, "application/json" );
            var response = await client.PostAsync ( "tests/httprequest/getJsonContentAsync", httpContent );
            response.EnsureSuccessStatusCode ();
            string responseBody = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( "null", responseBody.Trim () );
        }
    }

    [TestMethod]
    public async Task GetFormContent_ServerParsesAndEchoes_ClientVerifies () {
        using (var client = Server.GetHttpClient ()) {
            var formData = new Dictionary<string, string> { { "name", "John Doe" }, { "age", "30" } };
            var httpContent = new FormUrlEncodedContent ( formData );
            var response = await client.PostAsync ( "tests/httprequest/getFormContent", httpContent );
            response.EnsureSuccessStatusCode ();
            var echoedForm = await response.Content.ReadFromJsonAsync<Dictionary<string, string?>> ();
            Assert.IsNotNull ( echoedForm );
            Assert.AreEqual ( formData.Count, echoedForm.Count );
            Assert.AreEqual ( formData [ "name" ], echoedForm [ "name" ] );
            Assert.AreEqual ( formData [ "age" ], echoedForm [ "age" ] );
        }
    }

    [TestMethod]
    public async Task GetFormContentAsync_ServerParsesAndEchoes_ClientVerifies () {
        using (var client = Server.GetHttpClient ()) {
            var formData = new Dictionary<string, string> { { "email", "test@example.com" }, { "message", "Hello World with spaces & symbols!" } };
            var httpContent = new FormUrlEncodedContent ( formData );
            var response = await client.PostAsync ( "tests/httprequest/getFormContentAsync", httpContent );
            response.EnsureSuccessStatusCode ();
            var echoedForm = await response.Content.ReadFromJsonAsync<Dictionary<string, string?>> ();
            Assert.IsNotNull ( echoedForm );
            Assert.AreEqual ( formData.Count, echoedForm.Count );
            Assert.AreEqual ( formData [ "email" ], echoedForm [ "email" ] );
            Assert.AreEqual ( formData [ "message" ], echoedForm [ "message" ] );
        }
    }

    [TestMethod]
    public async Task GetFormContent_EmptyBody_ServerReceivesEmptyCollection () {
        using (var client = Server.GetHttpClient ()) {
            var httpContent = new FormUrlEncodedContent ( [] );
            var response = await client.PostAsync ( "tests/httprequest/getFormContent", httpContent );
            response.EnsureSuccessStatusCode ();
            var echoedForm = await response.Content.ReadFromJsonAsync<Dictionary<string, string?>> ();
            Assert.IsNotNull ( echoedForm );
            Assert.AreEqual ( 0, echoedForm.Count );
        }
    }

    [TestMethod]
    public async Task GetFormContentAsync_EmptyBody_ServerReceivesEmptyCollection () {
        using (var client = Server.GetHttpClient ()) {
            var httpContent = new FormUrlEncodedContent ( [] );
            var response = await client.PostAsync ( "tests/httprequest/getFormContentAsync", httpContent );
            response.EnsureSuccessStatusCode ();
            var echoedForm = await response.Content.ReadFromJsonAsync<Dictionary<string, string?>> ();
            Assert.IsNotNull ( echoedForm );
            Assert.AreEqual ( 0, echoedForm.Count );
        }
    }

    [TestMethod]
    public async Task GetRequestStream_ServerReadsFromStream_ClientVerifiesEcho () {
        using (var client = Server.GetHttpClient ()) {
            string testBody = "Content to be read via GetRequestStream";
            var httpContent = new StringContent ( testBody, Encoding.UTF8, "text/plain" );

            var response = await client.PostAsync ( "tests/httprequest/getRequestStream/read", httpContent );
            response.EnsureSuccessStatusCode ();

            string echoedBody = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( testBody, echoedBody );
        }
    }

    [TestMethod]
    public async Task GetRequestStream_EmptyBody_ServerConfirmsStreamIsEmpty () {
        using (var client = Server.GetHttpClient ()) {
            var httpContent = new StringContent ( "", Encoding.UTF8, "text/plain" );

            var response = await client.PostAsync ( "tests/httprequest/getRequestStream/empty", httpContent );
            response.EnsureSuccessStatusCode ();

            string responseMessage = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( "Stream was empty as expected.", responseMessage );
        }
    }

    // Refactored GetRequestStream tests for server-side exceptions/state
    [TestMethod]
    public async Task GetRequestStream_AfterBodyReadByGetBodyContents_ServerConfirmsException () {
        using (var client = Server.GetHttpClient ()) {
            var httpContent = new StringContent ( "some body", Encoding.UTF8, "text/plain" );
            var response = await client.PostAsync ( "tests/httprequest/getRequestStream/afterGetBodyContents", httpContent );

            response.EnsureSuccessStatusCode ();
            string responseMessage = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( "Caught InvalidOperationException as expected.", responseMessage );
        }
    }

    [TestMethod]
    public async Task GetRequestStream_AfterBodyReadByRawBodyProperty_ServerConfirmsException () {
        using (var client = Server.GetHttpClient ()) {
            var httpContent = new StringContent ( "some body for raw", Encoding.UTF8, "text/plain" );
            var response = await client.PostAsync ( "tests/httprequest/getRequestStream/afterRawBody", httpContent );

            response.EnsureSuccessStatusCode ();
            string responseMessage = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( "Caught InvalidOperationException as expected.", responseMessage );
        }
    }

    [TestMethod]
    public async Task GetRequestStream_AfterBodyReadByGetJsonContent_ServerConfirmsStreamState () {
        using (var client = Server.GetHttpClient ()) {
            var poco = new TestPoco { Name = "StreamState Test", Value = 999 };
            var httpContent = new StringContent ( JsonSerializer.Serialize ( poco ), Encoding.UTF8, "application/json" );

            var response = await client.PostAsync ( "tests/httprequest/getRequestStream/afterGetJsonContent", httpContent );

            if (!response.IsSuccessStatusCode) {
                string error = await response.Content.ReadAsStringAsync ();
                Assert.Fail ( $"Server returned error: {response.StatusCode} - {error}" );
            }
            response.EnsureSuccessStatusCode ();

            string responseMessage = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( "Stream returned and was consumed (0 bytes read), contentBytes is null.", responseMessage );
        }
    }
}
