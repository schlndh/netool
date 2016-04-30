using Netool.Network.DataFormats;
using System;

namespace Netool.Network
{
    [Serializable]
    public abstract class BaseServerChannel : BaseChannel
    {
        [field: NonSerialized]
        public event RequestReceivedHandler RequestReceived;
        [field: NonSerialized]
        public event ResponseSentHandler ResponseSent;

        protected virtual void OnRequestReceived(IDataStream request, ICloneable state)
        {
            if (RequestReceived != null) RequestReceived(this, new DataEventArgs { Data = request, State = state });
        }

        protected virtual void OnRequestReceived(IDataStream request)
        {
            if (RequestReceived != null) RequestReceived(this, new DataEventArgs { Data = request, State = null });
        }

        protected virtual void OnResponseSent(IDataStream response, ICloneable state)
        {
            if (ResponseSent != null) ResponseSent(this, new DataEventArgs { Data = response, State = state });
        }

        protected virtual void OnResponseSent(IDataStream response)
        {
            if (ResponseSent != null) ResponseSent(this, new DataEventArgs { Data = response, State = null });
        }

        protected virtual void OnRequestReceived(DataEventArgs e)
        {
            if (RequestReceived != null) RequestReceived(this, e);
        }

        protected virtual void OnResponseSent(DataEventArgs e)
        {
            if (ResponseSent != null) ResponseSent(this, e);
        }
    }
}