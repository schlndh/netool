using Netool.Logging;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.WebSocket;
using Netool.Network.WebSocket;
using System;
using Xunit;

namespace Tests.Http.Network.Http
{
    public class BaseWebSocketTests : IDisposable
    {
        private InstanceLogger logger;

        public BaseWebSocketTests()
        {
            logger = new InstanceLogger();
        }

        [Fact]
        public void TestReceiveOne_Full()
        {
            // 0xC2 = 1100 0010
            //        FRRR opco
            //        ISSS de
            //        NVVV
            //         123
            var buffer = new byte[] { 0xC2, 10 };
            var payload = new DummyDataStream(10);
            var header = new ByteArray(buffer);
            var websocket = new BaseWebSocket(logger);
            bool called = false;
            websocket.MessageParsed += delegate(object sender, WebSocketMessage msg)
            {
                called = true;
                Assert.True(msg.FIN);
                Assert.True(msg.RSV1);
                Assert.False(msg.RSV2);
                Assert.False(msg.RSV3);
                Assert.Equal(WebSocketMessage.OpcodeType.Binary, msg.Opcode);
                Assert.Equal(10, msg.PayloadLength);
                Assert.False(msg.MASK);
            };

            var binMsg = new StreamList();
            binMsg.Add(header);
            binMsg.Add(payload);
            websocket.Receive(binMsg);
            Assert.True(called);
        }

        [Fact]
        public void TestReceiveTwo_Full()
        {
            // 0xC2 = 1100 0010
            //        FRRR opco
            //        ISSS de
            //        NVVV
            //         123
            var buffer = new byte[] { 0xC2, 10 };
            var payload = new DummyDataStream(10);
            var header = new ByteArray(buffer);
            var websocket = new BaseWebSocket(logger);
            int called = 0;
            websocket.MessageParsed += delegate(object sender, WebSocketMessage msg)
            {
                ++called;
                Assert.True(msg.FIN);
                Assert.True(msg.RSV1);
                Assert.False(msg.RSV2);
                Assert.False(msg.RSV3);
                Assert.Equal(WebSocketMessage.OpcodeType.Binary, msg.Opcode);
                Assert.Equal(10, msg.PayloadLength);
                Assert.False(msg.MASK);
            };

            var binMsg = new StreamList();
            binMsg.Add(header);
            binMsg.Add(payload);
            binMsg.Add(binMsg);
            websocket.Receive(binMsg);
            Assert.Equal(2, called);
        }

        [Fact]
        public void TestReceiveFour_Full()
        {
            // 0xC2 = 1100 0010
            //        FRRR opco
            //        ISSS de
            //        NVVV
            //         123
            var buffer = new byte[] { 0xC2, 10 };
            var payload = new DummyDataStream(10);
            var header = new ByteArray(buffer);
            var websocket = new BaseWebSocket(logger);
            int called = 0;
            websocket.MessageParsed += delegate (object sender, WebSocketMessage msg)
            {
                ++called;
                Assert.True(msg.FIN);
                Assert.True(msg.RSV1);
                Assert.False(msg.RSV2);
                Assert.False(msg.RSV3);
                Assert.Equal(WebSocketMessage.OpcodeType.Binary, msg.Opcode);
                Assert.Equal(10, msg.PayloadLength);
                Assert.False(msg.MASK);
            };

            var binMsg = new StreamList();
            binMsg.Add(header);
            binMsg.Add(payload);
            binMsg.Add(binMsg);
            binMsg.Add(binMsg);
            websocket.Receive(binMsg);
            Assert.Equal(4, called);
        }

        [Fact]
        public void TestReceiveOne_Partial()
        {
            // 0xC2 = 1100 0010
            //        FRRR opco
            //        ISSS de
            //        NVVV
            //         123
            var buffer = new byte[] { 0xC2, 10 };
            var payload = new DummyDataStream(5);
            var header = new ByteArray(buffer);
            var websocket = new BaseWebSocket(logger);
            bool called = false;
            websocket.MessageParsed += delegate(object sender, WebSocketMessage msg)
            {
                called = true;
                Assert.True(msg.FIN);
                Assert.True(msg.RSV1);
                Assert.False(msg.RSV2);
                Assert.False(msg.RSV3);
                Assert.Equal(WebSocketMessage.OpcodeType.Binary, msg.Opcode);
                Assert.Equal(10, msg.PayloadLength);
                Assert.False(msg.MASK);
            };

            var binMsg = new StreamList();
            binMsg.Add(header);
            binMsg.Add(payload);
            websocket.Receive(binMsg);
            Assert.False(called);
            // second half of the payload
            websocket.Receive(payload);
            Assert.True(called);
        }

        [Fact]
        public void TestReceiveTwo_PartialAndFull()
        {
            // 0xC2 = 1100 0010
            //        FRRR opco
            //        ISSS de
            //        NVVV
            //         123
            var buffer = new byte[] { 0xC2, 10 };
            var payload = new DummyDataStream(5);
            var header = new ByteArray(buffer);
            var websocket = new BaseWebSocket(logger);
            int called = 0;
            websocket.MessageParsed += delegate(object sender, WebSocketMessage msg)
            {
                ++called;
                Assert.True(msg.FIN);
                Assert.True(msg.RSV1);
                Assert.False(msg.RSV2);
                Assert.False(msg.RSV3);
                Assert.Equal(WebSocketMessage.OpcodeType.Binary, msg.Opcode);
                Assert.Equal(10, msg.PayloadLength);
                Assert.False(msg.MASK);
            };

            var binMsg = new StreamList();
            binMsg.Add(header);
            binMsg.Add(payload);
            websocket.Receive(binMsg);
            Assert.Equal(0, called);
            // second half of the payload + another message
            var binMsg2 = new StreamList();
            binMsg2.Add(payload);
            binMsg2.Add(binMsg);
            binMsg2.Add(payload);
            websocket.Receive(binMsg2);
            Assert.Equal(2, called);
        }

        public void Dispose()
        {
            logger.Close();
            logger.DeleteFile();
        }
    }
}