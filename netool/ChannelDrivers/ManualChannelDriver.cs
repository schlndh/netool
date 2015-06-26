using Netool.Network;
using System.Threading;

namespace Netool.ChannelDrivers
{
    public class ManualChannelDriver : IChannelDriver
    {
        private int capacity;
        private int activeChannels = 0;
        public bool CanAccept(IChannel c) { return capacity > activeChannels; }
        public bool AllowManualControl { get { return true; } }
        public string ID { get { return "Manual"; } }

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
