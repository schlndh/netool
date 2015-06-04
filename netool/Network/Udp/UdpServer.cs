﻿using Netool.Network.DataFormats;
using Netool.Network.Helpers;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

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

    public class UdpServerSettings
    {
        public IPEndPoint LocalEndPoint;
    }

    public class UdpServerChannel : BaseServerChannel, IServerChannel
    {
        protected Socket socket;
        protected EndPoint remoteEP;

        public UdpServerChannel(Socket socket, EndPoint remoteEP)
        {
            this.socket = socket;
            this.remoteEP = remoteEP;
            id = remoteEP.ToString();
        }

        public void Close()
        {
            OnChannelClosed();
        }

        public void Send(IByteArrayConvertible response)
        {
            try
            {
                socket.SendTo(response.ToByteArray(), remoteEP);
                OnResponseSent(response);
            }
            catch (ObjectDisposedException)
            { }
        }

        public void InjectRequest(IByteArrayConvertible request)
        {
            OnRequestReceived(request);
        }
    }

    public class UdpServer : IServer
    {
        protected UdpServerSettings settings;
        protected Socket socket;
        private volatile bool stopped = true;
        private ConcurrentDictionary<string, UdpServerChannel> channels = new ConcurrentDictionary<string, UdpServerChannel>();
        public int ReceiveBufferSize { get; set; }

        public event EventHandler<IServerChannel> ChannelCreated;

        public UdpServer(UdpServerSettings settings)
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
                if (settings.LocalEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.PacketInformation, true);
                }
                else if (settings.LocalEndPoint.AddressFamily == AddressFamily.InterNetwork)
                {
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
                }

                scheduleNextReceive();
            }
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
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (ObjectDisposedException)
                { }
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
            client = (IPEndPoint)tmpEP;
            scheduleNextReceive();

            if (bytesRead > 0)
            {
                var id = getClientID(client);
                var request = processRequest(id, s.Buffer, bytesRead);
                UdpServerChannel channel;
                if (!channels.TryGetValue(id, out channel))
                {
                    channel = new UdpServerChannel(socket, client);
                    channel.ChannelClosed += channelClosedHandler;
                    channels.TryAdd(id, channel);
                    OnChannelCreated(channel);
                }
                channel.InjectRequest(request);
            }
        }

        private IByteArrayConvertible processRequest(string id, byte[] data, int length)
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
            if (ChannelCreated != null) ChannelCreated(this, channel);
        }

        private void channelClosedHandler(object channel)
        {
            UdpServerChannel c;
            channels.TryRemove(((UdpServerChannel)channel).ID, out c);
        }

        public bool TryGetByID(string ID, out IServerChannel channel)
        {
            UdpServerChannel c;
            if (channels.TryGetValue(ID, out c))
            {
                channel = c;
                return true;
            }
            channel = null;
            return false;
        }
    }
}