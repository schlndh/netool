using System;

namespace Netool.Network
{
    public class DataEventAgrs : EventArgs
    {
        public IByteArrayConvertible Data;
        public object State;
    }

    public delegate void RequestSentHandler(object sender, DataEventAgrs e);
    public delegate void RequestReceivedHandler(object sender, DataEventAgrs e);
    public delegate void RequestDroppedHandler(object sender, DataEventAgrs e);
    public delegate void ResponseSentHandler(object sender, DataEventAgrs e);
    public delegate void ResponseReceivedHandler(object sender, DataEventAgrs e);
    public delegate void ResponseDroppedHandler(object sender, DataEventAgrs e);
    public delegate void ChannelClosedHandler(object sender);

    /// <summary>
    /// interface for request/response modification callbacks for proxy channels
    /// </summary>
    /// <param name="data"></param>
    /// <returns>new data or null if request/response is to be dropped</returns>
    public delegate IByteArrayConvertible DataModifier(IByteArrayConvertible data);

    public interface IByteArrayConvertible
    {
        byte[] ToByteArray();
    }

    public interface IClientFactory
    {
        IClient CreateClient();
    }

    public interface IServer
    {
        event EventHandler<IServerChannel> ChannelCreated;

        void Start();
        void Stop();
        bool TryGetByID(string ID, out IServerChannel c);
    }

    public interface IClient
    {
        event EventHandler<IClientChannel> ChannelCreated;

        IClientChannel Start();
        void Stop();
    }

    public interface IProxy
    {
        event EventHandler<IProxyChannel> ChannelCreated;

        void Start();
        void Stop();
        bool TryGetByID(string ID, out IProxyChannel c);
    }

    public interface IClientChannel
    {
        string ID { get; }
        event RequestSentHandler RequestSent;
        event ResponseReceivedHandler ResponseReceived;
        event ChannelClosedHandler ChannelClosed;

        void Send(IByteArrayConvertible request);
        void Close();
    }

    public interface IServerChannel
    {
        string ID { get; }
        event RequestReceivedHandler RequestReceived;
        event ResponseSentHandler ResponseSent;
        event ChannelClosedHandler ChannelClosed;

        void Send(IByteArrayConvertible response);
        void Close();
    }

    public interface IProxyChannel
    {
        string ID { get; }
        event RequestReceivedHandler RequestReceived;
        event RequestSentHandler RequestSent;
        event RequestDroppedHandler RequestDropped;
        event ResponseReceivedHandler ResponseReceived;
        event ResponseSentHandler ResponseSent;
        event ResponseDroppedHandler ResponseDropped;
        event ChannelClosedHandler ChannelClosed;
        DataModifier RequestModifier { get; set; }
        DataModifier ResponseModifier { get; set; }

        void Close();
    }
}