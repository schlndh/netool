using Netool.Network.DataFormats;
using System;
using System.Net;
using System.Net.Sockets;

namespace Netool.Network.Udp
{
    public class UdpClientSettings
    {
        public IPEndPoint LocalEndPoint;
        public IPEndPoint RemoteEndPoint;
    }

    public class UdpClientChannel : BaseClientChannel, IClientChannel
    {
        protected Socket socket;
        protected EndPoint remoteEP;
        public int ReceiveBufferSize { get; set; }

        public UdpClientChannel(Socket socket, EndPoint remoteEP, int receiveBufferSize = 2048)
        {
            this.socket = socket;
            this.remoteEP = remoteEP;
            id = remoteEP.ToString();
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

        public void Send(IByteArrayConvertible request)
        {
            try
            {
                socket.SendTo(request.ToByteArray(), remoteEP);
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

        private IByteArrayConvertible processResponse(byte[] response, int length)
        {
            byte[] arr = new byte[length];
            Array.Copy(response, arr, length);
            return new ByteArray(arr);
        }

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
    public class UdpClient : IClient
    {
        protected UdpClientSettings settings;
        private UdpClientChannel channel;
        private volatile bool stopped = true;

        public int ReceiveBufferSize { get; set; }
        public event EventHandler<IClientChannel> ChannelCreated;

        public UdpClient(UdpClientSettings settings)
        {
            this.settings = settings;
            ReceiveBufferSize = 2048;
        }

        public IClientChannel Start()
        {
            if (stopped)
            {
                stopped = false;
                var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
                socket.Bind(settings.LocalEndPoint);
                channel = new UdpClientChannel(socket, settings.RemoteEndPoint, ReceiveBufferSize);
                channel.ChannelClosed += channelClosedHandler;
                OnChannelCreated(channel);
                channel.scheduleNextReceive();
            }
            return channel;
        }

        void channelClosedHandler(object sender)
        {
            channel.ChannelClosed -= channelClosedHandler;
            Stop();
        }

        public void Stop()
        {
            if (!stopped)
            {
                stopped = true;
                channel.Close();
                channel = null;
            }
        }

        private void OnChannelCreated(IClientChannel channel)
        {
            if (ChannelCreated != null) ChannelCreated(this, channel);
        }
    }
}