using Netool.Network;
using System.Threading;

namespace Netool.ChannelDrivers
{
    public class ManualChannelDriver : IServerChannelDriver, IProxyChannelDriver, IClientChannelDriver
    {
        private int capacity;
        private int activeChannels = 0;
        public bool CanAccept { get { return capacity > activeChannels; } }
        public bool AllowManualControl { get { return true; } }
        public string ID { get { return "Manual"; } }

        public ManualChannelDriver(int capacity)
        {
            this.capacity = capacity;
        }

        public void Handle(IServerChannel c)
        {
            handle(c);
        }

        public void Handle(IProxyChannel c)
        {
            handle(c);
        }

        public void Handle(IClientChannel c)
        {
            handle(c);
        }

        private void handle(IChannel c)
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
