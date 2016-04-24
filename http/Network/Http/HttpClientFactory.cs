using Netool.Logging;
using Netool.Network.Tcp;
using System;
using System.Net;

namespace Netool.Network.Http
{
    [Serializable]
    public class HttpClientFactory : IClientFactory
    {
        private TcpClientFactorySettings settings;
        [NonSerialized]
        private InstanceLogger logger;
        public object Settings { get { return settings; } }

        public HttpClientFactory(TcpClientFactorySettings s, InstanceLogger logger)
        {
            settings = s;
            this.logger = logger;
        }

        public IClient CreateClient()
        {
            return new HttpClient(
                new HttpClientSettings
                {
                    TcpSettings = new TcpClientSettings
                    {
                        RemoteEndPoint = settings.RemoteEndPoint,
                        LocalEndPoint = new IPEndPoint(settings.LocalIPAddress, 0),
                        Properties = settings.Properties,
                    },
                },
                logger);
        }

        internal void SetLogger(InstanceLogger logger)
        {
            if (this.logger != null) throw new InvalidOperationException("Logger already set!");
            this.logger = logger;
        }

        public override string ToString()
        {
            if (settings == null) return "";
            return settings.ToString();
        }
    }
}
