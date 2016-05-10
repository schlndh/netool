using Netool.Network.DataFormats;
using Netool.Network.Helpers;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Netool.Network.Tcp
{
    [Serializable]
    public class TcpClientSettings
    {
        public IPEndPoint LocalEndPoint;
        public IPEndPoint RemoteEndPoint;
        public SocketProperties Properties;

        public override string ToString()
        {
            return LocalEndPoint.ToString() + "->" + RemoteEndPoint.ToString() + ", SocketProperties=" + Properties.ToString();
        }
    }

    [Serializable]
    public class TcpClientChannel : BaseClientChannel, IClientChannel
    {
        private BaseTcpChannel baseChannel;

        public int ReceiveBufferSize { get; set; }

        public TcpClientChannel(Socket socket, int id, int receiveBufferSize = 8192)
        {
            this.baseChannel = new BaseTcpChannel(socket, OnResponseReceived, OnRequestSent, OnErrorOccured, OnChannelClosed, receiveBufferSize);
            this.id = id;
            name = socket.LocalEndPoint.ToString();
            // don't call scheduleNextReceive right away, the ChannelCreated event must be raised first
        }

        /// <summary>
        /// Never call this method directly unless when you're creating a object of this class manually,
        /// then call it after raising a ChannelCreated event
        /// </summary>
        internal void scheduleNextReceive()
        {
            baseChannel.scheduleNextReceive();
        }

        /// <inheritdoc />
        public void Send(IDataStream request)
        {
            baseChannel.Send(request);
        }

        /// <inheritdoc />
        public void Close()
        {
            baseChannel.Close();
        }
    }

    [Serializable]
    public class TcpClient : BaseInstance, IClient
    {
        protected TcpClientSettings settings;
        /// <inheritdoc />
        public object Settings { get { return settings; } }
        protected TcpClientChannel channel;
        private volatile bool stopped = true;
        private int channelID = 0;
        private object stopLock = new object();

        /// <inheritdoc />
        [field: NonSerialized]
        public event EventHandler<IClientChannel> ChannelCreated;

        public int ReceiveBufferSize { get; set; }
        /// <inheritdoc />
        public bool IsStarted { get { return !stopped; } }

        public TcpClient(TcpClientSettings settings)
        {
            this.settings = settings;
        }

        /// <inheritdoc />
        public IClientChannel Start()
        {
            if (stopped)
            {
                try
                {
                    stopped = false;
                    var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(settings.LocalEndPoint);
                    socket.Connect(settings.RemoteEndPoint);
                    settings.Properties.Apply(socket);
                    channel = new TcpClientChannel(socket, Interlocked.Increment(ref channelID), settings.Properties.ReceiveBufferSize);
                    OnChannelCreated(channel);
                    channel.ChannelClosed += channelClosedHandler;
                    channel.raiseChannelReady();
                    channel.scheduleNextReceive();
                }
                catch (Exception e)
                {
                    stopped = true;
                    OnErrorOccured(e);
                }
            }
            return channel;
        }

        /// <inheritdoc />
        public void Stop()
        {
            Debug.WriteLine("TcpClient stopping");
            lock(stopLock)
            {
                if (!stopped)
                {
                    stopped = true;
                    if (channel != null)
                    {
                        channel.ChannelClosed -= channelClosedHandler;
                        channel.Close();
                    }
                }
            }
            Debug.WriteLine("TcpClient stopped");
        }

        private void OnChannelCreated(IClientChannel channel)
        {
            var ev = ChannelCreated;
            if (ev != null) ev(this, channel);
        }

        private void channelClosedHandler(object channel)
        {
            Debug.WriteLine("TcpClient.channelClosedHandler");
            Stop();
        }
    }
}