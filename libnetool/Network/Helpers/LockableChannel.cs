using Netool.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Netool.Network.Helpers
{
    /// <summary>
    /// IChannel wrapper that allows event locking and unlocking
    /// </summary>
    [Serializable]
    public abstract class LockableChannel : IChannel
    {
        /// <inheritdoc/>
        [field: NonSerialized]
        public event ChannelReadyHandler ChannelReady;

        /// <inheritdoc/>
        [field: NonSerialized]
        public event ChannelClosedHandler ChannelClosed;

        /// <inheritdoc/>
        [field: NonSerialized]
        public event EventHandler<Exception> ErrorOccured;

        /// <inheritdoc/>
        public int ID { get { return InnerChannel.ID; } }

        /// <inheritdoc/>
        public string Name { get { return InnerChannel.Name; } }

        /// <inheritdoc/>
        public ChannelDrivers.IChannelDriver Driver { get { return InnerChannel.Driver; } set { InnerChannel.Driver = value; } }

        protected IChannel InnerChannel { get; set; }

        protected struct StoredEvent
        {
            public EventType Type;
            public DataEventArgs Data;
        }

        [NonSerialized]
        protected Queue<StoredEvent> Events = new Queue<StoredEvent>();

        [NonSerialized]
        protected object EventsLock = new object();

        [NonSerialized]
        protected bool Locked = false;

        public LockableChannel(IChannel innerChannel, LockableChannel channelWithSharedLock = null)
        {
            if (channelWithSharedLock != null)
            {
                EventsLock = channelWithSharedLock.EventsLock;
            }
            InnerChannel = innerChannel;
            InnerChannel.ChannelReady += InnerChannel_ChannelReady;
            InnerChannel.ChannelClosed += InnerChannel_ChannelClosed;
            InnerChannel.ErrorOccured += InnerChannel_ErrorOccured;
        }

        private void InnerChannel_ErrorOccured(object sender, Exception e)
        {
            // forward this event directly, because it's not logged anyway
            var ev = ErrorOccured;
            if (ev != null) ev(this, e);
        }

        protected void OnChannelClosed()
        {
            var ev = ChannelClosed;
            if (ev != null) ev(this);
        }

        protected void InnerChannel_ChannelClosed(object sender)
        {
            lock (EventsLock)
            {
                if (Locked)
                {
                    Events.Enqueue(new StoredEvent { Type = EventType.ChannelClosed, Data = null });
                }
                else if (ChannelClosed != null)
                {
                    ChannelClosed(this);
                }
            }
        }

        private void InnerChannel_ChannelReady(object sender)
        {
            // forward this event directly, because it's not logged anyway
            var ev = ChannelReady;
            if (ev != null) ev(this);
        }

        /// <inheritdoc/>
        public void Close()
        {
            lock(EventsLock)
            {
                InnerChannel.Close();
            }
        }

        /// <summary>
        /// Lock events - events will be stored until the channel is unlocked again.
        /// </summary>
        public void Lock()
        {
            lock (EventsLock)
            {
                Locked = true;
            }
        }

        /// <summary>
        /// This method raises the event even in locked state
        /// </summary>
        /// <param name="e"></param>
        protected abstract void DispatchEvent(StoredEvent e);

        /// <summary>
        /// Unlock events - old events will be raised in order and new events will be raised directly.
        /// </summary>
        public void Unlock()
        {
            lock (EventsLock)
            {
                while (Events.Count > 0)
                {
                    DispatchEvent(Events.Dequeue());
                }
                Locked = false;
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.EventsLock = new object();
            this.Events = new Queue<StoredEvent>();
            this.Locked = false;
        }
    }

    [Serializable]
    public class LockableServerChannel : LockableChannel, IServerChannel
    {
        /// <inheritdoc/>
        [field: NonSerialized]
        public event RequestReceivedHandler RequestReceived;

        /// <inheritdoc/>
        [field: NonSerialized]
        public event ResponseSentHandler ResponseSent;

        /// <summary>
        /// Get InnerChannel
        /// </summary>
        /// <remarks>
        /// Warning: Do NOT attach to InnerChannel's events, as that would effectively make the lock/unlock mechanism pointless.
        /// </remarks>
        public new IServerChannel InnerChannel { get; protected set; }

        public LockableServerChannel(IServerChannel innerChannel, LockableChannel channelWithSharedLock = null)
            : base(innerChannel, channelWithSharedLock)
        {
            InnerChannel = innerChannel;
            InnerChannel.RequestReceived += InnerChannel_RequestReceived;
            InnerChannel.ResponseSent += InnerChannel_ResponseSent;
        }

        private void InnerChannel_ResponseSent(object sender, DataEventArgs e)
        {
            lock (EventsLock)
            {
                var ev = ResponseSent;
                if (Locked)
                {
                    Events.Enqueue(new StoredEvent { Type = EventType.ResponseSent, Data = e });
                }
                else if (ev != null)
                {
                    ev(this, e);
                }
            }
        }

        private void InnerChannel_RequestReceived(object sender, DataEventArgs e)
        {
            lock (EventsLock)
            {
                var ev = RequestReceived;
                if (Locked)
                {
                    Events.Enqueue(new StoredEvent { Type = EventType.RequestReceived, Data = e });
                }
                else if (ev != null)
                {
                    ev(this, e);
                }
            }
        }

        /// <inheritdoc/>
        public void Send(DataFormats.IDataStream response)
        {
            lock(EventsLock)
            {
                InnerChannel.Send(response);
            }
        }

        protected override void DispatchEvent(StoredEvent e)
        {
            switch (e.Type)
            {
                case EventType.ChannelClosed:
                    OnChannelClosed();
                    break;

                case EventType.RequestReceived:
                    var ev = RequestReceived;
                    if (ev != null) ev(this, e.Data);
                    break;

                case EventType.ResponseSent:
                    var ev1 = ResponseSent;
                    if (ev1 != null) ev1(this, e.Data);
                    break;

                default:
                    break;
            }
        }
    }

    [Serializable]
    public class LockableClientChannel : LockableChannel, IClientChannel
    {
        /// <inheritdoc/>
        [field: NonSerialized]
        public event ResponseReceivedHandler ResponseReceived;

        /// <inheritdoc/>
        [field: NonSerialized]
        public event RequestSentHandler RequestSent;

        /// <inheritdoc cref="LockableServerChannel.InnerChannel"/>
        public new IClientChannel InnerChannel { get; protected set; }

        public LockableClientChannel(IClientChannel innerChannel, LockableChannel channelWithSharedLock = null)
            : base(innerChannel, channelWithSharedLock)
        {
            InnerChannel = innerChannel;
            InnerChannel.RequestSent += InnerChannel_RequestSent;
            InnerChannel.ResponseReceived += InnerChannel_ResponseReceived;
        }

        private void InnerChannel_ResponseReceived(object sender, DataEventArgs e)
        {
            lock (EventsLock)
            {
                var ev = ResponseReceived;
                if (Locked)
                {
                    Events.Enqueue(new StoredEvent { Type = EventType.ResponseReceived, Data = e });
                }
                else if (ev != null)
                {
                    ev(this, e);
                }
            }
        }

        private void InnerChannel_RequestSent(object sender, DataEventArgs e)
        {
            lock (EventsLock)
            {
                var ev = RequestSent;
                if (Locked)
                {
                    Events.Enqueue(new StoredEvent { Type = EventType.RequestSent, Data = e });
                }
                else if (ev != null)
                {
                    ev(this, e);
                }
            }
        }

        /// <inheritdoc/>
        public void Send(DataFormats.IDataStream response)
        {
            lock(EventsLock)
            {
                InnerChannel.Send(response);
            }
        }

        protected override void DispatchEvent(StoredEvent e)
        {
            switch (e.Type)
            {
                case EventType.ChannelClosed:
                    OnChannelClosed();
                    break;

                case EventType.RequestSent:
                    var ev = RequestSent;
                    if (ev != null) ev(this, e.Data);
                    break;

                case EventType.ResponseReceived:
                    var ev1 = ResponseReceived;
                    if (ev1 != null) ev1(this, e.Data);
                    break;

                default:
                    break;
            }
        }
    }

    [Serializable]
    public class LockableProxyChannel : LockableChannel, IProxyChannel
    {
        /// <inheritdoc/>
        [field: NonSerialized]
        public event ResponseReceivedHandler ResponseReceived;

        /// <inheritdoc/>
        [field: NonSerialized]
        public event RequestSentHandler RequestSent;

        /// <inheritdoc/>
        [field: NonSerialized]
        public event RequestReceivedHandler RequestReceived;

        /// <inheritdoc/>
        [field: NonSerialized]
        public event ResponseSentHandler ResponseSent;

        /// <inheritdoc cref="LockableServerChannel.InnerChannel"/>
        public new IProxyChannel InnerChannel { get; protected set; }

        public LockableProxyChannel(IProxyChannel innerChannel)
            : base(innerChannel)
        {
            InnerChannel = innerChannel;
            InnerChannel.RequestSent += InnerChannel_RequestSent;
            InnerChannel.RequestReceived += InnerChannel_RequestReceived;
            InnerChannel.ResponseReceived += InnerChannel_ResponseReceived;
            InnerChannel.ResponseSent += InnerChannel_ResponseSent;
        }

        private void InnerChannel_ResponseSent(object sender, DataEventArgs e)
        {
            lock (EventsLock)
            {
                var ev = ResponseSent;
                if (Locked)
                {
                    Events.Enqueue(new StoredEvent { Type = EventType.ResponseSent, Data = e });
                }
                else if (ev != null)
                {
                    ev(this, e);
                }
            }
        }

        private void InnerChannel_RequestReceived(object sender, DataEventArgs e)
        {
            lock (EventsLock)
            {
                var ev = RequestReceived;
                if (Locked)
                {
                    Events.Enqueue(new StoredEvent { Type = EventType.RequestReceived, Data = e });
                }
                else if (ev != null)
                {
                    ev(this, e);
                }
            }
        }

        private void InnerChannel_ResponseReceived(object sender, DataEventArgs e)
        {
            lock (EventsLock)
            {
                var ev = ResponseReceived;
                if (Locked)
                {
                    Events.Enqueue(new StoredEvent { Type = EventType.ResponseReceived, Data = e });
                }
                else if (ev != null)
                {
                    ev(this, e);
                }
            }
        }

        private void InnerChannel_RequestSent(object sender, DataEventArgs e)
        {
            lock (EventsLock)
            {
                var ev = RequestSent;
                if (Locked)
                {
                    Events.Enqueue(new StoredEvent { Type = EventType.RequestSent, Data = e });
                }
                else if (ev != null)
                {
                    ev(this, e);
                }
            }
        }

        protected override void DispatchEvent(StoredEvent e)
        {
            switch (e.Type)
            {
                case EventType.ChannelClosed:
                    OnChannelClosed();
                    break;

                case EventType.RequestSent:
                    var ev = RequestSent;
                    if (ev != null) ev(this, e.Data);
                    break;

                case EventType.ResponseReceived:
                    var ev1 = ResponseReceived;
                    if (ev1 != null) ev1(this, e.Data);
                    break;

                case EventType.RequestReceived:
                    var ev2 = RequestReceived;
                    if (ev2 != null) ev2(this, e.Data);
                    break;

                case EventType.ResponseSent:
                    var ev3 = ResponseSent;
                    if (ev3 != null) ev3(this, e.Data);
                    break;

                default:
                    break;
            }
        }

        /// <inheritdoc/>
        public void SendToServer(DataFormats.IDataStream request)
        {
            lock(EventsLock)
            {
                InnerChannel.SendToServer(request);
            }

        }

        /// <inheritdoc/>
        public void SendToClient(DataFormats.IDataStream response)
        {
            lock(EventsLock)
            {
                InnerChannel.SendToClient(response);
            }
        }
    }
}