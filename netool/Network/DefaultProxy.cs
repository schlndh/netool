using System.Collections.Concurrent;

namespace Netool.Network
{
    public class DefaultProxy : IProxy
    {
        protected class ClientHandler
        {
            protected IClient client;
            protected string clientID;
            public event ResponseReceivedHandler ResponseReceived;
            public event ConnectionClosedHandler ConnectionClosed;

            public ClientHandler(string clientID, IClient client)
            {
                this.clientID = clientID;
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
                args.ID = clientID;
                if (ResponseReceived != null) ResponseReceived(sender, args);
            }

            protected void connectionClosedHandler(object sender, ConnectionEventArgs args)
            {
                args.ID = clientID;
                if (ConnectionClosed != null) ConnectionClosed(sender, args);
                client.Stop();
            }
        }

        public event ConnectionCreatedHandler ConnectionCreated;
        public event RequestReceivedHandler RequestReceived;
        public event RequestSentHandler RequestSent;
        public event RequestDroppedHandler RequestDropped;
        public event ResponseSentHandler ResponseSent;
        public event ResponseReceivedHandler ResponseReceived;
        public event ResponseDroppedHandler ResponseDropped;
        public event ConnectionClosedHandler ConnectionClosed;

        public DataModifier RequestModifier { get; set; }
        public DataModifier ResponseModifier { get; set; }

        protected ConcurrentDictionary<string, ClientHandler> clientHandlers = new ConcurrentDictionary<string, ClientHandler>();
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
            server.Start();
        }

        public virtual void Stop()
        {
            server.ConnectionCreated -= connectionCreatedHandler;
            server.ConnectionClosed -= connectionClosedHandler;
            server.RequestReceived -= requestReceivedHandler;
            foreach (var client in clientHandlers)
            {
                client.Value.Stop();
                OnConnectionClosed(client.Key);
            }
            clientHandlers.Clear();
            server.Stop();
        }

        private void requestReceivedHandler(object sender, DataEventAgrs args)
        {
            OnRequestReceived(args.ID, args.Data, args.State);
            IByteArrayConvertible data = args.Data;
            if (RequestModifier != null)
            {
                data = RequestModifier(args.ID, args.Data);
                if (data == null)
                {
                    OnRequestDropped(args.ID, args.Data, args.State);
                    return;
                }
            }
            ClientHandler handler;
            if (!clientHandlers.TryGetValue(args.ID, out handler))
            {
                handler = createClientHandler(args.ID);
            }
            handler.Send(data);
            OnRequestSent(args.ID, data, args.State);
        }

        private void connectionCreatedHandler(object sender, ConnectionEventArgs args)
        {
            if (!clientHandlers.ContainsKey(args.ID))
            {
                createClientHandler(args.ID);
            }
            OnConnectionCreated(args.ID);
        }

        private void connectionClosedHandler(object sender, ConnectionEventArgs args)
        {
            ClientHandler handler;
            if (clientHandlers.TryGetValue(args.ID, out handler))
            {
                handler.Stop();
                clientHandlers.TryRemove(args.ID, out handler);
            }
            OnConnectionClosed(args.ID);
        }

        private ClientHandler createClientHandler(string clientID)
        {
            var client = clientFactory.CreateClient();
            var ret = new ClientHandler(clientID, client);
            ret.ResponseReceived += clientResponseReceivedHandler;
            ret.ConnectionClosed += clientConnectionClosedHandler;
            clientHandlers.TryAdd(clientID, ret);
            return ret;
        }

        private void clientResponseReceivedHandler(object sender, DataEventAgrs e)
        {
            OnResponseReceived(e.ID, e.Data, e.State);
            IByteArrayConvertible data = e.Data;
            if (ResponseModifier != null)
            {
                data = ResponseModifier(e.ID, e.Data);
                if (data == null)
                {
                    OnResponseDropped(e.ID, e.Data, e.State);
                    return;
                }
            }
            server.Send(e.ID, data);
            OnResponseSent(e.ID, data, e.State);
        }

        private void clientConnectionClosedHandler(object sender, ConnectionEventArgs e)
        {
            server.CloseConnection(e.ID);
        }

        protected virtual void OnConnectionCreated(string clientID)
        {
            if (ConnectionCreated != null) ConnectionCreated(this, new ConnectionEventArgs { ID = clientID });
        }

        protected virtual void OnRequestReceived(string clientID, IByteArrayConvertible request, object state)
        {
            if (RequestReceived != null) RequestReceived(this, new DataEventAgrs { ID = clientID, Data = request, State = state });
        }

        protected virtual void OnRequestSent(string clientID, IByteArrayConvertible request, object state)
        {
            if (RequestSent != null) RequestSent(this, new DataEventAgrs { ID = clientID, Data = request, State = state });
        }

        protected virtual void OnRequestDropped(string clientID, IByteArrayConvertible request, object state)
        {
            if (RequestDropped != null) RequestDropped(this, new DataEventAgrs { ID = clientID, Data = request, State = state });
        }

        protected virtual void OnResponseReceived(string clientID, IByteArrayConvertible response, object state)
        {
            if (ResponseReceived != null) ResponseReceived(this, new DataEventAgrs { ID = clientID, Data = response, State = state });
        }

        protected virtual void OnResponseSent(string clientID, IByteArrayConvertible response, object state)
        {
            if (ResponseSent != null) ResponseSent(this, new DataEventAgrs { ID = clientID, Data = response, State = state });
        }

        protected virtual void OnResponseDropped(string clientID, IByteArrayConvertible response, object state)
        {
            if (ResponseDropped != null) ResponseDropped(this, new DataEventAgrs { ID = clientID, Data = response, State = state });
        }

        protected virtual void OnConnectionClosed(string clientID)
        {
            if (ConnectionClosed != null) ConnectionClosed(this, new ConnectionEventArgs { ID = clientID });
        }
    }
}