using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Netool.Settings;
namespace Netool.Network
{
    public class DefaultProxy: IProxy
    {
        protected class ClientHandler
        {
            protected IClient client;
            protected IServer server;
            protected string clientID;
            public ClientHandler(string clientID, IClient client, IServer server)
            {
                this.clientID = clientID;
                this.server = server;
                this.client = client;
                this.client.ResponseReceived += responseReceivedHandler;
                this.client.ConnectionClosed += connectionClosedHandler;
                this.client.Start();
            }
            public void Send(IByteArrayConvertible request)
            {
                client.Send(request);
            }
            public void Stop()
            {
                client.Stop();
            }
            protected void responseReceivedHandler(object sender, DataEventAgrs args)
            {
                server.Send(clientID, args.Data);
            }
            protected void connectionClosedHandler(object sender, ConnectionEventArgs args)
            {
                server.CloseConnection(clientID);
                client.Stop();
            }
        }
        public event ConnectionCreatedHandler ConnectionCreated;
        public event RequestReceivedHandler RequestReceived;
        public event ResponseSentHandler ResponseSent;
        public event ConnectionClosedHandler ConnectionClosed;

        protected Dictionary<string, ClientHandler> clientHandlers = new Dictionary<string, ClientHandler>();
        protected IServer server;
        protected IClientFactory clientFactory;
        public DefaultProxy(IServer server, IClientFactory clientFactory)
        {
            this.server = server;
            this.clientFactory = clientFactory;
        }
        public virtual void Start()
        {
            server.ConnectionCreated += connectionCreatedHandler;
            server.ConnectionClosed += connectionClosedHandler;
            server.RequestReceived += requestReceivedHandler;
            server.ResponseSent += responseSentHandler;
            server.Start();
        }
        public virtual void Stop()
        {
            server.ConnectionCreated -= connectionCreatedHandler;
            server.ConnectionClosed -= connectionClosedHandler;
            server.RequestReceived -= requestReceivedHandler;
            server.ResponseSent -= responseSentHandler;
            foreach(var client in clientHandlers)
            {
                client.Value.Stop();
                OnConnectionClosed(client.Key);
            }
            clientHandlers.Clear();
            server.Stop();
        }
        protected virtual void requestReceivedHandler(object sender, DataEventAgrs args)
        {
            ClientHandler handler;
            if (!clientHandlers.TryGetValue(args.ID, out handler))
            {
                handler = createClientHandler(args.ID);
            }
            OnRequestReceived(args.ID, args.Data, args.State);
            handler.Send(args.Data);   
        }
        protected virtual void connectionCreatedHandler(object sender, ConnectionEventArgs args) 
        {
            if (!clientHandlers.ContainsKey(args.ID))
            {
                createClientHandler(args.ID);
            }
            OnConnectionCreated(args.ID);
        }
        protected virtual void connectionClosedHandler(object sender, ConnectionEventArgs args)
        {
            ClientHandler handler;
            if (clientHandlers.TryGetValue(args.ID, out handler))
            {
                handler.Stop();
                clientHandlers.Remove(args.ID);
            }
            OnConnectionClosed(args.ID);
        }
        protected virtual void responseSentHandler(object sender, DataEventAgrs args)
        {
            OnResponseSent(args.ID, args.Data, args.State);
        }
        protected virtual ClientHandler createClientHandler(string clientID)
        {
            var client = clientFactory.CreateClient();
            var ret = new ClientHandler(clientID, client, server);
            clientHandlers.Add(clientID, ret);
            return ret;
        }
        protected virtual void OnConnectionCreated(string clientID)
        {
            if (ConnectionCreated != null) ConnectionCreated(this, new ConnectionEventArgs { ID = clientID });
        }
        protected virtual void OnRequestReceived(string clientID, IByteArrayConvertible request, object state)
        {
            if (RequestReceived != null) RequestReceived(this, new DataEventAgrs { ID = clientID, Data = request, State = state });
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
