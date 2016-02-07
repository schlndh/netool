using System;
using System.Text;
using Xunit;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;

namespace Tests.Http.Network.DataFormats.Http
{
    public class ChunkedEncodingTests
    {
        [Fact]
        public void TestDecodeOneSimple()
        {
            var str = "A\r\n1234567890\r\n";
            str = str + str;
            var stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(str));
            var info = ChunkedDecoder.DecodeOneChunk(stream);
            Assert.Equal(str.Length/2, info.ChunkLength);
            Assert.Equal(10, info.DataLength);
            Assert.Equal('5', Convert.ToChar(stream.ReadByte(info.DataStart + 4)));
        }

        [Fact]
        public void TestDecodeOneLast()
        {
            var str = "0;aa=\"bb\"\r\nheader: value\r\nheader2: value2\r\n\r\n";
            var stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(str));
            var info = ChunkedDecoder.DecodeOneChunk(stream);
            Assert.Equal(str.Length, info.ChunkLength);
            Assert.Equal(0, info.DataLength);
        }

        [Fact]
        public void TestDecodeOneInvalid()
        {
            // invalid end
            var str = "2\r\nxabc";
            var stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(str));
            Assert.Throws(typeof(InvalidChunkException), delegate() { ChunkedDecoder.DecodeOneChunk(stream); });
        }

        [Fact]
        public void TestDecodeOnePartial()
        {
            var str = "A";
            var stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(str));
            Assert.Throws(typeof(PartialChunkException), delegate() { ChunkedDecoder.DecodeOneChunk(stream); });
            str = "A\r";
            stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(str));
            Assert.Throws(typeof(PartialChunkException), delegate() { ChunkedDecoder.DecodeOneChunk(stream); });
            str = "A\r\n";
            stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(str));
            Assert.Throws(typeof(PartialChunkException), delegate() { ChunkedDecoder.DecodeOneChunk(stream); });
            str = "A\r\n1234567890\r";
            stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(str));
            Assert.Throws(typeof(PartialChunkException), delegate() { ChunkedDecoder.DecodeOneChunk(stream); });

            // last chunk without end
            str = "0\r\nheader: value\r\n";
            stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(str));
            Assert.Throws(typeof(PartialChunkException), delegate() { ChunkedDecoder.DecodeOneChunk(stream); });
        }
    }
}
