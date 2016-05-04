using Netool.Logging;
using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using Netool.Network.Helpers;
using Netool.Network.Tcp;
using System;

namespace Netool.Network.Http
{
    [Serializable]
    public class HttpClientSettings
    {
        public TcpClientSettings TcpSettings;

        public override string ToString()
        {
            if (TcpSettings == null) return "";
            return TcpSettings.ToString();
        }
    }

    [Serializable]
    public class HttpClientChannel : BaseClientChannel, IClientChannel, IReplaceableChannel
    {
        /// <inheritdoc/>
        [field:NonSerialized]
        public event EventHandler<IChannel> ChannelReplaced;

        private LockableClientChannel channel;

        [NonSerialized]
        private HttpMessageParser parser;

        [NonSerialized]
        private InstanceLogger logger;

        [NonSerialized]
        private IChannelExtensions.ChannelHandlers handlers;

        public HttpClientChannel(IClientChannel channel, InstanceLogger logger, HttpServerChannel serverChannel = null)
        {
            var sharedLockChannel = serverChannel != null ? serverChannel.GetInnerChannel() : null;
            if(channel.GetType() != typeof(LockableClientChannel)) channel = new LockableClientChannel(channel, sharedLockChannel);
            this.channel = (LockableClientChannel)channel;
            this.id = channel.ID;
            this.name = channel.Name;
            handlers = new IChannelExtensions.ChannelHandlers
            {
                ChannelClosed = channelClosedHandler,
                ChannelReady = channelReadyHandler,
                ResponseReceived = responseReceivedHandler,
                RequestSent = requestSentHandler,
                ErrorOccured = errorOccuredHandler,
            };
            channel.BindAllEvents(handlers);
            parser = new HttpMessageParser(logger, true);
            this.logger = logger;
        }

        private void errorOccuredHandler(object sender, Exception e)
        {
            OnErrorOccured(e);
        }

        private void responseReceivedHandler(object sender, DataEventArgs e)
        {
            if (e.Data == null) return;
            lock(parser)
            {
                IDataStream data = null;
                try
                {
                    data = parser.Receive(e.Data);
                }
                catch
                {
                    data = parser.GetRawData();
                    parser = new HttpMessageParser(logger, true);
                }
                if(data != null)
                {
                    OnResponseReceived(data);
                    parser = new HttpMessageParser(logger, true);
                }
            }
        }

        private void requestSentHandler(object sender, DataEventArgs e)
        {
            OnRequestSent(e.Data, e.State);
        }

        private void channelReadyHandler(object sender)
        {
            OnChannelReady();
        }

        private void channelClosedHandler(object sender)
        {
            lock(parser)
            {
                var data = parser.Close();
                if(data != null)
                {
                    OnResponseReceived(data);
                    parser = new HttpMessageParser(logger, true);
                }
            }
            OnChannelClosed();
        }

        /// <inheritdoc/>
        public void Send(IDataStream request)
        {
            var http = request as HttpData;
            if(http != null)
            {
                parser.LastRequestMethod = http.Method;
            }
            channel.Send(request);
        }

        /// <inheritdoc/>
        public void Close()
        {
            channel.Close();
        }

        public void UpgradeProtocol(IProtocolUpgrader upgrader)
        {
            channel.Lock();
            channel.UnbindAllEvents(handlers);
            var newChannel = upgrader.UpgradeClientChannel(channel, logger);
            newChannel.Driver = Driver;
            if (ChannelReplaced != null)
            {
                ChannelReplaced(this, newChannel);
            }
            channel.Unlock();
        }
    }

    [Serializable]
    public class HttpClient : BaseInstance, IClient
    {
        protected HttpClientSettings setttings;
        protected TcpClient client;
        private HttpClientChannel channel;

        /// <inheritdoc/>
        public bool IsStarted { get { return client.IsStarted; } }

        /// <inheritdoc/>
        public object Settings { get { return setttings; } }

        /// <inheritdoc/>
        [field: NonSerialized]
        public event EventHandler<IClientChannel> ChannelCreated;

        [NonSerialized]
        private InstanceLogger logger;

        [NonSerialized]
        private HttpServerChannel serverChannel = null;

        public HttpClient(HttpClientSettings settings, InstanceLogger logger)
        {
            this.setttings = settings;
            client = new TcpClient(settings.TcpSettings);
            client.ChannelCreated += channelCreatedHandler;
            client.ErrorOccured += client_ErrorOccured;
            this.logger = logger;
        }

        /// <summary>
        /// Initializes HttpClient for use with proxy
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="logger"></param>
        /// <param name="serverChannel"></param>
        public HttpClient(HttpClientSettings settings, InstanceLogger logger, IServerChannel serverChannel)
        {
            this.setttings = settings;
            client = new TcpClient(settings.TcpSettings);
            client.ChannelCreated += channelCreatedHandler;
            client.ErrorOccured += client_ErrorOccured;
            this.logger = logger;
            this.serverChannel = serverChannel as HttpServerChannel;
        }

        private void client_ErrorOccured(object sender, Exception e)
        {
            OnErrorOccured(e);
        }

        private void channelCreatedHandler(object sender, IClientChannel e)
        {
            channel = new HttpClientChannel(e, logger, serverChannel);
            if (ChannelCreated != null) ChannelCreated(this, channel);
        }

        /// <inheritdoc/>
        public IClientChannel Start()
        {
            var c = client.Start();
            return channel;
        }

        /// <inheritdoc/>
        public void Stop()
        {
            client.Stop();
        }
    }
}