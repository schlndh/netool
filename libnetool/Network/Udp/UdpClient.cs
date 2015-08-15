using Netool.Network.DataFormats;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Netool.Network.Udp
{
    [Serializable]
    public class UdpClientSettings
    {
        public IPEndPoint LocalEndPoint;
        public IPEndPoint RemoteEndPoint;
    }

    [Serializable]
    public class UdpClientChannel : BaseClientChannel, IClientChannel
    {
        [NonSerialized]
        protected Socket socket;
        protected EndPoint remoteEP;
        public int ReceiveBufferSize { get; set; }
        private object stopLock = new object();

        public UdpClientChannel(Socket socket, EndPoint remoteEP, int id, int receiveBufferSize = 2048)
        {
            this.socket = socket;
            this.remoteEP = remoteEP;
            this.id = id;
            name = remoteEP.ToString();
            ReceiveBufferSize = receiveBufferSize;
            // don't call scheduleNextReceive right away, the ChannelCreated event must be raised first
        }

        /// <summary>
        /// Never call this method directly unless when you're creating a object of this class manually,
        /// then call it after raising a ChannelCreated event
        /// </summary>
        public void scheduleNextReceive()
        {
            // this is purposefully using the private method naming convetion
            var s = new ReceiveStateObject(ReceiveBufferSize);
            try
            {
                socket.BeginReceiveFrom(s.Buffer, 0, s.Buffer.Length, SocketFlags.None, ref remoteEP, handleResponse, s);
            }
            catch (ObjectDisposedException) { }
        }

        /// <inheritdoc/>
        public void Send(IDataStream request)
        {
            try
            {
                // TODO: improve this
                socket.SendTo(request.ReadBytes(0, request.Length), remoteEP);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            OnRequestSent(request);
        }

        private void handleResponse(IAsyncResult ar)
        {
            var s = (ReceiveStateObject)ar.AsyncState;
            int bytesRead;
            try
            {
                bytesRead = socket.EndReceiveFrom(ar, ref remoteEP);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            scheduleNextReceive();
            if (bytesRead > 0)
            {
                var response = processResponse(s.Buffer, bytesRead);
                OnResponseReceived(response);
            }
        }

        private IDataStream processResponse(byte[] response, int length)
        {
            byte[] arr = new byte[length];
            Array.Copy(response, arr, length);
            return new ByteArray(arr);
        }

        /// <inheritdoc/>
        public void Close()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                OnChannelClosed();
            }
            catch (ObjectDisposedException) { }
        }
    }

    [Serializable]
    public class UdpClient : IClient
    {
        protected UdpClientSettings settings;
        /// <inheritdoc/>
        public object Settings { get { return settings; } }
        protected UdpClientChannel channel;
        private volatile bool stopped = true;

        public int ReceiveBufferSize { get; set; }
        /// <inheritdoc/>
        public bool IsStarted { get { return !stopped; } }
        private object stopLock = new object();

        /// <inheritdoc/>
        [field: NonSerialized]
        public event EventHandler<IClientChannel> ChannelCreated;
        private int channelID = 0;

        public UdpClient(UdpClientSettings settings)
        {
            this.settings = settings;
            ReceiveBufferSize = 2048;
        }

        /// <inheritdoc/>
        public IClientChannel Start()
        {
            lock(stopLock)
            {
                if (stopped)
                {
                    stopped = false;
                    var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
                    socket.Bind(settings.LocalEndPoint);
                    channel = new UdpClientChannel(socket, settings.RemoteEndPoint, Interlocked.Increment(ref channelID), ReceiveBufferSize);
                    channel.ChannelClosed += channelClosedHandler;
                    OnChannelCreated(channel);
                    channel.raiseChannelReady();
                    channel.scheduleNextReceive();
                }
                return channel;
            }
        }

        void channelClosedHandler(object sender)
        {
            channel.ChannelClosed -= channelClosedHandler;
            Stop();
        }

        /// <inheritdoc/>
        public void Stop()
        {
            lock (stopLock)
            {
                if (!stopped)
                {
                    stopped = true;
                    if (channel != null) channel.Close();
                }
            }
        }

        private void OnChannelCreated(IClientChannel channel)
        {
            if (ChannelCreated != null) ChannelCreated(this, channel);
        }
    }
}