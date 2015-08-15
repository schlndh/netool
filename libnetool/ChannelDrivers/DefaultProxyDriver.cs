﻿using Netool.Network;
using System;
namespace Netool.ChannelDrivers
{
    [Serializable]
    public class DefaultProxyDriver : IChannelDriver
    {
        public string ID { get { return "DefaultProxyDriver"; } }
        public bool AllowManualControl { get { return allowManual; } }
        private bool allowManual;
        public object Settings { get { return allowManual; } }

        public DefaultProxyDriver(bool allowManual = false)
        {
            this.allowManual = allowManual;
        }

        public bool CanAccept(IChannel c)
        {
            return c is IProxyChannel;
        }

        public void Handle(IChannel c)
        {
            var ch = c as IProxyChannel;
            if (ch != null)
            {
                ch.RequestReceived += requestReceivedHandler;
                ch.ResponseReceived += responseReceivedHandler;
            }
        }

        private void responseReceivedHandler(object sender, DataEventArgs e)
        {
            var ch = sender as IProxyChannel;
            if (ch != null)
            {
                ch.SendToClient(e.Data);
            }
        }

        private void requestReceivedHandler(object sender, DataEventArgs e)
        {
            var ch = sender as IProxyChannel;
            if (ch != null)
            {
                ch.SendToServer(e.Data);
            }
        }
    }
}