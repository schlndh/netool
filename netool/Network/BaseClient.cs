namespace Netool.Network
{
    public class BaseClient
    {
        public event ConnectionCreatedHandler ConnectionCreated;
        public event RequestSentHandler RequestSent;
        public event ResponseReceivedHandler ResponseReceived;
        public event ConnectionClosedHandler ConnectionClosed;

        public string ID { get; set; }

        protected virtual void OnConnectionCreated()
        {
            if (ConnectionCreated != null) ConnectionCreated(this, new ConnectionEventArgs { ID = this.ID });
        }

        protected virtual void OnRequestSent(IByteArrayConvertible request, object state = null)
        {
            if (RequestSent != null) RequestSent(this, new DataEventAgrs { ID = this.ID, Data = request, State = state });
        }

        protected virtual void OnResponseReceived(IByteArrayConvertible response, object state = null)
        {
            if (ResponseReceived != null) ResponseReceived(this, new DataEventAgrs { ID = this.ID, Data = response, State = state });
        }

        protected virtual void OnConnectionClosed()
        {
            if (ConnectionClosed != null) ConnectionClosed(this, new ConnectionEventArgs { ID = this.ID });
        }
    }
}