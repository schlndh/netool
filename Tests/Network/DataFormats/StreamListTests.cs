using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Netool.Network.DataFormats;

namespace Tests.Network.DataFormats
{
    [TestClass]
    public class StreamListTests
    {
        [TestMethod]
        public void TestStreamList()
        {
            var list = new StreamList();
            Assert.AreEqual(0, list.Length);
            var stream = new ByteArray(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            list.Add(stream);
            Assert.AreEqual(10, list.Length);
            list.Add(stream);
            Assert.AreEqual(20, list.Length);
            list.Add(new EmptyData());
            Assert.AreEqual(20, list.Length);
            list.Add(new ByteArray(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }));
            Assert.AreEqual(31, list.Length);
            Assert.AreEqual(1, list.ReadByte(0));
            Assert.AreEqual(1, list.ReadByte(10));
            Assert.AreEqual(2, list.ReadByte(11));
            Assert.AreEqual(11, list.ReadByte(30));
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
            list.ReadBytesToBuffer(5, 0, buffers);
            Assert.AreEqual(99, arr1[2]);
            list.ReadBytesToBuffer(5, 1, buffers);
            Assert.AreEqual(6, arr1[2]);
            Assert.AreEqual(99, arr1[3]);
            list.ReadBytesToBuffer(9, 10, buffers);
            Assert.AreEqual(1, arr1[3]);
            Assert.AreEqual(4, arr1[6]);
            Assert.AreEqual(99, arr1[8]);
            Assert.AreEqual(5, arr2[0]);
            Assert.AreEqual(9, arr2[4]);
            Assert.AreEqual(99, arr2[5]);
            // test read-all for possible off-by-one errors
            list.ReadBytesToBuffer(0, list.Length, buffers);
            // test start > list.streams[0].length
            list.ReadBytesToBuffer(16, 5, buffers);
            Assert.AreEqual(7, arr1[2]);
        }
    }
}
