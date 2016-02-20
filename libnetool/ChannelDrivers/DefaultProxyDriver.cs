using Netool.Network;
using Netool.Network.DataFormats;
using System;
namespace Netool.ChannelDrivers
{
    [Serializable]
    public class DefaultProxyDriver : IChannelDriver
    {
        /// <inheritdoc/>
        public string Type { get { return "DefaultProxyDriver"; } }
        /// <inheritdoc/>
        public bool AllowManualControl { get { return allowManual; } }
        private bool allowManual;
        /// <inheritdoc/>
        public object Settings { get { return allowManual; } }
        /// <inheritdoc/>
        public string Name { get; set; }

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

        /// <summary>
        /// Maps response from server to response sent to client
        /// </summary>
        /// <param name="c"></param>
        /// <param name="s">response from server</param>
        /// <returns>modified response or null if the response is to be dropped</returns>
        protected virtual IDataStream responseMapper(IProxyChannel c, IDataStream s)
        {
            return s;
        }

        /// <summary>
        /// Maps request from client to request sent to server
        /// </summary>
        /// <param name="c"></param>
        /// <param name="s">request from client</param>
        /// <returns>modified request or null if the request is to be dropped</returns>
        protected virtual IDataStream requestMapper(IProxyChannel c, IDataStream s)
        {
            return s;
        }

        private void responseReceivedHandler(object sender, DataEventArgs e)
        {
            var ch = sender as IProxyChannel;
            if (ch != null)
            {
                var s = responseMapper(ch, e.Data);
                if(s != null) ch.SendToClient(s);
            }
        }

        private void requestReceivedHandler(object sender, DataEventArgs e)
        {
            var ch = sender as IProxyChannel;
            if (ch != null)
            {
                var s = requestMapper(ch, e.Data);
                if(s != null) ch.SendToServer(s);
            }
        }
    }
}