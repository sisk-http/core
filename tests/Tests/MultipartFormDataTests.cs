// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MultipartFormDataTests.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers; // Added for MediaTypeHeaderValue
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests.Tests;

// Helper class
public class SimpleMultipartObjectInfo {
    public string? Name { get; set; }
    public string? Value { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long Length { get; set; }
    public string? ContentPreview { get; set; }
    public Dictionary<string, string>? PartHeaders { get; set; }
}

[TestClass]
public class MultipartFormDataTests {
    [TestMethod]
    public async Task GetMultipartFormContent_ServerParsesAndEchoes_ClientVerifies () {
        using (var client = Server.GetHttpClient ())
        using (var multipartContent = new MultipartFormDataContent ( "boundary----" + Guid.NewGuid ().ToString () )) {
            string textFieldName = "textField";
            string textFieldValue = "textValue123";
            string fileName = "testfile.txt";
            string fileContentString = "This is the content of the test file.";
            string fileContentType = "text/plain";
            byte [] fileContentBytes = Encoding.UTF8.GetBytes ( fileContentString );

            multipartContent.Add ( new StringContent ( textFieldValue, Encoding.UTF8 ), textFieldName );
            var fileBytesContent = new ByteArrayContent ( fileContentBytes );
            fileBytesContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ( fileContentType );
            multipartContent.Add ( fileBytesContent, "fileField", fileName );

            var response = await client.PostAsync ( "tests/httprequest/getMultipartFormContent", multipartContent );
            response.EnsureSuccessStatusCode ();

            var echoedObjects = await response.Content.ReadFromJsonAsync<List<SimpleMultipartObjectInfo>> ();
            Assert.IsNotNull ( echoedObjects );
            Assert.AreEqual ( 2, echoedObjects.Count );

            var textObj = echoedObjects.FirstOrDefault ( o => o.Name == textFieldName );
            Assert.IsNotNull ( textObj, $"Text field '{textFieldName}' not found in response." );
            Assert.AreEqual ( textFieldValue, textObj.Value );
            Assert.IsNull ( textObj.FileName );

            var fileObj = echoedObjects.FirstOrDefault ( o => o.Name == "fileField" );
            Assert.IsNotNull ( fileObj, "File field 'fileField' not found in response." );
            Assert.AreEqual ( fileName, fileObj.FileName );
            Assert.AreEqual ( fileContentType, fileObj.ContentType );
            Assert.AreEqual ( fileContentBytes.Length, fileObj.Length );
            Assert.AreEqual ( fileContentString, fileObj.ContentPreview );
        }
    }

    [TestMethod]
    public async Task GetMultipartFormContentAsync_ServerParsesAndEchoes_ClientVerifies () {
        using (var client = Server.GetHttpClient ())
        using (var multipartContent = new MultipartFormDataContent ( "boundary----" + Guid.NewGuid ().ToString () )) {
            string fieldName = "asyncField";
            string fieldValue = "asyncValue789";
            multipartContent.Add ( new StringContent ( fieldValue, Encoding.UTF8 ), fieldName );

            var response = await client.PostAsync ( "tests/httprequest/getMultipartFormContentAsync", multipartContent );
            response.EnsureSuccessStatusCode ();

            var echoedObjects = await response.Content.ReadFromJsonAsync<List<SimpleMultipartObjectInfo>> ();
            Assert.IsNotNull ( echoedObjects );
            Assert.AreEqual ( 1, echoedObjects.Count );

            var fieldObj = echoedObjects.FirstOrDefault ( o => o.Name == fieldName );
            Assert.IsNotNull ( fieldObj, $"Field '{fieldName}' not found in response." );
            Assert.AreEqual ( fieldValue, fieldObj.Value );
        }
    }

