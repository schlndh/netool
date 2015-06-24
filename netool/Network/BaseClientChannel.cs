using System;
namespace Netool.Network
{
    public class BaseClientChannel : BaseChannel
    {
        public event RequestSentHandler RequestSent;
        public event ResponseReceivedHandler ResponseReceived;

        protected virtual void OnRequestSent(IByteArrayConvertible request, ICloneable state = null)
        {
            if (RequestSent != null) RequestSent(this, new DataEventArgs { Data = request, State = state });
        }

        protected virtual void OnResponseReceived(IByteArrayConvertible response, ICloneable state = null)
        {
            if (ResponseReceived != null) ResponseReceived(this, new DataEventArgs { Data = response, State = state });
        }
    }
}