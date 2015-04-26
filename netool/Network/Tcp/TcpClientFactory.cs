using System.Net;

namespace Netool.Network.Tcp
{
    public class TcpClientFactorySettings
    {
        public IPAddress LocalIPAddress;
        public IPEndPoint RemoteEndPoint;
    }

    public class TcpClientFactory : IClientFactory
    {
        private TcpClientFactorySettings settings;

        public TcpClientFactory(TcpClientFactorySettings s)
        {
            settings = s;
        }

        public IClient CreateClient()
        {
            return new TcpClient(new TcpClientSettings { RemoteEndPoint = settings.RemoteEndPoint, LocalEndPoint = new IPEndPoint(settings.LocalIPAddress, 0) });
        }
    }
}