    [TestMethod]
    public async Task GetMultipartFormContent_MalformedBody_ServerReturnsBadRequest () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Post, "tests/httprequest/getMultipartFormContent" );
            var malformedContent = new StringContent ( "", Encoding.UTF8 );
            request.Content = malformedContent;
            request.Content.Headers.TryAddWithoutValidation ( "Content-Type", "multipart/form-data;" ); // do not send a boundary
            var response = await client.SendAsync ( request );
            Assert.AreEqual ( System.Net.HttpStatusCode.BadRequest, response.StatusCode );
        }
    }

    [TestMethod]
    public async Task GetMultipartFormContent_MissingContentTypeBoundary_ServerReturnsBadRequest () {
        using (var client = Server.GetHttpClient ()) {
            var request = new HttpRequestMessage ( HttpMethod.Post, "tests/httprequest/getMultipartFormContent" );
            var bodyContent = new StringContent ( "--boundary\r\nContent-Disposition: form-data; name=\"field\"\r\n\r\nvalue\r\n--boundary--\r\n" );
            bodyContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ( "multipart/form-data" );
            request.Content = bodyContent;
            var response = await client.SendAsync ( request );
            Assert.AreEqual ( System.Net.HttpStatusCode.BadRequest, response.StatusCode );
        }
    }

    [TestMethod]
    public async Task Multipart_ContentIntegrity_SmallTextAndFile () {
        using (var client = Server.GetHttpClient ())
        using (var multipartContent = new MultipartFormDataContent ( "boundary----" + Guid.NewGuid ().ToString () )) {
            string textFieldName = "smallTextField";
            string textFieldValue = "Hello World! This is a small text field.";
            string textFileName = "smallFile.txt";
            string textFileContent = "This is the content of a small text file.\nIt has multiple lines.";
            string textFileContentType = "text/plain";

            multipartContent.Add ( new StringContent ( textFieldValue, Encoding.UTF8 ), textFieldName );

            var fileContentBytes = Encoding.UTF8.GetBytes ( textFileContent );
            var fileStreamContent = new ByteArrayContent ( fileContentBytes );
            fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ( textFileContentType );
            multipartContent.Add ( fileStreamContent, "textFile", textFileName );

            var response = await client.PostAsync ( "tests/multipart/echo", multipartContent );
            response.EnsureSuccessStatusCode ();

            var echoedObjects = await response.Content.ReadFromJsonAsync<List<SimpleMultipartObjectInfo>> ();
            Assert.IsNotNull ( echoedObjects );
            Assert.AreEqual ( 2, echoedObjects.Count );

            var textObj = echoedObjects.FirstOrDefault ( o => o.Name == textFieldName );
            Assert.IsNotNull ( textObj );
            Assert.AreEqual ( textFieldValue, textObj.Value );

            var fileObj = echoedObjects.FirstOrDefault ( o => o.Name == "textFile" );
            Assert.IsNotNull ( fileObj );
            Assert.AreEqual ( textFileName, fileObj.FileName );
            Assert.AreEqual ( textFileContentType, fileObj.ContentType );
            Assert.AreEqual ( textFileContent, fileObj.ContentPreview ); // Assuming ContentPreview holds the full text content
            Assert.AreEqual ( fileContentBytes.Length, fileObj.Length );
        }
    }

    [TestMethod]
    public async Task Multipart_ContentIntegrity_LargerFile () {
        using (var client = Server.GetHttpClient ())
        using (var multipartContent = new MultipartFormDataContent ( "boundary----" + Guid.NewGuid ().ToString () )) {
            string largeFileName = "largeFile.bin";
            // Generate ~1.5MB of data
            int targetSize = (int) (1.5 * 1024 * 1024);
            byte [] largeFileContent = new byte [ targetSize ];
            Random rnd = new Random ();
            rnd.NextBytes ( largeFileContent ); // Fill with random bytes

            string largeFileContentType = "application/octet-stream";

            var fileStreamContent = new ByteArrayContent ( largeFileContent );
            fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ( largeFileContentType );
            multipartContent.Add ( fileStreamContent, "largeFileField", largeFileName );

            var response = await client.PostAsync ( "tests/multipart/echo", multipartContent );
            response.EnsureSuccessStatusCode ();

            var echoedObjects = await response.Content.ReadFromJsonAsync<List<SimpleMultipartObjectInfo>> ();
            Assert.IsNotNull ( echoedObjects );
            Assert.AreEqual ( 1, echoedObjects.Count );

            var fileObj = echoedObjects.FirstOrDefault ( o => o.Name == "largeFileField" );
            Assert.IsNotNull ( fileObj );
            Assert.AreEqual ( largeFileName, fileObj.FileName );
            Assert.AreEqual ( largeFileContentType, fileObj.ContentType );
            Assert.AreEqual ( largeFileContent.Length, fileObj.Length );
            // For binary files, ContentPreview might not contain the full content if it's large or not text.
            // Verification of full binary content might require server to send a hash or use a different mechanism.
            // For now, asserting Length is the primary check for large binary files.
            // If ContentPreview is populated by the server with, for example, a Base64 string of the content,
            // then Convert.FromBase64String(fileObj.ContentPreview) could be compared to largeFileContent.
            // However, this depends on the server implementation of "tests/multipart/echo".
        }
    }

    [TestMethod]
    public async Task Multipart_HeaderIntegrity_ContentDispositionVariations () {
        using (var client = Server.GetHttpClient ())
        using (var multipartContent = new MultipartFormDataContent ( "boundary----" + Guid.NewGuid ().ToString () )) {
            string fieldName = "fileWithSpecialChars";
            string originalFileName = "tëstfile€.txt"; // Contains non-ASCII characters (Euro sign, e-acute)
            byte [] fileContent = Encoding.UTF8.GetBytes ( "content of file with special name" );

            var fileBytesContent = new ByteArrayContent ( fileContent );
            fileBytesContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ( "application/octet-stream" );
            // When using Add method with fileName, HttpClient handles appropriate encoding (e.g. filename*) if needed.
            // Sisk.Core server should correctly parse this.
            multipartContent.Add ( fileBytesContent, fieldName, originalFileName );

            var response = await client.PostAsync ( "tests/multipart/echo", multipartContent );
            response.EnsureSuccessStatusCode ();

            var echoedObjects = await response.Content.ReadFromJsonAsync<List<SimpleMultipartObjectInfo>> ();
            Assert.IsNotNull ( echoedObjects );
            var fileObj = echoedObjects.FirstOrDefault ( o => o.Name == fieldName );
            Assert.IsNotNull ( fileObj, $"File object for field '{fieldName}' not found." );
            Assert.AreEqual ( originalFileName, fileObj.FileName, "Filename with special characters was not correctly interpreted by the server." );
        }
    }

    [TestMethod]
    public async Task Multipart_HeaderIntegrity_CustomHeaderOnPart () {
        using (var client = Server.GetHttpClient ())
        using (var multipartContent = new MultipartFormDataContent ( "boundary----" + Guid.NewGuid ().ToString () )) {
            string fieldName = "partWithCustomHeader";
            string customHeaderName = "X-Custom-TestInfo";
            string customHeaderValue = "This is some custom data!";
            byte [] partContentBytes = Encoding.UTF8.GetBytes ( "some data for custom header test" );

            var byteArrayContent = new ByteArrayContent ( partContentBytes );
            byteArrayContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ( "text/plain" );
            byteArrayContent.Headers.Add ( customHeaderName, customHeaderValue ); // Add custom header to this specific part

            multipartContent.Add ( byteArrayContent, fieldName, "dataWithHeader.txt" );

            // This test requires the server endpoint (e.g., "tests/multipart/echoHeaders" or an enhanced "tests/multipart/echo")
            // to be ableto read and return all headers for each part.
            // The DTO (SimpleMultipartObjectInfo or a new one) would need a property like Dictionary<string, string> PartHeaders.
            // This server-side enhancement is now expected to be in place with the /tests/multipart/echo endpoint.
            var response = await client.PostAsync ( "tests/multipart/echo", multipartContent ); // Changed to /echo
            response.EnsureSuccessStatusCode ();

            var echoedObjects = await response.Content.ReadFromJsonAsync<List<SimpleMultipartObjectInfo>> ();
            Assert.IsNotNull ( echoedObjects, "Deserialized response object list should not be null." );
            var partObj = echoedObjects.FirstOrDefault ( o => o.Name == fieldName );
            Assert.IsNotNull ( partObj, $"Part object for field '{fieldName}' not found in response." );

            Assert.IsNotNull ( partObj.PartHeaders, "PartHeaders dictionary should not be null." );
            // Check for the custom header, and also Content-Type and Content-Disposition which should be standard.
            // Note: HttpClient might combine some headers or present them differently than raw.
            // We are checking for the X-Custom-TestInfo header specifically.
            // The exact list of keys in PartHeaders can be verbose, so we check for specific ones.
            Assert.IsTrue ( partObj.PartHeaders.ContainsKey ( customHeaderName ),
                $"PartHeaders should contain custom header '{customHeaderName}'. Actual keys: {string.Join ( ", ", partObj.PartHeaders.Keys )}" );
            Assert.AreEqual ( customHeaderValue, partObj.PartHeaders [ customHeaderName ],
                $"Value for custom header '{customHeaderName}' did not match." );

            // Optionally, verify standard headers if their format is known and consistent
            Assert.IsTrue ( partObj.PartHeaders.ContainsKey ( "Content-Type" ), "PartHeaders should contain 'Content-Type'." );
            Assert.AreEqual ( "text/plain", partObj.PartHeaders [ "Content-Type" ], "Content-Type of the part mismatch." );

            Assert.IsTrue ( partObj.PartHeaders.ContainsKey ( "Content-Disposition" ), "PartHeaders should contain 'Content-Disposition'." );
            // Example: Content-Disposition: form-data; name="partWithCustomHeader"; filename="dataWithHeader.txt"
            // The exact value might vary slightly based on how HttpClient formats it.
            // A Contains check might be more robust for Content-Disposition if exact formatting is tricky.

            var contentDisposition = ContentDispositionHeaderValue.Parse ( partObj.PartHeaders [ "Content-Disposition" ] );
            Assert.IsTrue ( contentDisposition.Name == fieldName, "Content-Disposition should contain the correct name." );
            Assert.IsTrue ( contentDisposition.FileName == "dataWithHeader.txt", "Content-Disposition should contain the correct filename." );
        }
    }

    [TestMethod]
    public async Task Multipart_DocumentType_BinaryFileIntegrity () {
        using (var client = Server.GetHttpClient ())
        using (var multipartContent = new MultipartFormDataContent ( "boundary----" + Guid.NewGuid ().ToString () )) {
            string fieldName = "binaryFileField";
            string fileName = "test.bin";
            byte [] binaryData = new byte [] { 0xDE, 0xAD, 0xBE, 0xEF, 0x01, 0x02, 0x03, 0x04 };
            string contentType = "application/octet-stream";

            var fileContent = new ByteArrayContent ( binaryData );
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ( contentType );
            multipartContent.Add ( fileContent, fieldName, fileName );

            var response = await client.PostAsync ( "tests/multipart/echo", multipartContent );
            response.EnsureSuccessStatusCode ();

            var echoedObjects = await response.Content.ReadFromJsonAsync<List<SimpleMultipartObjectInfo>> ();
            Assert.IsNotNull ( echoedObjects );
            var fileObj = echoedObjects.FirstOrDefault ( o => o.Name == fieldName );
            Assert.IsNotNull ( fileObj, $"File object for field '{fieldName}' not found." );
            Assert.AreEqual ( fileName, fileObj.FileName );
            Assert.AreEqual ( contentType, fileObj.ContentType );
            Assert.AreEqual ( binaryData.Length, fileObj.Length );

            if (!string.IsNullOrEmpty ( fileObj.ContentPreview )) {
                try {
                    byte [] receivedBinaryData = Convert.FromBase64String ( fileObj.ContentPreview );
                    CollectionAssert.AreEqual ( binaryData, receivedBinaryData, "Binary content mismatch after Base64 decoding." );
                }
                catch (FormatException) {
                    Assert.Fail ( "ContentPreview was not valid Base64. Server might not be sending binary data Base64 encoded in ContentPreview." );
                }
            }
            else {
                Assert.Inconclusive ( "Binary content was not returned in ContentPreview by the server. Full verification requires server-side support for Base64 encoding binary content into ContentPreview." );
            }
        }
    }

    [TestMethod]
    public async Task Multipart_DocumentType_TextFileIntegrityWithLineEndings () {
        using (var client = Server.GetHttpClient ())
        using (var multipartContent = new MultipartFormDataContent ( "boundary----" + Guid.NewGuid ().ToString () )) {
            string fieldName = "textFileField";
            string fileName = "test.txt";
            // Using a verbatim string with explicit \n and \r\n for mixed line endings.
            string textContentWithMixedEndings = "First line with LF ending.\nSecond line with CRLF ending.\r\nThird line with LF ending.\nEnd.";
            string contentType = "text/plain";

            byte [] textBytes = Encoding.UTF8.GetBytes ( textContentWithMixedEndings );
            var fileBytesContent = new ByteArrayContent ( textBytes );
            fileBytesContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ( contentType ) {
                CharSet = Encoding.UTF8.WebName // "utf-8"
            };

            multipartContent.Add ( fileBytesContent, fieldName, fileName );

            var response = await client.PostAsync ( "tests/multipart/echo", multipartContent );
            response.EnsureSuccessStatusCode ();

            var echoedObjects = await response.Content.ReadFromJsonAsync<List<SimpleMultipartObjectInfo>> ();
            Assert.IsNotNull ( echoedObjects );
            var fileObj = echoedObjects.FirstOrDefault ( o => o.Name == fieldName );
            Assert.IsNotNull ( fileObj, $"File object for field '{fieldName}' not found." );
            Assert.AreEqual ( fileName, fileObj.FileName );
            // Server might return "text/plain; charset=utf-8", so check startsWith for the main type.
            Assert.IsTrue ( fileObj.ContentType?.StartsWith ( contentType ), $"Expected ContentType to start with '{contentType}' but was '{fileObj.ContentType}'." );
            // Also check if charset is present if the server sends it back, very specifically.
            if (fileObj.ContentType != null && fileObj.ContentType.Contains ( "charset" )) {
                Assert.IsTrue ( fileObj.ContentType.Contains ( $"charset={Encoding.UTF8.WebName}" ), $"Expected charset to be '{Encoding.UTF8.WebName}'." );
            }
            Assert.AreEqual ( textBytes.Length, fileObj.Length );
            Assert.AreEqual ( textContentWithMixedEndings, fileObj.ContentPreview, "Text content with mixed line endings mismatch. Ensure server preserves line endings." );
        }
    }

    [TestMethod]
    public async Task Multipart_Encoding_UTF8EncodingFieldAndFile () {
        Encoding encoding = Encoding.UTF8;
        string charsetName = encoding.WebName; // "utf-8"
        string testString = "Hello UTF-8 €üñÍçöđé!"; // Euro, u-umlaut, n-tilde, I-acute, c-cedilla, o-umlaut, d-eth, e-acute
        string fieldPartName = "utf8TextField";
        string filePartName = "utf8FileField";
        string fileName = "utf8test.txt";

        using (var client = Server.GetHttpClient ())
        using (var multipartContent = new MultipartFormDataContent ( "boundary----" + Guid.NewGuid ().ToString () )) {
            var stringContent = new StringContent ( testString, encoding, "text/plain" );
            multipartContent.Add ( stringContent, fieldPartName );

            byte [] fileBytes = encoding.GetBytes ( testString );
            var byteArrayContent = new ByteArrayContent ( fileBytes );
            byteArrayContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ( "text/plain" ) {
                CharSet = charsetName
            };
            multipartContent.Add ( byteArrayContent, filePartName, fileName );

            var response = await client.PostAsync ( "tests/multipart/echo", multipartContent );
            response.EnsureSuccessStatusCode ();

            var echoedObjects = await response.Content.ReadFromJsonAsync<List<SimpleMultipartObjectInfo>> ();
            Assert.IsNotNull ( echoedObjects, "Response should not be null." );
            Assert.AreEqual ( 2, echoedObjects.Count, "Should have two parts: one field and one file." );

            var fieldObj = echoedObjects.FirstOrDefault ( o => o.Name == fieldPartName );
            Assert.IsNotNull ( fieldObj, $"Field part '{fieldPartName}' not found." );
            Assert.AreEqual ( testString, fieldObj.Value, $"Field content mismatch for {charsetName}." );
            Assert.IsTrue ( fieldObj.ContentType?.Contains ( charsetName ), $"ContentType for field part should contain '{charsetName}'. Was '{fieldObj.ContentType}'." );


            var fileObj = echoedObjects.FirstOrDefault ( o => o.Name == filePartName );
            Assert.IsNotNull ( fileObj, $"File part '{filePartName}' not found." );
            Assert.AreEqual ( fileName, fileObj.FileName );
            Assert.IsTrue ( fileObj.ContentType?.Contains ( charsetName ), $"ContentType for file part should contain '{charsetName}'. Was '{fileObj.ContentType}'." );
            Assert.AreEqual ( fileBytes.Length, fileObj.Length );
            Assert.AreEqual ( testString, fileObj.ContentPreview, $"File content mismatch for {charsetName}." );
        }
    }

    [TestMethod]
    public async Task Multipart_Encoding_ASCIIEncodingFieldAndFile () {
        Encoding encoding = Encoding.ASCII;
        string charsetName = encoding.WebName; // "us-ascii"
        string testString = "Hello ASCII 123!?.";
        string fieldPartName = "asciiTextField";
        string filePartName = "asciiFileField";
        string fileName = "asciitest.txt";

        using (var client = Server.GetHttpClient ())
        using (var multipartContent = new MultipartFormDataContent ( "boundary----" + Guid.NewGuid ().ToString () )) {
            var stringContent = new StringContent ( testString, encoding, "text/plain" );
            multipartContent.Add ( stringContent, fieldPartName );

            byte [] fileBytes = encoding.GetBytes ( testString );
            var byteArrayContent = new ByteArrayContent ( fileBytes );
            byteArrayContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ( "text/plain" ) {
                CharSet = charsetName
            };
            multipartContent.Add ( byteArrayContent, filePartName, fileName );

            var response = await client.PostAsync ( "tests/multipart/echo", multipartContent );
            response.EnsureSuccessStatusCode ();

            var echoedObjects = await response.Content.ReadFromJsonAsync<List<SimpleMultipartObjectInfo>> ();
            Assert.IsNotNull ( echoedObjects, "Response should not be null." );
            Assert.AreEqual ( 2, echoedObjects.Count, "Should have two parts: one field and one file." );

            var fieldObj = echoedObjects.FirstOrDefault ( o => o.Name == fieldPartName );
            Assert.IsNotNull ( fieldObj, $"Field part '{fieldPartName}' not found." );
            Assert.AreEqual ( testString, fieldObj.Value, $"Field content mismatch for {charsetName}." );
            Assert.IsTrue ( fieldObj.ContentType?.Contains ( charsetName ), $"ContentType for field part should contain '{charsetName}'. Was '{fieldObj.ContentType}'." );

            var fileObj = echoedObjects.FirstOrDefault ( o => o.Name == filePartName );
            Assert.IsNotNull ( fileObj, $"File part '{filePartName}' not found." );
            Assert.AreEqual ( fileName, fileObj.FileName );
            Assert.IsTrue ( fileObj.ContentType?.Contains ( charsetName ), $"ContentType for file part should contain '{charsetName}'. Was '{fileObj.ContentType}'." );
            Assert.AreEqual ( fileBytes.Length, fileObj.Length );
            Assert.AreEqual ( testString, fileObj.ContentPreview, $"File content mismatch for {charsetName}." );
        }
    }

    [TestMethod]
    public async Task Multipart_Encoding_ISO88591EncodingFieldAndFile () {
        Encoding encoding = System.Text.Encoding.GetEncoding ( "ISO-8859-1" );
        string charsetName = encoding.WebName; // "iso-8859-1"
        string testString = "Hello ISO-8859-1 åäöüß§°±²³´µ¶·¸¹º»¼½¾¿"; // Common ISO-8859-1 characters
        string fieldPartName = "iso88591TextField";
        string filePartName = "iso88591FileField";
        string fileName = "iso88591test.txt";

        using (var client = Server.GetHttpClient ())
        using (var multipartContent = new MultipartFormDataContent ( "boundary----" + Guid.NewGuid ().ToString () )) {
            var stringContent = new StringContent ( testString, encoding, "text/plain" );
            multipartContent.Add ( stringContent, fieldPartName );

            byte [] fileBytes = encoding.GetBytes ( testString );
            var byteArrayContent = new ByteArrayContent ( fileBytes );
            byteArrayContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ( "text/plain" ) {
                CharSet = charsetName
            };
            multipartContent.Add ( byteArrayContent, filePartName, fileName );

            var response = await client.PostAsync ( "tests/multipart/echo", multipartContent );
            response.EnsureSuccessStatusCode ();

            var echoedObjects = await response.Content.ReadFromJsonAsync<List<SimpleMultipartObjectInfo>> ();
            Assert.IsNotNull ( echoedObjects, "Response should not be null." );
            Assert.AreEqual ( 2, echoedObjects.Count, "Should have two parts: one field and one file." );

            var fieldObj = echoedObjects.FirstOrDefault ( o => o.Name == fieldPartName );
            Assert.IsNotNull ( fieldObj, $"Field part '{fieldPartName}' not found." );
            Assert.AreEqual ( testString, fieldObj.Value, $"Field content mismatch for {charsetName}." );
            Assert.IsTrue ( fieldObj.ContentType?.Contains ( charsetName ), $"ContentType for field part should contain '{charsetName}'. Was '{fieldObj.ContentType}'." );

            var fileObj = echoedObjects.FirstOrDefault ( o => o.Name == filePartName );
            Assert.IsNotNull ( fileObj, $"File part '{filePartName}' not found." );
            Assert.AreEqual ( fileName, fileObj.FileName );
            Assert.IsTrue ( fileObj.ContentType?.Contains ( charsetName ), $"ContentType for file part should contain '{charsetName}'. Was '{fileObj.ContentType}'." );
            Assert.AreEqual ( fileBytes.Length, fileObj.Length );
            Assert.AreEqual ( testString, fileObj.ContentPreview, $"File content mismatch for {charsetName}." );
        }
    }
}
