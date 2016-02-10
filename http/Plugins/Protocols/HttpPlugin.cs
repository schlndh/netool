using Netool.Controllers;
using Netool.Dialogs.Tcp;
using Netool.Logging;
using Netool.Network;
using Netool.Network.Http;
using Netool.Plugins.Http;
using Netool.Views;
using Netool.Views.Channel;
using Netool.Views.Instance;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Netool.Plugins.Protocols
{
    /// <summary>
    /// Plugin for basic HTTP functions.
    /// </summary>
    public class HttpPlugin : IProtocolPlugin, IExtensiblePlugin
    {
        /// <inheritdoc/>
        public long ID { get { return 2001; } }
        /// <inheritdoc/>
        public string Name { get { return "HttpPlugin"; } }
        /// <inheritdoc/>
        public string Description { get { return "Plugin for basic HTTP functions. Supports Client, Server and Proxy."; } }
        /// <inheritdoc/>
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
        public string ProtocolName { get { return "Http"; } }

        private List<IProtocolUpgradePlugin> upgradePlugins = new List<IProtocolUpgradePlugin>();

        /// <inheritdoc/>
        public InstancePack CreateInstance(InstanceLogger logger, InstanceType type)
        {
            IInstance instance;
            switch (type)
            {
                case InstanceType.Server:
                    instance = createServer(logger);
                    break;

                case InstanceType.Client:
                    instance = createClient(logger);
                    break;

                default:
                    throw new NotImplementedException();
                   // instance = createProxy();
                    break;
            }
            if (instance == null) throw new SetupAbortedByUserException();
            // for now set manual driver to everything
            var view = new DefaultInstanceView();
            var cont = new DefaultInstanceController(view, instance, logger);
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
                    var s = settings as HttpServerSettings;
                    if (s == null) throw new InvalidSettingsTypeException();
                    instance = new HttpServer(s, logger);
                    break;

                case InstanceType.Client:
                    var c = settings as HttpClientSettings;
                    if (c == null) throw new InvalidSettingsTypeException();
                    instance = new HttpClient(c, logger);
                    break;

                default:
                    throw new NotImplementedException();
                    var p = settings as DefaultProxySettings;
                    if (p == null) throw new InvalidSettingsTypeException();
                    instance = new DefaultProxy(p);
                    break;
            }
            // for now set manual driver to everything
            var view = new DefaultInstanceView();
            var cont = new DefaultInstanceController(view, instance, logger, new DefaultInstanceController.DefaultChannelViewFactory(channelViewCallback));
            view.SetController(cont);
            return new InstancePack(view, cont, type);
        }

        /// <inheritdoc/>
        public InstancePack RestoreInstance(InstanceLogger logger)
        {
            var view = new DefaultInstanceView();
            var cont = new DefaultInstanceController(view, logger, new DefaultInstanceController.DefaultChannelViewFactory(channelViewCallback));
            view.SetController(cont);
            return new InstancePack(view, cont, cont.GetInstanceType());
        }

        private HttpServer createServer(InstanceLogger logger)
        {
            var dialog = new TcpServerDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                var settings = dialog.Settings;
                return new HttpServer(new HttpServerSettings { TcpSettings = settings }, logger);
            }
            return null;
        }

        private HttpClient createClient(InstanceLogger logger)
        {
            var dialog = new TcpClientDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                var settings = dialog.Settings;
                return new HttpClient(new HttpClientSettings { TcpSettings = settings }, logger);
            }
            return null;
        }

        /*private DefaultProxy createProxy()
        {
            var dialog = new TcpProxyDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                var clFactory = new TcpClientFactory(dialog.ClientFactorySettings);
                var srv = new TcpServer(dialog.ServerSettings);
                return new DefaultProxy(new DefaultProxySettings { server = srv, ClientFactory = clFactory });
            }
            return null;
        }*/

        private IChannelView channelViewCallback(DefaultChannelView v)
        {
            var httpMenu = new ToolStripMenuItem("Http");
            if(upgradePlugins.Count > 0)
            {
                var upgrade = new ToolStripMenuItem("Protocol Upgrade");
                foreach(var p in upgradePlugins)
                {
                    upgrade.DropDownItems.Add(new ToolStripMenuItem(p.ProtocolName, null, delegate(object sender, EventArgs e) {
                        IProtocolUpgrader upgrader;
                        try
                        {
                            upgrader = p.CreateUpgrader();
                        }
                        catch(SetupAbortedByUserException)
                        {
                            return;
                        }
                        if(upgrader == null) return;
                        if(v.Channel.GetType() == typeof(HttpServerChannel))
                        {
                            ((HttpServerChannel)v.Channel).UpgradeProtocol(upgrader);
                        }
                        else if(v.Channel.GetType() == typeof(HttpClientChannel))
                        {
                            ((HttpClientChannel)v.Channel).UpgradeProtocol(upgrader);
                        }
                        // TODO: add upgrading for http proxy (once proxy is enabled)
                    }));
                }
                httpMenu.DropDownItems.Add(upgrade);
            }
            v.AddMenuItem(httpMenu);
            return v;
        }

        public void AfterLoad(PluginLoader loader)
        {
            foreach(var p in loader.Plugins)
            {
                loader_PluginLoaded(loader, p);

            }
            loader.PluginLoaded += loader_PluginLoaded;
        }

        private void loader_PluginLoaded(object sender, IPlugin e)
        {
            var upgrader = e as IProtocolUpgradePlugin;
            if (upgrader != null) upgradePlugins.Add(upgrader);
        }
    }
}