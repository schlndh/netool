namespace Netool.Network
{
    public abstract class BaseServerChannel
    {
        protected string id;
        public string ID { get { return id; } }

        public event RequestReceivedHandler RequestReceived;
        public event ResponseSentHandler ResponseSent;
        public event ChannelClosedHandler ChannelClosed;

        protected virtual void OnRequestReceived(IByteArrayConvertible request, object state = null)
        {
            if (RequestReceived != null) RequestReceived(this, new DataEventAgrs { Data = request, State = state });
        }

        protected virtual void OnResponseSent(IByteArrayConvertible response, object state = null)
        {
            if (ResponseSent != null) ResponseSent(this, new DataEventAgrs { Data = response, State = state });
        }

        protected virtual void OnChannelClosed()
        {
            if (ChannelClosed != null) ChannelClosed(this);
        }
    }
}