using Netool.Network.DataFormats;
using System;

namespace Netool.Network
{
    [Serializable]
    public class BaseClientChannel : BaseChannel
    {
        [field: NonSerialized]
        public event RequestSentHandler RequestSent;
        [field: NonSerialized]
        public event ResponseReceivedHandler ResponseReceived;

        protected virtual void OnRequestSent(IDataStream request, ICloneable state)
        {
            if (RequestSent != null) RequestSent(this, new DataEventArgs { Data = request, State = state });
        }

        protected virtual void OnRequestSent(IDataStream request)
        {
            if (RequestSent != null) RequestSent(this, new DataEventArgs { Data = request, State = null });
        }

        protected virtual void OnResponseReceived(IDataStream response, ICloneable state)
        {
            if (ResponseReceived != null) ResponseReceived(this, new DataEventArgs { Data = response, State = state });
        }

        protected virtual void OnResponseReceived(IDataStream response)
        {
            if (ResponseReceived != null) ResponseReceived(this, new DataEventArgs { Data = response, State = null });
        }

        protected virtual void OnRequestSent(DataEventArgs e)
        {
            if (RequestSent != null) RequestSent(this, e);
        }

        protected virtual void OnResponseReceived(DataEventArgs e)
        {
            if (ResponseReceived != null) ResponseReceived(this, e);
        }
    }
}