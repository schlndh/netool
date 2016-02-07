using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using System;
using System.Text;
using Xunit;

namespace Tests.Http.Network.DataFormats.Http
{
    public class DechunkedStreamTests
    {
        [Fact]
        public void TestEmptyStream()
        {
            var stream = new DechunkedStream(EmptyData.Instance);
            Assert.Equal(0, stream.Length);
            Assert.Throws(typeof(IndexOutOfRangeException), delegate() { stream.ReadByte(0); });
            Assert.Throws(typeof(IndexOutOfRangeException), delegate() { stream.ReadBytesToBuffer(new byte[] { 5 }, 0, 1, 0); });
        }

        [Theory,
        InlineData("5\r\n01234\r\n1\r\n5\r\n0\r\n\r\n", "012345")]
        public void TestReadAllAndLength(string chunkedData, string dechunkedData)
        {
            var stream = new DechunkedStream(new ByteArray(ASCIIEncoding.ASCII.GetBytes(chunkedData)));
            Assert.Equal(dechunkedData, ASCIIEncoding.ASCII.GetString(stream.ReadBytes()));
            stream = new DechunkedStream(new ByteArray(ASCIIEncoding.ASCII.GetBytes(chunkedData)));
            Assert.Equal(dechunkedData.Length, stream.Length);
        }

        [Theory,
        InlineData("5\r\n01234\r\n1\r\n5\r\n0\r\n\r\n", 2, 3, "234"),
        InlineData("5\r\n01234\r\n1\r\n5\r\n0\r\n\r\n", 2, 4, "2345"),
        InlineData("5\r\n\r\n234\r\n1\r\n5\r\n0\r\n\r\n", 2, 4, "2345"),
        InlineData("5\r\n\r\n\r\n1\r\n0\r\n\r\n", 2, 3, "\r\n1"),
        ]
        public void TestPartialRead(string chunkedData, long start, int length, string dechunkedData)
        {
            var stream = new DechunkedStream(new ByteArray(ASCIIEncoding.ASCII.GetBytes(chunkedData)));
            Assert.Equal(dechunkedData, ASCIIEncoding.ASCII.GetString(stream.ReadBytes(start, length)));
        }

        [Theory,
        InlineData("5\r\n01234\r\n1\r\n5\r\n0\r\n\r\n", 2, "2")]
        public void TestReadByte(string chunkedData, long index, string dechunkedData)
        {
            var stream = new DechunkedStream(new ByteArray(ASCIIEncoding.ASCII.GetBytes(chunkedData)));
            Assert.Equal(dechunkedData, ASCIIEncoding.ASCII.GetString(new byte[] { stream.ReadByte(index) }));
        }
    }
}