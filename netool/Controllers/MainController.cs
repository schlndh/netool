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
        private MainView view;
        private MainModel model;
        private List<IInstanceController> controllers = new List<IInstanceController>();
        private Dictionary<int, IChannelDriver> channelDrivers = new Dictionary<int, IChannelDriver>();
        private Dictionary<long, IProtocolPlugin> protocolPlugins = new Dictionary<long, IProtocolPlugin>();
        private Dictionary<long, IChannelDriverPlugin> channelDriverPlugins = new Dictionary<long, IChannelDriverPlugin>();
        private Dictionary<long, IEditorViewPlugin> editorViewPlugins = new Dictionary<long, IEditorViewPlugin>();
        private Dictionary<long, IEventViewPlugin> eventViewPlugins = new Dictionary<long, IEventViewPlugin>();
        private Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        private int itemID = 0;

        public MainController(MainView view)
        {
            this.view = view;
            this.view.SetController(this);
            // register built-in plugins
            IProtocolPlugin protoPlg = new TcpPlugin();
            protocolPlugins.Add(protoPlg.ID, protoPlg);

            IChannelDriverPlugin cdPlg = new DefaultProxyChannelDriverPlugin();
            channelDriverPlugins.Add(cdPlg.ID, cdPlg);
            cdPlg = new ManualChannelDriverPlugin();
            channelDriverPlugins.Add(cdPlg.ID, cdPlg);
            cdPlg = new CSScriptChannelDriverPlugin();
            channelDriverPlugins.Add(cdPlg.ID, cdPlg);

            var coreViewsPlugin = new CoreViewsPlugin();
            editorViewPlugins.Add(coreViewsPlugin.ID, coreViewsPlugin);
            eventViewPlugins.Add(coreViewsPlugin.ID, coreViewsPlugin);

            loadExternalPlugins();

            load();
        }

        private void loadExternalPlugins()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "Plugins";
            foreach (var dll in Directory.GetFiles(path, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFile(dll);
                    assemblies.Add(assembly.FullName, assembly);
                    foreach(var type in assembly.GetTypes())
                    {
                        if(type.GetInterface(typeof(IProtocolPlugin).FullName) != null)
                        {
                            var plugin = (IProtocolPlugin)Activator.CreateInstance(type);
                            protocolPlugins.Add(plugin.ID, plugin);
                        }
                        if (type.GetInterface(typeof(IChannelDriverPlugin).FullName) != null)
                        {
                            var plugin = (IChannelDriverPlugin)Activator.CreateInstance(type);
                            channelDriverPlugins.Add(plugin.ID, plugin);
                        }
                        if (type.GetInterface(typeof(IEventViewPlugin).FullName) != null)
                        {
                            var plugin = (IEventViewPlugin)Activator.CreateInstance(type);
                            eventViewPlugins.Add(plugin.ID, plugin);
                        }
                        if (type.GetInterface(typeof(IEditorViewPlugin).FullName) != null)
                        {
                            var plugin = (IEditorViewPlugin)Activator.CreateInstance(type);
                            editorViewPlugins.Add(plugin.ID, plugin);
                        }
                    }
                }
                catch(Exception e)
                {
                    Debug.WriteLine("Failed to load Assembly {0}, exception: {1}", path + "/" + dll, e.Message);
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
                        controllers.Add(pack.Controller);
                        view.AddPage(instance.Value.Name, pack.View.GetForm());
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
            assemblies.TryGetValue(args.Name, out assembly);
            return assembly;
        }

        public void Close()
        {
            foreach (var cont in controllers)
            {
                cont.Stop();
                cont.Close();
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
                    controllers.Add(pack.Controller);
                    model.AddInstance(++itemID, plugin.ID, name, type, pack.Controller.Instance.Settings);
                    foreach(var driver in drivers)
                    {
                        model.AddDriverToInstance(itemID, driver.Key, driver.Value);
                    }
                    bindInstanceEvents(pack.Controller.Instance);
                    view.AddPage(name, pack.View.GetForm());
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
                controllers.Add(pack.Controller);
                bindInstanceEvents(pack.Controller.Instance);
                view.AddPage(name ?? "unknown", pack.View.GetForm());
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