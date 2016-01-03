using Netool.Network.DataFormats;
using System.IO;
using Xunit;

namespace Tests.Network.DataFormats
{
    public class FromUnseekableStreamTests
    {
        [Fact]
        public void TestGetLength()
        {
            FromUnseekableStream.StreamFactory factory = delegate() { return new MemoryStream(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }); };
            var streamWrapper = new FromUnseekableStream(factory, 5);
            var stream = factory();
            Assert.Equal(stream.Length, streamWrapper.Length);
        }

        [Fact]
        public void TestReadByte()
        {
            int called = 0;
            FromUnseekableStream.StreamFactory factory = delegate() { ++called; return new MemoryStream(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }); };
            var streamWrapper = new FromUnseekableStream(factory, 5);
            Assert.Equal(0, streamWrapper.ReadByte(0));
            Assert.Equal(1, streamWrapper.ReadByte(1));

            // this should be cached
            Assert.Equal(0, streamWrapper.ReadByte(0));

            Assert.Equal(9, streamWrapper.ReadByte(9));

            // this should result in factory being called again
            Assert.Equal(1, streamWrapper.ReadByte(1));
            // one for Length, 2 for streamWrapper
            Assert.Equal(3, called);
        }

        [Fact]
        public void TestReadBytesToBuffer()
        {
            int called = 0;
            FromUnseekableStream.StreamFactory factory = delegate() { ++called; return new MemoryStream(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }); };
            var streamWrapper = new FromUnseekableStream(factory, 5);
            var buffer = new byte[10];
            streamWrapper.ReadBytesToBuffer(buffer, 0, 3);
            Assert.Equal(0, buffer[0]);
            Assert.Equal(2, buffer[2]);
            streamWrapper.ReadBytesToBuffer(buffer, 3, 2, 3);
            Assert.Equal(2, buffer[2]);
            Assert.Equal(3, buffer[3]);
            Assert.Equal(4, buffer[4]);
            streamWrapper.ReadBytesToBuffer(buffer, 5, 2, 5);
            Assert.Equal(5, buffer[5]);
            Assert.Equal(6, buffer[6]);

            // this should be cached
            streamWrapper.ReadBytesToBuffer(buffer, 2, 3, 0);
            Assert.Equal(2, buffer[0]);
            Assert.Equal(3, buffer[1]);
            Assert.Equal(4, buffer[2]);

            // this should be uncached
            streamWrapper.ReadBytesToBuffer(buffer, 0, 2, 7);
            Assert.Equal(0, buffer[7]);
            Assert.Equal(1, buffer[8]);

            // this should be partially cached and not create a new stream
            streamWrapper.ReadBytesToBuffer(buffer, 0, 7, 0);
            Assert.Equal(0, buffer[0]);
            Assert.Equal(1, buffer[1]);
            Assert.Equal(6, buffer[6]);

            Assert.Equal(3, called);
        }
    }
}