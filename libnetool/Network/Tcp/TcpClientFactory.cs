﻿using System;
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

        public override string ToString()
        {
            return LocalIPAddress.ToString() + ":0->" + RemoteEndPoint.ToString() + ", SocketProperties=" + Properties.ToString();
        }
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

        public IClient CreateClient(IServerChannel serverChannel)
        {
            return new TcpClient(
                new TcpClientSettings
                {
                    RemoteEndPoint = settings.RemoteEndPoint,
                    LocalEndPoint = new IPEndPoint(settings.LocalIPAddress, 0),
                    Properties = settings.Properties,
                });
        }

        public override string ToString()
        {
            if (settings == null) return "";
            return settings.ToString();
        }
    }
}