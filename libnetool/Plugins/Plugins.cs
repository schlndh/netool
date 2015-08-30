using Netool.ChannelDrivers;
using Netool.Controllers;
using Netool.Logging;
using Netool.Network;
using Netool.Views;
using System;
using System.Collections.Generic;

namespace Netool.Plugins
{
    public class InstancePack
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

    public class ChannelDriverPack
    {
        public readonly IChannelDriver Driver;
        /// <summary>
        /// A channel driver detail view (can be null)
        /// </summary>
        public readonly IFormView View;

        public ChannelDriverPack(IChannelDriver driver, IFormView view = null)
        {
            Driver = driver;
            View = view;
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

    /// <summary>
    /// Basic interface for plugins providing a protocol support
    /// </summary>
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

    /// <summary>
    /// Basic interface for plugins providing new channel driver
    /// </summary>
    public interface IChannelDriverPlugin : IPlugin
    {
        /// <summary>
        /// Create a new channel driver
        /// </summary>
        /// <returns></returns>
        ChannelDriverPack CreateChannelDriver();
        /// <summary>
        /// Create a new channel driver with given settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        ChannelDriverPack CreateChannelDriver(object settings);
    }

    /// <summary>
    /// Basic interface for plugins providing new EventView
    /// </summary>
    public interface IEventViewPlugin : IPlugin
    {
        /// <summary>
        /// Create Event Views for one channel view
        /// </summary>
        /// <returns></returns>
        List<IEventView> CreateEventViews();
    }

    /// <summary>
    /// Basic interface for plugins providing new EditorView
    /// </summary>
    public interface IEditorViewPlugin : IPlugin
    {
        /// <summary>
        /// Create Editor Views for one channel view
        /// </summary>
        /// <returns></returns>
        List<IEditorView> CreateEditorViews();
    }

    public static class ProtocolPluginExtensions
    {
        /// <summary>
        /// Check if protocol plugin supports given instance type
        /// </summary>
        /// <param name="p">protocol plugin</param>
        /// <param name="t">type</param>
        /// <returns></returns>
        public static bool SupportsType(this IProtocolPlugin p, InstanceType t)
        {
            if (t == InstanceType.Client) return p.SupportsClient;
            else if (t == InstanceType.Server) return p.SupportsServer;
            else return p.SupportsProxy;
        }
    }
}