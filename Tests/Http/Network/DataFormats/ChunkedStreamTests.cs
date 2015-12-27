using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using System.Text;
using Xunit;

namespace Tests.Http.Network.DataFormats
{
    public class ChunkedStreamTests
    {
        [Theory,
        InlineData("ABCDEF", 2, "2\r\nAB\r\n2\r\nCD\r\n2\r\nEF\r\n0\r\n\r\n"),
        InlineData("ABCDE", 2, "2\r\nAB\r\n2\r\nCD\r\n1\r\nE\r\n0\r\n\r\n"),
        InlineData("ABCDEABCDEFF", 10, "A\r\nABCDEABCDE\r\n2\r\nFF\r\n0\r\n\r\n"),
        ]
        public void TestReadBytes(string input, int chunkSize, string expected)
        {
            var inner = new ByteArray(ASCIIEncoding.ASCII.GetBytes(input));
            var chunked = new ChunkedStream(inner, chunkSize);
            Assert.Equal(expected.Length, chunked.Length);
            var result = ASCIIEncoding.ASCII.GetString(chunked.ReadBytes());
            Assert.Equal(expected, result);
            TestReadByte(input, chunkSize, expected);
        }

        private void TestReadByte(string input, int chunkSize, string expected)
        {
            var inner = new ByteArray(ASCIIEncoding.ASCII.GetBytes(input));
            var chunked = new ChunkedStream(inner, chunkSize);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.Equal(ASCIIEncoding.ASCII.GetBytes(expected.Substring(i, 1))[0], chunked.ReadByte(i));
            }
        }

        [Fact]
        public void TestEmptyData()
        {
            var chunked = new ChunkedStream(EmptyData.Instance, 10);
            Assert.Equal(5, chunked.Length);
            var result = ASCIIEncoding.ASCII.GetString(chunked.ReadBytes());
            Assert.Equal("0\r\n\r\n", result);
        }
    }
}