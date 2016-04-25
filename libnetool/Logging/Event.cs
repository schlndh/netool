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

        /// <summary>
        ///
        /// </summary>
        /// <param name="id">1-based ID</param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="time"></param>
        /// <exception cref="ArgumentOutOfRangeException">id </exception>
        public Event(int id, EventType type, DataEventArgs data, DateTime time)
        {
            if (id < 1) throw new ArgumentOutOfRangeException("id");
            ID = id;
            Type = type;
            Data = data;
            Time = time;
        }

        /// <summary>
        /// Use this constructor for ChannelReplaced event.
        /// </summary>
        /// <param name="id">1-based ID</param>
        /// <param name="newChannel"></param>
        /// <param name="time"></param>
        /// <exception cref="ArgumentOutOfRangeException">id </exception>
        public Event(int id, IChannel newChannel, DateTime time)
        {
            if (id < 1) throw new ArgumentOutOfRangeException("id");
            ID = id;
            Type = EventType.ChannelReplaced;
            Data = null;
            Time = time;
            Channel = newChannel;
        }
    }
}