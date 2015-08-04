using Netool.Network;
using System;

namespace Netool.Logging
{
    public enum EventType
    {
        ChannelCreated, ChannelClosed,
        RequestSent, RequestReceived,
        ResponseSent, ResponseReceived
    }

    [Serializable]
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
}