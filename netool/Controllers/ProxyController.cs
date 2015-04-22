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
            this.proxy.RequestSent += OnRequestSent;
            this.proxy.RequestDropped += OnRequestDropped;
            this.proxy.ResponseReceived += OnResponseReceived;
            this.proxy.ResponseSent += OnResponseSent;
            this.proxy.ResponseDropped += OnResponseDropped;
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

        private void OnConnectionCreated(object sender, ConnectionEventArgs e)
        {
            view.LogMessage(String.Format("\r\n=== Connection created ({0}) ===\r\n", e.ID));
        }
        private void OnConnectionClosed(object sender, ConnectionEventArgs e)
        {
            view.LogMessage(String.Format("\r\n=== Connection closed ({0}) ===\r\n", e.ID));
        }
        private void OnRequestReceived(object sender, DataEventAgrs e)
        {
            view.LogMessage(String.Format("\r\n=== Request received ({0}) ===\r\n{1}", e.ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
        private void OnRequestSent(object sender, DataEventAgrs e)
        {
            view.LogMessage(String.Format("\r\n=== Request sent ({0}) ===\r\n{1}", e.ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
        private void OnRequestDropped(object sender, DataEventAgrs e)
        {
            view.LogMessage(String.Format("\r\n=== Request dropped ({0}) ===\r\n{1}", e.ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
        private void OnResponseReceived(object sender, DataEventAgrs e)
        {
            view.LogMessage(String.Format("\r\n=== Response received ({0}) ===\r\n{1}", e.ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
        private void OnResponseSent(object sender, DataEventAgrs e)
        {
            view.LogMessage(String.Format("\r\n=== Response sent ({0}) ===\r\n{1}", e.ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
        private void OnResponseDropped(object sender, DataEventAgrs e)
        {
            view.LogMessage(String.Format("\r\n=== Response dropped ({0}) ===\r\n{1}", e.ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
    }
}
