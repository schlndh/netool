using Netool.ChannelDrivers;
using Netool.Network;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using Netool.Network.Http;
using Netool.Plugins.Http.ProtocolUpgrades;

namespace Examples.ChannelDrivers.CSScript
{
    public class WebSocketDefaultProxyDriver : DefaultProxyDriver
    {
        private static WebSocketPlugin.WebSocketProtocolUpgrader upgrader = WebSocketPlugin.WebSocketProtocolUpgrader.Instance;

        protected override IDataStream responseMapper(IProxyChannel c, IDataStream s)
        {
            var channel = c as DefaultProxyChannel;
            var data = s as HttpData;
            if (channel != null && data != null && data.Code == 101)
            {
                string upgrade;
                if (data.Headers.TryGetValue("Upgrade", out upgrade) && upgrade.ToLower() == "websocket")
                {
                    channel.ReplaceInnerChannels(upgradeServer, upgradeClient);
                }
            }
            return s;
        }

        private void upgradeServer(IServerChannel c)
        {
            var channel = c as HttpServerChannel;
            if (channel != null)
            {
                channel.UpgradeProtocol(upgrader);
            }
        }

        private void upgradeClient(IClientChannel c)
        {
            var channel = c as HttpClientChannel;
            if (channel != null)
            {
                channel.UpgradeProtocol(upgrader);
            }
        }
    }
}