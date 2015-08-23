using System;
using System.Collections.Generic;
using Xunit;
using Netool.Network.DataFormats.Http;

namespace Tests.Http.Network.DataFormats
{
    public class HttpDataTests
    {
        [Fact]
        public void TestCaseInsensitiveHeaders()
        {
            var builder = new HttpData.Builder();
            builder.AddHeader("Content-Length", "20");
            var data = builder.CreateAndClear();
            Assert.Equal("20", data.Headers["Content-Length"]);
            Assert.Equal("20", data.Headers["content-length"]);
            Assert.Equal("20", data.Headers["content-LENGTH"]);
            Assert.Equal("20", data.Headers["CONTENT-LENGTH"]);
            Assert.Throws(typeof(KeyNotFoundException), delegate() { var x = data.Headers["ContentLength"]; });
        }

        [Fact]
        public void TestStatusLine()
        {
            var builder = new HttpData.Builder();
            builder.IsRequest = true;
            builder.Method = HttpRequestMethod.GET;
            builder.HttpVersion = "1.1";
            builder.RequestTarget = "/index.html";
            Assert.Equal("GET /index.html HTTP/1.1", builder.CreateAndClear().StatusLine);

            builder = new HttpData.Builder();
            builder.IsRequest = false;
            builder.HttpVersion = "1.1";
            builder.StatusCode = 200;
            builder.ReasonPhrase = "OK";
            Assert.Equal("HTTP/1.1 200 OK", builder.CreateAndClear().StatusLine);
        }
    }
}
