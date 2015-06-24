using Netool.ChannelDrivers;

namespace Netool.Network
{
    public abstract class BaseChannel
    {
        protected int id;
        public int ID { get { return id; } }
        protected string name;
        public string Name { get { return name; } }
        protected IChannelDriver driver = null;
        public IChannelDriver Driver { get { return driver; } set { driver = value; } }

        public event ChannelReadyHandler ChannelReady;
        public event ChannelClosedHandler ChannelClosed;

        private bool channelReadyCalled = false;

        protected virtual void OnChannelClosed()
        {
            if (ChannelClosed != null) ChannelClosed(this);
        }

        /// <summary>
        /// This method is only supposed to be called by channel creator, don't call it directly
        /// </summary>
        public void raiseChannelReady()
        {
            if (!channelReadyCalled)
            {
                channelReadyCalled = true;
                if (ChannelReady != null) ChannelReady(this);
            }
        }
    }
}