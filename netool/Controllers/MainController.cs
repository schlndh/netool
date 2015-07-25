using Netool.Dialogs;
using Netool.Logging;
using Netool.Network;
using Netool.Plugins;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Netool.Controllers
{
    public class MainController
    {
        private MainView view;
        private MainModel model;
        private List<IInstanceController> controllers = new List<IInstanceController>();
        private List<IProtocolPlugin> protocols = new List<IProtocolPlugin>();

        public MainController(MainView view, MainModel model)
        {
            this.view = view;
            this.view.SetController(this);
            this.model = model;
            // register built-in plugins
            protocols.Add(new TcpPlugin());
        }

        /// <summary>
        /// Load settings, open previously open instances, etc
        /// </summary>
        public void Load()
        {
        }

        public void Close()
        {
            foreach (var cont in controllers)
            {
                cont.Stop();
            }
        }

        public void CreateServer()
        {
            createInstance(InstanceType.Server);
        }

        public void CreateClient()
        {
            createInstance(InstanceType.Client);
        }

        public void CreateProxy()
        {
            createInstance(InstanceType.Proxy);
        }

        private void createInstance(InstanceType type)
        {
            try
            {
                var dialog = new CreateInstanceDialog(protocols, type);
                dialog.ShowDialog();
                if (dialog.DialogResult == DialogResult.OK)
                {
                    var plugin = dialog.SelectedPlugin;
                    var name = dialog.InstanceName;
                    var pack = plugin.CreateInstance(new InstanceLogger(), type);
                    view.AddPage(name, pack.View.GetForm());
                }
            }
            catch (InstanceCreationAbortedByUser)
            { }
        }
    }
}