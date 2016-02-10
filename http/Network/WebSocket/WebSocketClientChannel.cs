using Netool.Logging;
using System;

namespace Netool.Network.WebSocket
{
    [Serializable]
    public class WebSocketClientChannel : BaseClientChannel, IClientChannel
    {
        private IClientChannel innerChannel;

        [NonSerialized]
        private BaseWebSocket webSocket;

        public WebSocketClientChannel(IClientChannel innerChannel, InstanceLogger logger)
        {
            this.id = innerChannel.ID;
            this.name = innerChannel.Name;
            this.innerChannel = innerChannel;
            innerChannel.ChannelClosed += innerChannel_ChannelClosed;
            innerChannel.ErrorOccured += innerChannel_ErrorOccured;
            innerChannel.ResponseReceived += innerChannel_ResponseReceived;
            innerChannel.RequestSent += innerChannel_RequestSent;
            webSocket = new BaseWebSocket(logger);
            webSocket.MessageParsed += webSocket_MessageParsed;
        }

        private void webSocket_MessageParsed(object sender, DataFormats.WebSocket.WebSocketMessage e)
        {
            OnResponseReceived(e);
        }

        private void innerChannel_RequestSent(object sender, DataEventArgs e)
        {
            OnRequestSent(e);
        }

        private void innerChannel_ResponseReceived(object sender, DataEventArgs e)
        {
            try
            {
                if (e.Data != null) webSocket.Receive(e.Data);
            }
            catch(Exception ex)
            {
                OnErrorOccured(ex);
            }

        }

        private void innerChannel_ErrorOccured(object sender, Exception e)
        {
            OnErrorOccured(e);
        }

        private void innerChannel_ChannelClosed(object sender)
        {
            OnChannelClosed();
        }

        /// <inheritdoc/>
        public void Send(DataFormats.IDataStream response)
        {
            innerChannel.Send(response);
        }

        /// <inheritdoc/>
        public void Close()
        {
            innerChannel.Close();
        }
    }
}