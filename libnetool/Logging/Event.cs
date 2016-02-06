using Netool.Network;
using System;

namespace Netool.Logging
{
    public enum EventType
    {
        ChannelCreated,
        ChannelClosed,
        RequestSent,
        RequestReceived,
        ResponseSent,
        ResponseReceived,
        ChannelReplaced,
    }

    [Serializable]
    public class Event
    {
        public readonly int ID;
        public readonly EventType Type;
        public readonly DataEventArgs Data;
        public readonly DateTime Time;
        /// <summary>
        /// Channel is not-null only when Type = ChannelReplaced
        /// </summary>
        public readonly IChannel Channel;

        public Event(int id, EventType type, DataEventArgs data, DateTime time)
        {
            ID = id;
            Type = type;
            Data = data;
            Time = time;
        }

        /// <summary>
        /// Use this constructor for ChannelReplaced event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newChannel"></param>
        /// <param name="time"></param>
        public Event(int id, IChannel newChannel, DateTime time)
        {
            ID = id;
            Type = EventType.ChannelReplaced;
            Data = null;
            Time = time;
            Channel = newChannel;
        }
    }
}