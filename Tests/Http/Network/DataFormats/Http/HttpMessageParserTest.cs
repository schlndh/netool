using Netool.Logging;
using Netool.Network.DataFormats;
using Netool.Network.Http;
using System;
using System.Text;
using Xunit;

namespace Tests.Http.Network.DataFormats.Http
{
    public class HttpMessageParserTest : IDisposable
    {
        private InstanceLogger logger;

        public HttpMessageParserTest()
        {
            logger = new InstanceLogger();
        }

        void IDisposable.Dispose()
        {
            logger.Close();
            logger.DeleteFile();
        }

        [Fact]
        public void TestChunked_WithTrailer()
        {
            var header1 = "HTTP/1.1 200 OK\r\n" +
                "Date: Mon, 27 Jul 2009 12:28:53 GMT\r\n" +
                "Server: Apache\r\n" +
                "Last-Modi";
            var header2 = "fied: Wed, 22 Jul 2009 19:15:56 GMT\r\n" +
                "Transfer-Encoding: chunked\r\n" +
                "Content-Type: text/plain\r\n\r\n";
            var body = "5\r\nABCDE\r\n0\r\n";
            var tailHeader1 = "Header1:Val";
            var tailHeader2 = "ue1\r\nHeader2: Value2\r\n\r\n";
            var parser = new HttpMessageParser(logger, true);
            Assert.Null(parser.Receive(new ByteArray(header1, Encoding.ASCII)));
            Assert.Null(parser.Receive(new ByteArray(header2, Encoding.ASCII)));
            Assert.Null(parser.Receive(new ByteArray(body, Encoding.ASCII)));
            Assert.Null(parser.Receive(new ByteArray(tailHeader1, Encoding.ASCII)));
            var response = parser.Receive(new ByteArray(tailHeader2, Encoding.ASCII));
            Assert.NotNull(response);
            Assert.Equal("Value1", response.Headers["Header1"]);
            Assert.Equal("Value2", response.Headers["Header2"]);
            Assert.Equal("Apache", response.Headers["Server"]);
            Assert.Equal(body + tailHeader1 + tailHeader2, Encoding.ASCII.GetString(response.BodyData.ReadBytes()));
        }

        [Fact]
        public void TestChunked_WithoutTrailer()
        {
            var header1 = "HTTP/1.1 200 OK\r\n" +
                "Date: Mon, 27 Jul 2009 12:28:53 GMT\r\n" +
                "Server: Apache\r\n" +
                "Last-Modi";
            var header2 = "fied: Wed, 22 Jul 2009 19:15:56 GMT\r\n" +
                "Transfer-Encoding: chunked\r\n" +
                "Content-Type: text/plain\r\n\r\n";
            var body = "5\r\nABCDE\r\n0\r\n";
            var bodyEnd = "\r\n";
            var parser = new HttpMessageParser(logger, true);
            Assert.Null(parser.Receive(new ByteArray(header1, Encoding.ASCII)));
            Assert.Null(parser.Receive(new ByteArray(header2, Encoding.ASCII)));
            Assert.Null(parser.Receive(new ByteArray(body, Encoding.ASCII)));
            var response = parser.Receive(new ByteArray(bodyEnd, Encoding.ASCII));
            Assert.NotNull(response);
            Assert.Equal("Apache", response.Headers["Server"]);
            Assert.Equal(body + bodyEnd, Encoding.ASCII.GetString(response.BodyData.ReadBytes()));
        }

        [Fact]
        public void TestChunked_ContentLength()
        {
            var header1 = "HTTP/1.1 200 OK\r\n" +
                "Date: Mon, 27 Jul 2009 12:28:53 GMT\r\n" +
                "Server: Apache\r\n" +
                "Last-Modi";
            var header2 = "fied: Wed, 22 Jul 2009 19:15:56 GMT\r\n" +
                "Content-Length: 5\r\n" +
                "Content-Type: text/plain\r\n\r\n";
            var body = "ABCDE";
            var parser = new HttpMessageParser(logger, true);
            Assert.Null(parser.Receive(new ByteArray(header1, Encoding.ASCII)));
            Assert.Null(parser.Receive(new ByteArray(header2, Encoding.ASCII)));
            var response = parser.Receive(new ByteArray(body, Encoding.ASCII));
            Assert.NotNull(response);
            Assert.Equal("Apache", response.Headers["Server"]);
            Assert.Equal(body, Encoding.ASCII.GetString(response.BodyData.ReadBytes()));
        }
    }
}