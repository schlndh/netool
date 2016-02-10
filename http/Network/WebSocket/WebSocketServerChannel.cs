using Netool.Logging;
using System;

namespace Netool.Network.WebSocket
{
    [Serializable]
    public class WebSocketServerChannel : BaseServerChannel, IServerChannel
    {
        private IServerChannel innerChannel;

        [NonSerialized]
        private BaseWebSocket webSocket;

        public WebSocketServerChannel(IServerChannel innerChannel, InstanceLogger logger)
        {
            this.id = innerChannel.ID;
            this.name = innerChannel.Name;
            this.innerChannel = innerChannel;
            innerChannel.ChannelClosed += innerChannel_ChannelClosed;
            innerChannel.ErrorOccured += innerChannel_ErrorOccured;
            innerChannel.RequestReceived += innerChannel_RequestReceived;
            innerChannel.ResponseSent += innerChannel_ResponseSent;
            webSocket = new BaseWebSocket(logger);
            webSocket.MessageParsed += webSocket_MessageParsed;
        }

        private void webSocket_MessageParsed(object sender, DataFormats.WebSocket.WebSocketMessage e)
        {
            OnRequestReceived(e);
        }

        private void innerChannel_ResponseSent(object sender, DataEventArgs e)
        {
            OnResponseSent(e);
        }

        private void innerChannel_RequestReceived(object sender, DataEventArgs e)
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