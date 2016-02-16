using Netool.Logging;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using Netool.Network.Helpers;
using Netool.Network.Tcp;
using Netool.Plugins.Http;
using System;

namespace Netool.Network.Http
{
    [Serializable]
    public class HttpServerSettings
    {
        public TcpServerSettings TcpSettings;
    }

    [Serializable]
    public class HttpServerChannel : BaseServerChannel, IServerChannel, IReplaceableChannel
    {
        private LockableServerChannel channel;

        [NonSerialized]
        private HttpMessageParser parser;

        [NonSerialized]
        private InstanceLogger logger;

        [field:NonSerialized]
        public event EventHandler<IChannel> ChannelReplaced;

        [NonSerialized]
        private IChannelExtensions.ChannelHandlers handlers;

        public HttpServerChannel(IServerChannel channel, InstanceLogger logger)
        {
            if (channel.GetType() != typeof(LockableServerChannel)) channel = new LockableServerChannel(channel);
            this.channel = (LockableServerChannel)channel;
            this.id = channel.ID;
            this.name = channel.Name;
            handlers = new IChannelExtensions.ChannelHandlers
            {
                ChannelClosed = channelClosedHandler,
                ChannelReady = channelReadyHandler,
                RequestReceived = requestReceivedHandler,
                ResponseSent = responseSentHandler,
                ErrorOccured = errorOccuredHandler,
            };
            channel.BindAllEvents(handlers);
            parser = new HttpMessageParser(logger, false);
            this.logger = logger;
        }

        private void errorOccuredHandler(object sender, Exception e)
        {
            OnErrorOccured(e);
        }

        private void requestReceivedHandler(object sender, DataEventArgs e)
        {
            if (e.Data == null) return;
            lock (parser)
            {
                HttpData data = null;
                try
                {
                    data = parser.Receive(e.Data);
                }
                catch
                {
                    Close();
                    return;
                }
                if (data != null)
                {
                    OnRequestReceived(data);
                    parser = new HttpMessageParser(logger, false);
                }
            }
        }

        private void responseSentHandler(object sender, DataEventArgs e)
        {
            OnResponseSent(e);
        }

        private void channelReadyHandler(object sender)
        {
            OnChannelReady();
        }

        private void channelClosedHandler(object sender)
        {
            OnChannelClosed();
        }

        /// <inheritdoc/>
        public void Send(IDataStream response)
        {
            channel.Send(response);
        }

        /// <inheritdoc/>
        public void Close()
        {
            channel.Close();
        }

        public void UpgradeProtocol(IProtocolUpgrader upgrader, IDataStream switchHeader = null)
        {
            channel.Lock();
            channel.UnbindAllEvents(handlers);
            if(switchHeader != null)
            {
                channel.Send(switchHeader);
            }
            var newChannel = upgrader.UpgradeServerChannel(channel, logger);
            if(ChannelReplaced != null)
            {
                ChannelReplaced(this, newChannel);
            }
            channel.Unlock();
        }
    }

    [Serializable]
    public class HttpServer : BaseInstance, IServer
    {
        protected HttpServerSettings setttings;
        protected TcpServer server;

        /// <inheritdoc/>
        public bool IsStarted { get { return server.IsStarted; } }

        /// <inheritdoc/>
        public object Settings { get { return setttings; } }

        /// <inheritdoc/>
        [field: NonSerialized]
        public event EventHandler<IServerChannel> ChannelCreated;

        [NonSerialized]
        private InstanceLogger logger;

        public HttpServer(HttpServerSettings settings, InstanceLogger logger)
        {
            this.setttings = settings;
            server = new TcpServer(settings.TcpSettings);
            server.ChannelCreated += channelCreatedHandler;
            server.ErrorOccured += server_ErrorOccured;
            this.logger = logger;
        }

        private void server_ErrorOccured(object sender, Exception e)
        {
            OnErrorOccured(e);
        }

        private void channelCreatedHandler(object sender, IServerChannel e)
        {
            var channel = new HttpServerChannel(e, logger);
            if (ChannelCreated != null) ChannelCreated(this, channel);
        }

        /// <inheritdoc/>
        public void Start()
        {
            server.Start();
        }

        /// <inheritdoc/>
        public void Stop()
        {
            server.Stop();
        }
    }
}