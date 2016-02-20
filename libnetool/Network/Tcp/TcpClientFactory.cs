using System;
using System.Net;
using Netool.Network.Helpers;

namespace Netool.Network.Tcp
{
    [Serializable]
    public class TcpClientFactorySettings
    {
        public IPAddress LocalIPAddress;
        public IPEndPoint RemoteEndPoint;
        public SocketProperties Properties;
    }

    [Serializable]
    public class TcpClientFactory : IClientFactory
    {
        private TcpClientFactorySettings settings;
        public object Settings { get { return settings; } }

        public TcpClientFactory(TcpClientFactorySettings s)
        {
            settings = s;
        }

        public IClient CreateClient()
        {
            return new TcpClient(
                new TcpClientSettings
                {
                    RemoteEndPoint = settings.RemoteEndPoint,
                    LocalEndPoint = new IPEndPoint(settings.LocalIPAddress, 0),
                    Properties = settings.Properties,
                });
        }
    }
}