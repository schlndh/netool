using Netool.ChannelDrivers;
using Netool.Network;
using Netool.Network.DataFormats.Http;
using Netool.Network.DataFormats.WebSocket;
using Netool.Network.Http;
using Netool.Network.WebSocket;
using Netool.Plugins.Http.ProtocolUpgrades;
using Netool.Plugins.MessageTemplates;
using System;

namespace Examples.ChannelDrivers.CSScript
{
    public class WebSocketUtilityDriver : IChannelDriver
    {
        public bool AllowManualControl { get { return true; } }

        public string Name { get; set; }

        public object Settings { get { return null; } }

        public string Type { get { return "WebSocketUtilityDriver"; } }

        private IChannelExtensions.ChannelHandlers handlers;

        public WebSocketUtilityDriver()
        {
            handlers = new IChannelExtensions.ChannelHandlers
            {
                RequestReceived = Channel_RequestReceived,
                ResponseReceived = Channel_ResponseReceived,
                ChannelClosed = Channel_ChannelClosed,
            };
        }

        public bool CanAccept(IChannel c)
        {
            return c is IReplaceableChannel;
        }

        public void Handle(IChannel c)
        {
            var channel = c as IReplaceableChannel;
            if (channel != null)
            {
                channel.ChannelReplaced += Channel_ChannelReplaced;
            }
        }

        private void Channel_ChannelClosed(object sender)
        {
            var channel = sender as IChannel;
            channel.UnbindAllEvents(handlers);
        }

        private void Channel_RequestReceived(object sender, DataEventArgs e)
        {
            var websocket = sender as WebSocketServerChannel;
            var data = e.Data as WebSocketMessage;
            if (data != null && websocket != null)
            {
                switch (data.Opcode)
                {
                    case WebSocketMessage.OpcodeType.Close:
                        websocket.Send(new WebSocketMessage(true, WebSocketMessage.OpcodeType.Close, null, null));
                        websocket.Close();
                        break;

                    case WebSocketMessage.OpcodeType.Ping:
                        websocket.Send(new WebSocketMessage(true, WebSocketMessage.OpcodeType.Pong, null, null));
                        break;

                    default:
                        // just ignore other opcodes
                        break;
                }
            }
        }

        private void Channel_ResponseReceived(object sender, DataEventArgs e)
        {
            Random randomGen = new Random();
            byte[] randKey = new byte[4];
            randomGen.NextBytes(randKey);
            var websocket = sender as WebSocketClientChannel;
            var data = e.Data as WebSocketMessage;
            if (data != null && websocket != null)
            {
                switch (data.Opcode)
                {
                    case WebSocketMessage.OpcodeType.Close:
                        websocket.Close();
                        break;

                    case WebSocketMessage.OpcodeType.Ping:
                        websocket.Send(new WebSocketMessage(true, WebSocketMessage.OpcodeType.Pong, randKey, null));
                        break;

                    default:
                        // just ignore other opcodes
                        break;
                }
            }
        }

        private void Channel_ChannelReplaced(object sender, IChannel e)
        {
            var channel = sender as IReplaceableChannel;
            channel.ChannelReplaced -= Channel_ChannelReplaced;
            e.BindAllEvents(handlers);
        }
    }
}