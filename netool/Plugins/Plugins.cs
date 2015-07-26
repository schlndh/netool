using Netool.Controllers;
using Netool.Logging;
using Netool.Network;
using Netool.Views;
using System;

namespace Netool.Plugins
{
    public struct InstancePack
    {
        public readonly IInstanceView View;
        public readonly IInstanceController Controller;
        public readonly InstanceType Type;

        public InstancePack(IInstanceView view, IInstanceController controller, InstanceType type)
        {
            View = view;
            Controller = controller;
            Type = type;
        }
    }

    /// <summary>
    /// Base interface for all plugins
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Unique plugin ID. See Plugins/ReservedPluginIDs.md.
        /// </summary>
        long ID { get; }
        string Name { get; }
        string Description { get; }
        Version Version { get; }
        string Author { get; }
    }

    public interface IProtocolPlugin : IPlugin
    {
        bool SupportsServer { get; }
        bool SupportsClient { get; }
        bool SupportsProxy { get; }
        string ProtocolName { get; }

        /// <summary>
        /// This method is called when new instance is to be created. Plugin is free to ask user for additional settings.
        /// </summary>
        /// <param name="logger">instance logger to be passed to the instance controller</param>
        /// <param name="t">type of the instance to create, one of the supported types</param>
        /// <returns>Newly created instance</returns>
        InstancePack CreateInstance(InstanceLogger logger, InstanceType t);

        /// <summary>
        /// Create instance with given settings.
        /// </summary>
        /// <param name="logger">instance logger to be passed to the instance controller</param>
        /// <param name="t">type of the instance to create, one of the supported types</param>
        /// <param name="settings"></param>
        /// <returns>Newly created instance with given settings</returns>
        InstancePack CreateInstance(InstanceLogger logger, InstanceType t, object settings);

        /// <summary>
        /// This method is called when instance is to be restored from file.
        /// The plugin has to recognize the proper instance type itself.
        /// </summary>
        /// <param name="logger">Instance logger for an instance created by this plugin</param>
        /// <returns>Restored instance</returns>
        InstancePack RestoreInstance(InstanceLogger logger);
    }

    public static class ProtocolPluginExtensions
    {
        public static bool SupportsType(this IProtocolPlugin p, InstanceType t)
        {
            if (t == InstanceType.Client) return p.SupportsClient;
            else if (t == InstanceType.Server) return p.SupportsServer;
            else return p.SupportsProxy;
        }
    }
}