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
        /// <summary>
        /// Indicates that new channel was created
        /// </summary>
        /// <remarks>
        /// Must be nonserializable, as nonserializable classes will bind to it
        /// </remarks>
        event EventHandler<IServerChannel> ChannelCreated;

        void Start();
    }

    public interface IClient : IInstance
    {
        /// <summary>
        /// Indicates that new channel was created. This event is raised every time the client is newly started.
        /// </summary>
        /// <remarks>
        /// Must be nonserializable, as nonserializable classes will bind to it
        /// </remarks>
        event EventHandler<IClientChannel> ChannelCreated;

        IClientChannel Start();
    }

    public interface IProxy : IInstance
    {
        /// <summary>
        /// Indicates that new channel was created
        /// </summary>
        /// <remarks>
        /// Must be nonserializable, as nonserializable classes will bind to it
        /// </remarks>
        event EventHandler<IProxyChannel> ChannelCreated;

        void Start();
    }

    /// <summary>
    /// Basic interface for all channel
    /// </summary>
    /// <remarks>
    /// All events on a channel must happen only after ChannelCreated event is completed.
    /// Must be serializable
    /// </remarks>
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
        /// <remarks>
        /// Must be nonserializable, as nonserializable classes will bind to it
        /// </remarks>
        event ChannelReadyHandler ChannelReady;

        /// <summary>
        /// Indicates that channel is closed and will receive no further commands
        /// </summary>
        /// <remarks>
        /// Must be nonserializable, as nonserializable classes will bind to it
        /// </remarks>
        event ChannelClosedHandler ChannelClosed;

        /// <summary>
        /// Close the channel, will raise the ChannelClosed event
        /// </summary>
        void Close();
    }

    /// <summary>
    /// Basic interface for client channels
    /// </summary>
    /// <inheritdoc select="remarks"/>
    public interface IClientChannel : IChannel
    {
        /// <summary>
        /// Indicates that request was sent
        /// </summary>
        /// <remarks>
        /// Must be nonserializable, as nonserializable classes will bind to it
        /// </remarks>
        event RequestSentHandler RequestSent;
        /// <summary>
        /// Indicates that response was recieved
        /// </summary>
        /// <remarks>
        /// Must be nonserializable, as nonserializable classes will bind to it
        /// </remarks>
        event ResponseReceivedHandler ResponseReceived;

        /// <summary>
        /// Send request to server
        /// </summary>
        /// <param name="request">request stream</param>
        void Send(IDataStream request);
    }

    /// <summary>
    /// Basic interface for client channels
    /// </summary>
    /// <inheritdoc select="remarks"/>
    public interface IServerChannel : IChannel
    {
        /// <summary>
        /// Indicates that request was received
        /// </summary>
        /// <remarks>
        /// Must be nonserializable, as nonserializable classes will bind to it
        /// </remarks>
        event RequestReceivedHandler RequestReceived;

        /// <summary>
        /// Indicates that response was sent
        /// </summary>
        /// <remarks>
        /// Must be nonserializable, as nonserializable classes will bind to it
        /// </remarks>
        event ResponseSentHandler ResponseSent;

        /// <summary>
        /// Send response to client
        /// </summary>
        /// <param name="response">response stream</param>
        void Send(IDataStream response);
    }

    /// <summary>
    /// Basic interface for client channels
    /// </summary>
    /// <inheritdoc select="remarks"/>
    public interface IProxyChannel : IChannel
    {
        /// <summary>
        /// Indicates that request was received
        /// </summary>
        /// <remarks>
        /// Must be nonserializable, as nonserializable classes will bind to it
        /// </remarks>
        event RequestReceivedHandler RequestReceived;
        /// <summary>
        /// Indicates that request was sent
        /// </summary>
        /// <remarks>
        /// Must be nonserializable, as nonserializable classes will bind to it
        /// </remarks>
        event RequestSentHandler RequestSent;
        /// <summary>
        /// Indicates that response was received
        /// </summary>
        /// <remarks>
        /// Must be nonserializable, as nonserializable classes will bind to it
        /// </remarks>
        event ResponseReceivedHandler ResponseReceived;
        /// <summary>
        /// Indicates that response was received
        /// </summary>
        /// <remarks>
        /// Must be nonserializable, as nonserializable classes will bind to it
        /// </remarks>
        event ResponseSentHandler ResponseSent;

        /// <summary>
        /// Send request to server
        /// </summary>
        /// <param name="response">requesr stream</param>
        void SendToServer(IDataStream request);

        /// <summary>
        /// Send response to client
        /// </summary>
        /// <param name="response">response stream</param>
        void SendToClient(IDataStream response);
    }

    public static class IInstanceExtensions
    {
        /// <summary>
        /// Start instance of any type
        /// </summary>
        /// <param name="c">instance</param>
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

        /// <summary>
        /// Get instance type
        /// </summary>
        /// <param name="c">instance</param>
        /// <returns>type</returns>
        public static InstanceType GetInstanceType(this IInstance c)
        {
            if (c is IClient) return InstanceType.Client;
            if (c is IServer) return InstanceType.Server;
            if (c is IProxy) return InstanceType.Proxy;
            throw new ArgumentException("Invalid IInstance!");
        }
    }
}