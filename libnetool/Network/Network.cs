using Netool.ChannelDrivers;
using Netool.Network.DataFormats;
using System;
using System.Collections.Generic;

namespace Netool.Network
{
    public enum InstanceType
    {
        Server, Client, Proxy
    }

    [Serializable]
    public class DataEventArgs : EventArgs, ICloneable
    {
        public IDataStream Data;
        public ICloneable State;

        public object Clone()
        {
            IDataStream nd = null;
            ICloneable ns = null;
            if(Data != null)
            {
                nd = (IDataStream)Data.Clone();
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
        /// <summary>
        /// A serializable settings that can be used to create a new instance with the same settings
        /// </summary>
        object Settings { get; }

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

        void Send(IDataStream request);
    }

    public interface IServerChannel : IChannel
    {
        event RequestReceivedHandler RequestReceived;
        event ResponseSentHandler ResponseSent;

        void Send(IDataStream response);
    }

    public interface IProxyChannel : IChannel
    {
        event RequestReceivedHandler RequestReceived;
        event RequestSentHandler RequestSent;
        event ResponseReceivedHandler ResponseReceived;
        event ResponseSentHandler ResponseSent;

        void SendToServer(IDataStream request);
        void SendToClient(IDataStream response);
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

        public static InstanceType GetInstanceType(this IInstance c)
        {
            if (c is IClient) return InstanceType.Client;
            if (c is IServer) return InstanceType.Server;
            if (c is IProxy) return InstanceType.Proxy;
            throw new ArgumentException("Invalid IInstance!");
        }
    }
}