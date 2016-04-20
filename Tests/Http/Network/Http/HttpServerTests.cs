using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Netool.Logging;
using Netool.Network;
using Netool.Network.Http;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;

namespace Tests.Http.Network.Http
{
    public class HttpServerTests : IDisposable
    {
        private InstanceLogger logger;

        public HttpServerTests()
        {
            logger = new InstanceLogger();
        }

        void IDisposable.Dispose()
        {
            logger.DeleteFile();
        }

        [Fact]
        public void TestReceiveValidRequest()
        {
            var receivedList = new List<HttpData>();
            var innerChannel = new TestServerChannel();
            var httpChannel = new HttpServerChannel(innerChannel, logger);
            httpChannel.RequestReceived += delegate(object sender, DataEventArgs e) {
                Assert.NotNull(e.Data);
                Assert.IsType(typeof(HttpData), e.Data);
                receivedList.Add(e.Data as HttpData);
            };
            // all at once with content length
            var response = "GET /index.html HTTP/1.1\r\n" +
                "Header: value\r\n" +
                "Header2: value2\r\n" +
                "Content-Length: 10\r\n\r\n0123456789";
            IDataStream stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(response));
            innerChannel.Receive(stream);
            Assert.Equal(1, receivedList.Count);
            var data = receivedList[0];
            Assert.Equal("1.1", data.Version);
            Assert.Equal(HttpRequestMethod.GET, data.Method);
            Assert.Equal("/index.html", data.RequestTarget);
            Assert.Equal("value2", data.Headers["Header2"]);
            string payload = ASCIIEncoding.ASCII.GetString(data.BodyData.ReadBytes());
            Assert.Equal("0123456789", payload);

            // chunked and split into multiple packets
            var responseParts = new string[]
            {
                "GET /index.html HTTP/1.1\r\n",
                "Header: value\r\nHeade",
                "r2: value2\r\n",
                "Transfer-Encoding: chunked\r\n\r\n",
                "5\r\n01",
                "234\r\n5\r\n567",
                "89\r\n0\r\n\r\n"
            };
            int c = 0;
            foreach(var r in responseParts)
            {
                c++;
                stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(r));
                innerChannel.Receive(stream);
                if(c < responseParts.Length) Assert.Equal(1, receivedList.Count);
            }
            Assert.Equal(2, receivedList.Count);
            data = receivedList[1];
            Assert.Equal("value2", data.Headers["Header2"]);
            payload = ASCIIEncoding.ASCII.GetString(data.BodyData.ReadBytes());
            Assert.Equal("5\r\n01234\r\n5\r\n56789\r\n0\r\n\r\n", payload);

            // neither transfer-encoding nor content-length is specified -> assume empty body data
            response = "GET /index.html HTTP/1.1\r\n" +
                "Header: value\r\n" +
                "Header2: value2\r\n" +
                "\r\n0123456789";
            stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(response));
            innerChannel.Receive(stream);
            Assert.Equal(3, receivedList.Count);
            data = receivedList[2];
            Assert.Equal("1.1", data.Version);
            Assert.Equal(HttpRequestMethod.GET, data.Method);
            Assert.Equal("/index.html", data.RequestTarget);
            Assert.Equal("value2", data.Headers["Header2"]);
            Assert.Equal(0, data.BodyData.Length);
        }

        [Theory,
        InlineData("GET /index.html HTTP/1.1\r\n    Content-Length: 0\r\n\r\n"),
        InlineData("aalasksdk\r\n\r\n"),
        InlineData("\r\nGET /index.html HTTP/1.1\r\nContent-Length: 0\r\n\r\n"),
        InlineData("GET /index.html HTTP/1.1\r\n: 0\r\n\r\n"),
        InlineData("HTTP/1.1 200 OK\r\nHeader: value\r\n\r\n")
        ]
        public void TestReceiveInvalidResponse(string response)
        {
            var innerChannel = new TestServerChannel();
            var httpChannel = new HttpServerChannel(innerChannel, logger);
            int i = 0;
            httpChannel.RequestReceived += delegate(object sender, DataEventArgs e)
            {
                ++i;
                Assert.IsNotType<HttpData>(e.Data);
            };
            Assert.Equal(0, i);
            IDataStream stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(response));
            innerChannel.Receive(stream);
            innerChannel.Close();
            Assert.Equal(1, i);
        }
    }
}
