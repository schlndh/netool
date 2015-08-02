using Netool.ChannelDrivers;
using Netool.Network.DataFormats;
using System;
using System.Collections.Concurrent;

namespace Netool.Network
{
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

        public new int ID { get { return serverChannel.ID; } }
        public new string Name { get { return serverChannel.Name; } }
        private volatile bool closed = false;

        public DefaultProxyChannel(IClientChannel clChannel, IServerChannel srvChannel)
        {
            clientChannel = clChannel;
            clientChannel.ChannelClosed += channelClosedHandler;
            clientChannel.ResponseReceived += responseReceivedHandler;
            clientChannel.RequestSent += clientChannel_RequestSent;
            serverChannel = srvChannel;
            serverChannel.ChannelClosed += channelClosedHandler;
            serverChannel.RequestReceived += requestReceivedHandler;
            serverChannel.ResponseSent += serverChannel_ResponseSent;
        }

        private void clientChannel_RequestSent(object sender, DataEventArgs e)
        {
            OnRequestSent(e.Data, e.State);
        }

        private void serverChannel_ResponseSent(object sender, DataEventArgs e)
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

        private void requestReceivedHandler(object sender, DataEventArgs args)
        {
            OnRequestReceived(args.Data, args.State);
        }

        private void responseReceivedHandler(object sender, DataEventArgs e)
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

    [Serializable]
    public class DefaultProxy : IProxy
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
            var pchannel = new DefaultProxyChannel(clientFactory.CreateClient().Start(), channel);
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
    }
}