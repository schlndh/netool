using System;
using System.Collections.Generic;
using Xunit;
using Netool.Network.DataFormats;

namespace Tests.Network.DataFormats
{
    public class StreamListTests
    {
        [Fact]
        public void TestStreamList()
        {
            var list = new StreamList();
            Assert.Equal(0, list.Length);
            var stream = new ByteArray(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            list.Add(stream);
            Assert.Equal(10, list.Length);
            list.Add(stream);
            Assert.Equal(20, list.Length);
            list.Add(new EmptyData());
            Assert.Equal(20, list.Length);
            list.Add(new ByteArray(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }));
            Assert.Equal(31, list.Length);
            Assert.Equal(1, list.ReadByte(0));
            Assert.Equal(1, list.ReadByte(10));
            Assert.Equal(2, list.ReadByte(11));
            Assert.Equal(11, list.ReadByte(30));
            var buffer = new byte[50];

            buffer[1] = 99;
            buffer[2] = 99;
            buffer[3] = 99;
            buffer[13] = 99;
            list.ReadBytesToBuffer(buffer, 5, 0, 2);
            Assert.Equal(99, buffer[1]);
            Assert.Equal(99, buffer[2]);
            list.ReadBytesToBuffer(buffer, 5, 1, 2);
            Assert.Equal(6, buffer[2]);
            Assert.Equal(99, buffer[3]);
            list.ReadBytesToBuffer(buffer, 9, 10, 3);
            Assert.Equal(10, buffer[3]);
            Assert.Equal(4, buffer[7]);
            Assert.Equal(5, buffer[8]);
            Assert.Equal(9, buffer[12]);
            Assert.Equal(99, buffer[13]);
            // test read-all for possible off-by-one errors
            list.ReadBytesToBuffer(buffer);
            // test start > contentData.streams[0].length
            list.ReadBytesToBuffer(buffer, 16, 5);
            Assert.Equal(7, buffer[0]);
        }
    }
}
