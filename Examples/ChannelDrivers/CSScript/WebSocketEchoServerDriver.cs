using Netool.ChannelDrivers;
using Netool.Network;
using Netool.Network.DataFormats.Http;
using Netool.Network.DataFormats.WebSocket;
using Netool.Network.Http;
using Netool.Network.WebSocket;
using Netool.Plugins.Http.ProtocolUpgrades;
using Netool.Plugins.MessageTemplates;

namespace Examples.ChannelDrivers.CSScript
{
    public class WebSocketEchoServerDriver : IChannelDriver
    {
        public bool AllowManualControl { get { return true; } }

        public string Name { get; set; }

        public object Settings { get { return null; } }

        public string Type { get { return "WebSocketEchoServer"; } }

        private IChannelExtensions.ChannelHandlers handlers;

        public WebSocketEchoServerDriver()
        {
            handlers = new IChannelExtensions.ChannelHandlers { ChannelReplaced = Channel_ChannelReplaced, RequestReceived = Channel_RequestReceived, ChannelClosed = Channel_ChannelClosed };
        }

        public bool CanAccept(IChannel c)
        {
            return c is HttpServerChannel;
        }

        public void Handle(IChannel c)
        {
            var channel = c as HttpServerChannel;
            if (channel != null)
            {
                channel.BindAllEvents(handlers);
            }
        }

        private void Channel_ChannelClosed(object sender)
        {
            var channel = sender as IChannel;
            channel.UnbindAllEvents(handlers);
        }

        private void Channel_RequestReceived(object sender, DataEventArgs e)
        {
            var http = sender as HttpServerChannel;
            if (http != null)
            {
                var data = e.Data as HttpData;
                string key;
                if (data == null || !data.Headers.TryGetValue("Sec-WebSocket-Key", out key))
                {
                    http.Close();
                    return;
                }
                var response = WebSocketTemplatesPlugin.WebSocketHandshakeServerTemplate.Instance.CreateMessage(key);
                http.UpgradeProtocol(WebSocketPlugin.WebSocketProtocolUpgrader.Instance, response);
            }
            else
            {
                var websocket = sender as WebSocketServerChannel;
                var data = e.Data as WebSocketMessage;
                if (data != null)
                {
                    switch (data.Opcode)
                    {
                        case WebSocketMessage.OpcodeType.Text:
                        case WebSocketMessage.OpcodeType.Binary:
                            // echo the unmasked data back to the client
                            websocket.Send(new WebSocketMessage(true, data.Opcode, null, data.InnerData));
                            break;

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
        }

        private void Channel_ChannelReplaced(object sender, IChannel e)
        {
            var channel = sender as IChannel;
            channel.UnbindAllEvents(handlers);
            e.BindAllEvents(handlers);
        }
    }
}