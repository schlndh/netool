﻿using Netool.Network.DataFormats;
using Netool.Network.Helpers;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

namespace Netool.Network.Udp
{
    internal class ReceiveStateObject
    {
        public byte[] Buffer;

        public ReceiveStateObject(int bufferSize)
        {
            Buffer = new byte[bufferSize];
        }
    }

    [Serializable]
    public class UdpServerSettings
    {
        public IPEndPoint LocalEndPoint;
        public SocketProperties Properties;

        public override string ToString()
        {
            return LocalEndPoint.ToString() + ", SocketProperties=" + Properties.ToString();
        }
    }

    [Serializable]
    public class UdpServerChannel : BaseServerChannel, IServerChannel
    {
        [NonSerialized]
        private Socket socket;
        private EndPoint remoteEP;
        private bool stopped = false;
        private object stopLock = new object();

        public UdpServerChannel(Socket socket, EndPoint remoteEP, int id)
        {
            this.socket = socket;
            this.remoteEP = remoteEP;
            this.id = id;
            name = remoteEP.ToString();
        }

        /// <inheritdoc/>
        public void Close()
        {
            lock(stopLock)
            {
                if (!stopped)
                {
                    stopped = true;
                    OnChannelClosed();
                }
            }
        }

        /// <inheritdoc/>
        public void Send(IDataStream response)
        {
            try
            {
                // TODO: improve this
                socket.SendTo(response.ReadBytes(), remoteEP);
                OnResponseSent(response);
            }
            catch (ObjectDisposedException)
            { }
            catch(Exception e)
            {
                OnErrorOccured(e);
            }
        }

        internal void InjectRequest(IDataStream request)
        {
            OnRequestReceived(request);
        }
    }

    [Serializable]
    public class UdpServer : BaseInstance, IServer
    {
        protected UdpServerSettings settings;
        /// <inheritdoc/>
        public object Settings { get { return settings; } }
        [NonSerialized]
        private Socket socket;
        private volatile bool stopped = true;
        private ConcurrentDictionary<string, UdpServerChannel> channels = new ConcurrentDictionary<string, UdpServerChannel>();
        public int ReceiveBufferSize { get; set; }
        /// <inheritdoc/>
        public bool IsStarted { get { return !stopped; } }
        private int channelID = 0;
        private object stopLock = new object();

        /// <inheritdoc/>
        [field: NonSerialized]
        public event EventHandler<IServerChannel> ChannelCreated;

        public UdpServer(UdpServerSettings settings)
        {
            this.settings = settings;
            ReceiveBufferSize = 2048;
        }

        /// <inheritdoc/>
        public void Start()
        {
            lock(stopLock)
            {
                if (stopped)
                {
                    stopped = false;
                    socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
                    socket.Bind(settings.LocalEndPoint);
                    if (settings.LocalEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.PacketInformation, true);
                    }
                    else if (settings.LocalEndPoint.AddressFamily == AddressFamily.InterNetwork)
                    {
                        socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
                    }
                    settings.Properties.Apply(socket);
                    scheduleNextReceive();
                }
            }
        }

        /// <inheritdoc/>
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
                    try
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                    catch (ObjectDisposedException)
                    { }
                    catch (Exception e)
                    {
                        OnErrorOccured(e);
                    }
                }
            }
        }

        private void scheduleNextReceive()
        {
            var s = new ReceiveStateObject(ReceiveBufferSize);
            EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                socket.BeginReceiveFrom(s.Buffer, 0, s.Buffer.Length, SocketFlags.None, ref ep, handleRequest, s);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception e)
            {
                OnErrorOccured(e);
                Stop();
            }
        }

        private void handleRequest(IAsyncResult ar)
        {
            var s = (ReceiveStateObject)ar.AsyncState;
            IPEndPoint client = new IPEndPoint(IPAddress.Any, 0);
            EndPoint tmpEP = (EndPoint)client;
            int bytesRead;
            try
            {
                bytesRead = socket.EndReceiveFrom(ar, ref tmpEP);
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
            client = (IPEndPoint)tmpEP;

            if (bytesRead > 0)
            {
                var id = getClientID(client);
                var request = processRequest(id, s.Buffer, bytesRead);
                UdpServerChannel channel;
                if (!channels.TryGetValue(id, out channel))
                {
                    channel = new UdpServerChannel(socket, client, Interlocked.Increment(ref channelID));
                    channel.ChannelClosed += channelClosedHandler;
                    channels.TryAdd(id, channel);
                    OnChannelCreated(channel);
                    channel.raiseChannelReady();
                }
                channel.InjectRequest(request);
            }
            scheduleNextReceive();
        }

        /// <summary>
        /// Constructs data stream from given byte array.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected virtual IDataStream processRequest(string id, byte[] data, int length)
        {
            byte[] arr = new byte[length];
            Array.Copy(data, arr, length);
            return new ByteArray(arr);
        }

        private string getClientID(IPEndPoint ep)
        {
            return ep.ToString();
        }

        private void OnChannelCreated(IServerChannel channel)
        {
            var ev = ChannelCreated;
            if (ev != null) ev(this, channel);
        }

        private void channelClosedHandler(object channel)
        {
            UdpServerChannel c;
            channels.TryRemove(((UdpServerChannel)channel).Name, out c);
        }
    }
}