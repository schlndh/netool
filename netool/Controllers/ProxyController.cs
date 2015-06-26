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
            this.proxy.ChannelCreated += OnConnectionCreated;

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

        private void OnConnectionCreated(object sender, IProxyChannel c)
        {
            view.LogMessage(String.Format("\r\n=== Connection created ({0}) ===\r\n", c.ID));
            c.ChannelClosed += OnConnectionClosed;
            c.RequestReceived += OnRequestReceived;
            c.RequestSent += OnRequestSent;
            c.ResponseReceived += OnResponseReceived;
            c.ResponseSent += OnResponseSent;
        }
        private void OnConnectionClosed(object sender)
        {
            view.LogMessage(String.Format("\r\n=== Connection closed ({0}) ===\r\n", ((IProxyChannel)sender).ID));
        }
        private void OnRequestReceived(object sender, DataEventArgs e)
        {
            view.LogMessage(String.Format("\r\n=== Request received ({0}) ===\r\n{1}", ((IProxyChannel)sender).ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
        private void OnRequestSent(object sender, DataEventArgs e)
        {
            view.LogMessage(String.Format("\r\n=== Request sent ({0}) ===\r\n{1}", ((IProxyChannel)sender).ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
        private void OnResponseReceived(object sender, DataEventArgs e)
        {
            view.LogMessage(String.Format("\r\n=== Response received ({0}) ===\r\n{1}", ((IProxyChannel)sender).ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
        private void OnResponseSent(object sender, DataEventArgs e)
        {
            view.LogMessage(String.Format("\r\n=== Response sent ({0}) ===\r\n{1}", ((IProxyChannel)sender).ID, ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray())));
        }
    }
}
