using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
namespace Netool.Network.Udp
{
    public class UdpClientFactorySettings
    {
        public IPAddress LocalIPAddress;
        public IPEndPoint RemoteEndPoint;
    }
    public class UdpClientFactory : IClientFactory
    {
        private UdpClientFactorySettings settings;
        public UdpClientFactory(UdpClientFactorySettings s)
        {
            settings = s;
        }
        public IClient CreateClient()
        {
            return new UdpClient(new UdpClientSettings { RemoteEndPoint = settings.RemoteEndPoint, LocalEndPoint = new IPEndPoint(settings.LocalIPAddress, 0) });
        }
    }
}
