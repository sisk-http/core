namespace tests.Tests;

[TestClass]
public sealed class HttpResponseTests {

    [TestMethod]
    public async Task OkPlainText () {
        using (var client = Server.GetHttpClient ()) {

            var request = new HttpRequestMessage ( HttpMethod.Get, "tests/plaintext" );
            var response = await client.SendAsync ( request );
            var content = await response.Content.ReadAsStringAsync ();

            Assert.IsTrue ( response.IsSuccessStatusCode );
            Assert.AreEqual ( content, "Hello, world!", ignoreCase: true );
            Assert.AreEqual ( response.Content.Headers.ContentType?.MediaType, "text/plain" );
            Assert.AreEqual ( response.Content.Headers.ContentType?.CharSet, "utf-8" );
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
            Assert.AreEqual ( content, "Hello, world!", ignoreCase: true );
            Assert.AreEqual ( response.Content.Headers.ContentType?.MediaType, "text/plain" );
            Assert.AreEqual ( response.Content.Headers.ContentType?.CharSet, "utf-8" );
        }
    }

    [TestMethod]
    public async Task NotFound () {
        using (var client = Server.GetHttpClient ()) {

            var request = new HttpRequestMessage ( HttpMethod.Get, "tests/not-found" );
            var response = await client.SendAsync ( request );
            var content = await response.Content.ReadAsStringAsync ();

            Assert.IsTrue ( response.StatusCode == System.Net.HttpStatusCode.NotFound );
        }
    }

    [TestMethod]
    public async Task MethodNotAllowed () {
        using (var client = Server.GetHttpClient ()) {

            var request = new HttpRequestMessage ( HttpMethod.Post, "tests/plaintext" );
            var response = await client.SendAsync ( request );
            var content = await response.Content.ReadAsStringAsync ();

            Assert.IsTrue ( response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed );
        }
    }
}
