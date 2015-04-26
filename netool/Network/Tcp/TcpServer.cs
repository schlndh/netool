using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Netool.Settings;
using Netool.Network.DataFormats;
namespace Netool.Network.Tcp
{
    class ReceiveStateObject
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
    public class TcpServer: BaseServer, IServer
    {
        protected class ClientData
        {
            public Socket Socket;
        }       
        protected TcpServerSettings settings;
        protected Socket socket;
        private volatile bool stopped = true;
        private ConcurrentDictionary<string, ClientData> clients = new ConcurrentDictionary<string, ClientData>();
        public int ReceiveBufferSize { get; set;}

        public TcpServer(TcpServerSettings settings)
        {
            this.settings = settings;
            ReceiveBufferSize = 2048;
        }

        public void Stop()
        {
            if(!stopped)
            {
                stopped = true;
                foreach (var client in clients)
                {
                    var s = client.Value.Socket;
                    try
                    {
                        s.Shutdown(SocketShutdown.Both);
                        s.Close();
                        OnConnectionClosed(client.Key);
                    }
                    catch (ObjectDisposedException)
                    { }
                }
                clients.Clear();
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

        public void Send(string clientID, IByteArrayConvertible response)
        {
            ClientData d;
            if (clients.TryGetValue(clientID, out d))
            {
                try
                {
                    sendResponse(d.Socket, response);
                }
                catch(ObjectDisposedException)
                {
                    clients.TryRemove(clientID, out d);
                    return;
                }
                OnResponseSent(clientID, response);
            }
            
        }
        public void CloseConnection(string clientID)
        {
            ClientData d;
            if (clients.TryGetValue(clientID, out d))
            {
                try
                {
                    d.Socket.Shutdown(SocketShutdown.Both);
                    d.Socket.Close();
                    OnConnectionClosed(clientID);
                }
                catch(ObjectDisposedException)
                {
                    // already closed
                }
                clients.TryRemove(clientID, out d);
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
            var id = getClientID(client);
            clients.TryAdd(id, new ClientData { Socket = client });
            OnConnectionCreated(id);
            scheduleNextReceive(client);
        }
        private void scheduleNextReceive(Socket client)
        {
            var s = new ReceiveStateObject(client, ReceiveBufferSize);
            try 
            {
                client.BeginReceive(s.Buffer, 0, s.Buffer.Length, SocketFlags.None, handleRequest, s);
            }
            catch(ObjectDisposedException)
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
            catch(ObjectDisposedException)
            {
                return;
            }
            if (bytesRead > 0)
            {
                string id = getClientID(client);
                IByteArrayConvertible processed = processRequest(id, stateObject.Buffer, bytesRead);
                OnRequestReceived(id, processed);
                scheduleNextReceive(client);
            }
            else 
            {
                CloseConnection(getClientID(client));
            }
        }
        private IByteArrayConvertible processRequest(string id, byte[] data, int length)
        {
            byte[] arr = new byte[length];
            Array.Copy(data,arr, length);
            return new ByteArray(arr);
        }
        private void sendResponse(Socket client, IByteArrayConvertible data)
        {
            client.Send(data.ToByteArray());
        }
        protected virtual string getClientID(Socket client)
        {
            // IPEndPoint correctly overrides ToString()
            return client.RemoteEndPoint.ToString();
        }
    }
}
