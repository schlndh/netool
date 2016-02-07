using Xunit;
using Netool.ChannelDrivers;
using Netool.Logging;
using Netool.Network;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace Tests.Http.Network.DataFormats.Http
{
    public class HttpHeaderParserTests
    {

        [Fact]
        public void TestValidResponse()
        {
            var header = "HTTP/1.1 200 OK\r\n" +
                "Date: Mon, 27 Jul 2009 12:28:53 GMT\r\n" +
                "Server: Apache\r\n" +
                "Last-Modified: Wed, 22 Jul 2009 19:15:56 GMT\r\n" +
                "ETag: \"34aa387-d-1568eb00\"\r\n" +
                "Accept-Ranges: bytes\r\n" +
                "Content-Length: 0\r\n" +
                "Vary: Accept-Encoding\r\n" +
                "Content-Type: text/plain\r\n\r\n";

            var parser = new HttpHeaderParser();
            var stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(header));
            parser.Parse(stream, true);
            Assert.Equal("Mon, 27 Jul 2009 12:28:53 GMT", parser.GetHeader("Date"));
            Assert.Equal("Apache", parser.GetHeader("Server"));
            Assert.Equal("\"34aa387-d-1568eb00\"", parser.GetHeader("ETag"));

            var data = parser.Create(stream, EmptyData.Instance);
        }

        [Fact]
        public void TestValidPartialKeyResponse()
        {
            var header1 = "HTTP/1.1 200 OK\r\n" +
                "Date: Mon, 27 Jul 2009 12:28:53 GMT\r\n" +
                "Server: Apache\r\n" +
                "Last-Modi";
            var header2 = header1 + "fied: Wed, 22 Jul 2009 19:15:56 GMT\r\n" +
                "ETag: \"34aa387-d-1568eb00\"\r\n" +
                "Accept-Ranges: bytes\r\n" +
                "Content-Length: 0\r\n" +
                "Vary: Accept-Encoding\r\n" +
                "Content-Type: text/plain\r\n\r\n";
            var parser = new HttpHeaderParser();
            var stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(header1));
            parser.Parse(stream, true);
            Assert.Equal("Mon, 27 Jul 2009 12:28:53 GMT", parser.GetHeader("Date"));
            Assert.Equal("Apache", parser.GetHeader("Server"));
            Assert.Null(parser.GetHeader("Last-Modi"));
            Assert.Null(parser.GetHeader("Last-Modified"));
            stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(header2));
            parser.Parse(stream, true);
            Assert.Equal("Apache", parser.GetHeader("Server"));
            Assert.Equal("Wed, 22 Jul 2009 19:15:56 GMT", parser.GetHeader("Last-Modified"));
            Assert.Equal("\"34aa387-d-1568eb00\"", parser.GetHeader("ETag"));

            var data = parser.Create(stream, EmptyData.Instance);
        }

        [Fact]
        public void TestValidPartialValueResponse()
        {
            var header1 = "HTTP/1.1 200 OK\r\n" +
                "Date: Mon, 27 Jul 2009 12:28:53 GMT\r\n" +
                "Server: Apache\r\n" +
                "Last-Modified: Wed, 22 Jul 2";
            var header2 = header1 + "009 19:15:56 GMT\r\n" +
                "ETag: \"34aa387-d-1568eb00\"\r\n" +
                "Accept-Ranges: bytes\r\n" +
                "Content-Length: 0\r\n" +
                "Vary: Accept-Encoding\r\n" +
                "Content-Type: text/plain\r\n\r\n";
            var parser = new HttpHeaderParser();
            var stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(header1));
            parser.Parse(stream, true);
            Assert.Equal("Mon, 27 Jul 2009 12:28:53 GMT", parser.GetHeader("Date"));
            Assert.Equal("Apache", parser.GetHeader("Server"));
            Assert.Null(parser.GetHeader("Last-Modi"));
            Assert.Null(parser.GetHeader("Last-Modified"));
            stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(header2));
            parser.Parse(stream, true);
            Assert.Equal("Apache", parser.GetHeader("Server"));
            Assert.Equal("Wed, 22 Jul 2009 19:15:56 GMT", parser.GetHeader("Last-Modified"));
            Assert.Equal("\"34aa387-d-1568eb00\"", parser.GetHeader("ETag"));

            var data = parser.Create(stream, EmptyData.Instance);
        }

        [Theory,
        InlineData("GET /index.html?a=&b=c&d[0]=c%20 HTTP/1.1", true),
        InlineData("HTTP/1.1 200 OK", false)
        ]
        public void TestParseStartLine(string line, bool isRequest)
        {
            var builder = HttpHeaderParser.ParseStartLine(line + "\r\n");
            Assert.Equal(isRequest, builder.IsRequest);
        }
    }
}