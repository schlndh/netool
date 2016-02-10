using Netool.ChannelDrivers;
using Netool.Network;
using Netool.Network.DataFormats;
using System;

namespace Tests
{
    [Serializable]
    internal class TestInstance : BaseInstance, IInstance
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
    internal class TestChannel : BaseChannel, IChannel
    {
        public new int id;
        public new int ID { get { return id; } }
        public new string name = "aaa";

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

    internal class TestServerChannel : BaseServerChannel, IServerChannel
    {
        public void Send(IDataStream s)
        {
            OnResponseSent(s);
        }

        public void Receive(IDataStream s)
        {
            OnRequestReceived(s);
        }

        public void Close()
        {
            OnChannelClosed();
        }
    }

    [Serializable]
    internal class DummyDataStream : IDataStream
    {
        private long length;

        public DummyDataStream(long length)
        {
            this.length = length;
        }

        public long Length { get { return length; } }

        public byte ReadByte(long index)
        {
            return (byte) index;
        }

        public void ReadBytesToBuffer(byte[] buffer, long start = 0, int length = -1, int offset = 0)
        {
        }

        public object Clone()
        {
            return this;
        }
    }
}