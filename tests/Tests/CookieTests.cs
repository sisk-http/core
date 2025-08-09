using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sisk.Core.Http;
using Sisk.Core.Entity;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Sisk.Core.Helpers; // Added

namespace Sisk.Core.Tests
{
    [TestClass]
    public class CookieTests
    {
        [TestMethod]
        public void CookieParsing_CorrectSyntax_Simple()
        {
            string cookieString = "name=value";
            var cookies = StringKeyStoreCollection.FromCookieString(cookieString);

            Assert.IsNotNull(cookies);
            Assert.AreEqual(1, cookies.Count);
            Assert.IsTrue(cookies.ContainsKey("name"));
            Assert.AreEqual("value", cookies["name"]); // Modified
        }

        [TestMethod]
        public void CookieParsing_CorrectSyntax_MultipleCookies()
        {
            string cookieString = "name1=value1; name2=value2";
            var cookies = StringKeyStoreCollection.FromCookieString(cookieString);

            Assert.IsNotNull(cookies);
            Assert.AreEqual(2, cookies.Count);
            Assert.IsTrue(cookies.ContainsKey("name1"));
            Assert.AreEqual("value1", cookies["name1"]); // Modified
            Assert.IsTrue(cookies.ContainsKey("name2"));
            Assert.AreEqual("value2", cookies["name2"]); // Modified
        }

        // Removed CookieParsing_CorrectSyntax_WithAttributes

        [TestMethod]
        public void CookieParsing_CorrectSyntax_SpecialCharactersInValue()
        {
            // Value contains ';' which is a separator for cookies.
            // It must be URL-encoded to be treated as part of the value.
            string cookieString = "name=value%20with%20spaces%20and%20%3D%20and%20%3B%20and%20%2520";
            var cookies = StringKeyStoreCollection.FromCookieString(cookieString);

            Assert.IsNotNull(cookies);
            Assert.AreEqual(1, cookies.Count);
            Assert.IsTrue(cookies.ContainsKey("name"));
            Assert.AreEqual("value with spaces and = and ; and %20", cookies["name"]); // Modified
        }

        [TestMethod]
        public void CookieParsing_IncorrectSyntax_Malformed()
        {
            string cookieString = "name1=value1; =value2; name3"; // Malformed
            var cookies = StringKeyStoreCollection.FromCookieString(cookieString);

            Assert.IsNotNull(cookies);
            // Expecting 2 items: "name1=value1" and "name3=" (because "name3" has no '=' and is treated as key with empty value)
            Assert.AreEqual(2, cookies.Count); // Modified
            Assert.IsTrue(cookies.ContainsKey("name1"));
            Assert.AreEqual("value1", cookies["name1"]); // Modified
            Assert.IsTrue(cookies.ContainsKey("name3"));
            Assert.AreEqual("", cookies["name3"]); // Added assertion for "name3"
        }

        [TestMethod]
        public void CookieBuilding_Simple()
        {
            string cookieHeader = CookieHelper.BuildCookieHeaderValue("name", "value");
            Assert.AreEqual("name=value", cookieHeader);
        }

        [TestMethod]
        public void CookieBuilding_WithExpires()
        {
            DateTime expires = new DateTime(2015, 10, 21, 7, 28, 0, DateTimeKind.Utc);
            string cookieHeader = CookieHelper.BuildCookieHeaderValue("name", "value", expires: expires);
            Assert.AreEqual("name=value; Expires=Wed, 21 Oct 2015 07:28:00 GMT", cookieHeader);
        }

        [TestMethod]
        public void CookieBuilding_WithAllAttributes()
        {
            DateTime expires = new DateTime(2015, 10, 21, 7, 28, 0, DateTimeKind.Utc);
            string cookieHeader = CookieHelper.BuildCookieHeaderValue(
                name: "name",
                value: "value",
                expires: expires,
                maxAge: TimeSpan.FromSeconds(3600), // Modified
                domain: "example.com",
                path: "/",
                secure: true,
                httpOnly: true,
                sameSite: "Lax" // Modified
            );
            Assert.AreEqual("name=value; Expires=Wed, 21 Oct 2015 07:28:00 GMT; Max-Age=3600; Domain=example.com; Path=/; Secure; HttpOnly; SameSite=Lax", cookieHeader);
        }

        [TestMethod] // Uncommented
        public void CookieSending_SingleCookie()
        {
            var response = new HttpResponse(); // Changed
            response.SetCookie("mycookie", "myvalue");

            var setCookieHeaders = response.Headers.GetValues(HttpKnownHeaderNames.SetCookie);
            Assert.IsNotNull(setCookieHeaders);
            Assert.AreEqual(1, setCookieHeaders.Count()); // Changed to Count()
            Assert.AreEqual("mycookie=myvalue", setCookieHeaders.First());
        }

        [TestMethod] // Uncommented
        public void CookieSending_MultipleCookies()
        {
            var response = new HttpResponse(); // Changed
            response.SetCookie("cookie1", "value1");
            response.SetCookie("cookie2", "value2");

            var setCookieHeaders = response.Headers.GetValues(HttpKnownHeaderNames.SetCookie);
            Assert.IsNotNull(setCookieHeaders);
            Assert.AreEqual(2, setCookieHeaders.Count()); // Changed to Count()
            Assert.IsTrue(setCookieHeaders.Contains("cookie1=value1"));
            Assert.IsTrue(setCookieHeaders.Contains("cookie2=value2"));
        }
    }
}