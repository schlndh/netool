using Netool.Dialogs;
using Netool.Logging;
using Netool.Network;
using Netool.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace Netool.Controllers
{
    public class MainController
    {
        private MainView view;
        private MainModel model;
        private List<IInstanceController> controllers = new List<IInstanceController>();
        private Dictionary<long, IProtocolPlugin> protocols = new Dictionary<long, IProtocolPlugin>();

        public MainController(MainView view)
        {
            this.view = view;
            this.view.SetController(this);
            // register built-in plugins
            IProtocolPlugin plugin = new TcpPlugin();
            protocols.Add(plugin.ID, plugin);

            load();
        }

        /// <summary>
        /// Load settings, open previously open instances, etc
        /// </summary>
        private void load()
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (appDataDir != "")
            {
                appDataDir += "/netool";
                if (!Directory.Exists(appDataDir))
                {
                    Directory.CreateDirectory(appDataDir);
                }
                try
                {
                    using(var file = new FileStream(appDataDir + "/instances.nest", FileMode.Open, FileAccess.Read))
                    {
                        var formatter = new BinaryFormatter();
                        model = (MainModel)formatter.Deserialize(file);
                        file.Close();
                    }
                }
                catch
                {
                    model = new MainModel();
                }
            }

            foreach(var instance in model.OpenInstances)
            {
                IProtocolPlugin plugin;
                if(protocols.TryGetValue(instance.PluginID, out plugin))
                {
                    // temp log file - dont bother user with log file dialogs now
                    var logger = new InstanceLogger();
                    logger.WritePluginID(plugin.ID);
                    var pack = plugin.CreateInstance(logger, instance.Type, instance.Settings);
                    controllers.Add(pack.Controller);
                    view.AddPage(instance.Name, pack.View.GetForm());
                }

            }
        }

        public void Close()
        {
            foreach (var cont in controllers)
            {
                cont.Stop();
                cont.Close();
            }
            if(model.OpenInstances.Count > 0)
            {
                var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (appDataDir != "")
                {
                    appDataDir += "/netool";
                    if (!Directory.Exists(appDataDir))
                    {
                        Directory.CreateDirectory(appDataDir);
                    }
                    using (var file = new FileStream(appDataDir + "/instances.nest", FileMode.Create, FileAccess.Write))
                    {
                        var formatter = new BinaryFormatter();
                        formatter.Serialize(file, model);
                        file.Close();
                    }
                }
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
                    controllers.Add(pack.Controller);
                    model.AddInstance(plugin.ID, name, type, pack.Controller.Instance.Settings);
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

        public void RestoreInstance(string filename)
        {
            InstanceLogger logger = new InstanceLogger(filename);
            var id = logger.ReadPluginID();
            IProtocolPlugin plugin;
            if(protocols.TryGetValue(id, out plugin))
            {
                var pack = plugin.RestoreInstance(logger);
                controllers.Add(pack.Controller);
                view.AddPage("placeholder", pack.View.GetForm());
            }
            else
            {
                throw new UnknownPluginException(id);
            }
        }
    }
}