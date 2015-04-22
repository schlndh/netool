using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Netool.Network;
using Netool.Network.Tcp;
using Netool.Network.DataFormats;
using Netool.Dialogs;
using System.Windows.Forms;
using Netool.Views;
namespace Netool.Controllers
{
    public class MainController
    {
        private MainView view;
        private MainModel model;
        public MainController(MainView view, MainModel model)
        {
            this.view = view;
            this.view.SetController(this);
            this.model = model;
        }
        /// <summary>
        /// Load settings, open previously open instances, etc
        /// </summary>
        public void Load()
        {
            {
                var server = new TcpServer(new TcpServerSettings { LocalEndPoint = new IPEndPoint(IPAddress.Loopback, 8081) });
                var sview = new ServerView();
                var cont = new ServerController(sview, server);
                sview.SetController(cont);
                view.AddPage("Server", sview);
                cont.Start();
            }
            {
                var server = new TcpServer(new TcpServerSettings { LocalEndPoint = new IPEndPoint(IPAddress.Loopback, 8080) });
                var factory = new TcpClientFactory(new TcpClientFactorySettings { LocalIPAddress = IPAddress.Loopback, RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 8081) });
                var proxy = new DefaultProxy(server, factory);
                proxy.RequestModifier = delegate(string id, IByteArrayConvertible data)
                {
                    if (data.ToByteArray().Length > 10) return null;
                    return new ByteArray(ASCIIEncoding.ASCII.GetBytes(ASCIIEncoding.ASCII.GetString(data.ToByteArray()).ToUpper()));
                };
                proxy.ResponseModifier = delegate(string id, IByteArrayConvertible data)
                {
                    if (data.ToByteArray().Length > 10) return null;
                    return new ByteArray(ASCIIEncoding.ASCII.GetBytes(ASCIIEncoding.ASCII.GetString(data.ToByteArray()).ToLower()));
                };
                var pview = new ProxyView();
                var cont = new ProxyController(pview, proxy);
                pview.SetController(cont);
                view.AddPage("Proxy", pview);
                cont.Start();
            }
            {
                var client = new TcpClient(new TcpClientSettings { LocalEndPoint = new IPEndPoint(IPAddress.Loopback, 0), RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 8080) });
                var cview = new ClientView();
                var cont = new ClientController(cview, client);
                cview.SetController(cont);
                view.AddPage("Client", cview);
                cont.Start();
            }
        }
        public void CreateServer()
        {
            var dialog = new TcpServerDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                var settings = dialog.Settings;
                var server = new TcpServer(settings);
                var sview = new ServerView();
                var cont = new ServerController(sview, server);
                sview.SetController(cont);
                view.AddPage("Server", sview);
                cont.Start();
            }
        }
        public void CreateClient()
        {
            var dialog = new TcpClientDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                var settings = dialog.Settings;
                var client = new TcpClient(settings);
                var cview = new ClientView();
                var cont = new ClientController(cview, client);
                cview.SetController(cont);
                view.AddPage("Client", cview);
                cont.Start();
            }
        }
        public void CreateProxy()
        {
            var sdialog = new TcpServerDialog();
            sdialog.ShowDialog();
            TcpServer server;
            if(sdialog.DialogResult == DialogResult.OK)
            {
                var settings = sdialog.Settings;
                server = new TcpServer(settings);
            }
            else
            {
                return;
            }
            var cdialog = new TcpClientDialog();
            cdialog.ShowDialog();
            TcpClientFactory factory;
            if(cdialog.DialogResult == DialogResult.OK)
            {
                var settings = cdialog.Settings;
                factory = new TcpClientFactory(new TcpClientFactorySettings { LocalIPAddress = settings.LocalEndPoint.Address, RemoteEndPoint = settings.RemoteEndPoint });
                var proxy = new DefaultProxy(server, factory);
                var pview = new ProxyView();
                var cont = new ProxyController(pview, proxy);
                pview.SetController(cont);
                view.AddPage("Proxy", pview);
                cont.Start();
            }
        }
    }
}
