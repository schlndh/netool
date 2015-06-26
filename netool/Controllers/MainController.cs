using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
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
            var manualDriver = new ManualChannelDriver(2);
            var dummyDriver = new DummyDriver();
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
                var pview = new ProxyView();
                var cont = new ProxyController(pview, proxy);
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
            {
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
                proxy.ChannelCreated += delegate(object sender, IProxyChannel channel)
                {
                    channel.RequestReceived += delegate(object s, DataEventArgs e)
                    {
                        if (e.Data != null && e.Data.ToByteArray().Length < 10)
                            channel.SendToServer(new ByteArray(ASCIIEncoding.ASCII.GetBytes(ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray()).ToUpper())));
                    };
                    channel.ResponseReceived += delegate(object s, DataEventArgs e)
                    {
                        if (e.Data != null && e.Data.ToByteArray().Length < 10)
                            channel.SendToClient(new ByteArray(ASCIIEncoding.ASCII.GetBytes(ASCIIEncoding.ASCII.GetString(e.Data.ToByteArray()).ToLower())));
                    };
                };
                var pview = new ProxyView();
                var cont = new ProxyController(pview, proxy);
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
                var sview = new DefaultInstanceView();
                var cont = new DefaultInstanceController(sview, server);
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
