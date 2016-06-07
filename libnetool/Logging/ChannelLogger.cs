using Netool.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Netool.Logging
{
    /// <summary>
    /// Logs channel events to a log file and allows reading them back.
    /// </summary>
    public class ChannelLogger
    {
        public IChannel channel;
        private object eventsLock = new object();
        private int eventCount = 0;
        private FileLog log;
        private FileLog.ChannelInfo hint;
        private IChannelExtensions.ChannelHandlers handlers;

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="count">current event count</param>
        /// <param name="lastEvent">event with ID = count</param>
        public delegate void EventCountChangedHandler(object sender, int count, Event lastEvent);
        public event EventCountChangedHandler EventCountChanged;

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
        public ChannelLogger(FileLog log, FileLog.ChannelInfo hint, IChannel channel): this()
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
        public ChannelLogger(FileLog log, FileLog.ChannelInfo hint, IChannel channel, int eventCount)
        {
            this.log = log;
            this.hint = hint;
            this.channel = channel;
            this.eventCount = eventCount;
        }

        public int GetEventCount()
        {
            lock (eventsLock)
            {
                return eventCount;
            }
        }

        /// <summary>
        /// Get event by id
        /// </summary>
        /// <param name="id">1-based event ID</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Event GetEventByID(int id)
        {
            if (id < 1 || id > GetEventCount()) throw new ArgumentOutOfRangeException("id");
            using (var reader = log.ReaderPool.Get())
            {
                return reader.ReadEvent(hint, id);
            }
        }

        /// <summary>
        /// Get several consecutive events at once
        /// </summary>
        /// <param name="firstID">1-based event ID of the first event</param>
        /// <param name="count">how many events to return</param>
        /// <returns></returns>
        /// <remarks>This method is faster than respective amount of calls to GetEventByID</remarks>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public IList<Event> GetEventRange(int firstID, int count)
        {
            if (firstID < 1 || count < 1 || firstID + count - 1 > GetEventCount()) throw new ArgumentOutOfRangeException("firstID,count");
            using (var reader = log.ReaderPool.Get())
            {
                return reader.ReadEvents(hint, firstID, count);
            }
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
            lock (eventsLock)
            {
                if(type != EventType.ChannelReplaced)
                {
                    e = new Event(++eventCount, type, nd, DateTime.Now);
                }
                else
                {
                    e = new Event(++eventCount, newChannel, DateTime.Now);
                }
                c = eventCount;
                log.LogEvent(hint, e);
            }
            OnEventCountChanged(c, e);
        }

        private void OnEventCountChanged(int count, Event e)
        {
            var ev = EventCountChanged;
            if (ev != null) ev(this, count, e);
        }
    }
}