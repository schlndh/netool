using Netool.ChannelDrivers;
using Netool.Logging;
using Netool.Network;
using System.Collections.Generic;

namespace Netool.Controllers
{
    public interface IInstanceController
    {
        IInstance Instance { get; }
        InstanceLogger Logger { get; }
        /// <summary>
        /// Starts the associated instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the associated instance.
        /// </summary>
        void Stop();

        /// <summary>
        /// Closes all associated resource.
        /// </summary>
        /// <remarks>
        /// Called when instance tab is about to be closed.
        /// </remarks>
        void Close();

        /// <summary>
        /// Show channel detail.
        /// </summary>
        /// <param name="id">1-based channel ID</param>
        void ShowDetail(int id);

        /// <summary>
        /// Adds a driver to driver queue.
        /// </summary>
        /// <param name="d">driver</param>
        /// <param name="order">lower number = higher priority</param>
        void AddDriver(IChannelDriver d, int order);
    }
}