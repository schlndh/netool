using Netool.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace Netool
{
    [Serializable]
    public struct InstanceTab
    {
        public long PluginID;
        public string Name;
        public InstanceType Type;
        public object Settings;
    }

    [Serializable]
    public class MainModel
    {
        private List<InstanceTab> openInstances = new List<InstanceTab>();
        public ReadOnlyCollection<InstanceTab> OpenInstances { get { return openInstances.AsReadOnly(); } }

        public void AddInstance(long pluginID, string name, InstanceType type, object settings)
        {
            openInstances.Add(new InstanceTab { PluginID = pluginID, Name = name, Type = type, Settings = settings });
        }
    }
}