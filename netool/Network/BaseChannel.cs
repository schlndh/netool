using Netool.ChannelDrivers;
using System;
using System.Runtime.Serialization;

namespace Netool.Network
{
    [Serializable]
    public abstract class BaseChannel
    {
        protected int id;
        public int ID { get { return id; } }
        protected string name;
        public string Name { get { return name; } }
        protected IChannelDriver driver = null;
        public IChannelDriver Driver { get { return driver; } set { driver = value; } }

        [field: NonSerialized]
        public event ChannelReadyHandler ChannelReady;
        [field: NonSerialized]
        public event ChannelClosedHandler ChannelClosed;

        private bool channelReadyCalled = false;

        /// <summary>
        /// Indicates whether current channel was created through deserialization, which means that it's read-only.
        /// </summary>
        protected bool deserialized = false;

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

        [OnDeserialized]
        private void onDeserialized(StreamingContext c)
        {
            deserialized = true;
        }
    }
}