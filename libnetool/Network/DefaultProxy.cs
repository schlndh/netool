using Netool.ChannelDrivers;
using Netool.Network.DataFormats;
using System;
using System.Collections.Concurrent;

namespace Netool.Network
{
    [Serializable]
    public class DefaultProxyException : Exception
    {
        public DefaultProxyException() { }
        public DefaultProxyException(string message) : base(message) { }
        public DefaultProxyException(string message, Exception inner) : base(message, inner) { }
        protected DefaultProxyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class DefaultProxyChannel : BaseChannel, IProxyChannel
    {
        /// <summary>
        /// Channel from Proxy to Server
        /// </summary>
        protected IClientChannel clientChannel;
        /// <summary>
        /// Channel from Proxy to Client
        /// </summary>
        protected IServerChannel serverChannel;

        [field: NonSerialized]
        public event RequestReceivedHandler RequestReceived;
        [field: NonSerialized]
        public event RequestSentHandler RequestSent;
        [field: NonSerialized]
        public event ResponseSentHandler ResponseSent;
        [field: NonSerialized]
        public event ResponseReceivedHandler ResponseReceived;

        public delegate void ChannelCallback<T>(T c) where T : IChannel;

        public new int ID { get { return serverChannel.ID; } }
        public new string Name { get { return serverChannel.Name; } }
        private volatile bool closed = false;
        [NonSerialized]
        private IChannelExtensions.ChannelHandlers clientHandlers;
        [NonSerialized]
        private IChannelExtensions.ChannelHandlers serverHandlers;

        public DefaultProxyChannel(IClient client, IServerChannel srvChannel)
        {
            clientHandlers = new IChannelExtensions.ChannelHandlers
            {
                ErrorOccured = errorOccuredHandler,
                ChannelClosed = channelClosedHandler,
                ChannelReplaced = client_ChannelReplaced,
                RequestSent = client_RequestSent,
                ResponseReceived = client_ResponseReceived,
            };

            serverHandlers = new IChannelExtensions.ChannelHandlers
            {
                ErrorOccured = errorOccuredHandler,
                ChannelClosed = channelClosedHandler,
                ChannelReplaced = server_ChannelReplaced,
                RequestReceived = server_RequestReceived,
                ResponseSent = server_ResponseSent,
            };

            clientChannel = client.Start();
            if(clientChannel != null)
            {
                clientChannel.BindAllEvents(clientHandlers);
                serverChannel = srvChannel;
                serverChannel.BindAllEvents(serverHandlers);
            }
            else
            {
                OnErrorOccured(new DefaultProxyException("Unable to start a client channel."));
            }
        }

        private void server_ChannelReplaced(object sender, IChannel e)
        {
            var c = sender as IServerChannel;
            c.UnbindAllEvents(serverHandlers);
            e.BindAllEvents(serverHandlers);
        }

        private void client_ChannelReplaced(object sender, IChannel e)
        {
            var c = sender as IClientChannel;
            c.UnbindAllEvents(clientHandlers);
            e.BindAllEvents(clientHandlers);
        }

        private void errorOccuredHandler(object sender, Exception e)
        {
            OnErrorOccured(e);
        }

        private void client_RequestSent(object sender, DataEventArgs e)
        {
            OnRequestSent(e.Data, e.State);
        }

        private void server_ResponseSent(object sender, DataEventArgs e)
        {
            OnResponseSent(e.Data, e.State);
        }

        private void channelClosedHandler(object sender)
        {
            Close();
        }

        public void Close()
        {
            if (!closed)
            {
                closed = true;
                clientChannel.Close();
                serverChannel.Close();
                OnChannelClosed();
            }
        }

        public void SendToClient(IDataStream data)
        {
            serverChannel.Send(data);
        }

        public void SendToServer(IDataStream data)
        {
            clientChannel.Send(data);
        }

        /// <summary>
        /// Use this method to replace inner channels
        /// </summary>
        /// <param name="serverChannelCallback"></param>
        /// <param name="clientChannelCallback"></param>
        public void ReplaceInnerChannels(ChannelCallback<IServerChannel> serverChannelCallback, ChannelCallback<IClientChannel> clientChannelCallback)
        {
            serverChannelCallback(serverChannel);
            clientChannelCallback(clientChannel);
        }

        private void server_RequestReceived(object sender, DataEventArgs args)
        {
            OnRequestReceived(args.Data, args.State);
        }

        private void client_ResponseReceived(object sender, DataEventArgs e)
        {
            OnResponseReceived(e.Data, e.State);
        }

        protected virtual void OnRequestReceived(IDataStream request, ICloneable state)
        {
            if (RequestReceived != null) RequestReceived(this, new DataEventArgs { Data = request, State = state });
        }

        protected virtual void OnRequestSent(IDataStream request, ICloneable state)
        {
            if (RequestSent != null) RequestSent(this, new DataEventArgs { Data = request, State = state });
        }

        protected virtual void OnResponseReceived(IDataStream response, ICloneable state)
        {
            if (ResponseReceived != null) ResponseReceived(this, new DataEventArgs { Data = response, State = state });
        }

        protected virtual void OnResponseSent(IDataStream response, ICloneable state)
        {
            if (ResponseSent != null) ResponseSent(this, new DataEventArgs { Data = response, State = state });
        }
    }

    [Serializable]
    public class DefaultProxySettings
    {
        public IServer Server;
        public IClientFactory ClientFactory;
    }

    /// <summary>
    /// Default proxy class
    /// </summary>
    /// <remarks>
    /// If you use this proxy with a IReplaceableChannel then make sure that channel type stays
    /// even after it is replaced.
    /// </remarks>
    [Serializable]
    public class DefaultProxy : BaseInstance, IProxy
    {
        [field: NonSerialized]
        public event EventHandler<IProxyChannel> ChannelCreated;

        protected ConcurrentDictionary<int, IProxyChannel> channels = new ConcurrentDictionary<int, IProxyChannel>();
        protected DefaultProxySettings settings;
        public object Settings { get { return settings; } }
        protected IServer server { get { return settings.Server; } }
        protected IClientFactory clientFactory { get { return settings.ClientFactory; } }

        public bool IsStarted { get { return server.IsStarted; } }

        public DefaultProxy(DefaultProxySettings settings)
        {
            this.settings = settings;
            settings.Server.ErrorOccured += handleErrorOccured;
        }

        public virtual void Start()
        {
            server.ChannelCreated += connectionCreatedHandler;
            server.Start();
        }

        public virtual void Stop()
        {
            server.ChannelCreated -= connectionCreatedHandler;
            foreach (var channel in channels)
            {
                channel.Value.ChannelClosed -= channelClosedHandler;
                channel.Value.Close();
            }
            channels.Clear();
            server.Stop();
        }

        private void connectionCreatedHandler(object sender, IServerChannel channel)
        {
            var pchannel = new DefaultProxyChannel(clientFactory.CreateClient(), channel);
            pchannel.ChannelClosed += channelClosedHandler;
            channels.TryAdd(pchannel.ID, pchannel);
            OnChannelCreated(pchannel);
            pchannel.raiseChannelReady();
        }

        private void channelClosedHandler(object channel)
        {
            IProxyChannel c;
            channels.TryRemove(((IProxyChannel)channel).ID, out c);
        }

        protected virtual void OnChannelCreated(IProxyChannel channel)
        {
            if (ChannelCreated != null) ChannelCreated(this, channel);
        }

        private void handleErrorOccured(object sender, Exception e)
        {
            OnErrorOccured(e);
        }
    }
}