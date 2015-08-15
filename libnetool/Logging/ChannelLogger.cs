using Netool.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Netool.Logging
{
    public class ChannelLogger
    {
        public IChannel channel;
        private LinkedList<Event> events = new LinkedList<Event>();
        private LinkedList<long> logPositions = new LinkedList<long>();
        private int eventCount = 0;
        private FileLog log;
        private long hint = 0;

        public event EventHandler<int> EventCountChanged;

        /// <summary>
        /// Normal constructor
        /// </summary>
        /// <param name="log"></param>
        /// <param name="hint"></param>
        /// <param name="channel"></param>
        public ChannelLogger(FileLog log, long hint, IChannel channel)
        {
            this.log = log;
            this.hint = hint;
            this.channel = channel;
            generalHandler(EventType.ChannelCreated);
            channel.ChannelClosed += channelClosedHandler;
            if (channel is IClientChannel)
            {
                var c = channel as IClientChannel;
                c.RequestSent += requestSentHandler;
                c.ResponseReceived += responseReceivedHandler;
            }
            else if (channel is IServerChannel)
            {
                var c = channel as IServerChannel;
                c.RequestReceived += requestReceivedHandler;
                c.ResponseSent += responseSentHandler;
            }
            else if (channel is IProxyChannel)
            {
                var c = channel as IProxyChannel;
                c.RequestReceived += requestReceivedHandler;
                c.ResponseSent += responseSentHandler;
                c.RequestSent += requestSentHandler;
                c.ResponseReceived += responseReceivedHandler;
            }
        }

        /// <summary>
        /// Used with deserialized channels, doesn't bind any event handlers
        /// </summary>
        /// <param name="log"></param>
        /// <param name="hint"></param>
        /// <param name="channel"></param>
        /// <param name="eventCount"></param>
        public ChannelLogger(FileLog log, long hint, IChannel channel, int eventCount)
        {
            this.log = log;
            this.hint = hint;
            this.channel = channel;
            this.eventCount = eventCount;
            // temporary workaround - load all events
            GetByID(eventCount);
        }

        public int GetEventCount()
        {
            lock (events)
            {
                return eventCount;
            }
        }

        /// <summary>
        /// Get node by id
        /// </summary>
        /// <param name="id"><![CDATA[Position has to be <= count given by GetEventCount or by EventCountChanged event.
        /// This method DOESN'T check that.]]>
        /// </param>
        public LinkedListNode<Event> GetByID(int id)
        {
            lock (events)
            {
                if (events.Count < id)
                {
                    var reader = log.CreateReader();
                    // read all channels between the last already read and the one requested
                    var missing = reader.ReadEvents(hint, events.Count + 1, id - events.Count);
                    foreach (var item in missing)
                    {
                        events.AddLast(item);
                    }
                    reader.Close();
                }

            }
            var curr = events.First;
            while (id-- > 1)
            {
                curr = curr.Next;
            }
            return curr;
        }

        private void responseSentHandler(object sender, DataEventArgs e)
        {
            generalHandler(EventType.ResponseSent, e);
        }

        private void requestReceivedHandler(object sender, DataEventArgs e)
        {
            generalHandler(EventType.RequestReceived, e);
        }

        private void channelClosedHandler(object sender)
        {
            generalHandler(EventType.ChannelClosed);
            Debug.WriteLine("ChannelLogger (type: {0}, id: {1}, name: {2}) writing channel data", channel.GetType(), channel.ID, channel.Name);
            log.WriteChannelData(hint, eventCount, channel);
        }

        private void responseReceivedHandler(object sender, DataEventArgs e)
        {
            generalHandler(EventType.ResponseReceived, e);
        }

        private void requestSentHandler(object sender, DataEventArgs e)
        {
            generalHandler(EventType.RequestSent, e);
        }

        private void generalHandler(EventType type, DataEventArgs data = null)
        {
            Debug.WriteLine("ChannelLogger (type: {0}, id: {1}, name: {2}) logging event (type: {3})", channel.GetType(), channel.ID, channel.Name, type);
            int c = 0;
            DataEventArgs nd = null;
            if (data != null)
            {
                nd = (DataEventArgs)data.Clone();
            }
            Event e;
            lock (events)
            {
                e = events.AddLast(new Event(++eventCount, type, nd, DateTime.Now)).Value;
                c = events.Count;
            }
            log.LogEvent(hint, e);
            OnEventCountChanged(c);
        }

        private void OnEventCountChanged(int count)
        {
            if (EventCountChanged != null) EventCountChanged(this, count);
        }
    }
}