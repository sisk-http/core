// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseTests.cs
// Repository:  https://github.com/sisk-http/core

using System.IO;
using System.Net; // For HttpStatusCode
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests.Tests;

[TestClass]
public sealed class HttpResponseTests {
    // Existing tests from previous steps
    [TestMethod]
    public async Task OkPlainText () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Get, "tests/plaintext" );
            var response = await client.SendAsync ( request );
            var content = await response.Content.ReadAsStringAsync ();
            Assert.IsTrue ( response.IsSuccessStatusCode );
            Assert.AreEqual ( "Hello, world!", content, ignoreCase: true );
            Assert.AreEqual ( "text/plain", response.Content.Headers.ContentType?.MediaType );
            Assert.AreEqual ( "utf-8", response.Content.Headers.ContentType?.CharSet );
        }
    }

    [TestMethod]
    public async Task OkPlainTextChunked () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Get, "tests/plaintext/chunked" );
            var response = await client.SendAsync ( request );
            var content = await response.Content.ReadAsStringAsync ();
            Assert.IsTrue ( response.IsSuccessStatusCode );
            Assert.IsTrue ( response.Headers.TransferEncodingChunked == true );
            Assert.AreEqual ( "Hello, world!", content, ignoreCase: true );
            Assert.AreEqual ( "text/plain", response.Content.Headers.ContentType?.MediaType );
            Assert.AreEqual ( "utf-8", response.Content.Headers.ContentType?.CharSet );
        }
    }

    [TestMethod]
    public async Task NotFound () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Get, "tests/not-found" );
            var response = await client.SendAsync ( request );
            Assert.IsTrue ( response.StatusCode == System.Net.HttpStatusCode.NotFound );
        }
    }

    [TestMethod]
    public async Task MethodNotAllowed () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Post, "tests/plaintext" );
            var response = await client.SendAsync ( request );
            Assert.IsTrue ( response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed );
        }
    }

    [TestMethod]
    public async Task HttpResponse_WithSiskByteArrayContent_CustomContentType () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Get, "tests/bytearray" );
            var response = await client.SendAsync ( request );
            byte [] responseBytes = await response.Content.ReadAsByteArrayAsync ();
            string responseString = System.Text.Encoding.UTF8.GetString ( responseBytes );
            Assert.IsTrue ( response.IsSuccessStatusCode );
            Assert.AreEqual ( "This is a Sisk byte array response.", responseString );
            Assert.AreEqual ( "application/custom-binary", response.Content.Headers.ContentType?.MediaType );
            Assert.IsNotNull ( response.Content.Headers.ContentLength );
            Assert.AreEqual ( responseBytes.Length, response.Content.Headers.ContentLength );
        }
    }

    [TestMethod]
    public async Task HttpResponse_WithSiskByteArrayContent_DefaultContentType () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Get, "tests/bytearray/defaulted" );
            var response = await client.SendAsync ( request );
            byte [] responseBytes = await response.Content.ReadAsByteArrayAsync ();
            string responseString = System.Text.Encoding.UTF8.GetString ( responseBytes );
            Assert.IsTrue ( response.IsSuccessStatusCode );
            Assert.AreEqual ( "Defaulted Sisk byte array.", responseString );
            Assert.IsNotNull ( response.Content.Headers.ContentLength );
            Assert.AreEqual ( responseBytes.Length, response.Content.Headers.ContentLength );
        }
    }

    [TestMethod]
    public async Task HttpResponse_WithHtmlContent_DefaultsToTextHtmlUtf8 () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Get, "tests/htmlcontent" );
            var response = await client.SendAsync ( request );
            string responseString = await response.Content.ReadAsStringAsync ();
            Assert.IsTrue ( response.IsSuccessStatusCode );
            Assert.AreEqual ( "<html><body><h1>Hello from Sisk HtmlContent</h1></body></html>", responseString );
            Assert.AreEqual ( "text/html", response.Content.Headers.ContentType?.MediaType );
            Assert.AreEqual ( "utf-8", response.Content.Headers.ContentType?.CharSet?.ToLowerInvariant () );
            Assert.IsNotNull ( response.Content.Headers.ContentLength );
            Assert.AreEqual ( Encoding.UTF8.GetByteCount ( "<html><body><h1>Hello from Sisk HtmlContent</h1></body></html>" ), response.Content.Headers.ContentLength );
        }
    }

    [TestMethod]
    public async Task HttpResponse_WithHtmlContent_CustomEncoding () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Get, "tests/htmlcontent/customchar" );
            var response = await client.SendAsync ( request );
            string responseString = await response.Content.ReadAsStringAsync ();
            Assert.IsTrue ( response.IsSuccessStatusCode );
            Assert.AreEqual ( "<html><body><h1>Custom Charset</h1></body></html>", responseString );
            Assert.AreEqual ( "text/html", response.Content.Headers.ContentType?.MediaType );
            Assert.AreEqual ( "us-ascii", response.Content.Headers.ContentType?.CharSet?.ToLowerInvariant () );
            Assert.IsNotNull ( response.Content.Headers.ContentLength );
            Assert.AreEqual ( Encoding.ASCII.GetByteCount ( "<html><body><h1>Custom Charset</h1></body></html>" ), response.Content.Headers.ContentLength );
        }
    }

    [TestMethod]
    public async Task HttpResponse_WithStreamContent_Seekable_SetsContentLength () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Get, "tests/streamcontent/seekable" );
            var response = await client.SendAsync ( request );
            string responseString = await response.Content.ReadAsStringAsync ();
            Assert.IsTrue ( response.IsSuccessStatusCode );
            Assert.AreEqual ( "This is data from a seekable stream.", responseString );
            Assert.IsNotNull ( response.Content.Headers.ContentLength, "Content-Length should be set for seekable streams." );
            Assert.IsTrue ( response.Headers.TransferEncodingChunked == null, "Should not be chunked if Content-Length is known." );
        }
    }

    [TestMethod]
    public async Task HttpResponse_WithStreamContent_NonSeekable_UsesChunkedEncoding () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Get, "tests/streamcontent/nonseekable" );
            var response = await client.SendAsync ( request );
            byte [] responseBytes = await response.Content.ReadAsByteArrayAsync ();
            string responseString = Encoding.UTF8.GetString ( responseBytes );
            Assert.IsTrue ( response.IsSuccessStatusCode );
            Assert.AreEqual ( "Data from a non-seekable stream.", responseString );
        }
    }

    [TestMethod]
    public async Task HttpResponse_WithStreamContent_PredefinedLength_SetsContentLength () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Get, "tests/streamcontent/predefinedlength" );
            var response = await client.SendAsync ( request );
            string responseString = await response.Content.ReadAsStringAsync ();
            long expectedLength = Encoding.UTF8.GetByteCount ( "Data from stream with predefined length." );
            Assert.IsTrue ( response.IsSuccessStatusCode );
            Assert.AreEqual ( "Data from stream with predefined length.", responseString );
            Assert.AreEqual ( "text/example", response.Content.Headers.ContentType?.MediaType );
            Assert.IsNotNull ( response.Content.Headers.ContentLength, "Content-Length should be set as it was predefined." );
            Assert.AreEqual ( expectedLength, response.Content.Headers.ContentLength );
            Assert.IsFalse ( response.Headers.TransferEncodingChunked == true, "Should not be chunked if Content-Length is provided via StreamContent constructor." );
        }
    }

    [TestMethod]
    public async Task GetResponseStream_SimpleContent_WritesHeadersAndBody () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Get, "tests/responsestream/simple" );
            var response = await client.SendAsync ( request );
            string responseString = await response.Content.ReadAsStringAsync ();

            Assert.IsTrue ( response.IsSuccessStatusCode );
            Assert.AreEqual ( HttpStatusCode.OK, response.StatusCode );
            Assert.AreEqual ( "Hello from GetResponseStream!", responseString );
            Assert.AreEqual ( "text/plain", response.Content.Headers.ContentType?.MediaType );
            Assert.AreEqual ( "utf-8", response.Content.Headers.ContentType?.CharSet?.ToLowerInvariant () );

            Assert.IsNotNull ( response.Content.Headers.ContentLength, "Content-Length should be set." );
            Assert.AreEqual ( Encoding.UTF8.GetByteCount ( "Hello from GetResponseStream!" ), response.Content.Headers.ContentLength );
            Assert.IsFalse ( response.Headers.TransferEncodingChunked == true, "Should not be chunked." );
        }
    }

    // New test for GetResponseStream with chunked content
    [TestMethod]
    public async Task GetResponseStream_ChunkedContent_WritesHeadersAndBodyInChunks () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Get, "tests/responsestream/chunked" );
            var response = await client.SendAsync ( request );
            string responseString = await response.Content.ReadAsStringAsync (); // HttpClient reassembles chunked content

            Assert.IsTrue ( response.IsSuccessStatusCode );
            Assert.AreEqual ( HttpStatusCode.OK, response.StatusCode );

            string expectedFullContent = "This is the first chunk. This is the second chunk. This is the final chunk.";
            Assert.AreEqual ( expectedFullContent, responseString );

            Assert.AreEqual ( "text/plain", response.Content.Headers.ContentType?.MediaType );
            Assert.AreEqual ( "utf-8", response.Content.Headers.ContentType?.CharSet?.ToLowerInvariant () );
        }
    }
}
