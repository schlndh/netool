using Netool.Network.DataFormats;
using Netool.Settings;
using System;
using System.Net;
using System.Net.Sockets;

namespace Netool.Network.Tcp
{
    public class TcpClientSettings : BaseSettings
    {
        public IPEndPoint LocalEndPoint;
        public IPEndPoint RemoteEndPoint;
    }

    public class TcpClientChannel : BaseClientChannel, IClientChannel
    {
        protected Socket socket;

        public int ReceiveBufferSize { get; set; }

        public TcpClientChannel(Socket socket, int receiveBufferSize = 2048)
        {
            this.socket = socket;
            id = socket.LocalEndPoint.ToString();
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
            var s = new ReceiveStateObject(socket, ReceiveBufferSize);
            try
            {
                socket.BeginReceive(s.Buffer, 0, s.Buffer.Length, SocketFlags.None, handleResponse, s);
            }
            catch (ObjectDisposedException) { }
        }
        private void handleResponse(IAsyncResult ar)
        {
            var s = (ReceiveStateObject)ar.AsyncState;
            int bytesRead = 0;
            try
            {
                bytesRead = socket.EndReceive(ar);
            }
            catch (ObjectDisposedException)
            {
                // closed
                return;
            }
            if (bytesRead > 0)
            {
                var response = processResponse(s.Buffer, bytesRead);
                OnResponseReceived(response);
                scheduleNextReceive();
            }
            else
            {
                Close();
            }
        }

        private IByteArrayConvertible processResponse(byte[] response, int length)
        {
            byte[] arr = new byte[length];
            Array.Copy(response, arr, length);
            return new ByteArray(arr);
        }

        public void Send(IByteArrayConvertible request)
        {
            try
            {
                socket.Send(request.ToByteArray());
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            OnRequestSent(request);
        }
        public void Close()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (ObjectDisposedException)
            {
                // already closed
                return;
            }

            OnChannelClosed();
        }
    }
    public class TcpClient : IClient
    {
        protected TcpClientSettings settings;
        protected TcpClientChannel channel;
        protected volatile bool stopped = true;

        public event EventHandler<IClientChannel> ChannelCreated;

        public int ReceiveBufferSize { get; set; }

        public TcpClient(TcpClientSettings settings)
        {
            this.settings = settings;
            ReceiveBufferSize = 2048;
        }

        public IClientChannel Start()
        {
            if (stopped)
            {
                stopped = false;
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(settings.LocalEndPoint);
                socket.Connect(settings.RemoteEndPoint);
                channel = new TcpClientChannel(socket, ReceiveBufferSize);
                channel.ChannelClosed += channelClosedHandler;
                OnChannelCreated(channel);
                channel.scheduleNextReceive();
            }
            return channel;
        }

        public void Stop()
        {
            if (!stopped)
            {
                stopped = true;
                if (channel != null)
                {
                    channel.Close();
                    channel = null;
                }
            }
        }
        private void OnChannelCreated(IClientChannel channel)
        {
            if (ChannelCreated != null) ChannelCreated(this, channel);
        }
        private void channelClosedHandler(object channel)
        {
            Stop();
        }
    }
}