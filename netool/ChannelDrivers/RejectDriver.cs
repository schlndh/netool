using Netool.Network;

namespace Netool.ChannelDrivers
{
    public class RejectDriver : IServerChannelDriver, IProxyChannelDriver, IClientChannelDriver
    {
        public bool CanAccept(IChannel c) { return true; }
        public bool AllowManualControl { get { return false; } }
        public string ID { get { return "Reject"; } }

        public void Handle(IServerChannel c)
        {
            c.ChannelReady += channelReadyHandler;
        }

        public void Handle(IProxyChannel c)
        {
            c.ChannelReady += channelReadyHandler;
        }

        public void Handle(IClientChannel c)
        {
            c.ChannelReady += channelReadyHandler;
        }

        private void channelReadyHandler(object c)
        {
            ((IChannel)c).Close();
        }
    }
}