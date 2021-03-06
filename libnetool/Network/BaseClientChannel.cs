﻿using Netool.Network.DataFormats;
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
            var ev = RequestSent;
            if (ev != null) ev(this, new DataEventArgs { Data = request, State = state });
        }

        protected virtual void OnRequestSent(IDataStream request)
        {
            var ev = RequestSent;
            if (ev != null) ev(this, new DataEventArgs { Data = request, State = null });
        }

        protected virtual void OnResponseReceived(IDataStream response, ICloneable state)
        {
            var ev = ResponseReceived;
            if (ev != null) ev(this, new DataEventArgs { Data = response, State = state });
        }

        protected virtual void OnResponseReceived(IDataStream response)
        {
            var ev = ResponseReceived;
            if (ev != null) ev(this, new DataEventArgs { Data = response, State = null });
        }

        protected virtual void OnRequestSent(DataEventArgs e)
        {
            var ev = RequestSent;
            if (ev != null) ev(this, e);
        }

        protected virtual void OnResponseReceived(DataEventArgs e)
        {
            var ev = ResponseReceived;
            if (ev != null) ev(this, e);
        }
    }
}