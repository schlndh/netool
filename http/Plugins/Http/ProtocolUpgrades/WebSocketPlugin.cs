using Netool.Network.Http;
using Netool.Network.WebSocket;
using System;

namespace Netool.Plugins.Http.ProtocolUpgrades
{
    public class WebSocketPlugin : IProtocolUpgradePlugin
    {
        public class WebSocketProtocolUpgrader : IProtocolUpgrader
        {
            private static WebSocketProtocolUpgrader instance;
            public static WebSocketProtocolUpgrader Instance { get { if (instance == null) instance = new WebSocketProtocolUpgrader(); return instance; } }
            private WebSocketProtocolUpgrader() {  }

            /// <inheritdoc/>
            public Network.IServerChannel UpgradeServerChannel(Network.IServerChannel c, Logging.InstanceLogger logger)
            {
                return new WebSocketServerChannel(c, logger);
            }

            /// <inheritdoc/>
            public Network.IClientChannel UpgradeClientChannel(Network.IClientChannel c, Logging.InstanceLogger logger)
            {
                return new WebSocketClientChannel(c, logger);
            }
        }

        /// <inheritdoc/>
        public long ID { get { return 11001; } }

        /// <inheritdoc/>
        public string Name { get { return "WebSocketPlugin"; } }

        /// <inheritdoc/>
        public string Description { get { return "WebSocket protocol upgrader for HTTP"; } }

        /// <inheritdoc/>
        public Version Version { get { return new Version(0, 0, 1); } }

        /// <inheritdoc/>
        public string Author { get { return "Hynek Schlindenbuch"; } }

        /// <inheritdoc/>
        public string ProtocolName { get { return "WebSocket"; } }

        /// <inheritdoc/>
        public Network.Http.IProtocolUpgrader CreateUpgrader()
        {
            return WebSocketProtocolUpgrader.Instance;
        }
    }
}