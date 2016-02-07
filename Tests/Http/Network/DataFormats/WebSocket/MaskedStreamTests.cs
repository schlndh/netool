using Netool.Network.DataFormats;
using Netool.Network.DataFormats.WebSocket;
using Xunit;

namespace Tests.Http.Network.DataFormats.WebSocket
{
    public class MaskedStreamTests
    {
        [Fact]
        public void TestReadByte()
        {
            var mask = new byte[] { 0, 1, 128, 255 };
            var innerStream = new ByteArray(new byte[] { 0, 1, 129, 5, 15, 16, 17, 0 });
            var stream = new MaskedStream(mask, innerStream);
            Assert.Equal(innerStream.Length, stream.Length);
            Assert.Equal(0, stream.ReadByte(0));
            Assert.Equal(0, stream.ReadByte(1));
            Assert.Equal(1, stream.ReadByte(2));
            Assert.Equal(250, stream.ReadByte(3));
            Assert.Equal(15, stream.ReadByte(4));
            Assert.Equal(17, stream.ReadByte(5));
            Assert.Equal(145, stream.ReadByte(6));
            Assert.Equal(255, stream.ReadByte(7));
        }

        [Fact]
        public void TestReadBytesToBuffer()
        {
            var mask = new byte[] { 0, 1, 128, 255 };
            var innerStream = new ByteArray(new byte[] { 0, 1, 129, 5, 15, 16, 17, 0 });
            var stream = new MaskedStream(mask, innerStream);
            var buffer = new byte[6];
            stream.ReadBytesToBuffer(buffer, 1, buffer.Length - 1, 1);
            Assert.Equal(0, buffer[1]);
            Assert.Equal(1, buffer[2]);
            Assert.Equal(250, buffer[3]);
            Assert.Equal(15, buffer[4]);
            Assert.Equal(17, buffer[5]);
        }
    }
}