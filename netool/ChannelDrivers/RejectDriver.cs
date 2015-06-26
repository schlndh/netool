using Netool.Network;

namespace Netool.ChannelDrivers
{
    public class RejectDriver : IChannelDriver
    {
        public bool CanAccept(IChannel c) { return true; }
        public bool AllowManualControl { get { return false; } }
        public string ID { get { return "Reject"; } }

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