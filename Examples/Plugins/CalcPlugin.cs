using Examples.Network.Calc;
using Netool.Controllers;
using Netool.Dialogs.Tcp;
using Netool.Network;
using Netool.Network.Tcp;
using Netool.Plugins;
using Netool.Views.Instance;
using System;
using System.Windows.Forms;

namespace Examples.Plugins
{
    public class CalcPlugin : IProtocolPlugin, IExtensiblePlugin
    {
        public bool SupportsClient { get { return false; } }
        public bool SupportsProxy { get { return false; } }
        public bool SupportsServer { get { return true; } }
        public string Author { get { return "Hynek Schlindenbuch"; } }
        public string Description { get { return "Example plugin for a simple protocol called Calc"; } }
        public long ID { get { return 100001; } }
        public string Name { get { return "CalcPlugin"; } }
        public Version Version { get { return new Version(0, 1); } }
        public string ProtocolName { get { return "Calc"; } }
        private PluginLoader loader;

        public InstancePack CreateInstance(Netool.Logging.InstanceLogger logger, InstanceType type, object settings)
        {
            IInstance instance;
            if (settings == null) throw new ArgumentNullException("settings");
            switch (type)
            {
                case InstanceType.Server:
                    var s = settings as TcpServerSettings;
                    if (s == null) throw new InvalidSettingsTypeException();
                    instance = new TcpServer(s);
                    break;

                default:
                    throw new NotImplementedException();
            }
            var view = new DefaultInstanceView();
            var cont = new DefaultInstanceController(view, instance, logger, loader);
            view.SetController(cont);
            return new InstancePack(view, cont, type);
        }

        public InstancePack CreateInstance(Netool.Logging.InstanceLogger logger, InstanceType type)
        {
            IInstance instance;
            switch (type)
            {
                case InstanceType.Server:
                    instance = createServer();
                    break;

                default:
                    throw new NotImplementedException();
            }
            if (instance == null) throw new SetupAbortedByUserException();
            var view = new DefaultInstanceView();
            var cont = new DefaultInstanceController(view, instance, logger, loader);
            view.SetController(cont);
            return new InstancePack(view, cont, type);
        }

        public InstancePack RestoreInstance(Netool.Logging.InstanceLogger logger)
        {
            var view = new DefaultInstanceView();
            var cont = new DefaultInstanceController(view, logger, loader);
            view.SetController(cont);
            return new InstancePack(view, cont, cont.GetInstanceType());
        }

        private CalcServer createServer()
        {
            var dialog = new TcpServerDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                var settings = dialog.Settings;
                return new CalcServer(settings);
            }
            return null;
        }

        void IExtensiblePlugin.AfterLoad(PluginLoader loader)
        {
            this.loader = loader;
        }
    }
}