using Netool.Controllers;
using Netool.Dialogs.Tcp;
using Netool.Logging;
using Netool.Network;
using Netool.Network.Http;
using Netool.Views.Instance;
using System;
using System.Windows.Forms;

namespace Netool.Plugins.Protocols
{
    /// <summary>
    /// Plugin for basic HTTP functions.
    /// </summary>
    public class HttpPlugin : IProtocolPlugin
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
            var cont = new DefaultInstanceController(view, instance, logger);
            view.SetController(cont);
            return new InstancePack(view, cont, type);
        }

        /// <inheritdoc/>
        public InstancePack RestoreInstance(InstanceLogger logger)
        {
            var view = new DefaultInstanceView();
            var cont = new DefaultInstanceController(view, logger, new DefaultInstanceController.DefaultChannelViewFactory());
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
    }
}