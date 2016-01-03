using Netool.Network.DataFormats.Utils;
using System;
using Xunit;

namespace Tests.Network.DataFormats.Utils
{
    public class ByteCacheTests
    {
        [Fact]
        public void TestEmpty()
        {
            var cache = new ByteCache(256);
            byte res;
            Assert.False(cache.TryReadByte(0, out res));
            Assert.False(cache.TryReadByte(1, out res));
        }

        [Fact]
        public void TestFilled()
        {
            ByteCache.FillCache callback = delegate(byte[] buffer, out long cacheStart, out int cacheLength)
            {
                cacheStart = 5;
                cacheLength = 4;
                buffer[0] = 5;
                buffer[1] = 6;
                buffer[2] = 7;
                buffer[3] = 8;
            };
            var cache = new ByteCache(256);
            byte res;
            cache.Cache(callback);
            Assert.Equal(5, cache.Start);
            Assert.Equal(4, cache.Length);
            Assert.False(cache.TryReadByte(0, out res));
            Assert.False(cache.TryReadByte(4, out res));
            for(int i = 0; i < 4; ++i)
            {
                Assert.True(cache.TryReadByte(i + 5, out res));
                Assert.Equal(i + 5, res);
            }
        }

        [Fact]
        public void TestTryReadBytesToBuffer()
        {
            ByteCache.FillCache callback = delegate(byte[] buffer, out long cacheStart, out int cacheLength)
            {
                cacheStart = 5;
                cacheLength = 4;
                buffer[0] = 5;
                buffer[1] = 6;
                buffer[2] = 7;
                buffer[3] = 8;
            };
            var tmp = new byte[10];
            var cache = new ByteCache(256);
            long firstRead;
            int read;
            Assert.False(cache.TryReadBytesToBuffer(tmp, 5, 1, 0, out firstRead, out read));
            cache.Cache(callback);

            Assert.True(cache.TryReadBytesToBuffer(tmp, 5, 1, 0, out firstRead, out read));
            Assert.Equal(5, firstRead);
            Assert.Equal(1, read);
            Assert.Equal(5, tmp[0]);

            Assert.True(cache.TryReadBytesToBuffer(tmp, 3, 5, 2, out firstRead, out read));
            Assert.Equal(5, firstRead);
            Assert.Equal(3, read);
            Assert.Equal(5, tmp[4]);
            Assert.Equal(6, tmp[5]);
            Assert.Equal(7, tmp[6]);

            Assert.True(cache.TryReadBytesToBuffer(tmp, 7, 5, 2, out firstRead, out read));
            Assert.Equal(7, firstRead);
            Assert.Equal(2, read);
            Assert.Equal(7, tmp[2]);
            Assert.Equal(8, tmp[3]);
        }

        [Fact]
        public void TestShiftLeft()
        {
            ByteCache.FillCache callback = delegate(byte[] buffer, out long cacheStart, out int cacheLength)
            {
                cacheStart = 5;
                cacheLength = 4;
                buffer[0] = 5;
                buffer[1] = 6;
                buffer[2] = 7;
                buffer[3] = 8;
            };
            byte res;
            var cache = new ByteCache(256);
            cache.Cache(callback);
            cache.ShiftLeft(6);
            Assert.Equal(6, cache.Start);
            Assert.Equal(3, cache.Length);
            Assert.False(cache.TryReadByte(5, out res));
            Assert.True(cache.TryReadByte(6, out res));
            Assert.Equal(6, res);
        }
    }
}
