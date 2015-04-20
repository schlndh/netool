using System;
using System.Collections.Generic;
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
    class ReadStateObject
    {
        public Socket Client;
        public byte[] Buffer;
        public ReadStateObject(Socket client, int bufferSize)
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
    public class TcpServer: IServer
    {
        protected class ClientData
        {
            public Socket Socket;
            public object State;
        }       
        protected TcpServerSettings settings;
        protected Socket socket;
        protected volatile bool stopped = true;
        protected Dictionary<string, ClientData> clients = new Dictionary<string, ClientData>();

        public event ConnectionCreatedHandler ConnectionCreated;
        public event RequestReceivedHandler RequestReceived;
        public event ResponseSentHandler ResponseSent;
        public event ConnectionClosedHandler ConnectionClosed;

        public TcpServer(TcpServerSettings settings)
        {
            this.settings = settings;
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public virtual void Stop()
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
                    catch (ObjectDisposedException e)
                    { }
                }
                clients.Clear();
                socket.Close();
            }
        }

        public virtual void Start()
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
                catch (ObjectDisposedException e)
                {
                    // socket closed
                    return;
                }
            }
        }

        public virtual void Send(string clientID, IByteArrayConvertible response)
        {
            ClientData d;
            if (clients.TryGetValue(clientID, out d))
            {
                try
                {
                    sendResponse(d.Socket, response);
                }
                catch(ObjectDisposedException e)
                {
                    clients.Remove(clientID);
                    return;
                }
                OnResponseSent(clientID, response, d.State);
            }
            
        }
        public virtual void CloseConnection(string clientID)
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
                catch(ObjectDisposedException e)
                {
                    // already closed
                }
                clients.Remove(clientID);
            }
        }
        protected virtual void acceptRequest(IAsyncResult ar)
        {
            Socket client;
            Socket srv = (Socket)ar.AsyncState;
            try
            {
                client = srv.EndAccept(ar);
                srv.BeginAccept(new AsyncCallback(acceptRequest), srv);
            }
            catch (ObjectDisposedException e)
            {
                // socket closed
                return;
            }
            var id = getClientID(client);
            clients.Add(id, new ClientData { Socket = client, State = null });
            OnConnectionCreated(id);
            scheduleNextReceive(client);
        }
        protected virtual void scheduleNextReceive(Socket client)
        {
            var s = new ReadStateObject(client, 1024);
            try 
            {
                client.BeginReceive(s.Buffer, 0, s.Buffer.Length, SocketFlags.None, handleRequest, s);
            }
            catch(ObjectDisposedException e)
            { }
            
        }
        protected virtual void handleRequest(IAsyncResult ar)
        {
            var stateObject = (ReadStateObject)ar.AsyncState;
            Socket client = stateObject.Client;
            int bytesRead = 0;
            try
            {
                bytesRead = client.EndReceive(ar);
            }
            catch(ObjectDisposedException e)
            {
                return;
            }
            if (bytesRead > 0)
            {
                string id = getClientID(client);
                IByteArrayConvertible processed = processRequest(id, stateObject.Buffer, bytesRead);
                object state = null;
                ClientData d;
                if (clients.TryGetValue(id, out d))
                {
                    state = d.State;
                }
                OnRequestReceived(id, processed, state);
                scheduleNextReceive(client);
            }
            else 
            {
                CloseConnection(getClientID(client));
            }
        }
        protected virtual IByteArrayConvertible processRequest(string id, byte[] data, int length)
        {
            byte[] arr = new byte[length];
            Array.Copy(data,arr, length);
            return new ByteArray(arr);
        }
        protected virtual void sendResponse(Socket client, IByteArrayConvertible data)
        {
            client.Send(data.ToByteArray());
            object state = null;
            ClientData d;
            var id = getClientID(client);
            if(clients.TryGetValue(id, out d))
            {
                state = d.State;
            }
        }
        protected virtual string getClientID(Socket client)
        {
            // IPEndPoint correctly overrides ToString()
            return client.RemoteEndPoint.ToString();
        }
        protected virtual void OnConnectionCreated(string clientID)
        {
            if (ConnectionCreated != null) ConnectionCreated(this, new ConnectionEventArgs { ID = clientID });
        }
        protected virtual void OnRequestReceived(string clientID, IByteArrayConvertible request, object state)
        {
            if (RequestReceived != null) RequestReceived(this, new DataEventAgrs{ID = clientID, Data = request, State = state});
        }
        protected virtual void OnResponseSent(string clientID, IByteArrayConvertible response, object state)
        {
            if (ResponseSent != null) ResponseSent(this, new DataEventAgrs { ID = clientID, Data = response, State = state });
        }
        protected virtual void OnConnectionClosed(string clientID)
        {
            if (ConnectionClosed != null) ConnectionClosed(this, new ConnectionEventArgs { ID = clientID });
        }
    }
}
