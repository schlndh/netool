using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Netool.Network;
using Netool.Views;
using Netool.Network.DataFormats;
namespace Netool.Controllers
{
    public class ClientController
    {
        private ClientView view;
        private IClient client;
        private IClientChannel channel;
        public ClientController(ClientView view, IClient client)
        {
            this.view = view;
            this.client = client;
            this.client.ChannelCreated += OnConnectionCreated;
        }
        public void Start()
        {
            var t = new Thread(delegate() { channel = client.Start(); });
            t.Start();
        }
        public void Stop()
        {
            client.Stop();
        }
        public void Send(string data)
        {
            channel.Send(new ByteArray(ASCIIEncoding.ASCII.GetBytes(data)));
        }
        private void OnConnectionCreated(object sender, IClientChannel c)
        {
            view.LogMessage(String.Format("\r\n=== Connection created ({0}) ===\r\n", c.ID));
            c.RequestSent += OnRequestSent;
            c.ResponseReceived += OnResponseReceived;
            c.ChannelClosed += OnConnectionClosed;
        }
        private void OnConnectionClosed(object sender)
        {
            view.LogMessage(String.Format("\r\n=== Connection closed ({0}) ===\r\n", ((IClientChannel)sender).ID));
        }
        private void OnResponseReceived(object sender, DataEventArgs e)
        {
            view.LogMessage(String.Format("\r\n=== Response received ({0}) ===\r\n{1}", ((IClientChannel)sender).ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
        private void OnRequestSent(object sender, DataEventArgs e)
        {
            view.LogMessage(String.Format("\r\n=== Request sent ({0}) ===\r\n{1}", ((IClientChannel)sender).ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
    }
}
