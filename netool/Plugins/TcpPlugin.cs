using Netool.Controllers;
using Netool.Dialogs.Tcp;
using Netool.ChannelDrivers;
using Netool.Logging;
using Netool.Network;
using Netool.Network.Tcp;
using Netool.Views.Instance;
using System;
using System.Windows.Forms;

namespace Netool.Plugins
{
    public class TcpPlugin : IProtocolPlugin
    {
        public long ID { get { return 1; } }
        public string Name { get { return "TcpPlugin"; } }
        public string Description { get { return "Plugin for basic TCP functions. Supports Client, Server and Proxy."; } }
        private Version version = new Version(0, 1);
        public Version Version { get { return version; } }
        public string Author { get { return "Hynek Schlindenbuch"; } }

        public bool SupportsServer { get { return true; } }
        public bool SupportsClient { get { return true; } }
        public bool SupportsProxy { get { return true; } }
        public string ProtocolName { get { return "Tcp"; } }

        public InstancePack CreateInstance(InstanceLogger logger, InstanceType type)
        {
            IInstance instance;
            switch (type)
            {
                case InstanceType.Server:
                    instance = createServer();
                    break;

                case InstanceType.Client:
                    instance = createClient();
                    break;

                default:
                    instance = createProxy();
                    break;
            }
            if (instance == null) throw new InstanceCreationAbortedByUser();
            // for now set manual driver to everything
            var manualDriver = new ManualChannelDriver(-1);
            var view = new DefaultInstanceView();
            var cont = new DefaultInstanceController(view, instance, logger);
            cont.AddDriver(manualDriver, 0);
            view.SetController(cont);
            return new InstancePack(view, cont, type);
        }

        public InstancePack RestoreInstance(InstanceLogger logger)
        {
            var view = new DefaultInstanceView();
            var cont = new DefaultInstanceController(view, logger, new DefaultInstanceController.DefaultChannelViewFactory());
            view.SetController(cont);
            return new InstancePack(view, cont, cont.GetInstanceType());
        }

        private TcpServer createServer()
        {
            var dialog = new TcpServerDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                var settings = dialog.Settings;
                return new TcpServer(settings);
            }
            return null;
        }

        private TcpClient createClient()
        {
            var dialog = new TcpClientDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                var settings = dialog.Settings;
                return new TcpClient(settings);
            }
            return null;
        }

        private DefaultProxy createProxy()
        {
            var dialog = new TcpProxyDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                var clFactory = new TcpClientFactory(dialog.ClientFactorySettings);
                var srv = new TcpServer(dialog.ServerSettings);
                return new DefaultProxy(srv, clFactory);
            }
            return null;
        }
    }
}