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
        public ClientController(ClientView view, IClient client)
        {
            this.view = view;
            this.client = client;
            this.client.ConnectionCreated += OnConnectionCreated;
            this.client.ConnectionClosed += OnConnectionClosed;
            this.client.ResponseReceived += OnResponseReceived;
            this.client.RequestSent += OnRequestSent;
        }
        public void Start()
        {
            var t = new Thread(delegate() { client.Start(); });
            t.Start();
        }
        public void Stop()
        {
            client.Stop();
        }
        public void Send(string data)
        {
            client.Send(new ByteArray(ASCIIEncoding.ASCII.GetBytes(data)));
        }
        public void OnConnectionCreated(object sender, ConnectionEventArgs e)
        {
            view.LogMessage(String.Format("\r\n=== Connection created ({0}) ===\r\n", e.ID));
        }
        public void OnConnectionClosed(object sender, ConnectionEventArgs e)
        {
            view.LogMessage(String.Format("\r\n=== Connection closed ({0}) ===\r\n", e.ID));
        }
        public void OnResponseReceived(object sender, DataEventAgrs e)
        {
            view.LogMessage(String.Format("\r\n=== Response received ({0}) ===\r\n{1}", e.ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
        public void OnRequestSent(object sender, DataEventAgrs e)
        {
            view.LogMessage(String.Format("\r\n=== Request sent ({0}) ===\r\n{1}", e.ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
    }
}
