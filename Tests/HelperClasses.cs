﻿using Netool.ChannelDrivers;
using Netool.Network;
using Netool.Network.DataFormats;
using System;

namespace Tests
{
    [Serializable]
    internal class TestInstance : IInstance
    {
        public object Settings { get { return null; } }
        public string Serialized;

        [NonSerialized]
        public string NonSerialized;

        public bool IsStarted { get { return true; } }

        public void Stop()
        { }
    }

    [Serializable]
    internal class TestChannel : IChannel
    {
        public int id;
        public int ID { get { return id; } }
        public string name = "aaa";
        public string Name { get { return name; } }
        public IChannelDriver Driver { get { return null; } set { } }

        public event ChannelReadyHandler ChannelReady;

        public event ChannelClosedHandler ChannelClosed;

        public void Close()
        {
        }

        [NonSerialized]
        public string NonSerialized;
    }

    internal class TestClientChannel : BaseClientChannel, IClientChannel
    {
        public void Send(IDataStream s)
        {
            OnRequestSent(s);
        }

        public void Receive(IDataStream s)
        {
            OnResponseReceived(s);
        }

        public void Close()
        {
            OnChannelClosed();
        }
    }
}