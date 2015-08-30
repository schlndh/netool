using Netool.Network;
using System;

namespace Netool.ChannelDrivers
{
    [Serializable]
    public class RejectDriver : IChannelDriver
    {
        /// <inheritdoc/>
        public bool CanAccept(IChannel c) { return true; }
        /// <inheritdoc/>
        public bool AllowManualControl { get { return false; } }
        /// <inheritdoc/>
        public string Type { get { return "Reject"; } }
        /// <inheritdoc/>
        public object Settings { get { return null; } }
        /// <inheritdoc/>
        public string Name { get; set; }

        public void Handle(IChannel c)
        {
            c.ChannelReady += channelReadyHandler;
        }

        private void channelReadyHandler(object c)
        {
            ((IChannel)c).Close();
        }
    }
}