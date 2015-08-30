using Netool.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace Netool
{
    [Serializable]
    internal struct InstanceTab
    {
        public long PluginID;
        public string Name;
        public InstanceType Type;
        public object Settings;
        /// <summary>
        /// Key = DriverID, Value = Order
        /// </summary>
        public Dictionary<int, int> Drivers;
    }

    [Serializable]
    internal struct ChannelDriverTab
    {
        public long PluginID;
        public string Name;
        public object Settings;
    }

    [Serializable]
    internal class MainModel
    {
        private Dictionary<int, InstanceTab> openInstances = new Dictionary<int, InstanceTab>();
        public IReadOnlyDictionary<int, InstanceTab> OpenInstances { get { return openInstances; } }

        private Dictionary<int, ChannelDriverTab> channelDrivers = new Dictionary<int, ChannelDriverTab>();
        public IReadOnlyDictionary<int, ChannelDriverTab> ChannelDrivers { get { return channelDrivers; } }

        public void AddInstance(int id, long pluginID, string name, InstanceType type, object settings)
        {
            openInstances.Add(id, new InstanceTab { Drivers = new Dictionary<int,int>(), PluginID = pluginID, Name = name, Type = type, Settings = settings });
        }

        public void AddChannelDriver(int id, long pluginID, string name, object settings)
        {
            channelDrivers.Add(id, new ChannelDriverTab { PluginID = pluginID, Name = name, Settings = settings });
        }

        public void RemoveInstance(int id)
        {
            openInstances.Remove(id);
        }

        public void AddDriverToInstance(int instance, int driver, int order)
        {
            InstanceTab i;
            if (openInstances.TryGetValue(instance, out i))
            {
                i.Drivers.Add(driver, order);
            }
        }

        public void RemoveDriverFromInstance(int instance, int driver)
        {
            InstanceTab i;
            if(openInstances.TryGetValue(instance, out i))
            {
                i.Drivers.Remove(driver);
            }
        }

        public void RemoveChannelDriver(int id)
        {
            foreach(var instance in OpenInstances)
            {
                instance.Value.Drivers.Remove(id);
            }
            channelDrivers.Remove(id);
        }
    }
}