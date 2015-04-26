namespace Netool.Network
{
    public abstract class BaseServer
    {
        public event ConnectionCreatedHandler ConnectionCreated;
        public event RequestReceivedHandler RequestReceived;
        public event ResponseSentHandler ResponseSent;
        public event ConnectionClosedHandler ConnectionClosed;

        protected virtual void OnConnectionCreated(string clientID)
        {
            if (ConnectionCreated != null) ConnectionCreated(this, new ConnectionEventArgs { ID = clientID });
        }

        protected virtual void OnRequestReceived(string clientID, IByteArrayConvertible request, object state = null)
        {
            if (RequestReceived != null) RequestReceived(this, new DataEventAgrs { ID = clientID, Data = request, State = state });
        }

        protected virtual void OnResponseSent(string clientID, IByteArrayConvertible response, object state = null)
        {
            if (ResponseSent != null) ResponseSent(this, new DataEventAgrs { ID = clientID, Data = response, State = state });
        }

        protected virtual void OnConnectionClosed(string clientID)
        {
            if (ConnectionClosed != null) ConnectionClosed(this, new ConnectionEventArgs { ID = clientID });
        }
    }
}