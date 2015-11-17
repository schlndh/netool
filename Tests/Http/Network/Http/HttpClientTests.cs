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
    public class HttpClientTests : IDisposable
    {
        private InstanceLogger logger;

        public HttpClientTests()
        {
            logger = new InstanceLogger();
        }

        void IDisposable.Dispose()
        {
            logger.DeleteFile();
        }

        [Fact]
        public void TestReceiveValidResponse()
        {
            var receivedList = new List<HttpData>();
            var innerChannel = new TestClientChannel();
            var httpChannel = new HttpClientChannel(innerChannel, logger);
            httpChannel.ResponseReceived += delegate(object sender, DataEventArgs e) {
                Assert.NotNull(e.Data);
                Assert.IsType(typeof(HttpData), e.Data);
                receivedList.Add(e.Data as HttpData);
            };
            // all at once with content length
            var response = "HTTP/1.1 200 OK\r\n" +
                "Header: value\r\n" +
                "Header2: value2\r\n" +
                "Content-Length: 10\r\n\r\n0123456789";
            IDataStream stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(response));
            innerChannel.Receive(stream);
            Assert.Equal(1, receivedList.Count);
            var data = receivedList[0];
            Assert.Equal("1.1", data.Version);
            Assert.Equal(200, data.Code);
            Assert.Equal("OK", data.ReasonPhrase);
            Assert.Equal("value2", data.Headers["Header2"]);
            string payload = ASCIIEncoding.ASCII.GetString(data.BodyData.ReadBytes());
            Assert.Equal("0123456789", payload);

            // chunked and split into multiple packets
            var responseParts = new string[]
            {
                "HTTP/1.1 200 OK\r\n",
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
            Assert.Equal("0123456789", payload);

            // data delimited by closing the channel
            response = "HTTP/1.1 200 OK\r\n" +
                "Header: value\r\n" +
                "Header2: value2\r\n" +
                "\r\n0123456789";
            stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(response));
            innerChannel.Receive(stream);
            Assert.Equal(2, receivedList.Count);
            innerChannel.Close();
            Assert.Equal(3, receivedList.Count);
            data = receivedList[2];
            Assert.Equal("1.1", data.Version);
            Assert.Equal(200, data.Code);
            Assert.Equal("OK", data.ReasonPhrase);
            Assert.Equal("value2", data.Headers["Header2"]);
            payload = ASCIIEncoding.ASCII.GetString(data.BodyData.ReadBytes());
            Assert.Equal("0123456789", payload);
        }

        [Theory,
        InlineData("HTTP/1.1 200 OK\r\n    Content-Length: 0\r\n\r\n"),
        InlineData("aalasksdk\r\n\r\n"),
        InlineData("\r\nHTTP/1.1 200 OK\r\nContent-Length: 0\r\n\r\n"),
        InlineData("HTTP/1.1 200 OK\r\n: 0\r\n\r\n"),
        InlineData("GET /index.html HTTP/1.1\r\nContent-Length: 0\r\n\r\n"),
        ]
        public void TestReceiveInvalidResponse(string response)
        {
            var innerChannel = new TestClientChannel();
            var httpChannel = new HttpClientChannel(innerChannel, logger);
            httpChannel.ResponseReceived += delegate(object sender, DataEventArgs e)
            {
                // this is not a valid response
                Assert.True(false);
            };
            IDataStream stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(response));
            innerChannel.Receive(stream);
            innerChannel.Close();
        }
    }
}
