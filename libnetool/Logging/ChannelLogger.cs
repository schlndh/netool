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
        private IChannelExtensions.ChannelHandlers handlers;

        public event EventHandler<int> EventCountChanged;

        private ChannelLogger()
        {
            handlers = new IChannelExtensions.ChannelHandlers
            {
                ChannelClosed = channelClosedHandler,
                RequestSent = requestSentHandler,
                RequestReceived = requestReceivedHandler,
                ResponseSent = responseSentHandler,
                ResponseReceived = responseReceivedHandler,
                ChannelReplaced = channelReplacedHandler,
            };
        }

        /// <summary>
        /// Normal constructor
        /// </summary>
        /// <param name="log"></param>
        /// <param name="hint"></param>
        /// <param name="channel"></param>
        public ChannelLogger(FileLog log, long hint, IChannel channel): this()
        {
            this.log = log;
            this.hint = hint;
            this.channel = channel;
            generalHandler(EventType.ChannelCreated);
            channel.BindAllEvents(handlers);
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

        private void channelReplacedHandler(object sender, IChannel e)
        {
            channel.UnbindAllEvents(handlers);
            e.BindAllEvents(handlers);
            generalHandler(EventType.ChannelReplaced, null, e);
            channel = e;
        }

        private void generalHandler(EventType type, DataEventArgs data = null, IChannel newChannel = null)
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
                if(type != EventType.ChannelReplaced)
                {
                    e = events.AddLast(new Event(++eventCount, type, nd, DateTime.Now)).Value;
                }
                else
                {
                    e = events.AddLast(new Event(++eventCount, newChannel, DateTime.Now)).Value;
                }
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