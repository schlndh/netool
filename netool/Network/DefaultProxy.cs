using Netool.ChannelDrivers;
using System;
using System.Collections.Concurrent;

namespace Netool.Network
{
    public class DefaultProxyChannel : BaseChannel, IProxyChannel
    {
        protected IClientChannel clientChannel;
        protected IServerChannel serverChannel;

        public event RequestReceivedHandler RequestReceived;
        public event RequestSentHandler RequestSent;
        public event RequestDroppedHandler RequestDropped;
        public event ResponseSentHandler ResponseSent;
        public event ResponseReceivedHandler ResponseReceived;
        public event ResponseDroppedHandler ResponseDropped;

        public DataModifier RequestModifier { get; set; }
        public DataModifier ResponseModifier { get; set; }

        public new int ID { get { return serverChannel.ID; } }
        public new string Name { get { return serverChannel.Name; } }
        private volatile bool closed = false;

        public DefaultProxyChannel(IClientChannel clChannel, IServerChannel srvChannel)
        {
            clientChannel = clChannel;
            clientChannel.ChannelClosed += channelClosedHandler;
            clientChannel.ResponseReceived += responseReceivedHandler;
            serverChannel = srvChannel;
            serverChannel.ChannelClosed += channelClosedHandler;
            serverChannel.RequestReceived += requestReceivedHandler;
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

        private void requestReceivedHandler(object sender, DataEventArgs args)
        {
            OnRequestReceived(args.Data, args.State);
            IByteArrayConvertible data = args.Data;
            if (RequestModifier != null)
            {
                data = RequestModifier(args.Data);
                if (data == null)
                {
                    OnRequestDropped(args.Data, args.State);
                    return;
                }
            }
            clientChannel.Send(data);
            OnRequestSent(data, args.State);
        }

        private void responseReceivedHandler(object sender, DataEventArgs e)
        {
            OnResponseReceived(e.Data, e.State);
            IByteArrayConvertible data = e.Data;
            if (ResponseModifier != null)
            {
                data = ResponseModifier(e.Data);
                if (data == null)
                {
                    OnResponseDropped(e.Data, e.State);
                    return;
                }
            }
            serverChannel.Send(data);
            OnResponseSent(data, e.State);
        }

        protected virtual void OnRequestReceived(IByteArrayConvertible request, ICloneable state)
        {
            if (RequestReceived != null) RequestReceived(this, new DataEventArgs { Data = request, State = state });
        }

        protected virtual void OnRequestSent(IByteArrayConvertible request, ICloneable state)
        {
            if (RequestSent != null) RequestSent(this, new DataEventArgs { Data = request, State = state });
        }

        protected virtual void OnRequestDropped(IByteArrayConvertible request, ICloneable state)
        {
            if (RequestDropped != null) RequestDropped(this, new DataEventArgs { Data = request, State = state });
        }

        protected virtual void OnResponseReceived(IByteArrayConvertible response, ICloneable state)
        {
            if (ResponseReceived != null) ResponseReceived(this, new DataEventArgs { Data = response, State = state });
        }

        protected virtual void OnResponseSent(IByteArrayConvertible response, ICloneable state)
        {
            if (ResponseSent != null) ResponseSent(this, new DataEventArgs { Data = response, State = state });
        }

        protected virtual void OnResponseDropped(IByteArrayConvertible response, ICloneable state)
        {
            if (ResponseDropped != null) ResponseDropped(this, new DataEventArgs { Data = response, State = state });
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