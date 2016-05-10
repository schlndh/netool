using Netool.Network.DataFormats;
using Netool.Network.Helpers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;

namespace Netool.Network.Tcp
{
    [Serializable]
    public class TcpServerSettings
    {
        public IPEndPoint LocalEndPoint;
        public int MaxPendingConnections;
        public SocketProperties Properties;

        public override string ToString()
        {
            return LocalEndPoint.ToString() + ", SocketProperties=" + Properties.ToString();
        }
    }

    [Serializable]
    public class TcpServerChannel : BaseServerChannel, IServerChannel
    {
        private BaseTcpChannel baseChannel;
        public int ReceiveBufferSize { get { return baseChannel.ReceiveBufferSize; } set { baseChannel.ReceiveBufferSize = value; } }

        public TcpServerChannel(Socket socket, int id, int receiveBufferSize = 8192)
        {
            this.baseChannel = new BaseTcpChannel(socket, OnRequestReceived, OnResponseSent, OnErrorOccured, OnChannelClosed, receiveBufferSize);
            this.id = id;
            name = socket.RemoteEndPoint.ToString();
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
        public void Send(IDataStream response)
        {
            baseChannel.Send(response);
        }

        /// <inheritdoc />
        public void Close()
        {
            baseChannel.Close();
        }
    }

    [Serializable]
    public class TcpServer : BaseInstance, IServer
    {
        protected class ClientData
        {
            public Socket Socket;
        }

        protected TcpServerSettings settings;
        /// <inheritdoc />
        public object Settings { get { return settings; } }
        [NonSerialized]
        private Socket socket;
        private volatile bool stopped = true;
        private ConcurrentDictionary<int, IServerChannel> channels = new ConcurrentDictionary<int, IServerChannel>();
        [NonSerialized]
        private int channelID = 0;
        private object stopLock = new object();
        public int ReceiveBufferSize { get; set; }
        /// <inheritdoc />
        public bool IsStarted { get { return !stopped; } }

        /// <inheritdoc />
        [field: NonSerialized]
        public event EventHandler<IServerChannel> ChannelCreated;

        public TcpServer(TcpServerSettings settings)
        {
            this.settings = settings;
            ReceiveBufferSize = 8192;
        }

        /// <inheritdoc />
        public void Stop()
        {
            lock(stopLock)
            {
                if (!stopped)
                {
                    stopped = true;
                    foreach (var channel in channels)
                    {
                        channel.Value.ChannelClosed -= channelClosedHandler;
                        channel.Value.Close();
                    }
                    channels.Clear();
                    socket.Close();
                }
            }
        }

        /// <inheritdoc />
        public void Start()
        {
            lock(stopLock)
            {
                if (stopped)
                {
                    stopped = false;
                    socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        socket.Bind(settings.LocalEndPoint);
                        socket.Listen(settings.MaxPendingConnections);
                        settings.Properties.Apply(socket);
                        socket.BeginAccept(new AsyncCallback(acceptRequest), socket);
                    }
                    catch (ObjectDisposedException)
                    {
                        // socket closed
                        return;
                    }
                    catch (Exception e)
                    {
                        stopped = true;
                        OnErrorOccured(e);
                    }
                }
            }
        }

        private void acceptRequest(IAsyncResult ar)
        {
            Socket client;
            Socket srv = (Socket)ar.AsyncState;
            try
            {
                client = srv.EndAccept(ar);
            }
            catch (ObjectDisposedException)
            {
                // socket closed
                return;
            }
            catch (Exception e)
            {
                OnErrorOccured(e);
                return;
            }
            settings.Properties.Apply(client);
            var channel = new TcpServerChannel(client, Interlocked.Increment(ref channelID), settings.Properties.ReceiveBufferSize);
            channel.ChannelClosed += channelClosedHandler;
            channels.TryAdd(channel.ID,channel);
            OnChannelCreated(channel);
            channel.raiseChannelReady();
            channel.scheduleNextReceive();
            try
            {
                srv.BeginAccept(new AsyncCallback(acceptRequest), srv);
            }
            catch (ObjectDisposedException)
            {
                // socket closed
                return;
            }
            catch (Exception e)
            {
                OnErrorOccured(e);
                return;
            }
        }

        private void OnChannelCreated(IServerChannel channel)
        {
            var ev = ChannelCreated;
            if (ev != null) ev(this, channel);
        }

        private void channelClosedHandler(object channel)
        {
            IServerChannel c;
            channels.TryRemove(((IServerChannel)channel).ID, out c);
        }

        [OnDeserialized]
        private void onDeserialized(StreamingContext ctx)
        {
            channelID = 0;
        }
    }
}