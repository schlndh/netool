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
            this.server.ChannelCreated += OnConnectionCreated;
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
            IServerChannel c;
            if (server.TryGetByID(id, out c))
            {
                c.Close();
            }
        }
        public void Send(string id, string data)
        {
            IServerChannel c;
            if (server.TryGetByID(id, out c))
            {
                c.Send(new ByteArray(ASCIIEncoding.ASCII.GetBytes(data)));
            }
        }
        private void OnConnectionCreated(object sender, IServerChannel c)
        {
            view.LogMessage(String.Format("\r\n=== Connection created ({0}) ===\r\n", c.ID));
            view.AddClient(c.ID);
            c.ChannelClosed += OnConnectionClosed;
            c.RequestReceived += OnRequestReceived;
            c.ResponseSent += OnResponseSent;
        }
        private void OnConnectionClosed(object sender)
        {
            view.LogMessage(String.Format("\r\n=== Connection closed ({0}) ===\r\n", ((IServerChannel)sender).ID));
            view.RemoveClient(((IServerChannel)sender).ID);
        }
        private void OnRequestReceived(object sender, DataEventAgrs e)
        {
            view.LogMessage(String.Format("\r\n=== Request received ({0}) ===\r\n{1}", ((IServerChannel)sender).ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
            view.AddClient(((IServerChannel)sender).ID);
        }
        private void OnResponseSent(object sender, DataEventAgrs e)
        {
            view.LogMessage(String.Format("\r\n=== Response sent ({0}) ===\r\n{1}", ((IServerChannel)sender).ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
    }
}
