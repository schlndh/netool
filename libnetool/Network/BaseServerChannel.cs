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
            var ev = RequestReceived;
            if (ev != null) ev(this, new DataEventArgs { Data = request, State = state });
        }

        protected virtual void OnRequestReceived(IDataStream request)
        {
            var ev = RequestReceived;
            if (ev != null) ev(this, new DataEventArgs { Data = request, State = null });
        }

        protected virtual void OnResponseSent(IDataStream response, ICloneable state)
        {
            var ev = ResponseSent;
            if (ev != null) ev(this, new DataEventArgs { Data = response, State = state });
        }

        protected virtual void OnResponseSent(IDataStream response)
        {
            var ev = ResponseSent;
            if (ev != null) ev(this, new DataEventArgs { Data = response, State = null });
        }

        protected virtual void OnRequestReceived(DataEventArgs e)
        {
            var ev = RequestReceived;
            if (ev != null) ev(this, e);
        }

        protected virtual void OnResponseSent(DataEventArgs e)
        {
            var ev = ResponseSent;
            if (ev != null) ev(this, e);
        }
    }
}