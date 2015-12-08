using Netool.Network.DataFormats;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;

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

    [Serializable]
    public class TcpServerSettings
    {
        public IPEndPoint LocalEndPoint;
        public int MaxPendingConnections;
    }

    [Serializable]
    public class TcpServerChannel : BaseServerChannel, IServerChannel
    {
        [NonSerialized]
        protected Socket socket;
        public int ReceiveBufferSize { get; set; }
        private object stopLock = new object();

        public TcpServerChannel(Socket socket, int id, int receiveBufferSize = 8192)
        {
            this.socket = socket;
            this.id = id;
            name = socket.RemoteEndPoint.ToString();
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
                socket.BeginReceive(s.Buffer, 0, s.Buffer.Length, SocketFlags.None, handleRequest, s);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception e)
            {
                OnErrorOccured(e);
                Close();
            }
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
            catch (Exception e)
            {
                OnErrorOccured(e);
                Close();
            }
            if (bytesRead > 0)
            {
                var processed = processRequest(stateObject.Buffer, bytesRead);
                OnRequestReceived(processed);
                scheduleNextReceive();
            }
            else
            {
                Close();
            }
        }

        private IDataStream processRequest(byte[] data, int length)
        {
            byte[] arr = new byte[length];
            Array.Copy(data, arr, length);
            return new ByteArray(arr);
        }

        /// <inheritdoc />
        public void Send(IDataStream response)
        {
            try
            {
                // TODO: improve this
                socket.Send(response.ReadBytes());
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception e)
            {
                OnErrorOccured(e);
                return;
            }
            OnResponseSent(response);
        }

        /// <inheritdoc />
        public void Close()
        {
            lock(stopLock)
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
                catch (Exception e)
                {
                    OnErrorOccured(e);
                }
            }
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
        protected Socket socket;
        private volatile bool stopped = true;
        private ConcurrentDictionary<int, IServerChannel> channels = new ConcurrentDictionary<int, IServerChannel>();
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
            var channel = new TcpServerChannel(client, Interlocked.Increment(ref channelID), ReceiveBufferSize);
            channel.ChannelClosed += channelClosedHandler;
            channels.TryAdd(channel.ID,channel);
            OnChannelCreated(channel);
            channel.raiseChannelReady();
            channel.scheduleNextReceive();
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
    }
}