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
        /// <inheritdoc/>
        public bool CanAccept(IChannel c) { return capacity == -1 || capacity > activeChannels; }
        /// <inheritdoc/>
        public bool AllowManualControl { get { return true; } }
        /// <inheritdoc/>
        public string Type { get { return "Manual"; } }
        /// <inheritdoc/>
        public object Settings { get { return capacity; } }
        /// <inheritdoc/>
        public string Name { get; set; }

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
