using Netool.Network.DataFormats;
using Netool.Network.DataFormats.Http;
using Netool.Network.Tcp;
using System;

namespace Netool.Network.Http
{
    [Serializable]
    public class HttpServerSettings
    {
        public TcpServerSettings TcpSettings;
    }

    [Serializable]
    public class HttpServerChannel : BaseServerChannel, IServerChannel
    {
        private IServerChannel channel;
        [NonSerialized]
        private HttpMessageParser parser = new HttpMessageParser(false);

        public HttpServerChannel(IServerChannel channel)
        {
            this.channel = channel;
            this.id = channel.ID;
            this.name = channel.Name;
            channel.ChannelClosed += channelClosedHandler;
            channel.ChannelReady += channelReadyHandler;
            channel.RequestReceived += requestReceivedHandler;
            channel.ResponseSent += responseSentHandler;
            channel.ErrorOccured += channel_ErrorOccured;
        }

        private void channel_ErrorOccured(object sender, Exception e)
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
                    parser = new HttpMessageParser(false);
                }
            }
        }

        private void responseSentHandler(object sender, DataEventArgs e)
        {
            OnResponseSent(e.Data, e.State);
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
    }

    [Serializable]
    public class HttpServer : BaseInstance, IServer
    {
        protected HttpServerSettings setttings;
        protected TcpServer server;
        private HttpServerChannel channel;

        /// <inheritdoc/>
        public bool IsStarted { get { return server.IsStarted; } }

        /// <inheritdoc/>
        public object Settings { get { return setttings; } }

        /// <inheritdoc/>
        [field: NonSerialized]
        public event EventHandler<IServerChannel> ChannelCreated;

        public HttpServer(HttpServerSettings settings)
        {
            this.setttings = settings;
            server = new TcpServer(settings.TcpSettings);
            server.ChannelCreated += channelCreatedHandler;
            server.ErrorOccured += server_ErrorOccured;
        }

        private void server_ErrorOccured(object sender, Exception e)
        {
            OnErrorOccured(e);
        }

        private void channelCreatedHandler(object sender, IServerChannel e)
        {
            channel = new HttpServerChannel(e);
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