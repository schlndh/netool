﻿using Netool.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Netool
{
    public enum EventType
    {
        ChannelCreated, ChannelClosed,
        RequestSent, RequestReceived,
        ResponseSent, ResponseReceived
    }

    public class Event
    {
        public readonly int ID;
        public readonly EventType Type;
        public readonly DataEventArgs Data;
        public readonly DateTime Time;

        public Event(int id, EventType type, DataEventArgs data, DateTime time)
        {
            ID = id;
            Type = type;
            Data = data;
            Time = time;
        }
    }

    public class ChannelLogger
    {
        public IChannel channel;
        private LinkedList<Event> Events = new LinkedList<Event>();
        private int eventID = 0;

        public event EventHandler<int> EventCountChanged;

        public ChannelLogger(IChannel channel)
        {
            this.channel = channel;
            Events.AddLast(new Event(0, EventType.ChannelCreated, null, DateTime.Now));
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

        public int GetEventCount()
        {
            lock (Events)
            {
                return Events.Count;
            }
        }

        /// <summary>
        /// Get node by position
        /// </summary>
        /// <param name="position">Position has to be lesser than count given by GetEventCount or by EventCountChanged event.
        /// This method DOESN'T check that.
        /// </param>
        public LinkedListNode<Event> GetByPosition(int position)
        {
            var curr = Events.First;
            while(position-- > 0)
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
            int c = 0;
            DataEventArgs nd = null;
            if(data != null)
            {
                nd = (DataEventArgs)data.Clone();
            }
            lock (Events)
            {
                Events.AddLast(new Event(++eventID, type, nd, DateTime.Now));
                c = Events.Count;
            }
            OnEventCountChanged(c);
        }

        private void OnEventCountChanged(int count)
        {
            if (EventCountChanged != null) EventCountChanged(this, count);
        }
    }

    public class InstanceLogger
    {
        private ConcurrentDictionary<int, ChannelLogger> channelsInfo = new ConcurrentDictionary<int, ChannelLogger>();
        private LinkedList<IChannel> channels = new LinkedList<IChannel>();

        public void AddChannel(IChannel channel)
        {
            lock(channels)
            {
                channels.AddLast(channel);
            }
            channelsInfo.TryAdd(channel.ID, new ChannelLogger(channel));
        }

        public ChannelLogger GetChannelLogger(int id)
        {
            ChannelLogger logger;
            channelsInfo.TryGetValue(id, out logger);
            return logger;
        }
    }
}