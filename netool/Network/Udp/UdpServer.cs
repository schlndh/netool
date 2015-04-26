using Netool.Network.DataFormats;
using Netool.Network.Helpers;
using System;
using System.Net;
using System.Net.Sockets;

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

    public class UdpServer : BaseServer, IServer
    {
        protected UdpServerSettings settings;
        protected Socket socket;
        private volatile bool stopped = true;

        public int ReceiveBufferSize { get; set; }

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
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (ObjectDisposedException)
                { }
            }
        }

        public void Send(string clientID, IByteArrayConvertible response)
        {
            IPEndPoint ep;
            if (IPEndPointParser.TryParse(clientID, out ep))
            {
                try
                {
                    socket.SendTo(response.ToByteArray(), ep);
                    OnResponseSent(clientID, response);
                }
                catch (ObjectDisposedException)
                { }
            }
        }

        public void CloseConnection(string clientID)
        {
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
                OnRequestReceived(id, request);
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
    }
}