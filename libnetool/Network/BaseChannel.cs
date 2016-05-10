using Netool.ChannelDrivers;
using System;
using System.Runtime.Serialization;

namespace Netool.Network
{
    [Serializable]
    public abstract class BaseChannel
    {
        protected int id;
        /// <inheritdoc/>
        public int ID { get { return id; } }
        protected string name;
        /// <inheritdoc/>
        public string Name { get { return name; } }
        protected IChannelDriver driver = null;
        /// <inheritdoc/>
        public IChannelDriver Driver { get { return driver; } set { driver = value; } }

        /// <inheritdoc/>
        [field: NonSerialized]
        public event ChannelReadyHandler ChannelReady;

        /// <inheritdoc/>
        [field: NonSerialized]
        public event ChannelClosedHandler ChannelClosed;

        /// <inheritdoc/>
        [field: NonSerialized]
        public event EventHandler<Exception> ErrorOccured;

        private bool channelReadyCalled = false;

        /// <summary>
        /// Indicates whether current channel was created through deserialization, which means that it's read-only.
        /// </summary>
        protected bool deserialized = false;

        protected virtual void OnChannelClosed()
        {
            var ev = ChannelClosed;
            if (ev != null) ev(this);
        }

        /// <summary>
        /// This method is only supposed to be called by channel creator, don't call it directly
        /// </summary>
        public void raiseChannelReady()
        {
            OnChannelReady();
        }

        protected virtual void OnChannelReady()
        {
            if (!channelReadyCalled)
            {
                channelReadyCalled = true;
                var ev = ChannelReady;
                if (ev != null) ev(this);
            }
        }

        protected virtual void OnErrorOccured(Exception e)
        {
            var ev = ErrorOccured;
            if (ev != null) ev(this, e);
        }

        [OnDeserialized]
        private void onDeserialized(StreamingContext c)
        {
            deserialized = true;
        }
    }
}