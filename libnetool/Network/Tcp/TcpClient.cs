﻿using Netool.Network.DataFormats;
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
    }

    [Serializable]
    public class TcpClientChannel : BaseClientChannel, IClientChannel
    {
        [NonSerialized]
        protected Socket socket;
        private object stopLock = new object();

        public int ReceiveBufferSize { get; set; }

        public TcpClientChannel(Socket socket, int id, int receiveBufferSize = 8192)
        {
            this.socket = socket;
            this.id = id;
            name = socket.LocalEndPoint.ToString();
            ReceiveBufferSize = receiveBufferSize;
            Debug.WriteLine("TcpClientChannel created: id {0}, name {1}", id, name);
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
            catch (Exception e)
            {
                OnErrorOccured(e);
                Close();
            }
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
            catch(Exception e)
            {
                OnErrorOccured(e);
                Close();
            }
            if (bytesRead > 0)
            {
                Debug.WriteLine("TcpClientChannel (id: {0}, name: {1}) received {2} bytes", ID, Name, bytesRead);
                var response = processResponse(s.Buffer, bytesRead);
                OnResponseReceived(response);
                scheduleNextReceive();
            }
            else
            {
                Debug.WriteLine("TcpClientChannel (id: {0}, name: {1}) received 0 bytes - closing", ID, Name);
                Close();
            }
        }

        private IDataStream processResponse(byte[] response, int length)
        {
            return new ByteArray(response, 0, length);
        }

        /// <inheritdoc />
        public void Send(IDataStream request)
        {
            try
            {
                TcpHelpers.Send(socket, request);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch(Exception e)
            {
                OnErrorOccured(e);
                return;
            }

            OnRequestSent(request);
        }

        /// <inheritdoc />
        public void Close()
        {
            lock (stopLock)
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
                catch(Exception e)
                {
                    OnErrorOccured(e);
                    return;
                }
                Debug.WriteLine("TcpClientChannel (id: {0}, name: {1}) calling OnChannelClosed", ID, Name);
                OnChannelClosed();
                Debug.WriteLine("TcpClientChannel (id: {0}, name: {1}) OnChannelClosed finished", ID, Name);
            }
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
            if (ChannelCreated != null) ChannelCreated(this, channel);
        }

        private void channelClosedHandler(object channel)
        {
            Debug.WriteLine("TcpClient.channelClosedHandler");
            Stop();
        }
    }
}