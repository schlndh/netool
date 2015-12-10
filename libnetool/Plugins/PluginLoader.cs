using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Netool.Plugins
{
    public class PluginLoader
    {
        private Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
        private Dictionary<long, IPlugin> plugins = new Dictionary<long, IPlugin>();

        /// <summary>
        /// This event is called when new plugin is loaded (before its AfterLoad)
        /// </summary>
        public event EventHandler<IPlugin> PluginLoaded;

        /// <summary>
        /// Loaded plugins
        /// </summary>
        public IEnumerable<IPlugin> Plugins { get { return plugins.Values; } }

        /// <summary>
        /// Try get loaded assembly by its full name
        /// </summary>
        /// <param name="fullname"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public bool TryGetAssembly(string fullname, out Assembly a)
        {
            return assemblies.TryGetValue(fullname, out a);
        }

        /// <summary>
        /// Loads plugins from all dlls in given directory
        /// </summary>
        /// <param name="dir"></param>
        public void LoadPluginsFromDirectory(string dir)
        {
            foreach (var dll in Directory.GetFiles(dir, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFile(dll);
                    assemblies.Add(assembly.FullName, assembly);
                    LoadPluginsFromAssembly(assembly);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to load Assembly {0}, exception: {1}", dir + "/" + dll, e.Message);
                }
            }
        }

        /// <summary>
        /// Load plugins from given assembly
        /// </summary>
        /// <param name="assembly"></param>
        public void LoadPluginsFromAssembly(Assembly assembly)
        {
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsInterface && !type.IsAbstract && type.IsVisible && type.GetInterface(typeof(IPlugin).FullName) != null)
                    {
                        try
                        {
                            var plugin = (IPlugin)Activator.CreateInstance(type);
                            plugins.Add(plugin.ID, plugin);
                            onPluginLoaded(plugin);
                            var setup = plugin as IExtensiblePlugin;
                            if (setup != null) setup.AfterLoad(this);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Failed to load type {0} from Assembly {1}, exception: {2}", type.Name, assembly.FullName, e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to load types from Assembly {0}, exception: {1}", assembly.FullName, e.Message);
            }
        }

        private void onPluginLoaded(IPlugin plugin)
        {
            if (PluginLoaded != null) PluginLoaded(this, plugin);
        }
    }
}