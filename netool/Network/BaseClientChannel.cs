namespace Netool.Network
{
    public class BaseClientChannel
    {
        protected string id;
        public string ID { get { return id; } }

        public event RequestSentHandler RequestSent;
        public event ResponseReceivedHandler ResponseReceived;
        public event ChannelClosedHandler ChannelClosed;

        protected virtual void OnRequestSent(IByteArrayConvertible request, object state = null)
        {
            if (RequestSent != null) RequestSent(this, new DataEventAgrs { Data = request, State = state });
        }

        protected virtual void OnResponseReceived(IByteArrayConvertible response, object state = null)
        {
            if (ResponseReceived != null) ResponseReceived(this, new DataEventAgrs { Data = response, State = state });
        }

        protected virtual void OnChannelClosed()
        {
            if (ChannelClosed != null) ChannelClosed(this);
        }
    }
}