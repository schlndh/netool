using Netool.Network.DataFormats;
using Netool.Network.DataFormats.WebSocket;
using System;
using Xunit;

namespace Tests.Http.Network.DataFormats.WebSocket
{
    public class WebSocketMessageParserTests
    {
        [Fact]
        public void TestParse_7bPayloadLength_NoMask()
        {
            // 0x23 = 0010 0011
            //        opco RRRF
            //        de   SSSI
            //             VVVN
            //             321
            var buffer = new byte[] { 0x23, 10 << 1 };
            var payload = new DummyDataStream(10);
            var header = new ByteArray(buffer);
            var parser = new WebSocketMessage.Parser();
            parser.ParseHeader(header);
            var binMsg = new StreamList();
            binMsg.Add(header);
            binMsg.Add(payload);
            var msg = parser.Close(binMsg);
            Assert.True(msg.FIN);
            Assert.True(msg.RSV1);
            Assert.False(msg.RSV2);
            Assert.False(msg.RSV3);
            Assert.Equal(WebSocketMessage.OpcodeType.Binary, msg.Opcode);
            Assert.Equal(10, msg.PayloadLength);
            Assert.False(msg.MASK);
        }

        [Fact]
        public void TestParse_16bPayloadLength_Masked()
        {
            // 0x23 = 0010 0011
            //        opco RRRF
            //        de   SSSI
            //             VVVN
            //             321
            var buffer = new byte[]
            {
                0x23, (126 << 1) + 1,
                0x08, 0, // 16b length in Big-Endian = 0x0800 = 2048
                5, 6, 7, 8 // 32b mask
            };
            var payload = new DummyDataStream(2048);
            var header = new ByteArray(buffer);
            var parser = new WebSocketMessage.Parser();
            parser.ParseHeader(header);
            var binMsg = new StreamList();
            binMsg.Add(header);
            binMsg.Add(payload);
            var msg = parser.Close(binMsg);
            Assert.True(msg.FIN);
            Assert.True(msg.RSV1);
            Assert.False(msg.RSV2);
            Assert.False(msg.RSV3);
            Assert.Equal(WebSocketMessage.OpcodeType.Binary, msg.Opcode);
            Assert.Equal(2048, msg.PayloadLength);
            Assert.True(msg.MASK);
            Assert.Equal(4, msg.MaskingKey.Length);
            Assert.Equal(6, msg.MaskingKey[1]);
        }

        [Fact]
        public void TestParse_64bPayloadLength_NoMask()
        {
            // 0x13 = 0001 0011
            //        opco RRRF
            //        de   SSSI
            //             VVVN
            //             321
            var buffer = new byte[]
            {
                0x13, (127 << 1),
                0, 0, 0, 0, 0, 1, 0, 0, // 64b length in Big-Endian = 0x10000 = 65 536
            };
            var payload = new DummyDataStream(2048);
            var header = new ByteArray(buffer);
            var parser = new WebSocketMessage.Parser();
            parser.ParseHeader(header);
            var binMsg = new StreamList();
            binMsg.Add(header);
            binMsg.Add(payload);
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => parser.Close(binMsg));
            binMsg.Add(new DummyDataStream(65536 - 2048));
            var msg = parser.Close(binMsg);
            Assert.True(msg.FIN);
            Assert.True(msg.RSV1);
            Assert.False(msg.RSV2);
            Assert.False(msg.RSV3);
            Assert.Equal(WebSocketMessage.OpcodeType.Text, msg.Opcode);
            Assert.Equal(65536, msg.PayloadLength);
            Assert.False(msg.MASK);
        }
    }
}