using Netool.Controllers;
using Netool.Dialogs;
using Netool.Logging;
using Netool.Network;
using Netool.Network.Udp;
using Netool.Views.Instance;
using System;
using System.Windows.Forms;

namespace Netool.Plugins.Protocols
{
    /// <summary>
    /// Plugin for basic Udp functions.
    /// </summary>
    public class UdpPlugin : IProtocolPlugin, IExtensiblePlugin
    {
        /// <inheritdoc/>
        public long ID { get { return 2; } }
        /// <inheritdoc/>
        public string Name { get { return "UdpPlugin"; } }
        /// <inheritdoc/>
        public string Description { get { return "Plugin for basic Udp functions. Supports Client, Server and Proxy."; } }
        private Version version = new Version(0, 1);
        /// <inheritdoc/>
        public Version Version { get { return version; } }
        /// <inheritdoc/>
        public string Author { get { return "Hynek Schlindenbuch"; } }

        /// <inheritdoc/>
        public bool SupportsServer { get { return true; } }
        /// <inheritdoc/>
        public bool SupportsClient { get { return true; } }
        /// <inheritdoc/>
        public bool SupportsProxy { get { return true; } }
        /// <inheritdoc/>
        public string ProtocolName { get { return "Udp"; } }

        private PluginLoader loader;

        /// <inheritdoc/>
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
            if (instance == null) throw new SetupAbortedByUserException();
            // for now set manual driver to everything
            var view = new DefaultInstanceView();
            var cont = new DefaultInstanceController(view, instance, logger, loader);
            view.SetController(cont);
            return new InstancePack(view, cont, type);
        }

        /// <inheritdoc/>
        public InstancePack CreateInstance(InstanceLogger logger, InstanceType type, object settings)
        {
            IInstance instance;
            if (settings == null) throw new ArgumentNullException("settings");
            switch(type)
            {
                case InstanceType.Server:
                    var s = settings as UdpServerSettings;
                    if (s == null) throw new InvalidSettingsTypeException();
                    instance = new UdpServer(s);
                    break;

                case InstanceType.Client:
                    var c = settings as UdpClientSettings;
                    if (c == null) throw new InvalidSettingsTypeException();
                    instance = new UdpClient(c);
                    break;

                default:
                    var p = settings as DefaultProxySettings;
                    if (p == null) throw new InvalidSettingsTypeException();
                    instance = new DefaultProxy(p);
                    break;
            }
            // for now set manual driver to everything
            var view = new DefaultInstanceView();
            var cont = new DefaultInstanceController(view, instance, logger, loader);
            view.SetController(cont);
            return new InstancePack(view, cont, type);
        }

        /// <inheritdoc/>
        public InstancePack RestoreInstance(InstanceLogger logger)
        {
            var view = new DefaultInstanceView();
            var cont = new DefaultInstanceController(view, logger, loader);
            view.SetController(cont);
            return new InstancePack(view, cont, cont.GetInstanceType());
        }

        private UdpServer createServer()
        {
            var dialog = new DefaultServerDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                var settings = dialog.UdpSettings;
                return new UdpServer(settings);
            }
            return null;
        }

        private UdpClient createClient()
        {
            var dialog = new DefaultClientDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                var settings = dialog.UdpSettings;
                return new UdpClient(settings);
            }
            return null;
        }

        private DefaultProxy createProxy()
        {
            var dialog = new DefaultProxyDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                var clFactory = new UdpClientFactory(dialog.UdpClientFactorySettings);
                var srv = new UdpServer(dialog.UdpServerSettings);
                return new DefaultProxy(new DefaultProxySettings { Server = srv, ClientFactory = clFactory });
            }
            return null;
        }

        void IExtensiblePlugin.AfterLoad(PluginLoader loader)
        {
            this.loader = loader;
        }
    }
}