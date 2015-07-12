using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Netool.Logging;
using Netool.Network;
using Netool.Network.Tcp;
using Netool.Network.Udp;
using Netool.Network.DataFormats;
using Netool.Dialogs;
using System.Windows.Forms;
using Netool.Views;
using Netool.Views.Instance;
using Netool.ChannelDrivers;
namespace Netool.Controllers
{
    public class MainController
    {
        private MainView view;
        private MainModel model;
        private List<IInstanceController> controllers = new List<IInstanceController>();

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
            /*{ // restored instance
                var sview = new DefaultInstanceView();
                var cont = new DefaultInstanceController(sview, new InstanceLogger(@"D:\test\tcp_server_2.tmp"),  new DefaultInstanceController.DefaultChannelViewFactory());
                sview.SetController(cont);
                view.AddPage("Restored TCP Server", sview);
            }*/
            /*{ // restored instance
                var pview = new DefaultInstanceView();
                var cont = new DefaultInstanceController(pview, new InstanceLogger(@"D:\test\tcp_proxy_2.tmp"), new DefaultInstanceController.DefaultChannelViewFactory());
                pview.SetController(cont);
                view.AddPage("Restored TCP Proxy", pview);
            }*/
            { // restored instance
                var pview = new DefaultInstanceView();
                var cont = new DefaultInstanceController(pview, new InstanceLogger(@"D:\test\tcp_client_2.tmp"), new DefaultInstanceController.DefaultChannelViewFactory());
                pview.SetController(cont);
                view.AddPage("Restored TCP Client", pview);
            }
            /*var manualDriver = new ManualChannelDriver(10);
            var dummyDriver = new DummyDriver();
            var proxyDriver = new DefaultProxyDriver(true);
            {
                var server = new TcpServer(new TcpServerSettings { LocalEndPoint = new IPEndPoint(IPAddress.Loopback, 8081) });
                var sview = new DefaultInstanceView();
                var cont = new DefaultInstanceController(sview, server);
                cont.AddDriver(manualDriver, 0);
                cont.AddDriver(dummyDriver, 1);
                sview.SetController(cont);
                view.AddPage("TCP Server", sview);
                cont.Start();
            }
            {
                var server = new TcpServer(new TcpServerSettings { LocalEndPoint = new IPEndPoint(IPAddress.Loopback, 8080) });
                var factory = new TcpClientFactory(new TcpClientFactorySettings { LocalIPAddress = IPAddress.Loopback, RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 8081) });
                var proxy = new DefaultProxy(server, factory);
                var pview = new DefaultInstanceView();
                var cont = new DefaultInstanceController(pview, proxy);
                cont.AddDriver(proxyDriver, 0);
                pview.SetController(cont);
                view.AddPage("TCP Proxy", pview);
                cont.Start();
            }
            {
                var client = new TcpClient(new TcpClientSettings { LocalEndPoint = new IPEndPoint(IPAddress.Loopback, 0), RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 8080) });
                var cview = new DefaultInstanceView();
                var cont = new DefaultInstanceController(cview, client);
                cont.AddDriver(manualDriver, 0);
                cont.AddDriver(dummyDriver, 1);
                cview.SetController(cont);
                view.AddPage("TCP Client", cview);
                cont.Start();
            }
            /*{
                var server = new UdpServer(new UdpServerSettings { LocalEndPoint = new IPEndPoint(IPAddress.Loopback, 7081) });
                var sview = new DefaultInstanceView();
                var cont = new DefaultInstanceController(sview, server);
                cont.AddDriver(new ManualChannelDriver(1), 0);
                cont.AddDriver(new DummyDriver(), 1);
                sview.SetController(cont);
                view.AddPage("UDP Server", sview);
                cont.Start();
            }
            {
                var server = new UdpServer(new UdpServerSettings { LocalEndPoint = new IPEndPoint(IPAddress.Loopback, 7080) });
                var factory = new UdpClientFactory(new UdpClientFactorySettings { LocalIPAddress = IPAddress.Loopback, RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 7081) });
                var proxy = new DefaultProxy(server, factory);
                var pview = new DefaultInstanceView();
                var cont = new DefaultInstanceController(pview, proxy);
                cont.AddDriver(manualDriver, 0);
                pview.SetController(cont);
                view.AddPage("UDP Proxy", pview);
                cont.Start();
            }
            {
                var client = new UdpClient(new UdpClientSettings { LocalEndPoint = new IPEndPoint(IPAddress.Loopback, 0), RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 7080) });
                var cview = new DefaultInstanceView();
                var cont = new DefaultInstanceController(cview, client);
                cont.AddDriver(manualDriver, 0);
                cont.AddDriver(dummyDriver, 1);
                cview.SetController(cont);
                view.AddPage("UDP Client", cview);
                cont.Start();
            }
            {
                var client = new UdpClient(new UdpClientSettings { LocalEndPoint = new IPEndPoint(IPAddress.Loopback, 0), RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 7080) });
                var cview = new DefaultInstanceView();
                var cont = new DefaultInstanceController(cview, client);
                cont.AddDriver(manualDriver, 0);
                cont.AddDriver(dummyDriver, 1);
                cview.SetController(cont);
                view.AddPage("UDP Client2", cview);
                cont.Start();
            }*/
        }

        public void Close()
        {
            foreach(var cont in controllers)
            {
                cont.Stop();
            }
        }

        public void CreateServer()
        {
            /*var dialog = new TcpServerDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                var settings = dialog.Settings;
                var server = new TcpServer(settings);
                var sview = new DefaultInstanceView();
                var cont = new DefaultInstanceController(sview, server);
                sview.SetController(cont);
                view.AddPage("Server", sview);
                cont.Start();
            }*/
        }
        public void CreateClient()
        {
            /*var dialog = new TcpClientDialog();
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
            }*/
        }
        public void CreateProxy()
        {
            /*var sdialog = new TcpServerDialog();
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
            }*/
        }
    }
}
