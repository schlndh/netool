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

    public class UdpClient : BaseClient, IClient
    {
        protected UdpClientSettings settings;
        protected Socket socket;
        private volatile bool stopped = true;

        public int ReceiveBufferSize { get; set; }

        public UdpClient(UdpClientSettings settings)
        {
            this.settings = settings;
            ReceiveBufferSize = 2048;
        }

        public void Start()
        {
            if (stopped)
            {
                stopped = false;
                socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
                socket.Bind(settings.LocalEndPoint);
                scheduleNextReceive();
            }
        }

        public void Stop()
        {
            if (!stopped)
            {
                stopped = true;
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (ObjectDisposedException) { }
            }
        }

        public void Send(IByteArrayConvertible request)
        {
            try
            {
                socket.SendTo(request.ToByteArray(), settings.RemoteEndPoint);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            OnRequestSent(request);
        }

        private void scheduleNextReceive()
        {
            var s = new ReceiveStateObject(ReceiveBufferSize);
            EndPoint ep = settings.RemoteEndPoint;
            try
            {
                socket.BeginReceiveFrom(s.Buffer, 0, s.Buffer.Length, SocketFlags.None, ref ep, handleResponse, s);
            }
            catch (ObjectDisposedException) { }
        }

        private void handleResponse(IAsyncResult ar)
        {
            var s = (ReceiveStateObject)ar.AsyncState;
            EndPoint ep = settings.RemoteEndPoint;
            int bytesRead;
            try
            {
                bytesRead = socket.EndReceiveFrom(ar, ref ep);
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
    }
}