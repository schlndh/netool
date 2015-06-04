using Netool.Network.DataFormats;
using Netool.Settings;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Netool.Network.Tcp
{
    internal class ReceiveStateObject
    {
        public Socket Client;
        public byte[] Buffer;

        public ReceiveStateObject(Socket client, int bufferSize)
        {
            Client = client;
            Buffer = new byte[bufferSize];
        }
    }

    public class TcpServerSettings : BaseSettings
    {
        public IPEndPoint LocalEndPoint;
        public int MaxConnections;
    }

    public class TcpServerChannel : BaseServerChannel, IServerChannel
    {
        protected Socket socket;
        public int ReceiveBufferSize { get; set; }
        public TcpServerChannel(Socket socket, int receiveBufferSize = 2048)
        {
            this.socket = socket;
            id = socket.RemoteEndPoint.ToString();
            ReceiveBufferSize = receiveBufferSize;
            scheduleNextReceive();
        }

        private void scheduleNextReceive()
        {
            var s = new ReceiveStateObject(socket, ReceiveBufferSize);
            try
            {
                socket.BeginReceive(s.Buffer, 0, s.Buffer.Length, SocketFlags.None, handleRequest, s);
            }
            catch (ObjectDisposedException)
            { }
        }

        private void handleRequest(IAsyncResult ar)
        {
            var stateObject = (ReceiveStateObject)ar.AsyncState;
            Socket client = stateObject.Client;
            int bytesRead = 0;
            try
            {
                bytesRead = client.EndReceive(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            if (bytesRead > 0)
            {
                IByteArrayConvertible processed = processRequest(stateObject.Buffer, bytesRead);
                OnRequestReceived(processed);
                scheduleNextReceive();
            }
            else
            {
                Close();
            }
        }

        private IByteArrayConvertible processRequest(byte[] data, int length)
        {
            byte[] arr = new byte[length];
            Array.Copy(data, arr, length);
            return new ByteArray(arr);
        }

        public void Send(IByteArrayConvertible response)
        {
            try
            {
                socket.Send(response.ToByteArray());
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            OnResponseSent(response);
        }
        public void Close()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                OnChannelClosed();
            }
            catch (ObjectDisposedException)
            {
                // already closed
            }
        }
    }
    public class TcpServer : IServer
    {
        protected class ClientData
        {
            public Socket Socket;
        }

        protected TcpServerSettings settings;
        protected Socket socket;
        private volatile bool stopped = true;
        private ConcurrentDictionary<string, IServerChannel> channels = new ConcurrentDictionary<string, IServerChannel>();
        public int ReceiveBufferSize { get; set; }

        public event EventHandler<IServerChannel> ChannelCreated;

        public TcpServer(TcpServerSettings settings)
        {
            this.settings = settings;
            ReceiveBufferSize = 2048;
        }

        public void Stop()
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

        public void Start()
        {
            if (stopped)
            {
                stopped = false;
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(settings.LocalEndPoint);
                socket.Listen(settings.MaxConnections);
                try
                {
                    socket.BeginAccept(new AsyncCallback(acceptRequest), socket);
                }
                catch (ObjectDisposedException)
                {
                    // socket closed
                    return;
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
                srv.BeginAccept(new AsyncCallback(acceptRequest), srv);
            }
            catch (ObjectDisposedException)
            {
                // socket closed
                return;
            }
            var channel = new TcpServerChannel(client, ReceiveBufferSize);
            channel.ChannelClosed += channelClosedHandler;
            channels.TryAdd(channel.ID,channel);
            OnChannelCreated(channel);
        }

        private void OnChannelCreated(IServerChannel channel)
        {
            if (ChannelCreated != null) ChannelCreated(this, channel);
        }

        private void channelClosedHandler(object channel)
        {
            IServerChannel c;
            channels.TryRemove(((IServerChannel)channel).ID, out c);
        }

        public bool TryGetByID(string ID, out IServerChannel c)
        {
            return channels.TryGetValue(ID, out c);
        }
    }
}