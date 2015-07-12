using Netool.Network;
using System;
using System.Threading;

namespace Netool.ChannelDrivers
{
    [Serializable]
    public class ManualChannelDriver : IChannelDriver
    {
        private int capacity;
        private int activeChannels = 0;
        public bool CanAccept(IChannel c) { return capacity == -1 || capacity > activeChannels; }
        public bool AllowManualControl { get { return true; } }
        public string ID { get { return "Manual"; } }

        /// <summary>
        /// Creates Manual channel driver with specified capacity of active connections
        /// </summary>
        /// <param name="capacity">set -1 for unlimited capacity</param>
        public ManualChannelDriver(int capacity)
        {
            this.capacity = capacity;
        }

        public void Handle(IChannel c)
        {
            Interlocked.Increment(ref activeChannels);
            c.ChannelClosed += channelClosedHandler;
        }

        private void channelClosedHandler(object sender)
        {
            Interlocked.Decrement(ref activeChannels);
        }
    }
}
