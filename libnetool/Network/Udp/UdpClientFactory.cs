using Netool.Network.Helpers;
using System;
using System.Net;

namespace Netool.Network.Udp
{
    [Serializable]
    public class UdpClientFactorySettings
    {
        public IPAddress LocalIPAddress;
        public IPEndPoint RemoteEndPoint;
        public SocketProperties Properties;

        public override string ToString()
        {
            return LocalIPAddress.ToString() + ":0->" + RemoteEndPoint.ToString() + ", SocketProperties=" + Properties.ToString();
        }
    }

    [Serializable]
    public class UdpClientFactory : IClientFactory
    {
        private UdpClientFactorySettings settings;
        public object Settings { get { return settings; } }

        public UdpClientFactory(UdpClientFactorySettings s)
        {
            settings = s;
        }

        public IClient CreateClient(IServerChannel serverChannel)
        {
            return new UdpClient(new UdpClientSettings { RemoteEndPoint = settings.RemoteEndPoint, LocalEndPoint = new IPEndPoint(settings.LocalIPAddress, 0), Properties = settings.Properties });
        }

        public override string ToString()
        {
            if (settings == null) return "";
            return settings.ToString();
        }
    }
}