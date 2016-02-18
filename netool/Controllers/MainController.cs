using Netool.ChannelDrivers;
using Netool.Dialogs;
using Netool.Logging;
using Netool.Network;
using Netool.Plugins;
using Netool.Plugins.ChannelDrivers;
using Netool.Plugins.Protocols;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace Netool.Controllers
{
    public class MainController : IMainController
    {
        private class ControllerData
        {
            public IInstanceController Controller;
            public bool Active;

            public ControllerData(IInstanceController cont, bool active)
            {
                Controller = cont;
                Active = active;
            }
        }

        private MainView view;
        private MainModel model;
        private Dictionary<int, ControllerData> controllers = new Dictionary<int, ControllerData>();
        private Dictionary<int, IChannelDriver> channelDrivers = new Dictionary<int, IChannelDriver>();
        private Dictionary<long, IProtocolPlugin> protocolPlugins = new Dictionary<long, IProtocolPlugin>();
        private Dictionary<long, IChannelDriverPlugin> channelDriverPlugins = new Dictionary<long, IChannelDriverPlugin>();
        private Dictionary<long, IEditorViewPlugin> editorViewPlugins = new Dictionary<long, IEditorViewPlugin>();
        private Dictionary<long, IEventViewPlugin> eventViewPlugins = new Dictionary<long, IEventViewPlugin>();
        private PluginLoader pluginLoader = new PluginLoader();

        private int itemID = 0;

        public MainController(MainView view)
        {
            this.view = view;
            this.view.SetController(this);
            loadExternalPlugins();
            load();
        }

        private void loadExternalPlugins()
        {
            // load plugins from libnetool
            pluginLoader.LoadPluginsFromAssembly(typeof(IPlugin).Assembly);
            pluginLoader.LoadPluginsFromDirectory(AppDomain.CurrentDomain.BaseDirectory + "Plugins");
            foreach(var pl in pluginLoader.Plugins)
            {
                var proto = pl as IProtocolPlugin;
                if (proto != null)
                {
                    protocolPlugins.Add(proto.ID, proto);
                }
                var cd = pl as IChannelDriverPlugin;
                if (cd != null)
                {
                    channelDriverPlugins.Add(cd.ID, cd);
                }
                var evt = pl as IEventViewPlugin;
                if (evt != null)
                {
                    eventViewPlugins.Add(evt.ID, evt);
                }
                var ed = pl as IEditorViewPlugin;
                if (ed != null)
                {
                    editorViewPlugins.Add(ed.ID, ed);
                }
            }
        }

        /// <summary>
        /// Load settings, open previously open instances, etc
        /// </summary>
        private void load()
        {
            // to enable deserializing types from plugins
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
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
                    using(var file = new FileStream(appDataDir + "/session.nest", FileMode.Open, FileAccess.Read))
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

            var invalidDrivers = new List<int>();
            foreach (var driver in model.ChannelDrivers)
            {
                try
                {
                    IChannelDriverPlugin plugin;
                    itemID = Math.Max(itemID, driver.Key);
                    if (channelDriverPlugins.TryGetValue(driver.Value.PluginID, out plugin))
                    {
                        var pack = plugin.CreateChannelDriver(driver.Value.Settings);
                        pack.Driver.Name = driver.Value.Name;
                        channelDrivers.Add(driver.Key, pack.Driver);
                        view.AddChannelDriver(driver.Key, pack);
                    }
                }
                catch
                {
                    invalidDrivers.Add(driver.Key);
                    // TODO: some error reporting here
                }
            }

            foreach(var driver in invalidDrivers)
            {
                model.RemoveChannelDriver(driver);
            }

            var invalidInstances = new List<int>();
            foreach(var instance in model.OpenInstances)
            {
                try
                {
                    IProtocolPlugin plugin;
                    itemID = Math.Max(itemID, instance.Key);
                    if(protocolPlugins.TryGetValue(instance.Value.PluginID, out plugin))
                    {
                        // temp log file - dont bother user with log file dialogs now
                        var logger = new InstanceLogger();
                        logger.WritePluginID(plugin.ID);
                        logger.WriteInstanceName(instance.Value.Name);
                        var pack = plugin.CreateInstance(logger, instance.Value.Type, instance.Value.Settings);
                        pack.Controller.SetMainController(this);
                        invalidDrivers.Clear();
                        foreach(var driver in instance.Value.Drivers)
                        {
                            IChannelDriver d = null;
                            if(channelDrivers.TryGetValue(driver.Key, out d))
                            {
                                pack.Controller.AddDriver(d, driver.Value);
                            }
                            else if(MessageBox.Show("Cannot load one of the attached drivers. Load the instance anyway?", "Missing Driver", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                            {
                                throw new Exception("Missing driver");
                            }
                            else
                            {
                                invalidDrivers.Add(driver.Key);
                            }
                        }
                        foreach (var driver in invalidDrivers)
                        {
                            model.RemoveDriverFromInstance(instance.Key, driver);
                        }
                        bindInstanceEvents(pack.Controller.Instance);
                        controllers.Add(instance.Key, new ControllerData(pack.Controller, true));
                        view.AddInstance(instance.Key, instance.Value.Name, pack.View);
                    }
                }
                catch
                {
                    invalidInstances.Add(instance.Key);
                }
            }

            foreach(var instance in invalidInstances)
            {
                model.RemoveInstance(instance);
            }
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;
            pluginLoader.TryGetAssembly(args.Name, out assembly);
            return assembly;
        }

        public void Close()
        {
            bool toAll = false;
            bool yes = false;
            foreach (var kv in controllers)
            {
                var cont = kv.Value.Controller;
                cont.Stop();
                cont.Close();
                if (!kv.Value.Active) continue;
                var instanceName = model.OpenInstances[kv.Key].Name;
                if (cont.Logger != null && cont.Logger.IsTempFile)
                {
                    if (toAll)
                    {
                        if(yes)
                        {
                            saveLogFile(cont, instanceName);
                        }
                        else
                        {
                            cont.Logger.DeleteFile();
                        }
                    }
                    else
                    {
                        // TODO: show actual name here
                        var res = view.ShowSaveTempLogFileDialog(instanceName);
                        switch (res)
                        {
                            case MainView.SaveTempLogResult.Cancel:
                                break;
                            case MainView.SaveTempLogResult.YesToAll:
                            case MainView.SaveTempLogResult.Yes:
                                if (res == MainView.SaveTempLogResult.YesToAll)
                                {
                                    toAll = true;
                                    yes = true;
                                }
                                saveLogFile(cont, instanceName);
                                break;
                            case MainView.SaveTempLogResult.No:
                                cont.Logger.DeleteFile();
                                break;
                            case MainView.SaveTempLogResult.NoToAll:
                                cont.Logger.DeleteFile();
                                toAll = true;
                                yes = false;
                                break;
                        }
                    }
                }
            }
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (appDataDir != "")
            {
                appDataDir += "/netool";
                if (!Directory.Exists(appDataDir))
                {
                    Directory.CreateDirectory(appDataDir);
                }
                using (var file = new FileStream(appDataDir + "/session.nest", FileMode.Create, FileAccess.Write))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(file, model);
                    file.Close();
                }
            }
        }

        private void saveLogFile(IInstanceController cont, string instanceName)
        {
            var filename = view.GetLogFileTargetName(instanceName);
            if (filename == null)
            {
                cont.Logger.DeleteFile();
            }
            else
            {
                cont.Logger.MoveToFile(filename);
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
                var dialog = new CreateInstanceDialog(protocolPlugins.Values, type);
                dialog.ShowDialog();
                if (dialog.DialogResult == DialogResult.OK)
                {
                    var plugin = dialog.SelectedPlugin;
                    var name = dialog.InstanceName;
                    var file = dialog.LogFileName;
                    try
                    {
                        logger = new InstanceLogger(file, FileMode.Create);
                    }
                    // something went wrong with the path, save it as temp file
                    catch
                    {
                        logger = new InstanceLogger();
                    }
                    logger.WritePluginID(plugin.ID);
                    logger.WriteInstanceName(name);
                    var pack = plugin.CreateInstance(logger, type);
                    pack.Controller.SetMainController(this);
                    var drivers = setupDrivers(pack.Controller);
                    ++itemID;
                    controllers.Add(itemID, new ControllerData(pack.Controller, true));
                    model.AddInstance(itemID, plugin.ID, name, type, pack.Controller.Instance.Settings);
                    foreach(var driver in drivers)
                    {
                        model.AddDriverToInstance(itemID, driver.Key, driver.Value);
                    }
                    bindInstanceEvents(pack.Controller.Instance);
                    view.AddInstance(itemID, name, pack.View);
                }
            }
            catch (SetupAbortedByUserException)
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
            if(protocolPlugins.TryGetValue(id, out plugin))
            {
                var name = logger.ReadInstanceName();
                var pack = plugin.RestoreInstance(logger);
                pack.Controller.SetMainController(this);
                ++itemID;
                controllers.Add(itemID, new ControllerData(pack.Controller, false));
                bindInstanceEvents(pack.Controller.Instance);
                view.AddInstance(itemID, name ?? "unknown", pack.View);
            }
            else
            {
                throw new UnknownPluginException(id);
            }
        }

        public void CreateChannelDriver()
        {
            try
            {
                var dialog = new CreateChannelDriverDialog(channelDriverPlugins.Values);
                dialog.ShowDialog();
                if (dialog.DialogResult == DialogResult.OK)
                {
                    var plugin = dialog.SelectedPlugin;
                    var name = dialog.DriverName;
                    var pack = plugin.CreateChannelDriver();
                    pack.Driver.Name = name;
                    model.AddChannelDriver(++itemID, plugin.ID, name, pack.Driver.Settings);
                    channelDrivers.Add(itemID, pack.Driver);
                    view.AddChannelDriver(itemID, pack);
                }
            }
            catch (SetupAbortedByUserException) { }
        }

        public Dictionary<int, int> setupDrivers(IInstanceController cont)
        {
            var dialog = new SetupChannelDriversDialog(channelDrivers);
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                foreach(var item in dialog.Drivers)
                {
                    cont.AddDriver(channelDrivers[item.Key], item.Value);
                }
                return dialog.Drivers;
            }
            else
            {
                throw new SetupAbortedByUserException();
            }

        }

        public List<Views.IEditorView> CreateEditorViews()
        {
            var ret = new List<Views.IEditorView>();
            foreach(var item in editorViewPlugins)
            {
                ret.AddRange(item.Value.CreateEditorViews());
            }
            return ret;
        }

        public List<Views.IEventView> CreateEventViews()
        {
            var ret = new List<Views.IEventView>();
            foreach (var item in eventViewPlugins)
            {
                ret.AddRange(item.Value.CreateEventViews());
            }
            return ret;
        }

        public void RemoveChannelDriver(int id)
        {
            channelDrivers.Remove(id);
            model.RemoveChannelDriver(id);
            // TODO: remove it from existing instances? if so change the ChannelDriversTab's Remove button's tooltip
        }

        public void RemoveInstance(int id)
        {
            ControllerData data;
            if(controllers.TryGetValue(id, out data))
            {
                var cont = data.Controller;
                cont.Stop();
                cont.Close();
                if(data.Active)
                {
                    var instanceName = model.OpenInstances[id].Name;
                    model.RemoveInstance(id);
                    if (cont.Logger != null && cont.Logger.IsTempFile)
                    {
                        if (view.ShowYesNoBox(string.Format("The log file for {0} is temporary. Do you want to save it?", instanceName), "Save the log file?"))
                        {
                            saveLogFile(cont, instanceName);
                        }
                        else
                        {
                            cont.Logger.DeleteFile();
                        }
                    }
                }
                controllers.Remove(id);
            }
        }

        private void bindInstanceEvents(IInstance instance)
        {
            instance.ErrorOccured += handleErrorOccured;
            var client = instance as IClient;
            if(client != null)
            {
                client.ChannelCreated += handleTypedChannelCreated;
            }
            var server = instance as IServer;
            if (server != null)
            {
                server.ChannelCreated += handleTypedChannelCreated;
            }
            var proxy = instance as IProxy;
            if (proxy != null)
            {
                proxy.ChannelCreated += handleTypedChannelCreated;
            }
        }

        private void handleErrorOccured(object sender, Exception e)
        {
            Debug.WriteLine("Error occured sender: {0}, Exception: {1}", sender, e);
            view.ShowErrorMessage(sender, e);
        }

        private void handleTypedChannelCreated(object sender, IClientChannel c)
        {
            handleChannelCreated(sender, c);
        }

        private void handleTypedChannelCreated(object sender, IServerChannel c)
        {
            handleChannelCreated(sender, c);
        }

        private void handleTypedChannelCreated(object sender, IProxyChannel c)
        {
            handleChannelCreated(sender, c);
        }

        private void handleChannelCreated(object sender, IChannel c)
        {
            c.ErrorOccured += handleErrorOccured;
        }
    }
}