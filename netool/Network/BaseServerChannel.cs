using System;
namespace Netool.Network
{
    public abstract class BaseServerChannel : BaseChannel
    {
        public event RequestReceivedHandler RequestReceived;
        public event ResponseSentHandler ResponseSent;

        protected virtual void OnRequestReceived(IByteArrayConvertible request, ICloneable state = null)
        {
            if (RequestReceived != null) RequestReceived(this, new DataEventArgs { Data = request, State = state });
        }

        protected virtual void OnResponseSent(IByteArrayConvertible response, ICloneable state = null)
        {
            if (ResponseSent != null) ResponseSent(this, new DataEventArgs { Data = response, State = state });
        }
    }
}