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
    }
}
