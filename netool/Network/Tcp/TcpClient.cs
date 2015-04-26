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

    internal class TcpClient : BaseClient, IClient
    {
        protected TcpClientSettings settings;
        protected Socket socket;
        protected volatile bool stopped = true;

        public int ReceiveBufferSize { get; set; }

        public TcpClient(TcpClientSettings settings)
        {
            this.settings = settings;
            ReceiveBufferSize = 2048;
        }

        public void Start()
        {
            if (stopped)
            {
                stopped = false;
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(settings.LocalEndPoint);
                socket.Connect(settings.RemoteEndPoint);
                OnConnectionCreated();
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
                catch (ObjectDisposedException)
                {
                    // already closed
                    return;
                }

                OnConnectionClosed();
            }
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

        private void scheduleNextReceive()
        {
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
                Stop();
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