using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Netool.Views;
using Netool.Network;
using Netool.Network.DataFormats;
namespace Netool.Controllers
{
    public class ServerController
    {
        private ServerView view;
        private IServer server;
        public ServerController(ServerView view, IServer server)
        {
            this.view = view;
            this.server = server;
            this.server.ConnectionCreated += OnConnectionCreated;
            this.server.ConnectionClosed += OnConnectionClosed;
            this.server.RequestReceived += OnRequestReceived;
            this.server.ResponseSent += OnResponseSent;
        }
        public void Start()
        {
            var t = new Thread(delegate() { server.Start(); });
            t.Start();
        }
        public void Stop()
        {
            server.Stop();
        }
        public void Close(string id)
        {
            server.CloseConnection(id);
        }
        public void Send(string id, string data)
        {
            server.Send(id, new ByteArray(ASCIIEncoding.ASCII.GetBytes(data)));
        }
        private void OnConnectionCreated(object sender, ConnectionEventArgs e)
        {
            view.LogMessage(String.Format("\r\n=== Connection created ({0}) ===\r\n", e.ID));
            view.AddClient(e.ID);
        }
        private void OnConnectionClosed(object sender, ConnectionEventArgs e)
        {
            view.LogMessage(String.Format("\r\n=== Connection closed ({0}) ===\r\n", e.ID));
            view.RemoveClient(e.ID);
        }
        private void OnRequestReceived(object sender, DataEventAgrs e)
        {
            view.LogMessage(String.Format("\r\n=== Request received ({0}) ===\r\n{1}", e.ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
            view.AddClient(e.ID);
        }
        private void OnResponseSent(object sender, DataEventAgrs e)
        {
            view.LogMessage(String.Format("\r\n=== Response sent ({0}) ===\r\n{1}", e.ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
    }
}
