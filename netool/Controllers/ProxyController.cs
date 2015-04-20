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
    public class ProxyController
    {
        private ProxyView view;
        private IProxy proxy;
        public ProxyController(ProxyView view, IProxy proxy)
        {
            this.view = view;
            this.proxy = proxy;
            this.proxy.ConnectionCreated += OnConnectionCreated;
            this.proxy.ConnectionClosed += OnConnectionClosed;
            this.proxy.RequestReceived += OnRequestReceived;
            this.proxy.ResponseSent += OnResponseSent;
        }
        public void Start()
        {
            var t = new Thread(delegate() { proxy.Start(); });
            t.Start();
        }
        public void Stop()
        {
            proxy.Stop();
        }

        public void OnConnectionCreated(object sender, ConnectionEventArgs e)
        {
            view.LogMessage(String.Format("\r\n=== Connection created ({0}) ===\r\n", e.ID));
        }
        public void OnConnectionClosed(object sender, ConnectionEventArgs e)
        {
            view.LogMessage(String.Format("\r\n=== Connection closed ({0}) ===\r\n", e.ID));
        }
        public void OnRequestReceived(object sender, DataEventAgrs e)
        {
            view.LogMessage(String.Format("\r\n=== Request received ({0}) ===\r\n{1}", e.ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
        public void OnResponseSent(object sender, DataEventAgrs e)
        {
            view.LogMessage(String.Format("\r\n=== Response sent ({0}) ===\r\n{1}", e.ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
    }
}
