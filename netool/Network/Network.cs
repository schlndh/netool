using System;
using Netool.ChannelDrivers;
namespace Netool.Network
{
    [Serializable]
    public class DataEventArgs : EventArgs, ICloneable
    {
        public IByteArrayConvertible Data;
        public ICloneable State;

        public object Clone()
        {
            IByteArrayConvertible nd = null;
            ICloneable ns = null;
            if(Data != null)
            {
                nd = (IByteArrayConvertible)Data.Clone();
            }
            if(State != null)
            {
                ns = (ICloneable)ns.Clone();
            }
            return new DataEventArgs { Data = nd, State = ns };
        }
    }

    public delegate void ChannelReadyHandler(object sender);
    public delegate void RequestSentHandler(object sender, DataEventArgs e);
    public delegate void RequestReceivedHandler(object sender, DataEventArgs e);
    public delegate void ResponseSentHandler(object sender, DataEventArgs e);
    public delegate void ResponseReceivedHandler(object sender, DataEventArgs e);
    public delegate void ChannelClosedHandler(object sender);

    public interface IByteArrayConvertible : ICloneable
    {
        byte[] ToByteArray();
    }

    /// <summary>
    /// Client factory interface for default proxy, must be serializable
    /// </summary>
    public interface IClientFactory
    {
        IClient CreateClient();
    }

    /// <summary>
    /// Basic interface common for every instance, must be serializable
    /// </summary>
    public interface IInstance
    {
        bool IsStarted { get; }
        void Stop();
    }

    public interface IServer : IInstance
    {
        event EventHandler<IServerChannel> ChannelCreated;

        void Start();
    }

    public interface IClient : IInstance
    {
        event EventHandler<IClientChannel> ChannelCreated;

        IClientChannel Start();
    }

    public interface IProxy : IInstance
    {
        event EventHandler<IProxyChannel> ChannelCreated;

        void Start();
    }

    /// <summary>
    /// All events on a channel must happen only after ChannelCreated event is completed, must be serializable
    /// </summary>
    public interface IChannel
    {
        /// <summary>
        /// ID uniquely identifying all current and past channels of parent instance, must be consecutive
        /// </summary>
        int ID { get; }
        /// <summary>
        /// A name of a channel, eg. IP:port
        /// </summary>
        string Name { get; }
        IChannelDriver Driver { get; set; }
        /// <summary>
        /// Indicates, that channel is ready to receive commands, useful for drivers
        /// </summary>
        event ChannelReadyHandler ChannelReady;
        event ChannelClosedHandler ChannelClosed;

        void Close();
    }

    public interface IClientChannel : IChannel
    {
        event RequestSentHandler RequestSent;
        event ResponseReceivedHandler ResponseReceived;

        void Send(IByteArrayConvertible request);
    }

    public interface IServerChannel : IChannel
    {
        event RequestReceivedHandler RequestReceived;
        event ResponseSentHandler ResponseSent;

        void Send(IByteArrayConvertible response);
    }

    public interface IProxyChannel : IChannel
    {
        event RequestReceivedHandler RequestReceived;
        event RequestSentHandler RequestSent;
        event ResponseReceivedHandler ResponseReceived;
        event ResponseSentHandler ResponseSent;

        void SendToServer(IByteArrayConvertible request);
        void SendToClient(IByteArrayConvertible response);
    }

    public static class IInstanceExtensions
    {
        public static void Start(this IInstance c)
        {
            if(c is IClient)
            {
                ((IClient)c).Start();
            }
            else if (c is IServer)
            {
                ((IServer)c).Start();
            }
            else if (c is IProxy)
            {
                ((IProxy)c).Start();
            }
        }
    }
}