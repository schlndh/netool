using System;
using System.Text;
using Xunit;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;

namespace Tests.Http.Network.DataFormats
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
            Assert.Equal(str.Length/2, info.BytesRead);
            Assert.Equal(10, info.Data.Length);
            Assert.Equal('5', Convert.ToChar(info.Data.ReadByte(4)));
        }

        [Fact]
        public void TestDecodeOneLast()
        {
            var str = "0;aa=\"bb\"\r\nheader: value\r\nheader2: value2\r\n\r\n";
            var stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(str));
            var info = ChunkedDecoder.DecodeOneChunk(stream);
            Assert.Equal(str.Length, info.BytesRead);
            Assert.Equal(0, info.Data.Length);
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

        [Fact]
        public void TestDecodeValid()
        {
            var parts = new string[] { "5\r", "\nABC", "DE\r\n3\r\nFGI\r\n", "0\r\n\r\n" };
            var decoder = new ChunkedDecoder();
            var stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(parts[0]));
            var info = decoder.Decode(stream);
            Assert.False(info.Finished);
            Assert.Equal(0, info.DecodedData.Length);

            stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(parts[1]));
            info = decoder.Decode(stream);
            Assert.False(info.Finished);
            Assert.Equal(0, info.DecodedData.Length);

            stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(parts[2]));
            info = decoder.Decode(stream);
            Assert.False(info.Finished);
            Assert.Equal(8, info.DecodedData.Length);
            Assert.Equal("ABCDEFGI", ASCIIEncoding.ASCII.GetString(info.DecodedData.ReadBytes()));

            stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(parts[3]));
            info = decoder.Decode(stream);
            Assert.True(info.Finished);
            Assert.Equal(0, info.DecodedData.Length);
        }

        [Fact]
        public void TestDecodeInvalid()
        {
            var str = "5\r\n12345\r\nxxx\r\n\r\n";
            var decoder = new ChunkedDecoder();
            var stream = new ByteArray(ASCIIEncoding.ASCII.GetBytes(str));
            Assert.Throws(typeof(InvalidChunkException), delegate() { decoder.Decode(stream); });
        }
    }
}
