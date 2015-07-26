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
        private Dictionary<long, IProtocolPlugin> protocols = new Dictionary<long, IProtocolPlugin>();

        public MainController(MainView view, MainModel model)
        {
            this.view = view;
            this.view.SetController(this);
            this.model = model;
            // register built-in plugins
            IProtocolPlugin plugin = new TcpPlugin();
            protocols.Add(plugin.ID, plugin);
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
            InstanceLogger logger = null;
            try
            {
                var dialog = new CreateInstanceDialog(protocols.Values, type);
                dialog.ShowDialog();
                if (dialog.DialogResult == DialogResult.OK)
                {
                    var plugin = dialog.SelectedPlugin;
                    var name = dialog.InstanceName;
                    var file = dialog.LogFileName;
                    try
                    {
                        logger = new InstanceLogger(file);
                    }
                    // something went wrong with the path, save it as temp file
                    catch
                    {
                        logger = new InstanceLogger();
                    }
                    logger.WritePluginID(plugin.ID);
                    var pack = plugin.CreateInstance(logger, type);
                    view.AddPage(name, pack.View.GetForm());
                }
            }
            catch (InstanceCreationAbortedByUser)
            {
                if(logger != null)
                {
                    logger.Close();
                }
            }
        }
    }
}