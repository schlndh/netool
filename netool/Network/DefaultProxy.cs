using Netool.ChannelDrivers;
using System;
using System.Collections.Concurrent;

namespace Netool.Network
{
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

        public event RequestReceivedHandler RequestReceived;
        public event RequestSentHandler RequestSent;
        public event ResponseSentHandler ResponseSent;
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

        public void SendToClient(IByteArrayConvertible data)
        {
            serverChannel.Send(data);
        }

        public void SendToServer(IByteArrayConvertible data)
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

        protected virtual void OnRequestReceived(IByteArrayConvertible request, ICloneable state)
        {
            if (RequestReceived != null) RequestReceived(this, new DataEventArgs { Data = request, State = state });
        }

        protected virtual void OnRequestSent(IByteArrayConvertible request, ICloneable state)
        {
            if (RequestSent != null) RequestSent(this, new DataEventArgs { Data = request, State = state });
        }

        protected virtual void OnResponseReceived(IByteArrayConvertible response, ICloneable state)
        {
            if (ResponseReceived != null) ResponseReceived(this, new DataEventArgs { Data = response, State = state });
        }

        protected virtual void OnResponseSent(IByteArrayConvertible response, ICloneable state)
        {
            if (ResponseSent != null) ResponseSent(this, new DataEventArgs { Data = response, State = state });
        }
    }

    public class DefaultProxy : IProxy
    {
        public event EventHandler<IProxyChannel> ChannelCreated;

        protected ConcurrentDictionary<int, IProxyChannel> channels = new ConcurrentDictionary<int, IProxyChannel>();
        protected IServer server;
        protected IClientFactory clientFactory;

        public bool IsStarted { get { return server.IsStarted; } }

        public DefaultProxy(IServer server, IClientFactory clientFactory)
        {
            this.server = server;
            this.clientFactory = clientFactory;
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