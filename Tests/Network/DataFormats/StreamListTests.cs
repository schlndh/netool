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
            var arr1 = new byte[10];
            var arr2 = new byte[15];
            var arr3 = new byte[20];
            arr1[2] = 99;
            arr1[3] = 99;
            arr1[8] = 99;
            arr2[5] = 99;
            var buffers = new List<ArraySegment<byte>>
            {
                new ArraySegment<byte>(arr1, 2, 5),
                new ArraySegment<byte>(arr2, 0, 10),
                new ArraySegment<byte>(arr3, 0, 20),
            };
            list.ReadBytesToBuffer(buffers, 5, 0);
            Assert.Equal(99, arr1[2]);
            list.ReadBytesToBuffer(buffers, 5, 1);
            Assert.Equal(6, arr1[2]);
            Assert.Equal(99, arr1[3]);
            list.ReadBytesToBuffer(buffers, 9, 10);
            Assert.Equal(1, arr1[3]);
            Assert.Equal(4, arr1[6]);
            Assert.Equal(99, arr1[8]);
            Assert.Equal(5, arr2[0]);
            Assert.Equal(9, arr2[4]);
            Assert.Equal(99, arr2[5]);
            // test read-all for possible off-by-one errors
            list.ReadBytesToBuffer(buffers, 0, list.Length);
            // test start > contentData.streams[0].length
            list.ReadBytesToBuffer(buffers, 16, 5);
            Assert.Equal(7, arr1[2]);
        }
    }
}
